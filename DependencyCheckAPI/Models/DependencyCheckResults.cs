namespace DependencyCheckAPI.Models
{
    public class DependencyCheckResults
    {
        public Guid Id { get; set; }
        public Guid? ScanId { get; set; }
        public string? PackageName { get; set; }
        public string? HighestSeverity { get; set; }
        public int? CveCount { get; set; }
        public int? EvidenceCount { get; set; }
        public double? BaseScore { get; set; }

    }
}
