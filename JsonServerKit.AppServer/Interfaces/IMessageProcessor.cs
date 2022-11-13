using JsonServerKit.AppServer.Data;
using JsonServerKit.AppServer.Operations;
using Serilog;

namespace JsonServerKit.AppServer.Interfaces
{
    public interface IMessageProcessor
    {
        public PayloadInfo ProcessMessage(object jsonObject, MessageContext context, ILogger logger);
        public void ConfigureDomainObjectHandlers(Dictionary<Type, IOperation> domainHandlerActions);
    }
}
