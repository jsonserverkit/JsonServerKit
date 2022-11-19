using System.Data;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks.Dataflow;
using JsonServerKit.AppServer.Data;
using JsonServerKit.AppServer.Interfaces;
using JsonServerKit.AppServer.LogTemplate;
using Newtonsoft.Json;
using Serilog;

namespace JsonServerKit.AppServer
{
    /// <summary>
    /// TLS/SSL (and .NET Framework 4.0)
    /// https://www.red-gate.com/simple-talk/development/dotnet-development/tlsssl-and-net-framework-4-0/
    /// Level of thread safety
    /// https://stackoverflow.com/questions/7969903/what-level-of-thread-safety-can-i-expect-from-system-net-security-sslstream
    /// APM (Asynchronous Programming Model) vs. TPL (Task Parallel Library) vs TAP (Task Asynchronous Programming-model)   
    /// https://stackoverflow.com/questions/73359085/how-to-read-continuously-and-asynchronously-from-network-stream-using-abstract-s
    /// APM
    /// https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/
    /// TPL (und Workflow)
    /// https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/tpl-and-traditional-async-programming
    /// https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-implement-a-producer-consumer-dataflow-pattern
    /// </summary>
    public class TcpSession : ITcpSession
    {
        #region Private members

        private readonly ILogger _logger;
        private X509Certificate2 _x509Certificate2;
        private TcpClient _tcpClient;
        private SslStream _sslStream;
        private readonly IProtocol _protocol;
        private IMessageProcessor _messageProcessor; 
        private Task _receiver = null;
        private Task _sender = null;
        private BufferBlock<ReceiveSendContext> _blockingBuffer = new();

        #region Messages

        private readonly string _msgStartReceiving = "Start receiving.";
        private readonly string _msgStartSending = "Start sending.";
        private readonly string _msgMessageReceived = "Message received:";
        private readonly string _msgStartProcessing = "Start message processing.";
        private readonly string _msgSessionStarted = "Session started.";
        private readonly string _msgMessageResponseToBuffer = "Message response posted to output buffer.";
        private readonly string _msgMessageResponseSent = "Message response sent.";
        private readonly string _msgMessageData = "Message data:";
        private readonly string _msgBusinessObject = "Business object:";
        private readonly string _msgResponseData = "Response data:";
        private readonly string _msgCloseSessionOnRequest = "Closing connection on session close request.";
        // Errors
        // Message not supported.
        private readonly string _msgErrorMessageNotSupported = "Message is not supported!";
        private readonly string _msgErrorCloseOnNotSupportedMessage = "Closing connection on unknown message.";
        // Exception occurred.
        private readonly string _msgErrorException = "This is not supported!";
        private readonly string _msgErrorCloseOnException = "Closing connection.";

        #endregion

        #region Serialization / Deserialization

        private readonly JsonSerializerSettings _jsonReceiveSerializerSettings = new() { TypeNameHandling = TypeNameHandling.Auto, StringEscapeHandling = StringEscapeHandling.Default };
        private readonly JsonSerializerSettings _jsonSendSerializerSettings = new() { TypeNameHandling = TypeNameHandling.All, StringEscapeHandling = StringEscapeHandling.Default };
        //private readonly string _endOfMessage = Environment.NewLine;
        private readonly string _endOfMessage = "\n\r";


        #endregion

        #endregion

        #region Constructor/s

        public TcpSession(TcpClient tcpClient, IMessageProcessor messageProcessor, X509Certificate2 x509Certificate2, ILogger logger, IProtocol protocol)
        {
            _tcpClient = tcpClient;
            // Create the SslStream using the client's network stream.
            _sslStream = new SslStream(_tcpClient.GetStream(), false);
            _x509Certificate2 = x509Certificate2;
            _logger = logger;
            _protocol = protocol;
            _messageProcessor = messageProcessor;
        }

        #endregion

        #region Interface methods

        public async Task<bool> AuthenticateServerAsync(bool doRequireClientAuthentication, bool checkCertificateRevocation, CancellationToken stoppingToken)
        {
            try
            {
                // If for whatever reason the SslStream would be null, an exeption will be thrown.
                if (_sslStream == null)
                    throw new ArgumentNullException(nameof(_sslStream));

                // If for whatever reason the certificate would be null, an exeption will be thrown.
                if (_x509Certificate2 == null)
                    throw new ArgumentNullException(nameof(_x509Certificate2));

                // Authenticate the server and do require the client to authenticate (optionally the client could be defined to not authenticate).
                await _sslStream.AuthenticateAsServerAsync(_x509Certificate2, clientCertificateRequired: doRequireClientAuthentication, SslProtocols.Tls13, checkCertificateRevocation: checkCertificateRevocation);
                DisplayStreamSecuritySettings(_sslStream);

                return true;
            }
            catch (AuthenticationException e)
            {
                _logger.Error("Exception: {0}", e.Message);
                if (e.InnerException != null)
                    _logger.Error("Inner exception: {0}", e.InnerException.Message);
                _logger.Error("Authentication failed - closing the connection.");
            }

            return false;
        }

