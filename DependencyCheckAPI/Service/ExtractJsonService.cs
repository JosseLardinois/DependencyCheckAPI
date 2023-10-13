using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Models;
using Newtonsoft.Json.Linq;

namespace DependencyCheckAPI.Service


{
    public class ExtractJsonService : IExtractJsonService
    {
        private readonly ISQLResultsService _sqlRepository;

        public ExtractJsonService(ISQLResultsService sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }

        public List<DependencyCheckResults> ExtractJson(string fileName)
        {
            string filename = Path.GetFileNameWithoutExtension(fileName);
            string jsonFilePath = filename + "/dependency-check-report.json";
            string jsonContent = File.ReadAllText(jsonFilePath);
            JArray dependenciesArray = GetDependenciesArray(jsonContent);

            List<DependencyCheckResults> dependencyCheckResults = ExtractDependencyInfos(dependenciesArray);
            _sqlRepository.InsertDependencyInfosIntoDatabase(filename, dependencyCheckResults);
            return dependencyCheckResults;
        }

        public Task<bool> MakeNewProject(string userId, string projectName)
        {
            string projectId = projectName.Replace(".zip", "");
            return _sqlRepository.InsertIfNotExistsInProjects(userId, projectId);
        }

        private JArray GetDependenciesArray(string jsonContent)
        {
            JObject jsonObject = JObject.Parse(jsonContent);
            return (JArray)jsonObject["dependencies"];
        }

        private List<DependencyCheckResults> ExtractDependencyInfos(JArray dependenciesArray)
        {
            List<DependencyCheckResults> dependencyInfos = new List<DependencyCheckResults>();

            foreach (JToken dependencyToken in dependenciesArray)
            {
                DependencyCheckResults dependencyCheckresults = ExtractDependencyInfo(dependencyToken);
                if (dependencyCheckresults != null)
                {
                    dependencyInfos.Add(dependencyCheckresults);
                }
            }

            return dependencyInfos;
        }

        private DependencyCheckResults ExtractDependencyInfo(JToken dependencyToken)
        {
            JToken vulnerabilitiesToken = dependencyToken["vulnerabilities"];
            if (vulnerabilitiesToken == null || vulnerabilitiesToken.Type != JTokenType.Array || vulnerabilitiesToken.Count() == 0)
            {
                return null;
            }

            int cveCount = vulnerabilitiesToken.Count();
            int evidenceCount = GetEvidenceCount(dependencyToken);

            string dependencyName = dependencyToken["fileName"]?.Value<string>();
            string packageName = dependencyToken["packages"]?[0]?["id"]?.Value<string>();
            double? baseScore = vulnerabilitiesToken.Max(v => v["cvssv3"]?["baseScore"]?.Value<double>());
            string baseSeverity = vulnerabilitiesToken.Max(v => v["cvssv3"]?["baseSeverity"]?.Value<string>());
            string severity = vulnerabilitiesToken.Max(v => v["severity"]?.Value<string>());

            if (string.IsNullOrEmpty(severity))
            {
                return null;
            }

            return new DependencyCheckResults
            {
                PackageName = packageName,
                HighestSeverity = baseSeverity,
                CveCount = cveCount,
                EvidenceCount = evidenceCount,
                BaseScore = (double)baseScore
            };
        }

        private int GetEvidenceCount(JToken dependencyToken)
        {
            JToken evidenceCollectedToken = dependencyToken["evidenceCollected"];
            int vendorEvidenceCount = evidenceCollectedToken?["vendorEvidence"]?.Count() ?? 0;
            int productEvidenceCount = evidenceCollectedToken?["productEvidence"]?.Count() ?? 0;
            int versionEvidenceCount = evidenceCollectedToken?["versionEvidence"]?.Count() ?? 0;

            return vendorEvidenceCount + productEvidenceCount + versionEvidenceCount;
        }

    }
}
