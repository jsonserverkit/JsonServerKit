namespace JsonServerKit.AppServer.Data
{
    /// <summary>
    /// Designed to be used for Message tracking between client/server.
    /// Server will allways attach the context to a response.
    /// </summary>
    public class MessageContext
    {
        public int MessageId { get; set; }
        public int SessionId { get; set; }
    }
}