        public void StartReceiving(CancellationToken stoppingToken)
        {
            var _receiver = Task.Factory.StartNew(Receiving, stoppingToken);
        }

        public void StartSending(CancellationToken stoppingToken)
        {
            var _sender = Task.Factory.StartNew(Sending, stoppingToken);
        }

        public void HandlePayload(dynamic incommingMsg, string msg, ReceiveSendContext receiveSendContext)
        {
            if (incommingMsg is not Payload payload)
                throw new InvalidCastException(nameof(incommingMsg));

            // Payload with Context null means close connection.
            if (payload.Context == null)
                throw new ArgumentNullException(nameof(payload.Context));

            var msgId = payload.Context.MessageId;
            _logger.Information(Messages.TemplateMessageWithIdAndTextThreePlacehodlers, msgId, _msgMessageData, msg);
            _logger.Information(Messages.TemplateMessageWithIdAndTextThreePlacehodlers, msgId, _msgBusinessObject, payload?.Message?.GetType());
      
            // Payload with Message null means connection close.
            if (payload.Message == null)
                throw new ArgumentNullException(nameof(payload.Message));

            _logger.Information(Messages.TemplateMessageWithIdAndTextTwoPlacehodlers, msgId, _msgStartProcessing);
            var payloadInfo = _messageProcessor.ProcessMessage(payload.Message, payload.Context, _logger);
        
            // Assure a return PayloadInfo exists.
            if (payloadInfo == null)
                payloadInfo = new PayloadInfo { Context = payload.Context };
            
            // Assure a MessageContext is returned.
            if (payloadInfo.Context == null)
                payloadInfo.Context = payload.Context;

            receiveSendContext.Context = payloadInfo.Context;
            receiveSendContext.OutputMessage = JsonConvert.SerializeObject(payloadInfo, Formatting.None, _jsonSendSerializerSettings);
        }

        public bool HandleSessionInfo(dynamic incommingMsg, string msg)
        {
            if (incommingMsg is not SessionInfo sessionInfo)
                throw new InvalidCastException(nameof(incommingMsg));

            _logger.Information("{0} {1}", _msgMessageData, msg);
            if (sessionInfo.CloseNow)
            {
                try
                {
                    // Before closing we mark the buffer as complete and wait for completion.
                    _blockingBuffer.Complete();
                    _blockingBuffer.Completion.GetAwaiter().GetResult();
                }
                catch (Exception e)
                {
                    _logger.Error("Exception: {0}", e.Message);
                }
                _logger.Information(_msgCloseSessionOnRequest);
                return true;
            }

            return false;
        }

        #endregion

        #region Private methods - Network communication - Receiving

        /// <summary>
        /// Read the message sent by the client.
        /// Post it to a buffer to be sent by the sending thread.
        /// </summary>
        private void Receiving()
        {
            try
            {
                _logger.Information(_msgStartReceiving);
                do
                {
                    var close = false;

                    // Read a message/s from the client.
                    var inputContext = new ReceiveSendContext();
                    if (!ReadMessage(inputContext))
                        break;

                    foreach (var message in inputContext.InputMessages)
                    {
                        var receiveSendContext = new ReceiveSendContext { InputMessages = new[] { message } };
                        if (!ProcessMessage(message, receiveSendContext))
                        {
                            close = true;
                            break;
                        }

                        try
                        {
                            // Post the outgoing message to the buffer.
                            PostResponseMessageToBuffer(receiveSendContext);
                        }
                        catch (Exception e)
                        {
                            // This would be fatal. Log with highest alert.
                            _logger.Error(e.Message);
                        }
                    }

                    if (close)
                        break;
                } while (true);
            }
            finally
            {
                // The client stream will be closed with the sslStream
                // because we specified this behavior when creating
                // the sslStream.
                _sslStream.Close();
                _tcpClient.Close();
            }
        }

        private bool ReadMessage(ReceiveSendContext receiveSendContext)
        {
            try
            {
                receiveSendContext.InputMessages = _protocol.ReadMessage(_sslStream);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                CloseOnException();
                return false;
            }
            return true;
        }

        private bool ProcessMessage(string msg, ReceiveSendContext receiveSendContext)
        {
            try
            {
                // Only if required. Feature: Make on/off available in config
                //_logger.Information("{0} {1}", _msgMessageReceived, msg);

                /*
                // Wrap to JsonStreamReader?
                // https://stackoverflow.com/questions/26601594/what-is-the-correct-way-to-use-json-net-to-parse-stream-of-json-objects
                var copyMsg = (string)msg.Clone();
                JsonTextReader reader = new JsonTextReader(new StringReader(copyMsg));
                reader.SupportMultipleContent = true;

                dynamic incommingMsg = null;
                while (true)
                {
                    if (!reader.Read())
                    {
                        break;
                    }

                    // TypeNameHandling = TypeNameHandling.Auto, StringEscapeHandling = StringEscapeHandling.Default
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.TypeNameHandling = TypeNameHandling.Auto;
                    serializer.StringEscapeHandling = StringEscapeHandling.Default;
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    var multiMsgElement = serializer.Deserialize<dynamic>(reader);
                    _logger.Information("Multimessgage Element: {0} {1}", _msgMessageReceived, multiMsgElement?.GetType());
                    incommingMsg = multiMsgElement;
                }
                */
                // Deserialize incomming message as dynamic.
                var incommingMsg = JsonConvert.DeserializeObject<dynamic>(msg, _jsonReceiveSerializerSettings);
                _logger.Information("{0} {1}", _msgMessageReceived, incommingMsg?.GetType());

                var close = false;
                switch (incommingMsg)
                {
                    case Payload:
                    {
                        HandlePayload(incommingMsg, msg, receiveSendContext);
                        break;
                    }
                    case SessionInfo:
                        close = HandleSessionInfo(incommingMsg, msg);
                        break;
                    default:
                        CloseOnUnknownMessage();
                        close = true;
                        break;
                }

                // Check if the connection will be closed.
                if (close)
                    return false;
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                CloseOnException();
                return false;
            }

            return true;
        }

