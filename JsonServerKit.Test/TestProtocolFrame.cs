namespace JsonServerKit.Test
{
    [TestClass]
    public class TestProtocolFrame
    {
        #region Test methods

        [TestMethod]
        [DataRow($"ii", 1)]
        [DataRow($"ii\r\n", 2)]
        [DataRow($"ii\r\njj", 2)]
        [DataRow($"ii\r\njj\r\nkk", 3)]
        [DataRow($"ii\r\njj\r\nkk\r\n", 4)]
        // Debug
        [DataRow("{\"$type\":\"JsonServerKit.AppServer.Data.Payload, JsonServerKit.AppServer\",\"Context\":{\"$type\":\"JsonServerKit.AppServer.Data.MessageContext, JsonServerKit.AppServer\",\"MessageId\":260875611,\"SessionGuid\":\"guid1\"},\"Message\":{\"$type\":\"JsonServerKit.AppServer.Data.Crud.Create`1[[Your.Domain.BusinessObjects.Account, Your.Domain]], JsonServerKit.AppServer\",\"Value\":{\"$type\":\"Your.Domain.BusinessObjects.Account, Your.Domain\",\"Id\":777,\"Email\":\"some.one.instead@of.null\",\"Active\":true,\"Roles\":{\"$type\":\"System.String[], System.Private.CoreLib\",\"$values\":[\"User\\r\\n\",\"Admin\"]},\"CreatedDate\":\"2022-11-17T21:00:29.3998129+01:00\",\"Version\":100}}}\r\n", 2)]
        public void MessageFrameSplit(string splitMeUp, int expectetElements)
        {
            string splitBy = Environment.NewLine;
            try
            {
                var splitted = splitMeUp.Split(splitBy);
                Assert.IsTrue(splitted.Length == expectetElements);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        #endregion
    }
}
