namespace Your.Domain.BusinessObjects
{
    public class Ort
    {
        public int Id { get; set; }
        public int Plz { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Version { get; set; }
    }
}
