// .Net
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Sockets;
// JsonServerKit
using JsonServerKit.Logging;
using JsonServerKit.AppServer;
using JsonServerKit.AppServer.Data;
using JsonServerKit.AppServer.Data.Crud;
// Your domain
using Your.BusinessLogic.CrudOperations;
using Your.BusinessLogic.Operations;
using Your.CliClient;
using Your.Domain.BusinessObjects;
using Newtonsoft.Json;
using System.Linq;
using System.Text;

namespace JsonServerKit.Test
{
    [TestClass]
    public class TestMessageProcessor : TestBase
    {

        [AssemblyInitialize()]
        public static void AssemblyInit(TestContext context)
        {
        }

        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
        }

        [ClassCleanup()]
        public static void ClassCleanup()
        {
        }

        [AssemblyCleanup()]
        public static void AssemblyCleanup()
        {

        }

        [TestMethod]
        [DataRow($"ii", 1)]
        [DataRow($"ii\r\n", 2)]
        [DataRow($"ii\r\njj", 2)]
        [DataRow($"ii\r\njj\r\nkk", 3)]
        [DataRow($"ii\r\njj\r\nkk\r\n", 4)]
        // Debug
        [DataRow("{\"$type\":\"JsonServerKit.AppServer.Data.Payload, JsonServerKit.AppServer\",\"Context\":{\"$type\":\"JsonServerKit.AppServer.Data.MessageContext, JsonServerKit.AppServer\",\"MessageId\":260875611,\"SessionGuid\":\"guid1\"},\"Message\":{\"$type\":\"JsonServerKit.AppServer.Data.Crud.Create`1[[Your.Domain.BusinessObjects.Account, Your.Domain]], JsonServerKit.AppServer\",\"Value\":{\"$type\":\"Your.Domain.BusinessObjects.Account, Your.Domain\",\"Id\":777,\"Email\":\"some.one.instead@of.null\",\"Active\":true,\"Roles\":{\"$type\":\"System.String[], System.Private.CoreLib\",\"$values\":[\"User\\r\\n\",\"Admin\"]},\"CreatedDate\":\"2022-11-17T21:00:29.3998129+01:00\",\"Version\":100}}}\r\n",2)]
        public void Split(string splitMeUp, int expectetElements)
        {
            string splitBy = Environment.NewLine;
            try
            {
                var splitted = splitMeUp.Split(splitBy);
                Assert.IsTrue(splitted.Length == expectetElements);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [TestMethod]
        public void ProcessingClientLoad()
        {
            CancellationTokenSource cancellationSource = new CancellationTokenSource();

            // Mockcreate a client.
            MockStartTcpClientAndRun(cancellationSource.Token);
        }

        [TestMethod]
        public void ProcessingClientLoadToServer()
        {
            CancellationTokenSource cancellationSource = new CancellationTokenSource();

            // Mockstart the server.
            // Get a runtime object instance.
            var tcpServerTask = MockStartTcpServer(cancellationSource);


            // Mockcreate a client.
            MockStartTcpClientAndRun(cancellationSource.Token);

            cancellationSource.Cancel();
            tcpServerTask.GetAwaiter().GetResult();
        }

        private static async Task MockStartTcpServer(CancellationTokenSource cancellationSource)
        {
            var runtime = new Runtime();

            // Provide appsettings.json configuration to DI.
            runtime.ConfigureAppConfiguration(new List<Action<HostBuilderContext, IConfigurationBuilder>>
            {
                (_, config) =>
                {
                    config
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json");
                }
            });

            // Configure required/additional services for DI.
            runtime.ConfigureServices(new List<Action<IServiceCollection>>
            {
                // Configure/provide a logger implementation.
                // This Version of "JsonServerKit.AppServer.Runtime" requires a Wrapper around the Serilog logger.
                // This dependency might become subject to change.
                services => { services.AddSingleton<ILog, Log>(); }
            });

            // Configure the domain handlers.
            runtime.ConfigureDomainObjectHandler(typeof(Product), new ProductOperation());
            runtime.ConfigureDomainObjectHandler(typeof(Account), new AccountOperation());
            runtime.ConfigureDomainObjectCrudHandler(new[] { typeof(Create<Account>), typeof(Read<Account>), typeof(Update<Account>), typeof(Delete<Account>) }, new AccountCrudOperations());

            // Call to run.
            await runtime.Run(null, cancellationSource);
        }

        public void MockStartTcpClientAndRun(CancellationToken cancellationToken)
        {
            Client.RunClient();
        }
    }
}
