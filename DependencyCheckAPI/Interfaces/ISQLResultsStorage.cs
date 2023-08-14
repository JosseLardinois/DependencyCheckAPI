
using DependencyCheckAPI.Dto;
using System.Data;

namespace DependencyCheckAPI.Interfaces
{
    public interface ISQLResultsStorage
    {
        void InsertIntoDependencyCheckResults(string projectId, string packageName, string highestSeverity, int? cveCount, int? evidenceCount, double? baseScore);

        List<DependencyCheckResultsDTO> RetrieveDependencyCheckResults(string projectId, string userId);
        bool CheckAndInsertIfNotExistsInProjects(string userId, string projectId);



    }
}
