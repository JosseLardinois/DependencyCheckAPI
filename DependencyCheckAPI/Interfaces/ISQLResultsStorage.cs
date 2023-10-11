
using DependencyCheckAPI.DTO;
using System.Data;

namespace DependencyCheckAPI.Interfaces
{
    public interface ISQLResultsStorage
    {
        Task InsertIntoDependencyCheckResults(string projectId, string packageName, string highestSeverity, int? cveCount, int? evidenceCount, double? baseScore);

        Task<List<DependencyCheckResultsDTO>> RetrieveDependencyCheckResults(string projectId, string userId);
        Task<bool> CheckAndInsertIfNotExistsInProjects(string userId, string projectId);



    }
}
