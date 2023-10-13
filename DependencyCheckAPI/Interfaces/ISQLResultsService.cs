using DependencyCheckAPI.DTO;
using DependencyCheckAPI.Models;

namespace DependencyCheckAPI.Interfaces
{
    public interface ISQLResultsService
    {
        Task <List<DependencyCheckResultsDTO>> GetResults(string userId, string projectId);
        Task InsertDependencyInfosIntoDatabase(string filename, List<DependencyCheckResults> dependencyInfos);

        Task<bool> InsertIfNotExistsInProjects(string userId, string projectId);
    }
}
