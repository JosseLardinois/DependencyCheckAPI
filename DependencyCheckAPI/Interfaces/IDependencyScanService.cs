namespace DependencyCheckAPI.Interfaces
{
    public interface IDependencyScanService
    {
        Task<string> UnzipFolder(string filename);

        Task ExecuteDependencyScan(string foldername, string outputFormat);
    }
}
