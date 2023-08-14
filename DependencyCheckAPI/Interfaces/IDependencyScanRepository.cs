namespace DependencyCheckAPI.Interfaces
{
    public interface IDependencyScanRepository
    {
        Task UnzipFolder(string filename);
    }
}
