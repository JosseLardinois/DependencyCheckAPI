
using DependencyCheckAPI.DTO;
using DependencyCheckAPI.Models;
using System.Data;

namespace DependencyCheckAPI.Interfaces
{
    public interface ISQLResultsStorageRepository
    {
        Task InsertIntoDependencyCheckResults(Guid scanId, string packageName, string highestSeverity, int? cveCount, int? evidenceCount, double? baseScore);

        Task<IEnumerable<DependencyCheckResults>> RetrieveDependencyCheckResults(string projectName);

        Task<Guid> CreateScan(string projectName, Guid createdBy);

        Task InsertDependencyInfosIntoDatabase(Guid scanId, List<DependencyCheckResults> dependencyCheckResults);

    }
}
