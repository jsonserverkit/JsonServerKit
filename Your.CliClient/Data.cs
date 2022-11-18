using System.Drawing;
using JsonServerKit.AppServer.Data.Crud;
using JsonServerKit.AppServer.Data;
using System.Numerics;
using Your.Domain.BusinessObjects;

namespace Your.CliClient
{
    public class Data
    {
        #region Create test data.

        /// <summary>
        /// Method to create some "basic" payload in form of "domain" business objects.
        /// </summary>
        /// <returns></returns>
        public Payload[] GetLoadTestPayload()
        {
            var product = GetProduct();

            // Create a payload object array based on the data created above.
            var payload = new[]
            {
                GetNewCreateAccountPayload(),
                GetNewProductPayload(product),
                GetNewReadAccountPayload(),
                GetNewProductPayload(product),
                GetNewUpdateAccountPayload(),
                GetNewProductPayload(product),
                GetNewDeleteAccountPayload(),
                GetNewProductPayload(product),
                GetNewAccountPayload(),
                GetNewProductPayload(product)
            };

            return payload;
        }

        public Payload GetNewAccountPayload()
        {
            var account = GetAccount();
            return new()
            {
                // Some random context information.
                Context = GetNewMessageContext(),
                Message = account
            };
        }
        
        public Payload GetNewCreateAccountPayload()
        {
            var account = GetAccount();
            return new()
            {
                // Some random context information.
                Context = GetNewMessageContext(),
                Message = new Create<Account> { Value = account }
            };
        }

        public Payload GetNewReadAccountPayload()
        {
            var account = GetAccount();
            return new()
            {
                // Some random context information.
                Context = GetNewMessageContext(),
                Message = new Read<Account> { Id = account.Id }
            };
        }
        
        public Payload GetNewUpdateAccountPayload()
        {
            var account = GetAccount();
            return new()
            {
                // Some random context information.
                Context = GetNewMessageContext(),
                Message = new Update<Account> { Value = account }
            };
        }

        public Payload GetNewDeleteAccountPayload()
        {
            var account = GetAccount();
            return new()
            {
                // Some random context information.
                Context = GetNewMessageContext(),
                Message = new Delete<Account> { Id = account.Id }
            };
        }
        
        public Payload GetNewProductPayload()
        {
            var product = GetProduct();
            return new()
            {
                // Some random context information.
                Context = GetNewMessageContext(),
                // Some payload data containing the user input.
                Message = product
            };
        }

        #region Private methods

        private static Account GetAccount()
        {
            // Account business objext.
            var account = new Account
            {
                Id = 777,
                Email = "some.one.instead.of@no.one",
                Active = true,
                CreatedDate = DateTime.Now,
                Roles = new[] { $"User{Environment.NewLine}", "Admin" },
                Version = 100
            };

            return account;
        }

        private Product GetProduct()
        {
            // File content of a picture of 104KB size.
            var filePath = "..\\..\\..\\product.jpg";
            var base64FileString = GetBase64StringFromFilePath(filePath);

            // Product business object that contains the image (as base64encoded string).
            var product = new Product
            {
                ItemPicture = base64FileString,
            };

            return product;
        }

        private Payload GetNewProductPayload(Product product)
        {
            return new()
            {
                // Some random context information.
                Context = GetNewMessageContext(),
                // Some payload data containing the user input.
                Message = product
            };
        }

        private MessageContext GetNewMessageContext()
        {
            return new MessageContext
            {
                MessageId = GetNewId()
            };
        }

        private int GetNewId()
        {
            // Konrad Rudolph :)
            // https://stackoverflow.com/questions/65292465/biginteger-intvalue-equivalent-in-c-sharp
            var id = (int)(uint)(new BigInteger(Guid.NewGuid().ToByteArray()) & uint.MaxValue);
            return id > 0 ? id : id * -1;
        }

        private string GetBase64StringFromFilePath(string filePath)
        {
            byte[] fileBytes;

            if (!File.Exists(filePath))
                return null;


            var image = Image.FromFile(filePath);
            using (var ms = new MemoryStream())
            {
                image.Save(ms, image.RawFormat);
                fileBytes = ms.ToArray();
            }
            return Convert.ToBase64String(fileBytes);
        }

        #endregion

        #endregion

    }
}
