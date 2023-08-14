using DependencyCheckAPI.Dto;
using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Models;
using System.Data;

namespace DependencyCheckAPI.Repositories
{
    public class SQLResultsRepository : ISQLResultsRepository
    {
        private readonly ISQLResultsStorage _storage;

        public SQLResultsRepository(ISQLResultsStorage storage)
        {

            _storage = storage;
        }

        public void InsertDependencyInfosIntoDatabase(string filename, List<DependencyInfo> dependencyInfos)
        {
            foreach (DependencyInfo info in dependencyInfos)
            {
                _storage.InsertIntoDependencyCheckResults(filename, info.PackageName, info.HighestSeverity, info.CveCount, info.EvidenceCount, info.BaseScore);
            }
        }

        public bool InsertIfNotExistsInProjects(string userId, string projectId)
        {
            return _storage.CheckAndInsertIfNotExistsInProjects(userId, projectId);
        }

        public List<DependencyCheckResultsDTO> GetResults(string userId, string projectId)
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
