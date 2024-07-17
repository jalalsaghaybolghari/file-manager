using FileServer.ViewModels.Enum;
using LazZiya.ImageResize;
using System.Drawing;

namespace FileServer.ViewModels
{
    public partial class TextWatermarkOption : TextWatermarkOptions
    {
        public TextWatermarkOption()
        {
            this.Location = TargetSpot.Center;
            this.Margin = 0;
            this.FontSize = 22;
            this.FontStyle = System.Drawing.FontStyle.Regular;
            this.TextColor = System.Drawing.Color.White;
            this.BGColor = System.Drawing.Color.White;
            this.OutlineColor = System.Drawing.Color.White;
            this.OutlineWidth = 0;
        }
        public TargetSpot? Location { get; set; }
        public int? Margin { get; set; }
        public int? FontSize { get; set; }
        public FontStyle? FontStyle { get; set; }
        public Color? TextColor { get; set; }
        public Color? BGColor { get; set; }
        public Color? OutlineColor { get; set; }
        public int? OutlineWidth { get; set; }
    }
}
