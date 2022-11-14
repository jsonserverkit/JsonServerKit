using JsonServerKit.AppServer.Data;
using JsonServerKit.AppServer.Data.Crud;
using Serilog;

namespace JsonServerKit.AppServer.Operations
{
    /// <summary>
    /// Provides basic methodology to implement the desired crud operations.
    /// Interface <see cref="IOperation"/> provides the method signature to handle a domain object.
    /// Interface <see cref="ICrudOperation{T,TCreate,TRead,TUpdate,TDelete}"/>. provides the type constraints.
    /// </summary>
    /// <typeparam name="T">The desired domain object to handle.</typeparam>
    public abstract class CrudOperation<T> : ICrudOperation<T, Create<T>, Read<T>, Update<T>, Delete<T>>, IOperation
    {
        /// <summary>
        /// Handles a domain object by dispatching it to its corresponding operation implementation.
        /// </summary>
        /// <param name="message">The message object.</param>
        /// <param name="context">The message context object.</param>
        /// <param name="logger">The interface to the logger used by the server component, intended to provide consistent logging.</param>
        /// <returns>PayloadInfo object.</returns>
        public PayloadInfo HandleObject(object message, MessageContext context, ILogger logger)
        {
            if (message is Create<T> genericCreateType)
                return HandleDomainObjectCreate(genericCreateType, context, logger);

            if (message is Read<T> genericReadType)
                return HandleDomainObjectRead(genericReadType, context, logger);

            if (message is Update<T> genericUpdateType)
                return HandleDomainObjectUpdate(genericUpdateType, context, logger);

            if (message is Delete<T> genericDeleteType)
                return HandleDomainObjectDelete(genericDeleteType, context, logger);

            return null;
        }

        /// <summary>
        /// Designed to implement the create operation on the domain object.
        /// </summary>
        /// <param name="message">The message object of type <see cref="Create{T}"/>.</param>
        /// <param name="context">The message context object.</param>
        /// <param name="logger">The interface to the logger. See also <see cref="CrudOperation{T}.HandleObject"/>.</param>
        /// <returns>PayloadInfo object.</returns>
        protected virtual PayloadInfo HandleDomainObjectCreate(Create<T> message, MessageContext context, ILogger logger)
        {
            return null;
        }

        /// <summary>
        /// Designed to implement the read operation on the domain object.
        /// </summary>
        /// <param name="message">The message object of type <see cref="Read{T}"/>.</param>
        /// <param name="context">The message context object.</param>
        /// <param name="logger">The interface to the logger. See also <see cref="CrudOperation{T}.HandleObject"/>.</param>
        /// <returns>PayloadInfo object.</returns>
        protected virtual PayloadInfo HandleDomainObjectRead(Read<T> message, MessageContext context, ILogger logger)
        {
            return null;
        }

        /// <summary>
        /// Designed to implement the update operation on the domain object.
        /// </summary>
        /// <param name="message">The message object of type <see cref="Update{T}"/>.</param>
        /// <param name="context">The message context object.</param>
        /// <param name="logger">The interface to the logger. See also <see cref="CrudOperation{T}.HandleObject"/>.</param>
        /// <returns>PayloadInfo object.</returns>
        protected virtual PayloadInfo HandleDomainObjectUpdate(Update<T> message, MessageContext context, ILogger logger)
        {
            return null;
        }

        /// <summary>
        /// Designed to implement the delete operation on the domain object.
        /// </summary>
        /// <param name="message">The message object of type <see cref="Delete{T}"/>.</param>
        /// <param name="context">The message context object.</param>
        /// <param name="logger">The interface to the logger. See also <see cref="CrudOperation{T}.HandleObject"/>.</param>
        /// <returns>PayloadInfo object.</returns>

        protected virtual PayloadInfo HandleDomainObjectDelete(Delete<T> message, MessageContext context, ILogger logger)
        {
            return null;
        }
    }
}
