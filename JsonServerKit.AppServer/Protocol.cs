using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Text;
using JsonServerKit.AppServer.Interfaces;
using Serilog;

namespace JsonServerKit.AppServer
{
    /// <summary>
    /// Designed to provide message handling on protocol level.
    /// </summary>
    public class Protocol : IProtocol
    {
        #region Private members

        private readonly ILogger _logger;
        private readonly StringBuilder _messageBuffer = new StringBuilder();
        //private readonly string _endOfMessage = Environment.NewLine;
        private readonly string _endOfMessage = "\n\r";
        private string _currentMessage;


        #endregion

        #region Constructor/s

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="logger">Logger interface.</param>
        public Protocol(ILogger logger)
        {
            _logger = logger;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Read a message from SslSteam.
        /// </summary>
        /// <param name="sslStream">SslStream object.</param>
        /// <returns>Message/s as string.</returns>
        public string[] ReadMessage(SslStream sslStream)
        {
            var networkBuffer = new byte[2048];
            // ReSharper disable once RedundantAssignment
            var networkReadBytesCount = -1;
            do
            {
                networkReadBytesCount = sslStream.Read(networkBuffer, 0, networkBuffer.Length);

                // Use Decoder class to convert from bytes to UTF8 in case a character spans two buffers.
                var decoder = Encoding.UTF8.GetDecoder();
                var chars = new char[decoder.GetCharCount(networkBuffer, 0, networkReadBytesCount)];
                decoder.GetChars(networkBuffer, 0, networkReadBytesCount, chars, 0);
                _messageBuffer.Append(chars);
                // Check for first end of "message".
                _currentMessage = _messageBuffer.ToString();
                if (_currentMessage.IndexOf(_endOfMessage, StringComparison.CurrentCulture) != -1)
                {
                    break;
                }
            } while (networkReadBytesCount != 0);

            if (networkReadBytesCount == 0)
                return new string[] { };

            // Bring the "wohle" messages to return, leave an additional new messages start in place.
            var inputSplit = _currentMessage.Split(_endOfMessage);
            string[] inputMessages = null;
            if (inputSplit.Length > 1)
            {
                inputMessages = inputSplit.Take(inputSplit.Length - 1).ToArray();
                _messageBuffer.Clear();
                var ongoingMessage = inputSplit.Last();
                if (!string.IsNullOrEmpty(ongoingMessage))
                    _messageBuffer.Append(ongoingMessage);
            }

            return inputMessages;
        }

        /// <summary>
        /// Read a message from SslSteam.
        /// Todo ooo https://github.com/smarkets/IronSmarkets/blob/master/IronSmarkets/Sockets/SafeSslStream.cs
        /// </summary>
        /// <param name="sslStream">SslStream object.</param>
        /// <returns>Message as string.</returns>
        public string ReadMessage(SslStream sslStream, ManualResetEvent run)
        {
            return null;
        }
        #endregion

    }
}
