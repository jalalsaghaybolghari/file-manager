namespace FileServer.ViewModels
{
    public partial class AddWatermark
    {
        public AddWatermark()
        {
        }
        public string[] Tags { get; set; }
        public TextWaterMark? TextWaterMark { get; set; }
        public ImageWaterMark? ImageWaterMark { get; set; }

    }
}