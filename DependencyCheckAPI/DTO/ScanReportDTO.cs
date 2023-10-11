namespace DependencyCheckAPI.DTO
{
    public class ScanReportDTO
    {
        public string? Uri { get; set; }
        public string? Name { get; set; }
        public string? ContentType { get; set; }
        public Stream? Content { get; set; }
        public string FilePath { get; internal set; }
    }
}
