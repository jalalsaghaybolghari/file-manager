using FileServer.ViewModels.Enum;

namespace FileServer.ViewModels
{
    public partial class ApplicationFileSettings
    {
        public ApplicationFileSettings()
        {
        }
        public string? PermittedExtentions { get; set; }
        public double? MaximumFileSize { get; set; }
    }
}