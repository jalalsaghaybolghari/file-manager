using FileServer.ViewModels.Enum;
using LazZiya.ImageResize;

namespace FileServer.ViewModels
{
    public partial class ImageWaterMark
    {
        public ImageWaterMark()
        {
        }
        public byte[] FileStream { get; set; }
        public ImageWatermarkOption? ImageWatermarkOptions { get; set; }
    }
}