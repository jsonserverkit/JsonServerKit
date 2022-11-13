using System.Diagnostics;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using JsonServerKit.AppServer;
using JsonServerKit.AppServer.Data;
using JsonServerKit.AppServer.Data.Crud;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Your.Domain.BusinessObjects;
using Log = JsonServerKit.Logging.Log;

namespace JsonServerKit.CliClient
{

    /// <summary>
    /// Some basic client functionality to communicate with "JsonServerKit.AppServer".
    /// </summary>
    public class Client
    {
        private readonly JsonSerializerSettings _jsonSendSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All, StringEscapeHandling = StringEscapeHandling.Default };
        private readonly JsonSerializerSettings _jsonReceiveSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, StringEscapeHandling = StringEscapeHandling.Default };

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            LogSink(string.Format("Certificate error: {0}", sslPolicyErrors));

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        public void RunClient(string machineName)
        {
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
            logConfig.PathLogFileJsonFormated += $".Client.{pId}.{DateTime.Now:yyyy.MM.dd}.txt";
            logConfig.PathLogFileTextFormated += $".Client.{pId}.{DateTime.Now:yyyy.MM.dd}.txt";
            Serilog.Log.Logger = new Log(configuration, logConfig).GetLogger();
            var protocol = new Protocol(Serilog.Log.Logger);
            LogSink("Client Hello.");

            // Create a TCP/IP client socket.
            // machineName is the host running the server application.
            var tcpServerConfig = new TcpServer.TcpServerConfig();
            var tcpServerConfigSection = configuration.GetSection("TcpServerConfig");
            tcpServerConfigSection.Bind(tcpServerConfig);

            var client = new TcpClient(machineName, tcpServerConfig.Port);
            LogSink("Client connected.");

            // Create an SSL stream that will close the client's stream.
            var sslStream = new SslStream(client.GetStream(), false, ValidateServerCertificate, null);
            try
            {
                // Get client certificate from store.
                var clientCert = CertificateHandling.GetCertificateFromStore(tcpServerConfig.CertificateThumbprint, StoreLocation.CurrentUser);
                sslStream.AuthenticateAsClient(machineName, new X509Certificate2Collection(clientCert), SslProtocols.Tls13, false);
            }
            catch (AuthenticationException e)
            {
                LogSink(string.Format("Exception: {0}", e.Message));
                if (e.InnerException != null)
                {
                    LogSink(string.Format("Inner exception: {0}", e.InnerException.Message));
                }

                LogSink("Authentication failed - closing the connection.");
                client.Close();
                return;
            }
            LogSink("Client authenticated.");

            // Init session.
            var sessionName = "CliClient-Main";
            var sessionRequest = new SessionRequest { SessionName = sessionName };
            var jsonStringSessionRequest = JsonConvert.SerializeObject(sessionRequest, Formatting.None, _jsonSendSerializerSettings);
            sslStream.Write(Encoding.UTF8.GetBytes(new StringBuilder().AppendLine(jsonStringSessionRequest).ToString()));
            sslStream.Flush();

            // Read server response.
            var serverMsg = protocol.ReadMessage(sslStream);
            ConsoleWriteServerMessage(serverMsg);
            var serverSessionInfo = JsonConvert.DeserializeObject<SessionInfo>(serverMsg, _jsonReceiveSerializerSettings);

            do
            {
                LogSink("Enter an e-mail address to be applied to an Account object that will b sent to the server. Or enter quit to exit:");
                var msg = GetInput();

                // Check for quit/shutdown.
                if (msg?.ToLower().Equals("quit") ?? false)
                    break;

                try
                {
                    var payloads = CreatePayload(serverSessionInfo, msg);
                    foreach (var payload in payloads)
                        SendData(payload, sslStream, protocol);
                }
                catch (Exception e)
                {
                    LogSink(e.Message);
                }

            } while (true);

            try
            {
                // End session.
                var sessionInfo = new SessionInfo { SessionName = sessionName, Id = serverSessionInfo?.Id ?? 0, CloseNow = true };
                var jsonSessionInfoString = JsonConvert.SerializeObject(sessionInfo, Formatting.None, _jsonSendSerializerSettings);
                sslStream.Write(Encoding.UTF8.GetBytes(new StringBuilder().AppendLine(jsonSessionInfoString).ToString()));
                sslStream.Flush();
            }
            catch (Exception e)
            {
                LogSink(e.Message);
            }

            try
            {
                if (client.Client.Connected)
                    client.Client.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                // Close the client connection.
                client.Client.Close();
                client.Close();
            }
        }

        private void SendData(Payload payload, SslStream sslStream, Protocol protocol)
        {
            string serverMsg;
            // Spezial serializer Settings.
            var jsonPayloadString = JsonConvert.SerializeObject(payload, Formatting.None, _jsonSendSerializerSettings);
            sslStream.Write(Encoding.UTF8.GetBytes(new StringBuilder().AppendLine(jsonPayloadString).ToString()));
            sslStream.Flush();

            // Read server response.
            serverMsg = protocol.ReadMessage(sslStream);
            ConsoleWriteServerMessage(serverMsg);
        }

        private static string? GetInput()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            var msg = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            return msg;
        }

        private static Payload[] CreatePayload(SessionInfo? serverSessionInfo, string? msg)
        {
            var account = new Account
            {
                Id = 777,
                Email = msg ?? "some.default.instead@of.null",
                Active = true,
                CreatedDate = DateTime.Now,
                Roles = new[] { $"User{Environment.NewLine}", "Admin" },
            };

            var id = DateTime.Now.Millisecond * DateTime.Now.Second;

            var payload = new Payload[]
            {
                new()
                {
                    // Some random context information.
                    Context = new MessageContext
                    {
                        SessionId = serverSessionInfo?.Id ?? 0,
                        MessageId = ++id
                    },
                    Message = new Create<Account>
                    {
                        Value = account
                    }
                },
                new()
                {
                    Context = new MessageContext
                    {
                        SessionId = serverSessionInfo?.Id ?? 0,
                        MessageId = ++id
                    },
                    Message = new Read<Account>
                    {
                        Id = account.Id
                    }
                },
                new()
                {
                    // Some random context information.
                    Context = new MessageContext
                    {
                        SessionId = serverSessionInfo?.Id ?? 0,
                        MessageId = ++id
                    },
                    Message = new Update<Account>
                    {
                        Value = account
                    }
                },
                new()
                {
                    Context = new MessageContext
                    {
                        SessionId = serverSessionInfo?.Id ?? 0,
                        MessageId = ++id
                    },
                    Message = new Delete<Account>
                    {
                        Id = account.Id
                    }
                },
                new()
                {
                    // Some random context information.
                    Context = new MessageContext
                    {
                        SessionId = serverSessionInfo?.Id ?? 0,
                        MessageId = ++id
                    },
                    // Some payload data containing the user input.
                    Message = account
                },


            };

            return payload;
        }

        private void LogSink(string msg)
        {
            Console.WriteLine(msg);
            Serilog.Log.Logger.Information(msg);
        }

        private void ConsoleWriteServerMessage(string serverMessage)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Server response: {0}", serverMessage);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
