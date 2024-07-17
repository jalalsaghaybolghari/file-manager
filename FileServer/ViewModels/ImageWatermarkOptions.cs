using LazZiya.ImageResize;

namespace FileServer.ViewModels
{
    public partial class ImageWatermarkOption: ImageWatermarkOptions
    {
        public ImageWatermarkOption()
        {
            this.Location = TargetSpot.Center;
            this.Opacity = 50;
        }
        public TargetSpot? Location { get; set; }
        public int? Opacity { get; set; }
    }
}