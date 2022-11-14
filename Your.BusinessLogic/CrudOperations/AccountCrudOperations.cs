using JsonServerKit.AppServer.Data;
using JsonServerKit.AppServer.Data.Crud;
using JsonServerKit.AppServer.LogTemplate;
using JsonServerKit.AppServer.Operations;
using Serilog;
using Your.Domain.BusinessObjects;

namespace Your.BusinessLogic.CrudOperations
{
    /// <summary>
    /// Implements the desired crud operations.
    /// The abstraction <see cref="CrudOperation{T}"/> provides the method signature to apply the desired crud operation to domain object.
    /// </summary>
    public class AccountCrudOperations : CrudOperation<Account>
    {
        private readonly string _msgPayloadCreateProcessed = "Payload Create processed:";
        private readonly string _msgPayloadReadProcessed = "Payload Read processed:";
        private readonly string _msgPayloadUpdateProcessed = "Payload Update processed:";
        private readonly string _msgPayloadDeleteProcessed = "Payload Delete processed:";

        /// <summary>
        /// Implements the create operation on the domain object.
        /// </summary>
        /// <param name="message">The account object inside of type <see cref="Create{Account}"/>.</param>
        /// <param name="context">The message context object.</param>
        /// <param name="logger">The interface to the logger used by the server component, intended to provide consistent logging.</param>
        /// <returns>PayloadInfo object.</returns>
        protected override PayloadInfo HandleDomainObjectCreate(Create<Account> message, MessageContext context, ILogger logger)
        {
            // Add some logging with the message templates that take into account the MessageId sent by the client.
            logger.Information(Messages.TemplateMessageWithIdAndTextThreePlacehodlers, context.MessageId, _msgPayloadCreateProcessed, ((Account)message.Value).Email);

            var account = message.Value;
            // --------------------------------------------------------------
            // add businesslogic/code here
            // --------------------------------------------------------------
            // ...
            // ...
            // ...
            // return a payload info to the sender.
            // Beside the Context object that can be returned, the PayloadInfo also has a return of type object to return what ever is required.
            // In this example it returns the object it received.
            return new PayloadInfo { Context = context, Message = account };
        }

        /// <summary>
        /// Implements the read operation on the domain object.
        /// </summary>
        /// <param name="message">The account object inside of type <see cref="Read{Account}"/>.</param>
        /// <param name="context">The message context object.</param>
        /// <param name="logger">The interface to the logger. See also <see cref="AccountCrudOperations.HandleDomainObjectCreate"/>.</param>
        /// <returns>PayloadInfo object.</returns>
        protected override PayloadInfo HandleDomainObjectRead(Read<Account> message, MessageContext context, ILogger logger)
        {
            // Add some logging with the message templates that take into account the MessageId sent by the client.
            logger.Information(Messages.TemplateMessageWithIdAndTextThreePlacehodlers, context.MessageId, _msgPayloadReadProcessed, message.Id);

            var accountId = message.Id;
            var query = message.Query;
            // --------------------------------------------------------------
            // add businesslogic/code here
            // --------------------------------------------------------------
            // ...
            // ...
            // ...
            // return a payload info to the sender.
            // Beside the Context object that can be returned, the PayloadInfo also has a return of type object to return what ever is required.
            // In this example it returns a new empty object.
            return new PayloadInfo
            {
                Context = context, Message = new Account
                {

                }
            };
        }

        /// <summary>
        /// Implements the update operation on the domain object.
        /// </summary>
        /// <param name="message">The account object inside of type <see cref="Update{Account}"/>.</param>
        /// <param name="context">The message context object.</param>
        /// <param name="logger">The interface to the logger. See also <see cref="AccountCrudOperations.HandleDomainObjectCreate"/>.</param>
        /// <returns>PayloadInfo object.</returns>
        protected override PayloadInfo HandleDomainObjectUpdate(Update<Account> message, MessageContext context, ILogger logger)
        {
            // Add some logging with the message templates that take into account the MessageId sent by the client.
            logger.Information(Messages.TemplateMessageWithIdAndTextThreePlacehodlers, context.MessageId, _msgPayloadUpdateProcessed, ((Account)message.Value).Email);

            var account = message.Value;
            // --------------------------------------------------------------
            // add businesslogic/code here
            // --------------------------------------------------------------
            // ...
            // ...
            // ...
            // return a payload info to the sender.
            // Beside the Context object that can be returned, the PayloadInfo also has a return of type object to return what ever is required.
            // In this example it returns the object it received.
            return new PayloadInfo { Context = context , Message = account};
        }

        /// <summary>
        /// Implements the delete operation on the domain object.
        /// </summary>
        /// <param name="message">The account object inside of type <see cref="Delete{Account}"/>.</param>
        /// <param name="context">The message context object.</param>
        /// <param name="logger">The interface to the logger. See also <see cref="AccountCrudOperations.HandleDomainObjectCreate"/>.</param>
        /// <returns>PayloadInfo object.</returns>
        protected override PayloadInfo HandleDomainObjectDelete(Delete<Account> message, MessageContext context, ILogger logger)
        {
            // Add some logging with the message templates that take into account the MessageId sent by the client.
            logger.Information(Messages.TemplateMessageWithIdAndTextThreePlacehodlers, context.MessageId, _msgPayloadDeleteProcessed, message.Id);

            var accountId = message.Id;
            var query = message.Query;
            // --------------------------------------------------------------
            // Add businesslogic/code here.
            // --------------------------------------------------------------
            // ...
            // ...
            // ...
            // return a payload info to the sender.
            // Beside the Context object that can be returned, the PayloadInfo also has a return of type object to return what ever is required.
            return new PayloadInfo { Context = context };
        }
    }
}
