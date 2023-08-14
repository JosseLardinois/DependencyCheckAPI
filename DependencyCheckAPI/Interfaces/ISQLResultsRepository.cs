using DependencyCheckAPI.Dto;
using DependencyCheckAPI.Models;

namespace DependencyCheckAPI.Interfaces
{
    public interface ISQLResultsRepository
    {
        public List<DependencyCheckResultsDTO> GetResults(string userId, string projectId);
        void InsertDependencyInfosIntoDatabase(string filename, List<DependencyInfo> dependencyInfos);

        bool InsertIfNotExistsInProjects(string userId, string projectId);
    }
}
