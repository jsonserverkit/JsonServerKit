namespace JsonServerKit.AppServer.Data
{
    /// <summary>
    /// Designed to be used for Message tracking between client/server.
    /// Server will allways attach the context to a response.
    /// </summary>
    public class MessageContext
    {
        public string MessageGuid { get; set; }
        public long MessageId { get; set; }
        public object BusinessContext { get; set; }
    }
}
