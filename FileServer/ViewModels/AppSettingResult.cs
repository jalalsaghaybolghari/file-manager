namespace FileServer.ViewModels
{
    public partial class AppSettingResult
    {
        public AppSettingResult()
        {
        }
        public WatermarkInput[] Watermarks { get; set; }
        public ApplicationFileSettings FileSettings { get; set; }

    }
}