using JsonServerKit.AppServer.Data.Crud;
using JsonServerKit.AppServer;
using JsonServerKit.AppServer.Data;
using JsonServerKit.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Your.BusinessLogic.CrudOperations;
using Your.BusinessLogic.Operations;
using Your.CliClient;
using Your.Domain.BusinessObjects;

namespace JsonServerKit.Test 
{
    [TestClass]
    public class TestSendReceive : TestBase
    {
        #region Private members

        private static CancellationTokenSource cancellationSource;
        private static Task tcpServerTask;

        #endregion

        #region Test setup

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            // Get new CancellationTokenSource
            cancellationSource = new CancellationTokenSource();

            // Mockstart the server.
            // Get a runtime object instance.
            tcpServerTask = MockStartTcpServer();
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            cancellationSource.Cancel();
            tcpServerTask.GetAwaiter().GetResult();
        }

        #endregion

        #region Test methods

        [TestMethod]
        public void Account()
        {
            // Payload/s
            var payloads = new []{ new Data().GetNewAccountPayload() };

            // Run a number of clients in parallel to send a load to the server.
            Client.Startup(1, payloads);
        }

        [TestMethod]
        public void CreateAccount()
        {
            // Payload/s
            var payloads = new[] { new Data().GetNewCreateAccountPayload() };

            // Run a number of clients in parallel to send a load to the server.
            Client.Startup(1, payloads);
        }

        [TestMethod]
        public void ReadAccount()
        {
            // Payload/s
            var payloads = new[] { new Data().GetNewReadAccountPayload() };

            // Run a number of clients in parallel to send a load to the server.
            Client.Startup(1, payloads);
        }

        [TestMethod]
        public void UpdateAccount()
        {
            // Payload/s
            var payloads = new[] { new Data().GetNewUpdateAccountPayload() };

            // Run a number of clients in parallel to send a load to the server.
            Client.Startup(1, payloads);
        }

        [TestMethod]
        public void DeleteAccount()
        {
            // Payload/s
            var payloads = new[] { new Data().GetNewDeleteAccountPayload() };

            // Run a number of clients in parallel to send a load to the server.
            Client.Startup(1, payloads);
        }

        [TestMethod]
        public void Product()
        {
            // Payload/s
            var payloads = new[] { new Data().GetNewProductPayload() };

            // Run a number of clients in parallel to send a load to the server.
            Client.Startup(1, payloads);
        }

        [TestMethod]
        public void LoadTest()
        {
            // Run a number of clients in parallel to send a load (of Payload's) to the server.
            Client.Startup(32, () =>
            {
                var payloads = new List<Payload>();
                // Create some payload objects.
                foreach (var i in Enumerable.Range(1, 25))
                    payloads.AddRange(new Data().GetLoadTestPayload());

                return payloads.ToArray();
            });
        }

        #endregion

        #region Private methods

        public static async Task MockStartTcpServer()
        {
            var startup = new Startup();

            // Provide appsettings.json configuration to DI.
            startup.ConfigureAppConfiguration(new List<Action<HostBuilderContext, IConfigurationBuilder>>
            {
                (_, config) =>
                {
                    config
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json");
                }
            });

            // Configure required/additional services for DI.
            startup.ConfigureServices(new List<Action<IServiceCollection>>
            {
                // Configure/provide a logger implementation.
                // This Version of "JsonServerKit.AppServer.Startup" requires a Wrapper around the Serilog logger.
                // This dependency might become subject to change.
                services => { services.AddSingleton<ILog, Log>(); }
            });

            // Configure the domain handlers.
            startup.ConfigureDomainObjectHandler(typeof(Product), new ProductOperation());
            startup.ConfigureDomainObjectHandler(typeof(Account), new AccountOperation());
            startup.ConfigureDomainObjectCrudHandler(new[] { typeof(Create<Account>), typeof(Read<Account>), typeof(Update<Account>), typeof(Delete<Account>) }, new AccountCrudOperations());

            // Call to run.
            await startup.Run(new string[] { }, cancellationSource);
        }

        #endregion

    }
}
