using DependencyCheckAPI.Models;

namespace DependencyCheckAPI.Interfaces
{
    public interface IExtractJsonService
    {
        public List<DependencyInfo> ExtractJson(string fileName);
        public Task<bool> MakeNewProject(string userId, string projectName);
    }
}
