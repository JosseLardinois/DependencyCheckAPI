namespace DependencyCheckAPI.Models
{
    public class DependencyCheckResults
    {
        public int Id { get; set; }
        public string ProjectId { get; set; }
        public string PackageName { get; set; }
        public string HighestSeverity { get; set; }
        public int CveCount { get; set; }
        public int EvidenceCount { get; set; }
        public double BaseScore { get; set; }
    }
}
