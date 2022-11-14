using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using JsonServerKit.AppServer.Interfaces;
using JsonServerKit.AppServer.Operations;
using JsonServerKit.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Log = Serilog.Log;

namespace JsonServerKit.AppServer
{
    /// <summary>
    /// --------------------------------------------------------------------------------------------------------------------------------------
    /// Does & Don't
    /// --------------------------------------------------------------------------------------------------------------------------------------
    /// Do use Task.Run for asynchronous things that use a thread.
    /// Do use Task.ResultFrom for things that do not require separate thread pool threads to execute.
    /// Do use ValueTask<![CDATA[<T>]]> for cheap computation tasks.
    /// Don't use .Result or .Wait as they might block/deadlock and because they waste rescources (separate thread) and prevent scaling.
    /// Don't use .ContinueWith if it is not the specific match for the current application scenario.
    /// Do pass the cancellation token and to catch the TaskCanceledException with proper logging.
    /// --------------------------------------------------------------------------------------------------------------------------------------
    /// Documentation
    /// --------------------------------------------------------------------------------------------------------------------------------------
    /// DI - Pattern
    /// https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection
    /// Framework support work "worker services"
    /// https://learn.microsoft.com/en-us/dotnet/core/extensions/workers
    /// Generic Host - "Einstiegspunkt" zu den Ressourcen einer Applikation
    /// https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host
    /// Anwendung: DI Tutorial 
    /// https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-usage
    /// .Net Core "Worker Projekt" als Windows Service (via OS Boardmittel sc.exe) konfigurieren.
    /// https://code-maze.com/aspnetcore-running-applications-as-windows-service/
    /// </summary>
    public class Runtime
    {
        #region Private members

        private List<Action<HostBuilderContext, IConfigurationBuilder>> _appConfigActions = new();
        private List<Action<IServiceCollection>> _diConfigureServices = new();
        private Dictionary<Type, IOperation> _domainObjectHandlers = new();

        #endregion

        #region Public methods

        #region Configuration methods

        public void ConfigureAppConfiguration(List<Action<HostBuilderContext,IConfigurationBuilder>> appConfigActions)
        {
            _appConfigActions.AddRange(appConfigActions);
        }

        public void ConfigureServices(List<Action<IServiceCollection>> diInitActions)
        {
            _diConfigureServices.AddRange(diInitActions);
        }

        public void ConfigureDomainObjectHandler(Type domainType, IOperation handler)
        {
            _domainObjectHandlers.Add(domainType, handler);
        }

        public void ConfigureDomainObjectCrudHandler(Type[] domainTypes, IOperation handler)
        {
            foreach (var type in domainTypes)
                _domainObjectHandlers.Add(type, handler);
        }

        #endregion

        public async Task Run(string[] args)
        {
            CancellationTokenSource cancellationSource = new CancellationTokenSource();
            await Run(args, cancellationSource);
        }

        public async Task Run(string[] args, CancellationTokenSource cancellationSource)
        {
            try
            {
                // Cancelation objects.
                var token = cancellationSource.Token;

                // Host Builder erstellen.
                var hostBuilder = HostConfiguration.CreateHostBuilder(args);

                // Configure app configuration.
                foreach (var appConfigAction in _appConfigActions)
                    hostBuilder.ConfigureAppConfiguration(appConfigAction);

                // Check if required config sections are present in config
                hostBuilder.ConfigureServices((hostContext, services) =>
                {
                    // Check existence of LogConfig.
                    var logConfig = hostContext.Configuration.GetSection("LogConfig");
                    if (logConfig == null)
                        throw new ArgumentNullException($"{nameof(hostContext.Configuration.GetSection)} section LogConfig.");
                    // Check existence of TcpServerConfig.
                    var tcpServerConfig = hostContext.Configuration.GetSection("TcpServerConfig");
                    if(tcpServerConfig==null)
                        throw new ArgumentNullException($"{nameof(hostContext.Configuration.GetSection)} section TcpServerConfig.");

                    // Configure "config sections" as service and therefrom specific signletons of it as service.
                    // See https://stackoverflow.com/questions/58114199/net-core-resolve-config-interface
                    // Add config object/s so it can be injected.
                    services.Configure<Logging.Log.LogConfig>(logConfig);
                    services.Configure<TcpServer.TcpServerConfig>(tcpServerConfig);
                    // Add configuration as singletons.
                    services.AddSingleton<IConfiguration>(hostContext.Configuration);
                    services.AddSingleton<ILogConfig>(sp => sp.GetRequiredService<IOptions<Logging.Log.LogConfig>>().Value);
                    services.AddSingleton<ITcpServerConfig>(sp => sp.GetRequiredService<IOptions<TcpServer.TcpServerConfig>>().Value);
                });

                // Configure general services.
                hostBuilder.ConfigureServices(services => { services.AddSingleton<ILogger>(sp => sp.GetRequiredService<ILog>().GetLogger()); })
                    .ConfigureServices((hc,services) =>
                    {
                        var messageProcessor = new MessageProcessor();
                        // Configure domain object handler.
                        messageProcessor.ConfigureDomainObjectHandlers(_domainObjectHandlers); 
                        services.AddSingleton<IMessageProcessor>(messageProcessor);
                    })
                    .ConfigureServices(services =>
                    {
                        services.AddTransient<ICertificateHandling>(sp => new CertificateHandling());
                        services.AddTransient<IProtocol>(sp => new Protocol(sp.GetRequiredService<ILogger>()));
                        // Resolving Dependencies at Runtime.
                        services.AddSingleton<Func<TcpClient, X509Certificate2, ITcpSession>>(SessionFactory.GetTcpSession);
                    })
                    // TcpServer erstellen.
                    .ConfigureServices(services => { services.AddHostedService<TcpServer>();});

                // Configure domain services.
                foreach (var diInitAction in _diConfigureServices)
                    hostBuilder.ConfigureServices(diInitAction);

                var host = hostBuilder.Build();

                // Host starten.
                await host.RunAsync(token);
            }
            finally
            {
                Log.CloseAndFlush();
                cancellationSource.Dispose();
            }
        }

        #endregion
    }
    }
