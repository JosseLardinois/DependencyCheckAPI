﻿using DependencyCheckAPI.Interfaces;
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
            string sourcePath = filename;
            string destinationPath = foldername;
            try
                {
                Console.WriteLine("Unzipping");
                await Task.Run(() => ZipFile.ExtractToDirectory(sourcePath, destinationPath));
                Console.WriteLine("extracted to directory");
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
            try
            {
                string projectPath = foldername;
                string outputPath = projectPath;
                string dependencyCheckPath = "/app/dependency-check/dependency-check/bin/dependency-check.sh";

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
