using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using JsonServerKit.AppServer;
using JsonServerKit.AppServer.Data;
using JsonServerKit.AppServer.LogTemplate;
using JsonServerKit.DataAccess;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Your.DataAccessLayer.Repositories;
using Your.Domain.BusinessObjects;
using Log = JsonServerKit.Logging.Log;

namespace Your.CliClient
{
    public class Client
    {
        #region Static method to setup a test run.

        public static void Startup(int parallelClientsCount, Payload[] payloads)
        {
            Startup(parallelClientsCount, () => payloads);
        }

        public static void Startup(int parallelClientsCount, Func<Payload[]> createPayload)
        {
            var serverName = "localhost";

            // Load appsettings file.
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();


            var logConfig = new Log.LogConfig();
            var logConfigSection = configuration.GetSection("LogConfig");
            logConfigSection.Bind(logConfig);

            // Logger
            // Logging erstellen.
            var pId = Process.GetCurrentProcess().Id;
            logConfig.PathLogFileJsonFormated += $".{pId}.Client.txt";//{pId}.{DateTime.Now:yyyy.MM.dd}.txt
            logConfig.PathLogFileTextFormated += $".{pId}.Client.txt";
            Serilog.Log.Logger = new Log(configuration, logConfig).GetLogger();

            var tcpServerConfig = new TcpServer.TcpServerConfig();
            var tcpServerConfigSection = configuration.GetSection("TcpServerConfig");
            tcpServerConfigSection.Bind(tcpServerConfig);
            var clients = new List<Client>();
            foreach (var i in Enumerable.Range(1, parallelClientsCount))
            {
                // Create a TCP/IP client socket.
                // machineName is the host running the server application.
                var tcpClient = new TcpClient(serverName, tcpServerConfig.Port);
                var serversClient = new Client(tcpClient, tcpServerConfig.CertificateThumbprint, serverName, Serilog.Log.Logger);
                clients.Add(serversClient);
            }

            var taskList = new List<Task>();
            foreach (var client in clients)
            {
                // Start execution of the server/listener on a separate task (Threadpool Thread) and therefor leave the current thread Nr.1. 
                // This wills proper cancellation while the listener blocks in AcceptTcpClient method.
                var clientTask = Task.Run(() => { client.ClientSession(createPayload()); });
                taskList.Add(clientTask);
            }

            Task.WaitAll(taskList.ToArray());
        }

        #endregion

        #region Private members

        private readonly JsonSerializerSettings _jsonSendSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All, 
            StringEscapeHandling = StringEscapeHandling.Default,
            NullValueHandling = NullValueHandling.Ignore
        };

