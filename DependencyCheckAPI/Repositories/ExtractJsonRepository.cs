using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DependencyCheckAPI.Repositories
{
    public class ExtractJsonRepository : IExtractJson
    {
        private readonly ISQLResultsStorage _sqlStorage;
        private readonly ISQLResultsRepository _sqlRepository;

        public ExtractJsonRepository(ISQLResultsStorage storage, ISQLResultsRepository sqlRepository)
        {
            _sqlStorage = storage;
            _sqlRepository = sqlRepository;
        }

        public List<DependencyInfo> ExtractJson(string fileName)
        {
            string filename = Path.GetFileNameWithoutExtension(fileName);
            string jsonFilePath = Path.Combine(filename, "/dependency-check-report.json");
            string jsonContent = File.ReadAllText(jsonFilePath);
            JArray dependenciesArray = GetDependenciesArray(jsonContent);

            List<DependencyInfo> dependencyInfos = ExtractDependencyInfos(dependenciesArray);
            _sqlRepository.InsertDependencyInfosIntoDatabase(filename, dependencyInfos);
            return dependencyInfos;
        }

        public bool MakeNewProject(string userId, string projectName)
        {
            string projectId = projectName.Replace(".zip", "");
            return _sqlRepository.InsertIfNotExistsInProjects(userId, projectId);
        }

        private JArray GetDependenciesArray(string jsonContent)
        {
            JObject jsonObject = JObject.Parse(jsonContent);
            return (JArray)jsonObject["dependencies"];
        }

        private List<DependencyInfo> ExtractDependencyInfos(JArray dependenciesArray)
        {
            List<DependencyInfo> dependencyInfos = new List<DependencyInfo>();

            foreach (JToken dependencyToken in dependenciesArray)
            {
                DependencyInfo dependencyInfo = ExtractDependencyInfo(dependencyToken);
                if (dependencyInfo != null)
                {
                    dependencyInfos.Add(dependencyInfo);
                }
            }

            return dependencyInfos;
        }

        private DependencyInfo ExtractDependencyInfo(JToken dependencyToken)
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

            return new DependencyInfo
            {
                DependencyName = dependencyName,
                PackageName = packageName,
                HighestSeverity = baseSeverity,
                CveCount = cveCount,
                EvidenceCount = evidenceCount,
                BaseScore = baseScore
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
