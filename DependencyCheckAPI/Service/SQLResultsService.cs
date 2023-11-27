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

        public async Task<Guid> CreateScan(Guid createdBy, string projectName)
        {
            var scanId = await _storage.CreateScan(projectName, createdBy);
            return scanId;
        }

        public async Task<List<DependencyCheckResultsDTO>> GetResults(string projectName)
        {
            try
            {
                var sqlResults = await _storage.RetrieveDependencyCheckResults(projectName);
                return sqlResults.Select(MapToDTO).ToList();
            }
            catch (Exception ex)
            {
                // Log the exception if necessary
                throw new Exception("An error occurred while retrieving results.", ex);
            }
        }


        private DependencyCheckResults MapToModel(DependencyCheckResultsDTO dependencyCheckResultsDTO)
        {
            return new DependencyCheckResults
            {
                Id = dependencyCheckResultsDTO.Id,
                ScanId = dependencyCheckResultsDTO.ScanId,
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
                ScanId = dependencyCheckResults.ScanId,
                PackageName = dependencyCheckResults.PackageName,
                HighestSeverity = dependencyCheckResults.HighestSeverity,
                CveCount = dependencyCheckResults.CveCount,
                EvidenceCount = dependencyCheckResults.EvidenceCount,
                BaseScore = dependencyCheckResults.BaseScore,
            };
        }
    }
}
