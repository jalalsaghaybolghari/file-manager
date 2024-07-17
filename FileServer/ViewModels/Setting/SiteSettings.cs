namespace FileServer.ViewModels.Setting
{
    public class SiteSettings
    {
        public double MaximumFileSize { get; set; }
        public string EncryptKey { get; set; }
        public string ApiKeyHeaderName { get; set; }
        public SwaggerSettings SwaggerSettings { get; set; }

    }
}
