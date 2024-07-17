using FileServer.ViewModels.Enum;

namespace FileServer.ViewModels
{
    public partial class DocumentOperation
    {
        public DocumentOperation()
        {
        }        
        public DocumentSize? DocumentSize { get; set; }
        public string? Format { get; set; }
    }
}