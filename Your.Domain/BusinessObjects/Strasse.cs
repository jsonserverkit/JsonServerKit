namespace Your.Domain.BusinessObjects
{
    public class Strasse
    {
        public int Id { get; set; }
        public string Strassenname { get; set; }
        public string Hausnummer { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Version { get; set; }

    }
}
