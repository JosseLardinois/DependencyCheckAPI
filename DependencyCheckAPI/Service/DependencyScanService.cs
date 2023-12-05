using DependencyCheckAPI.Interfaces;
using System.Diagnostics;
using System.IO.Compression;

namespace DependencyCheckAPI.Service
{
    public class DependencyScanService : IDependencyScanService
    {
        public async Task<string> UnzipFolder(string scanId)
        {
            string foldername = scanId.Replace(".zip", "");
            string sourcePath = scanId;
            string destinationPath = foldername;
            try
                {
                Console.WriteLine("Unzipping");
                await Task.Run(() => ZipFile.ExtractToDirectory(sourcePath, destinationPath));
                Console.WriteLine("extracted to directory");

                return foldername;
                }
            catch (IOException ex) when (ex.Message.Contains("already exists"))
            {
                throw new Exception($"The file '{scanId}' has already been scanned.");
            }
            catch (Exception ex)
                {
                    // You can also log the error if needed.
                    throw new Exception($"Error extracting zip file '{scanId}': {ex.Message}");
                }
        }
        public async Task ExecuteDependencyScan(string foldername, string outputFormat)
        {
            try
            {
                string projectPath = foldername;
                string outputPath = projectPath;
                string dependencyCheckPath = "/app/dependency-check/dependency-check/bin/dependency-check.sh";
               // string dependencyCheckPath = "C:\\Users\\jlardinois\\Downloads\\dependency-check-8.4.0-release\\dependency-check\\bin\\dependency-check.bat";

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
            }
            catch (Exception ex)
            {
                throw new Exception($"Error extracting zip file: {ex.Message}");
            }
        }
    }
}
