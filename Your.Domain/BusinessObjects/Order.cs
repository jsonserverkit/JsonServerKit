namespace Your.Domain.BusinessObjects
{
    public class Order
    {
        public virtual long Id { get; set; }
        public virtual string OrderId { get; set; }
        public virtual long ItemId { get; set; }
        public virtual int Count { get; set; }
        public virtual long Version { get; set; }
    }
}
