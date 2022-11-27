namespace JsonServerKit.AppServer.Data
{
    public class PayloadStatistic
    {
        public Payload Payload { get; set; }

        public double TimeInMsMessageSent { get; set; }
        public double TimeInMsMessageReceived { get; set; }
        public string MessageType { get; set; }
    }

}
