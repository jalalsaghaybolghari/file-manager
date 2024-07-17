namespace FileServer.ViewModels
{
    public partial class AddDocument 
    {
        public AddDocument()
        {
        }
        public string Id { get; set; }
        public byte[] FileStream { get; set; }
        public string? FileName { get; set; }
        public string? Tag { get; set; }
    }
}