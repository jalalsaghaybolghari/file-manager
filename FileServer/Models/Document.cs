using System.ComponentModel.DataAnnotations;

namespace FileServer.Models
{
    public partial class Document : IEntity
    {
        public Document()
        {
        }
        [Key]
        public Guid Id { get; set; }
        public string? ClientDocumentId { get; set; }
        public string FileExtention { get; set; }
        public long FileLength { get; set; }
        public long FileWidth { get; set; }
        public long FileHeight { get; set; }
        public string? FileName { get; set; }
        public string? Tag { get; set; }
        public int ApplicationId { get; set; }
        public virtual Application Application { get; set; }

    }
}