        private void PostResponseMessageToBuffer(ReceiveSendContext receiveSendContext)
        {
            if (receiveSendContext.Context != null)
                _logger.Information(Messages.TemplateMessageWithIdAndTextTwoPlacehodlers, receiveSendContext.Context.MessageId, _msgMessageResponseToBuffer);
            else
                _logger.Information(_msgMessageResponseToBuffer);

            var x = (ITargetBlock<ReceiveSendContext>)_blockingBuffer;
            x.Post(receiveSendContext);
        }

        private void CloseOnUnknownMessage()
        {
            _logger.Error(_msgErrorMessageNotSupported);
            _logger.Error(_msgErrorCloseOnNotSupportedMessage);
        }
        private void CloseOnException()
        {
            _logger.Error(_msgErrorException);
            _logger.Error(_msgErrorCloseOnException);
        }

        #endregion

        #region Private methods - Network communication - Sending

        private async Task Sending()
        {
            _logger.Information(_msgStartSending);
            // Sending is done by one consumer, so use ISourcBlock.
            // In case of multiple consumers IReceivableSourceBlock with TryReceive should be used.
            var source = (ISourceBlock<ReceiveSendContext>)_blockingBuffer;
            while (source.OutputAvailableAsync().Result)
            {
                var receiveSendContext = await source.ReceiveAsync();

                var messageBytes = Encoding.UTF8.GetBytes(new StringBuilder().Append(receiveSendContext.OutputMessage).Append(_endOfMessage).ToString());
                _sslStream.Write(messageBytes);
                _sslStream.Flush();

                if (receiveSendContext.Context != null)
                {
                    _logger.Information(Messages.TemplateMessageWithIdAndTextThreePlacehodlers, receiveSendContext.Context.MessageId, _msgResponseData, receiveSendContext.OutputMessage);
                    _logger.Information(Messages.TemplateMessageWithIdAndTextTwoPlacehodlers, receiveSendContext.Context.MessageId, _msgMessageResponseSent);
                }
                else
                {
                    _logger.Information("{0} {1}", _msgResponseData, receiveSendContext.OutputMessage);
                    _logger.Information(_msgMessageResponseSent);
                }
            }
        }

        #endregion

        #region Private methods - Show security info

        private void DisplayStreamSecuritySettings(SslStream sslStream)
        {
            // Display the properties and settings for the authenticated stream.
            DisplaySecurityLevel(sslStream);
            DisplaySecurityServices(sslStream);
            DisplayCertificateInformation(sslStream);
            DisplayStreamProperties(sslStream);
        }

        private void DisplaySecurityLevel(SslStream stream)
        {
            _logger.Information("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
            _logger.Information("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
            _logger.Information("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
            _logger.Information("Protocol: {0}", stream.SslProtocol);
        }
        private void DisplaySecurityServices(SslStream stream)
        {
            _logger.Information("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
            _logger.Information("IsSigned: {0}", stream.IsSigned);
            _logger.Information("Is Encrypted: {0}", stream.IsEncrypted);
        }
        private void DisplayStreamProperties(SslStream stream)
        {
            _logger.Information("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
            _logger.Information("Can timeout: {0}", stream.CanTimeout);
        }

        private void DisplayCertificateInformation(SslStream stream)
        {
            _logger.Information("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

            var localCertificate = stream.LocalCertificate;
            if (localCertificate != null)
            {
                _logger.Information("Local cert was issued to {0} and is valid from {1} until {2}.",
                    localCertificate.Subject,
                    localCertificate.GetEffectiveDateString(),
                    localCertificate.GetExpirationDateString());
            }
            else
            {
                _logger.Information("Local certificate is null.");
            }
            // Display the properties of the client's certificate.
            var remoteCertificate = stream.RemoteCertificate;
            if (remoteCertificate != null)
            {
                _logger.Information("Remote cert was issued to {0} and is valid from {1} until {2}.",
                    remoteCertificate.Subject,
                    remoteCertificate.GetEffectiveDateString(),
                    remoteCertificate.GetExpirationDateString());
            }
            else
            {
                _logger.Information("Remote certificate is null.");
            }
        }

        #endregion
    }
}
