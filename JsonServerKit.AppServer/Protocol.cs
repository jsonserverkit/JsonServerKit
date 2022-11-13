using System.Net.Security;
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
        /// <returns>Message as string.</returns>
        public string ReadMessage(SslStream sslStream)
        {
            var buffer = new byte[2048];
            var messageData = new StringBuilder();
            // ReSharper disable once RedundantAssignment
            var bytes = -1;
            do
            {
                bytes = sslStream.Read(buffer, 0, buffer.Length);
                //bytes = sslStream.ReadAsync(buffer, default(CancellationToken)).Result;
                //sslStream.BeginRead()


                // Use Decoder class to convert from bytes to UTF8
                // in case a character spans two buffers.
                var decoder = Encoding.UTF8.GetDecoder();
                //var encoder = Encoding.UTF8.GetEncoder();
                var chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
                decoder.GetChars(buffer, 0, bytes, chars, 0);
                messageData.Append(chars);
                // Check for EOF or an empty message.
                if (messageData.ToString().IndexOf($"{Environment.NewLine}", StringComparison.CurrentCulture) != -1)
                {
                    break;
                }
            } while (bytes != 0);

            // Replace this dumb/slow version with something faster.
            var msgString = messageData.ToString();
            return msgString.ReplaceLineEndings("");
        }

        #endregion

    }
}
