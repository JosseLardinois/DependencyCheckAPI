using DependencyCheckAPI.DTO;
using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Models;
using System.Data;

namespace DependencyCheckAPI.Service
{
    public class SQLResultsService : ISQLResultsService
    {
        private readonly ISQLResultsStorageRepository _storage;

        public SQLResultsService(ISQLResultsStorageRepository storage)
        {

            _storage = storage;
        }

        public Task InsertDependencyInfosIntoDatabase(string filename, List<DependencyCheckResults> dependencyCheckResults)
        {
            foreach (DependencyCheckResults result in dependencyCheckResults)
            {
                _storage.InsertIntoDependencyCheckResults(filename, result.PackageName, result.HighestSeverity, result.CveCount, result.EvidenceCount, result.BaseScore);
            }
            return Task.CompletedTask;
        }

        public Task<bool> InsertIfNotExistsInProjects(string userId, string projectId)
        {
            return _storage.CheckAndInsertIfNotExistsInProjects(userId, projectId);
        }

        public async Task<List<DependencyCheckResultsDTO>> GetResults(string userId, string projectId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(projectId))
                {
                    throw new ArgumentException("User ID and project ID cannot be empty or null.");
                }
                var sqlResults = await _storage.RetrieveDependencyCheckResults(projectId, userId);
                return sqlResults.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving results.", ex);
            }
        }


        private DependencyCheckResults MapToModel(DependencyCheckResultsDTO dependencyCheckResultsDTO)
        {
            return new DependencyCheckResults
            {
                Id = dependencyCheckResultsDTO.Id,
                ProjectId = dependencyCheckResultsDTO.ProjectId,
                PackageName = dependencyCheckResultsDTO.PackageName,
                HighestSeverity = dependencyCheckResultsDTO.HighestSeverity,
                CveCount = dependencyCheckResultsDTO.CveCount,
                EvidenceCount = dependencyCheckResultsDTO.EvidenceCount,
                BaseScore = dependencyCheckResultsDTO.BaseScore,

            };

    }
        private DependencyCheckResultsDTO MapToDTO(DependencyCheckResults dependencyCheckResults)
        {
            return new DependencyCheckResultsDTO
            {
                Id = dependencyCheckResults.Id,
                ProjectId = dependencyCheckResults.ProjectId,
                PackageName = dependencyCheckResults.PackageName,
                HighestSeverity = dependencyCheckResults.HighestSeverity,
                CveCount = dependencyCheckResults.CveCount,
                EvidenceCount = dependencyCheckResults.EvidenceCount,
                BaseScore = dependencyCheckResults.BaseScore,
            };
        }
    }
}
