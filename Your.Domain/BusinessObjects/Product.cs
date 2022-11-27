namespace Your.Domain.BusinessObjects
{
    public class Product
    {
        public virtual long Id { get; set; }
        public virtual long ItemId { get; set; }
        public virtual string ItemName { get; set; }
        public virtual string ItemDescription { get; set; }
        public virtual string ItemPicture { get; set; }
        public virtual long Version { get; set; }

    }
}
