namespace JsonServerKit.AppServer.Data
{
    public class ReceiveSendContext
    {
        public MessageContext? Context { get; set; }
        public string[] InputMessages { get; set; }
        public string OutputMessage { get; set; }
    }
}
