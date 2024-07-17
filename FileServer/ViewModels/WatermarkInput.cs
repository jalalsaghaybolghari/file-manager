using FileServer.ViewModels.Enum;

namespace FileServer.ViewModels
{
    public partial class WatermarkInput
    {
        public WatermarkInput()
        {
            this.TextWaterMark = null;
        }
        public Guid Id { get; set; }
        public string[] Tags { get; set; }
        public string? TextWaterMark { get; set; }
        public WatermarkType WatermarkType { get; set; }
        public TextWatermarkOption? TextWaterMarkOptions { get; set; }
        public ImageWatermarkOption? ImageWatermarkOptions { get; set; }

    }
}