using DependencyCheckAPI.Models;

namespace DependencyCheckAPI.Interfaces
{
    public interface IExtractJson
    {
        public List<DependencyInfo> ExtractJson(string fileName);
        public bool MakeNewProject(string userId, string projectName);
    }
}
