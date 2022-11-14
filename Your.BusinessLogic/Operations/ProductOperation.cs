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
    public class ProductOperation : Operation<Product>
    {
        private readonly string _msgPayloadProcessed = "Payload processed:";

        /// <summary>
        /// Implements the operation on the domain object.
        /// </summary>
        /// <param name="product">The product object of type <see cref="Account"/>.</param>
        /// <param name="context">The message context object.</param>
        /// <param name="logger">The interface to the logger used by the server component, intended to provide consistent logging.</param>
        /// <returns>PayloadInfo object.</returns>
        protected override PayloadInfo HandleDomainObject(Product product, MessageContext context, ILogger logger)
        {
            // Add some logging with the message templates that take into account the MessageId sent by the client.
            //logger.Information(Messages.TemplateMessageWithIdAndTextThreePlacehodlers, context.MessageId, _msgPayloadProcessed, product?.ItemPicture);

            // --------------------------------------------------------------
            // add businesslogic/code here
            // --------------------------------------------------------------
            // ...
            // ...
            // ...
            // return a payload info to the sender.
            // Beside the Context object that can be returned, the PayloadInfo also has a return of type object to return what ever is required.
            // In this example it returns the object it received.
            return new PayloadInfo { Context = context , Message = product };
        }
    }

}
