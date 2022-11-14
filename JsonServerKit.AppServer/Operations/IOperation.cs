using JsonServerKit.AppServer.Data;
using Serilog;

namespace JsonServerKit.AppServer.Operations
{
    /// <summary>
    /// Provides the method signature to handle a domain object
    /// </summary>
    public interface IOperation
    {
        /// <summary>
        /// Handles a domain object.
        /// </summary>
        /// <param name="message">The message object.</param>
        /// <param name="context">The message context object.</param>
        /// <param name="logger">The interface to the logger used by the server component, intended to provide consistent logging.</param>
        /// <returns>PayloadInfo object.</returns>
        public PayloadInfo HandleObject(object message, MessageContext context, ILogger logger);
    }
}
