using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using JsonServerKit.AppServer.Interfaces;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace JsonServerKit.AppServer
{
    /// <summary>
    /// Designed to serve as a TcpServer wrapper.
    /// </summary>
    public sealed class TcpServer : BackgroundService
    {
        #region Server configuration

        public class TcpServerConfig : ITcpServerConfig
        {
            public int Port { get; set; }
            public string CertificatePath { get; set; }
            public string CertificatePassword { get; set; }
            public string CertificateThumbprint { get; set; }
        }

        #endregion

        #region Private members

        private readonly ILogger _logger;
        private readonly ITcpServerConfig _configuration;
        private readonly Func<TcpClient, X509Certificate2, ITcpSession> _tcpSessionFactory;
        private readonly X509Certificate2 _serverCertificate;

        #endregion

        #region Constructor/s

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger interface.</param>
        /// <param name="configuration">Configuration interface.</param>
        /// <param name="certificateHandling">CertificateHandling interface.</param>
        /// <param name="tcpSessionFactory">TcpSessionFactory function.</param>
        public TcpServer(ILogger logger, ITcpServerConfig configuration, ICertificateHandling certificateHandling , Func<TcpClient, X509Certificate2, ITcpSession> tcpSessionFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _tcpSessionFactory = tcpSessionFactory;
            // Get server certificate from store.
            _serverCertificate = certificateHandling.GetServerCertificateFromStore(_configuration.CertificateThumbprint, StoreLocation.LocalMachine);
        }

        #endregion

        #region Public properties


        #endregion

        #region Method overrides

        /// <summary>
        /// Build a TcpServer using SslStream for communication.
        /// </summary>
        /// <param name="stoppingToken">The CancellationToken object to be used in case of execution cancellation.</param>
        /// <returns>Task object to execute.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information("{0} entering runtime. Hello.", nameof(TcpServer));
            _logger.Information("AssemblyVersion: {0}", Assembly.GetEntryAssembly()?.FullName);
            _logger.Information("Execution on managed thread Id:{0} IsThreadPoolThread:{1}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.IsThreadPoolThread);

            // ------------------------------------------------------------------------------------------------------------------------------------------------------
            // Start main server component.
            // ------------------------------------------------------------------------------------------------------------------------------------------------------
            Task tcpServer = null;
            try
            {
                // Start execution of the server/listener on a separate task (Threadpool Thread) and therefor leave the current thread Nr.1. 
                // This wills proper cancellation while the listener blocks in AcceptTcpClient method.
                tcpServer = Task.Run(() => { StartTcpServer(stoppingToken); }, stoppingToken);
            }
            catch (Exception e)
            {
                _logger.Error("{0}", e.Message);
            }

            // ------------------------------------------------------------------------------------------------------------------------------------------------------
            // Add further server business logic.
            // ------------------------------------------------------------------------------------------------------------------------------------------------------
            // ------------------------------------------------------------------------------------------------------------------------------------------------------

            if (tcpServer != null)
                await tcpServer;
        }

        #endregion

        #region Private methods
        
        private void StartTcpServer(CancellationToken stoppingToken)
        {
            var tcpPort = _configuration.Port;

            // Create a TCP/IP (IPv4) socket and listen for incoming connections.
            var listener = new TcpListener(IPAddress.Any, tcpPort);
            listener.Start();
            _logger.Information("Listener started on managed thread Id:{0} Name:\"{1}\" IsThreadPoolThread:{2}", Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.Name, Thread.CurrentThread.IsThreadPoolThread);

            // Start accepting clients.
            while (!stoppingToken.IsCancellationRequested)
            {
                // Application blocks while waiting for an incoming connection.
                // Type CNTL-C to terminate the server.
                var client = listener.AcceptTcpClient();
                try
                {
                    // Log info about the connected client.
                    _logger.Information("Client {2} family:{0} sockettyp:{1} connected.", client.Client.AddressFamily, client.Client.SocketType, client.Client.RemoteEndPoint);
                    
                    // Create a TcpSession object that handles communication.
                    var tcpSession = _tcpSessionFactory(client, _serverCertificate);

                    // Authenticate will be handlet async on a seperate thread.
                    var authenticationTask = tcpSession.AuthenticateServerAsync(stoppingToken);
                    if (!authenticationTask.Result)
                        return;

                    // Communication - On seperate Task's.
                    tcpSession.StartReceiving(stoppingToken);
                    tcpSession.StartSending(stoppingToken);

                }
                catch (Exception e)
                {
                    _logger.Error(e.Message);
                }
            }
        }

        #endregion
    }
}