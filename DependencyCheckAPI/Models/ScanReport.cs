namespace DependencyCheckAPI.Models
{
    public class ScanReport
    {
        public string? Uri { get; set; }
        public string? Name { get; set; }
        public string? ContentType { get; set; }
        public Stream? Content { get; set; }
        public string FilePath { get; internal set; }
    }
}
