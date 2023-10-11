
using DependencyCheckAPI.DTO;
using DependencyCheckAPI.Models;
using System.Data;

namespace DependencyCheckAPI.Interfaces
{
    public interface ISQLResultsStorageRepository
    {
        Task InsertIntoDependencyCheckResults(string projectId, string packageName, string highestSeverity, int? cveCount, int? evidenceCount, double? baseScore);

        Task<IEnumerable<DependencyCheckResults>> RetrieveDependencyCheckResults(string projectId, string userId);
        Task<bool> CheckAndInsertIfNotExistsInProjects(string userId, string projectId);



    }
}