        private readonly JsonSerializerSettings _jsonReceiveSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            StringEscapeHandling = StringEscapeHandling.Default,
            NullValueHandling = NullValueHandling.Ignore
        };
        
        private TcpClient _tcpClient { get; set; }
        private readonly SslStream _sslStream;
        private string _certThumbprint { get; set; }
        private string _serverName { get; set; }
        private ILogger _logger { get; set; }
        private Protocol _protocol;

        //private readonly string _endOfMessage = Environment.NewLine;
        private readonly string _endOfMessage = "\n\r";

        private readonly string _msgMessageSent = "Message sent.";
        private readonly string _msgMessageReceived = "Message received.";


        #endregion

        #region Constructor/s

        public Client(TcpClient tcpClient, string certThumbprint, string serverName, ILogger logger)
        {
            _tcpClient = tcpClient;
            _certThumbprint = certThumbprint;
            _serverName = serverName;
            // Create an SSL stream that will close the client's stream.
            _sslStream = new SslStream(_tcpClient.GetStream(), false, ValidateServerCertificate);
            _logger = logger;
            _protocol = new Protocol(_logger);
        }

        #endregion

        #region Public methods

        public void ClientSession(Payload[] payloads)
        {
            var endSession = new ManualResetEvent(false);
            try
            {
                // Get client certificate from store.
                var clientCert = CertificateHandling.GetCertificateFromStore(_certThumbprint, StoreLocation.CurrentUser);
                _sslStream.AuthenticateAsClient(_serverName, new X509Certificate2Collection(clientCert), SslProtocols.Tls13, false);
            }
            catch (AuthenticationException e)
            {
                _tcpClient.Close();
                return;
            }

            var messageTrackingSend = new Dictionary<long, PayloadStatistic>();
            var messageTrackingReceive = new Dictionary<long, PayloadStatistic>();
            // Stopwatch (Statistic)
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var cancellationSource = new CancellationTokenSource();
            // Send data.
            var sendingTask = Task.Run(() =>
            {
                try
                {

                    foreach (var payload in payloads)
                    {
                        var payloadStatistic = new PayloadStatistic { Payload = payload, MessageType = payload.Message.GetType().ToString() };
                        messageTrackingSend.Add(payload.Context.MessageId, payloadStatistic);
                        SendPayload(payload);
                        // Statistic
                        payloadStatistic.TimeInMsMessageSent = stopWatch.ElapsedMilliseconds;
                        _logger.Information(Messages.TemplateMessageWithIdAndTextTwoPlacehodlers, payload.Context.MessageId, _msgMessageSent);
                        // Wait some milliseconds (play with 1ms - 100ms) to keep some real world context.
                        Thread.Sleep(50);
                    }

                    endSession.WaitOne();

                    // When finished sending, we send the SessionInfo.CloseNow message.
                    SendSessionInfo(new SessionInfo { CloseNow = true });
                }
                catch (Exception e)
                {
                }
            });

            var receivefailed = false;
            var receivefinished = false;
            // Receive data.
            var receivingTask = Task.Run<bool>(() =>
            {
                try
                {
                    // Read server response.
                    while (!cancellationSource.Token.IsCancellationRequested)
                    {
                        if (!_sslStream.CanRead)
                            throw new OperationCanceledException($"{nameof(_sslStream.CanRead)} is false.");

                        var serverMessages = _protocol.ReadMessage(_sslStream);
                        if (serverMessages.Length == 0)
                            return receivefinished = true;

                        var payloadStatistic = new PayloadStatistic { TimeInMsMessageReceived = stopWatch.ElapsedMilliseconds };

                        foreach (var message in serverMessages)
                        {
                            try
                            {
                                //_logger.Information("Message received:{0}", message);
                                var messageObject = JsonConvert.DeserializeObject<dynamic>(message, _jsonReceiveSerializerSettings);
                                if (messageObject is PayloadInfo payloadInfo)
                                {
                                    _logger.Information(Messages.TemplateMessageWithIdAndTextTwoPlacehodlers,
                                        payloadInfo.Context.MessageId, _msgMessageReceived);
                                    messageTrackingReceive.Add(payloadInfo.Context.MessageId, payloadStatistic);

                                    if (payloads.Length == messageTrackingReceive.Count)
                                        return true;

                                }
                            }
                            catch (Exception e)
                            {
                                // Log info about the message that may have caused an exception. Then throw.
                                _logger.Error("Exception: {0}, on message:{1}", e.Message, message);
                                throw;
                            }
                        }
                    }

                    return true;
                }
                catch (Exception e)
                {
                    receivefailed = true;
                    return false;
                }
                finally
                {
                    endSession.Set();
                }


            }, cancellationSource.Token);

            // Wait for the sending to be completed.
            sendingTask.GetAwaiter().GetResult();

            // Async check all messages where answered (received a payload info from server)
            var checkCompetion = Task.Run(() =>
            {
                do
                {
                    // If everything was answerd.
                    if (messageTrackingSend.Count == messageTrackingReceive.Count)
                    {
                        break;
                    }

                    // Or we finish if receiving task fails.
                    if (receivefailed)
                        break;

                    // Or we finish if receiving task fails.
                    if (receivefinished)
                        break;

                    Thread.Sleep(200);

                } while (true);
                _logger.Information("Sent {0} messages. Received {1} messages.", messageTrackingSend.Count, messageTrackingReceive.Count);
                var diff = messageTrackingSend.Keys.Except(messageTrackingReceive.Keys).ToList();
                _logger.Information("Missing {0} messages.", diff.Count);
                foreach (var messageId in diff)
                    _logger.Information("Missing message with id: {0}", messageId);
            });
            checkCompetion.GetAwaiter().GetResult();

            // Signal cancel to the receiver.
            cancellationSource.Cancel();
            receivingTask.GetAwaiter().GetResult();

            try
            {
                if (_tcpClient.Client.Connected)
                    _tcpClient.Client.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                // Close the client connection.
                _tcpClient.Client.Close();
                _tcpClient.Close();
            }

            // Statistics 
            DumpStatistics(messageTrackingSend, messageTrackingReceive);
        }

        private void DumpStatistics(Dictionary<long, PayloadStatistic> messageTrackingSend, Dictionary<long, PayloadStatistic> messageTrackingReceive)
        {
            try
            {
                // Statistics
                var statisticItemList = messageTrackingSend.Join(messageTrackingReceive, x => x.Key, y => y.Key,
                    (x, y) => new Statistic
                    {
                        MessageId = x.Key,
                        MessageType = x.Value.MessageType,
                        TimeInMsMessageSent = x.Value.TimeInMsMessageSent,
                        TimeInMsMessageReceived = y.Value.TimeInMsMessageReceived,
                        TimeDiff = y.Value.TimeInMsMessageReceived - x.Value.TimeInMsMessageSent
                    }).ToList();

                IRepository<Statistic> repository = new StatisticRepository();

                foreach (var statistic in statisticItemList)
                    repository.Create(statistic);
            }
            catch (Exception e)
            {
                _logger.Error("Exception: {0}, on message:{1}", e.Message);
            }
        }

        #endregion

        #region Private methods

        private void WriteToSslStream(string jsonStringSessionRequest)
        {
            var msgBuilder = new StringBuilder().Append(jsonStringSessionRequest).Append(_endOfMessage);
            _sslStream.Write(Encoding.UTF8.GetBytes(msgBuilder.ToString()));
            _sslStream.Flush();
        }

        private void SendPayload(Payload payload)
        {
            var jsonPayloadString = JsonConvert.SerializeObject(payload, Formatting.None, _jsonSendSerializerSettings);
//            var jsonPayloadString = GetShortFormMessage(payload);
            WriteToSslStream(jsonPayloadString);
        }
        private void SendSessionInfo(SessionInfo sessionInfo)
        {
            var jsonPayloadString = JsonConvert.SerializeObject(sessionInfo, Formatting.None, _jsonSendSerializerSettings);
            //            var jsonPayloadString = GetShortFormMessage(sessionInfo);
            WriteToSslStream(jsonPayloadString);
        }

        private string GetShortFormMessage(object msg)
        {
            return JsonConvert.SerializeObject(msg, Formatting.None, _jsonSendSerializerSettings);

            var msgFrag = JsonConvert.SerializeObject(msg, Formatting.None, _jsonSendSerializerSettings).Substring(1);
            return "{" + string.Format("\"$type\":\"{0}\",{1}", msg.GetType().AssemblyQualifiedName, msgFrag);

        }
        private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        #endregion
    }
}
