namespace Your.Domain.BusinessObjects
{
    public class Account
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }
        public IList<string> Roles { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Version { get; set; }
    }
}
