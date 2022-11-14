using System.Drawing;
using System.Net.Security;
using System.Net.Sockets;
using System.Numerics;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using JsonServerKit.AppServer;
using JsonServerKit.AppServer.Data;
using JsonServerKit.AppServer.Data.Crud;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Your.Domain.BusinessObjects;

namespace Your.CliClient
{
    public class Client
    {
        #region Static method to setup a test run.

        public static void RunClient()
        {
            var serverName = "localhost";

            // Load appsettings file.
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var tcpServerConfig = new TcpServer.TcpServerConfig();
            var tcpServerConfigSection = configuration.GetSection("TcpServerConfig");
            tcpServerConfigSection.Bind(tcpServerConfig);
            var clients = new List<Client>();
            foreach (var i in Enumerable.Range(1, 32))
            {
                // Create a TCP/IP client socket.
                // machineName is the host running the server application.
                var tcpClient = new TcpClient(serverName, tcpServerConfig.Port);
                var serversClient = new Client(tcpClient, tcpServerConfig.CertificateThumbprint, serverName);
                clients.Add(serversClient);
            }

            var taskList = new List<Task>();
            foreach (var client in clients)
            {
                try
                {
                    // Start execution of the server/listener on a separate task (Threadpool Thread) and therefor leave the current thread Nr.1. 
                    // This wills proper cancellation while the listener blocks in AcceptTcpClient method.
                    var clientTask = Task.Run(() => { client.ClientSession(); });
                    taskList.Add(clientTask);
                }
                catch (Exception e)
                {
                    var a = e.Message;
                }
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
        //private readonly string _endOfMessage = Environment.NewLine;
        private readonly string _endOfMessage = "\n\r";

        #endregion

        #region Constructor/s

        public Client(TcpClient tcpClient, string certThumbprint, string serverName)
        {
            _tcpClient = tcpClient;
            _certThumbprint = certThumbprint;
            _serverName = serverName;
            // Create an SSL stream that will close the client's stream.
            _sslStream = new SslStream(_tcpClient.GetStream(), false, ValidateServerCertificate);
        }

        #endregion

        #region Public methods

        public void ClientSession()
        {
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

            CancellationTokenSource cancellationSource = new CancellationTokenSource();

            var sendingTask = Task.Run(() =>
            {
                try
                {
                    var payloads = CreatePayload();

                    for (int i = 0; i < 25; i++)
                    {
                        foreach (var payload in payloads)
                        {
                            SendData(payload);
                            // Wait some milliseconds (play with 1ms - 10ms) to keep some real world context.
                            Thread.Sleep(5);
                        }
                    }

                    try
                    {
                        // End session.
                        var sessionInfo = new SessionInfo { CloseNow = true };
                        var jsonStringSessionInfo = JsonConvert.SerializeObject(sessionInfo, Formatting.None, _jsonSendSerializerSettings);
                        //                var jsonSessionInfoString = GetShortFormMessage(sessionInfo);
                        WriteToSslStream(jsonStringSessionInfo);
                    }
                    catch (Exception e)
                    {
                    }
                }
                catch (Exception e)
                {
                }

            });

            var receivingTask = Task.Run(() =>
            {
                try
                {
                    // Read server response.
                    while (!cancellationSource.Token.IsCancellationRequested)
                    {
                        var protocol = new Protocol(null); // Get logger from somewhere.
                        var serverMsg = protocol.ReadMessage(_sslStream);
                        //var serverSessionInfo = JsonConvert.DeserializeObject<SessionInfo>(serverMsg, _jsonReceiveSerializerSettings);
                    }
                }
                catch (Exception e)
                {
                }

            }, cancellationSource.Token);


            // When finished sending, we send the SessionInfo.CloseNow message.
            sendingTask.GetAwaiter().GetResult();
            Thread.Sleep(10000);
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
        }

        #endregion

        #region Private methods

        private void WriteToSslStream(string jsonStringSessionRequest)
        {
            var msgBuilder = new StringBuilder().Append(jsonStringSessionRequest).Append(_endOfMessage);
            _sslStream.Write(Encoding.UTF8.GetBytes(msgBuilder.ToString()));
            _sslStream.Flush();
        }

        private void SendData(Payload payload)//, Func<string> objectSerializer)
        {
            // Spezial serializer Settings.
            var jsonPayloadString = JsonConvert.SerializeObject(payload, Formatting.None, _jsonSendSerializerSettings);
//            var jsonPayloadString = GetShortFormMessage(payload);
            WriteToSslStream(jsonPayloadString);

            //var protocol = new Protocol(null); // Get logger from somewhere.
            //var serverMsg = protocol.ReadMessage(_sslStream);
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


        #region Create test data.

        private Payload[] CreatePayload()
        {
            // File content of a picture of 104KB size.
            var filePath = "..\\..\\..\\product.jpg";
            var base64FileString = GetBase64StringFromFilePath(filePath);

            // Product business object that contains the image (as base64encoded string).
            var product = new Product
            {
                ItemPicture = base64FileString,
            };

            // Account business objext.
            var account = new Account
            {
                Id = 777,
                Email = "some.one.instead.of@no.one",
                Active = true,
                CreatedDate = DateTime.Now,
                Roles = new[] { $"User{Environment.NewLine}", "Admin" },
                Version = 100
            };

            // Create a payload object array based on the data created above.
            var payload = new[]
            {
                new()
                {
                    // Some random context information.
                    Context = GetNewMessageContext(),
                    Message = new Create<Account> { Value = account }
                },
                GetNewProductPayload(product),
                new()
                {
                    // Some random context information.
                    Context = GetNewMessageContext(),
                    Message = new Read<Account> { Id = account.Id }
                },
                GetNewProductPayload(product),
                new()
                {
                    // Some random context information.
                    Context = GetNewMessageContext(),
                    Message = new Update<Account> { Value = account }
                },
                GetNewProductPayload(product),
                new()
                {
                    // Some random context information.
                    Context = GetNewMessageContext(),
                    Message = new Delete<Account> { Id = account.Id }
                },
                GetNewProductPayload(product),
                new()
                {
                    // Some random context information.
                    Context = GetNewMessageContext(),
                    Message = account
                },
                GetNewProductPayload(product)
            };
            
            return payload;
        }

        private MessageContext GetNewMessageContext()
        {
            return new MessageContext
            {
                MessageId = GetNewId()
            };
        }

        private Payload GetNewProductPayload(Product product)
        {
            return new()
            {
                // Some random context information.
                Context = GetNewMessageContext(),
                // Some payload data containing the user input.
                Message = product
            };
        }

        private int GetNewId()
        {
            // Konrad Rudolph :)
            // https://stackoverflow.com/questions/65292465/biginteger-intvalue-equivalent-in-c-sharp
            return (int)(uint)(new BigInteger(Guid.NewGuid().ToByteArray()) & uint.MaxValue);
        }

        private string GetBase64StringFromFilePath(string filePath)
        {
            byte[] fileBytes;

            if (!File.Exists(filePath))
                return null;


            var image = Image.FromFile(filePath);
            using (var ms = new MemoryStream())
            {
                image.Save(ms, image.RawFormat);
                fileBytes = ms.ToArray();
            }
            return Convert.ToBase64String(fileBytes);
        }

        #endregion
    }
}
