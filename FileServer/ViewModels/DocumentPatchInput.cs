using FileServer.ViewModels.Enum;

namespace FileServer.ViewModels
{
    public partial class DocumentPatchInput
    {
        public DocumentPatchInput()
        {
        }
        public Guid DocumentId { get; set; }
        public int ApplicationId { get; set; }
        public DocumentPathType DocumentPathType { get; set; }
        public string? FileExtention { get; set; }
        public string? Tag { get; set; }

    }
}