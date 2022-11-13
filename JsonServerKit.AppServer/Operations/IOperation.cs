using JsonServerKit.AppServer.Data;
using Serilog;

namespace JsonServerKit.AppServer.Operations
{
    public interface IOperation
    {
        public PayloadInfo HandleObject(object message, MessageContext context, ILogger logger);
    }
}
