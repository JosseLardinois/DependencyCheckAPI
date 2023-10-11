namespace DependencyCheckAPI.Interfaces
{
    public interface IDependencyScanService
    {
        Task UnzipFolder(string filename);
    }
}
