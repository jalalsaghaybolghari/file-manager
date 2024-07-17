namespace FileServer.Models
{
    public partial class User : IEntity
    {
        public User()
        {
            Applications = new HashSet<Application>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string DirectoryPath { get; set; }
        public virtual ICollection<Application> Applications { get; set; }
    }
}