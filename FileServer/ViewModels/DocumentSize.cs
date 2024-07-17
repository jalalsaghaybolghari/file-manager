using FileServer.ViewModels.Enum;

namespace FileServer.ViewModels
{
    public partial class DocumentSize
    {
        public DocumentSize()
        {
        }
        public int? Height { get; set; }
        public int? Width { get; set; }
        public DocumentSizeType? Type { get; set; }
    }
}