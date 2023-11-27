using DependencyCheckAPI.DTO;
using DependencyCheckAPI.Models;

namespace DependencyCheckAPI.Interfaces
{
    public interface ISQLResultsService
    {
        Task<List<DependencyCheckResultsDTO>> GetResults(string projectName);
    }
}
