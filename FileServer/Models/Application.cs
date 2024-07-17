namespace FileServer.Models
{
    public partial class Application : IEntity
    {
        public Application()
        {
            Documents = new HashSet<Document>();
            ApplicationApiKeys = new HashSet<ApplicationApiKey>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string? AppSettings { get; set; }
        public bool IsActive { get; set; }
        public string DirectoryPath { get; set; }
        public string? Description { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<ApplicationApiKey> ApplicationApiKeys { get; set; }
    }
}