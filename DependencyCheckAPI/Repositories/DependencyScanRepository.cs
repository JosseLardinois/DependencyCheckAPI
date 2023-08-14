using DependencyCheckAPI.Interfaces;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading.Tasks;

namespace DependencyCheckAPI.Repositories
{
    public class DependencyScanRepository : IDependencyScanRepository
    {
        public async Task UnzipFolder(string filename)
        {
            string foldername = filename.Replace(".zip", "");
            string sourcePath = Path.Combine("C:\\Users\\josse\\source\\repos\\test", filename);
            string destinationPath = Path.Combine("C:\\Users\\josse\\source\\repos\\test", foldername);
            try
                {
                    await Task.Run(() => ZipFile.ExtractToDirectory(sourcePath, destinationPath));
                    await ExecuteDependencyScan(foldername, "JSON");
                    await ExecuteDependencyScan(foldername, "HTML");
                }
            catch (IOException ex) when (ex.Message.Contains("already exists"))
            {
                throw new Exception($"The file '{filename}' has already been scanned.");
            }
            catch (Exception ex)
                {
                    // You can also log the error if needed.
                    throw new Exception($"Error extracting zip file '{filename}': {ex.Message}");
                }
        }




        public async Task ExecuteDependencyScan(string foldername, string outputFormat)
        {
            string projectPath = Path.Combine("C:\\Users\\josse\\source\\repos\\test", foldername);
            string outputPath = projectPath;
            string dependencyCheckPath = "C:\\Users\\josse\\Downloads\\dependency-check-8.3.1-release\\dependency-check\\bin\\dependency-check.bat";

            if (Directory.Exists(projectPath))
            {
                await Task.Run(() =>
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        UseShellExecute = false,
                        FileName = dependencyCheckPath,
                        Arguments = $"--project \"testproject\" -s \"{projectPath}\" -f \"{outputFormat}\" -o \"{outputPath}\"",
                        Verb = "runas"
                    };

                    using (Process process = Process.Start(startInfo))
                    {
                        process.WaitForExit();
                    }
                });
            }
            else
            {
                throw new DirectoryNotFoundException($"Directory '{projectPath}' not found.");
            }
        }
    }
}
