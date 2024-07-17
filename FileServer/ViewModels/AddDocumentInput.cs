namespace FileServer.ViewModels
{
    public partial class AddDocumentInput 
    {
        public AddDocumentInput()
        {
        }
        public Guid Id { get; set; }
        public int AppId { get; set; }
        public string ClientDocumentId { get; set; }
        public string FileExtention { get; set; }
        public long FileLength { get; set; }
        public long FileWidth { get; set; }
        public long FileHeight { get; set; }
        public string? FileName { get; set; }
        public string? Tag { get; set; }
    }
}