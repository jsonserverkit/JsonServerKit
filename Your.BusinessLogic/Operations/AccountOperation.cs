using JsonServerKit.AppServer.Data;
using JsonServerKit.AppServer.LogTemplate;
using JsonServerKit.AppServer.Operations;
using Serilog;
using Your.Domain.BusinessObjects;

namespace Your.BusinessLogic.Operations
{
    /// <summary>
    /// Implements the desired operation.
    /// The abstraction <see cref="Operation{T}"/> provides the method signature to handle a domain object.
    /// </summary>
    public class AccountOperation : Operation<Account>
    {
        private readonly string _msgPayloadProcessed = "Payload processed:";

        /// <summary>
        /// Implements the operation on the domain object.
        /// </summary>
        /// <param name="account">The account object of type <see cref="Account"/>.</param>
        /// <param name="context">The message context object.</param>
        /// <param name="logger">The interface to the logger used by the server component, intended to provide consistent logging.</param>
        /// <returns>PayloadInfo object.</returns>
        protected override PayloadInfo HandleDomainObject(Account account, MessageContext context, ILogger logger)
        {
            logger.Information(Messages.TemplateMessageWithIdAndTextThreePlacehodlers, context.MessageId,_msgPayloadProcessed, account.Email);
            return new PayloadInfo { Context = context };
        }
    }
}
