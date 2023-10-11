using DependencyCheckAPI.DTO;
using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Models;
using System.Data;

namespace DependencyCheckAPI.Service
{
    public class SQLResultsService : ISQLResultsService
    {
        private readonly ISQLResultsStorage _storage;

        public SQLResultsService(ISQLResultsStorage storage)
        {

            _storage = storage;
        }

        public Task InsertDependencyInfosIntoDatabase(string filename, List<DependencyInfo> dependencyInfos)
        {
            foreach (DependencyInfo info in dependencyInfos)
            {
                return _storage.InsertIntoDependencyCheckResults(filename, info.PackageName, info.HighestSeverity, info.CveCount, info.EvidenceCount, info.BaseScore);
            }
            return Task.CompletedTask;
        }

        public Task<bool> InsertIfNotExistsInProjects(string userId, string projectId)
        {
            return _storage.CheckAndInsertIfNotExistsInProjects(userId, projectId);
        }

        public Task<List<DependencyCheckResultsDTO>> GetResults(string userId, string projectId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(projectId))
                {
                    throw new ArgumentException("User ID and project ID cannot be empty or null.");
                }
                return _storage.RetrieveDependencyCheckResults(projectId, userId);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving results.", ex);
            }
        }
    }
}
