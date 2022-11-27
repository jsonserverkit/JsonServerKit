namespace Your.Domain.BusinessObjects
{
    public class Statistic
    {
        public virtual long Id { get; set; }
        public virtual long MessageId { get; set; }
        public virtual double TimeInMsMessageSent { get; set; }
        public virtual double TimeInMsMessageReceived { get; set; }
        public virtual double TimeDiff { get; set; }
        public virtual string MessageType { get; set; }
    }
}
