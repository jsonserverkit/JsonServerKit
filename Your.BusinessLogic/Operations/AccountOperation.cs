using JsonServerKit.AppServer.Data;
using JsonServerKit.AppServer.LogTemplate;
using JsonServerKit.AppServer.Operations;
using Serilog;
using Your.Domain.BusinessObjects;

namespace Your.BusinessLogic.Operations
{
    public class AccountOperation : Operation<Account>
    {
        private readonly string _msgPayloadProcessed = "Payload processed:";

        protected override PayloadInfo HandleDomainObject(Account account, MessageContext context, ILogger logger)
        {
            logger.Information(Messages.TemplateMessageWithIdAndTextThreePlacehodlers, context.MessageId,_msgPayloadProcessed, account.Email);
            return new PayloadInfo { Context = context };
        }
    }
}
