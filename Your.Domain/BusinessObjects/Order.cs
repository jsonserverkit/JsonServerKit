namespace Your.Domain.BusinessObjects
{
    public class Order
    {
        public int Id { get; set; }
        public int Article { get; set; }
        public int Count { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Version { get; set; }
    }
}
