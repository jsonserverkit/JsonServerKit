namespace Your.Domain.BusinessObjects
{
    public class Account
    {
        public virtual long Id { get; set; }
        public virtual string AccountId { get; set; }
        public virtual string Email { get; set; }
        public virtual bool Active { get; set; }
        public virtual IList<string> Roles { get; set; }
        public virtual DateTime CreatedDate { get; set; }
        public virtual long Version { get; set; }
    }
}
