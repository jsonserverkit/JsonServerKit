namespace Your.Domain.BusinessObjects
{
    public class Product
    {
        public long Id { get; set; }
        public long ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public string ItemPicture { get; set; }
        public long Version { get; set; }

    }
}
