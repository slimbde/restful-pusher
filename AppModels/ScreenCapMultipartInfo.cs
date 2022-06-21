namespace AppModels
{
    public class ScreenCapMultipartInfo
    {
        public bool Success { get; set; }
        public string ContentType { get; set; }
        public string Filename { get; set; }
        public byte[] FileContents { get; set; }
    }
}
