using DependencyCheckAPI.Models;

namespace DependencyCheckAPI.Interfaces
{
    public interface IExtractJsonService
    {
        List<DependencyCheckResults> ExtractJson(string fileName, Guid scanId);
    }
}
