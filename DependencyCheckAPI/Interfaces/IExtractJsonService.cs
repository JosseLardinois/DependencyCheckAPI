using DependencyCheckAPI.Models;

namespace DependencyCheckAPI.Interfaces
{
    public interface IExtractJsonService
    {
        public List<DependencyCheckResults> ExtractJson(string fileName);
        public Task<bool> MakeNewProject(string userId, string projectName);
    }
}
