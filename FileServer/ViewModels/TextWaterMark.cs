using FileServer.ViewModels.Enum;
using LazZiya.ImageResize;
using System.Drawing;

namespace FileServer.ViewModels
{
    public partial class TextWaterMark
    {
        public TextWaterMark()
        {
        }
        public string Title { get; set; }
        public TextWatermarkOption? TextWatermarkOptions { get; set; }
    }
}
