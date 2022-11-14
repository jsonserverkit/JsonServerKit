using JsonServerKit.AppServer.Data;
using Serilog;

namespace JsonServerKit.AppServer.Operations
{

    /// <summary>
    /// Implementation of the IOperation interface.
    /// Interface <see cref="IOperation"/> provides the method signature to handle a domain object.
    /// </summary>
    /// <typeparam name="T">The desired domain object to handle.</typeparam>
    public abstract class Operation<T> : IOperation
    {
        /// <summary>
        /// Handles a domain object.
        /// </summary>
        /// <param name="message">The message object.</param>
        /// <param name="context">The message context object.</param>
        /// <param name="logger">The interface to the logger used by the server component, intended to provide consistent logging.</param>
        /// <returns>PayloadInfo object.</returns>
        public PayloadInfo HandleObject(object message, MessageContext context, ILogger logger)
        {
            if (message is T genericType)
                return HandleDomainObject(genericType, context, logger);

            return null;
        }

        /// <summary>
        /// Designed to implement the create operation on the domain object.
        /// </summary>
        /// <param name="message">The message object.</param>
        /// <param name="context">The message context object.</param>
        /// <param name="logger">The interface to the logger. See also <see cref="Operation{T}.HandleObject"/>.</param>
        /// <returns>PayloadInfo object.</returns>
        protected virtual PayloadInfo HandleDomainObject(T message, MessageContext context, ILogger logger)
        {
            return null;
        }
    }
}