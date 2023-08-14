namespace DependencyCheckAPI.Models
{
    public class DependencyInfo
    {
        public string? DependencyName { get; set; }
        public string? PackageName { get; set; }
        public string? HighestSeverity { get; set; }
        public int? CveCount { get; set; }
        public int? EvidenceCount { get; set; }
        public double? BaseScore { get; set; }
    }
}
