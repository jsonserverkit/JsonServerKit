using JsonServerKit.AppServer.Data;
using JsonServerKit.AppServer.Data.Crud;
using Serilog;

namespace JsonServerKit.AppServer.Operations
{
    public abstract class CrudOperation<T> : ICrudOperation<T, Create<T>, Read<T>, Update<T>, Delete<T>>
    {
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

        protected virtual PayloadInfo HandleDomainObjectCreate(Create<T> message, MessageContext context, ILogger logger)
        {
            return null;
        }

        protected virtual PayloadInfo HandleDomainObjectRead(Read<T> message, MessageContext context, ILogger logger)
        {
            return null;
        }

        protected virtual PayloadInfo HandleDomainObjectUpdate(Update<T> message, MessageContext context, ILogger logger)
        {
            return null;
        }

        protected virtual PayloadInfo HandleDomainObjectDelete(Delete<T> message, MessageContext context, ILogger logger)
        {
            return null;
        }
    }
}
