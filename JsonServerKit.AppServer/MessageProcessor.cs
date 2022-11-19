using JsonServerKit.AppServer.Data;
using JsonServerKit.AppServer.Interfaces;
using JsonServerKit.AppServer.LogTemplate;
using JsonServerKit.AppServer.Operations;
using Serilog;

namespace JsonServerKit.AppServer
{
    public class MessageProcessor : IMessageProcessor
    {
        #region Private members

        private Dictionary<Type, IOperation> _domainObjectHandlers = new();


        #region Messages

        // Errors
        private readonly string _msgErrorNoOperationFound = "No operation registered!";

        #endregion


        #endregion

        #region Interface methods

        public PayloadInfo ProcessMessage(object jsonObject, MessageContext context, ILogger logger)
        {
            // Process 
            return ProcessObject(jsonObject, context, logger);
        }


        public void ConfigureDomainObjectHandlers(Dictionary<Type,IOperation> domainObjectHandlers)
        {
            if (domainObjectHandlers.Count > 0)
                _domainObjectHandlers = domainObjectHandlers;
        }

        #endregion

        #region Private methods

        private PayloadInfo ProcessObject(object jsonObject, MessageContext context, ILogger logger)
        {
            var type = jsonObject.GetType();
            if (!_domainObjectHandlers.ContainsKey(type))
            {
                logger.Warning(Messages.TemplateMessageWithIdAndTextTwoPlacehodlers, context.MessageId, _msgErrorNoOperationFound);
                return null;
            }

            return _domainObjectHandlers[type].HandleObject(jsonObject, context, logger);
        }

        #endregion
    }
    }
