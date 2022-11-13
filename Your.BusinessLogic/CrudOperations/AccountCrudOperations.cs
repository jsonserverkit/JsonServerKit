using JsonServerKit.AppServer.Data;
using JsonServerKit.AppServer.Data.Crud;
using JsonServerKit.AppServer.LogTemplate;
using JsonServerKit.AppServer.Operations;
using Serilog;
using Your.Domain.BusinessObjects;

namespace Your.BusinessLogic.CrudOperations
{
    public class AccountCrudOperations : CrudOperation<Account>
    {
        private readonly string _msgPayloadCreateProcessed = "Payload Create processed:";
        private readonly string _msgPayloadReadProcessed = "Payload Read processed:";
        private readonly string _msgPayloadUpdateProcessed = "Payload Update processed:";
        private readonly string _msgPayloadDeleteProcessed = "Payload Delete processed:";

        protected override PayloadInfo HandleDomainObjectCreate(Create<Account> message, MessageContext context, ILogger logger)
        {
            logger.Information(Messages.TemplateMessageWithIdAndTextThreePlacehodlers, context.MessageId, _msgPayloadCreateProcessed, ((Account)message.Value).Email);
            return new PayloadInfo { Context = context };
        }

        protected override PayloadInfo HandleDomainObjectRead(Read<Account> message, MessageContext context, ILogger logger)
        {
            logger.Information(Messages.TemplateMessageWithIdAndTextThreePlacehodlers, context.MessageId, _msgPayloadReadProcessed, message.Id);
            return new PayloadInfo { Context = context };
        }

        protected override PayloadInfo HandleDomainObjectUpdate(Update<Account> message, MessageContext context, ILogger logger)
        {
            logger.Information(Messages.TemplateMessageWithIdAndTextThreePlacehodlers, context.MessageId, _msgPayloadUpdateProcessed, ((Account)message.Value).Email);
            return new PayloadInfo { Context = context };
        }

        protected override PayloadInfo HandleDomainObjectDelete(Delete<Account> message, MessageContext context, ILogger logger)
        {
            logger.Information(Messages.TemplateMessageWithIdAndTextThreePlacehodlers, context.MessageId, _msgPayloadDeleteProcessed, message.Id);
            return new PayloadInfo { Context = context };
        }
    }
}
