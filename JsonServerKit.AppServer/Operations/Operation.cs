using JsonServerKit.AppServer.Data;
using Serilog;

namespace JsonServerKit.AppServer.Operations
{
    public abstract class Operation<T> : IOperation
    {
        public PayloadInfo HandleObject(object message, MessageContext context, ILogger logger)
        {
            if (message is T genericType)
                return HandleDomainObject(genericType, context, logger);

            return null;
        }

        protected virtual PayloadInfo HandleDomainObject(T message, MessageContext context, ILogger logger)
        {
            return null;
        }
    }
}