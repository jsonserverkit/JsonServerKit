using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using JsonServerKit.AppServer;

namespace JsonServerKit.Test
{
    [TestClass]
    public class TestSsl
    {
        //SslTestContext _ctx;
        static readonly byte[] clientMessage = Encoding.ASCII.GetBytes("This is a message from the client");
        static readonly byte[] serverMessage = Encoding.ASCII.GetBytes("This is a message from the server");

        public void Setup()
        {
        }

        public void Teardown()
        {
        }

        [TestMethod]
        public void TestServerCertificate()
        {
            var thumbprint = "3070B82C7AE269684EF4C18537DDC0C7E6FDD88A";

            var store = StoreLocation.LocalMachine;
            try
            {
                var serverCertificate = CertificateHandling.GetCertificateFromStore("3070B82C7AE269684EF4C18537DDC0C7E6FDD88A", store);
                Assert.IsTrue(serverCertificate.Thumbprint == thumbprint);
            }
            catch (Exception e)
            {
                Assert.Fail($"Certificat with thumbprint {thumbprint} not fount. Exception: {e.Message}");
            }
        }

        [TestMethod]
        public void TestAsyncBasic()
        {
            var listener = new TcpListener(IPAddress.Loopback, 7777);
            listener.Start(5);
            var ep = (IPEndPoint)listener.LocalEndpoint;

            Console.WriteLine("Server> waiting for accept");

            listener.BeginAcceptTcpClient((IAsyncResult ar) =>
            {
                var client = listener.EndAcceptTcpClient(ar);

                var sslStream = new SslStream(client.GetStream(), false);
                Console.WriteLine("Server> authenticate");

                var serverCertificate = CertificateHandling.GetCertificateFromStore("3070B82C7AE269684EF4C18537DDC0C7E6FDD88A", StoreLocation.LocalMachine);
                sslStream.BeginAuthenticateAsServer(serverCertificate, async (ar2) =>
                {
                    sslStream.EndAuthenticateAsServer(ar2);

                    var buf = new byte[256];
                    await sslStream.ReadAsync(buf, 0, buf.Length);
                    Assert.AreEqual(clientMessage.ToString(), buf.ToString());

                    await sslStream.WriteAsync(serverMessage, 0, serverMessage.Length);

                    sslStream.Close();
                    client.Close();

                    Console.WriteLine("Server> done");
                }, null);
            }, null);

            var evtDone = new AutoResetEvent(false);

            var tcp = new TcpClient(AddressFamily.InterNetwork);
            tcp.BeginConnect(ep.Address.ToString(), ep.Port, (IAsyncResult ar) =>
            {
                tcp.EndConnect(ar);

                var sslStream = new SslStream(tcp.GetStream());
                Console.WriteLine("Client> authenticate");

                sslStream.BeginAuthenticateAsClient("localhost", async (ar2) =>
                {
                    sslStream.EndAuthenticateAsClient(ar2);

                    await sslStream.WriteAsync(clientMessage, 0, clientMessage.Length);

                    var buf = new byte[256];
                    await sslStream.ReadAsync(buf, 0, buf.Length);
                    Assert.AreEqual(serverMessage.ToString(), buf.ToString());

                    sslStream.Close();
                    tcp.Close();

                    Console.WriteLine("Client> done");

                    evtDone.Set();
                }, null);
            }, null);

            evtDone.WaitOne();
        }
    }
}