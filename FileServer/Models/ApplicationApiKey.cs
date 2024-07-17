namespace FileServer.Models
{
    public partial class ApplicationApiKey : IEntity
    {
        public ApplicationApiKey()
        {
        }
        public int Id { get; set; }
        public string ApiKey { get; set; }
        public bool IsActive { get; set; }
        public string? PermittedIPs { get; set; }
        public DateTime? ExpireTime { get; set; }
        public int ApplicationId { get; set; }
        public virtual Application Application { get; set; }
    }
}