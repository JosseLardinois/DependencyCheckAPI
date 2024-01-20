using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Models;

namespace DependencyCheckAPI.DAL
{
    public class ReportRepository : IReportRepository
    {
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        private readonly ILogger<ReportRepository> _logger;

        public ReportRepository(ILogger<ReportRepository> logger)
        {
                _storageConnectionString = Environment.GetEnvironmentVariable("DCAzureBlobCS");
                _storageContainerName = Environment.GetEnvironmentVariable("BlobContainerName");
                _logger = logger;
        }


        public async Task<ScanReport> DownloadAsyncInstantDownload(string blobFilename,string userId)
        {
            BlobContainerClient client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
            string destinationFilePath = blobFilename;
            try
            {
                BlobClient file = client.GetBlobClient(userId+"\\"+blobFilename);
                if (await file.ExistsAsync())
                {
                    await file.DownloadToAsync(destinationFilePath);
                    BlobProperties properties = await file.GetPropertiesAsync();
                    string name = blobFilename;
                    string contentType = properties.ContentType;
                    return new ScanReport { FilePath = destinationFilePath, Name = name, ContentType = contentType };
                }
            }
            catch (RequestFailedException ex)
                when (ex.ErrorCode == BlobErrorCode.BlobNotFound)

            {
                _logger.LogError($"File {blobFilename} was not found.");
            }
            return null;
        }

        public async Task<ScanReport> UploadHtmlFileToBlobAsync(string blobFileName, string userId)
        {
            string foldername = blobFileName.Replace(".zip", "");
            string htmlFilePath = foldername + @"/dependency-check-report.html";
            //string htmlFilePath = @"C:\Users\josse\source\repos\"+foldername+"\\dependency-check-report.html";
            BlobContainerClient containerClient = new BlobContainerClient(_storageConnectionString, _storageContainerName);
            string destinationBlobPath = userId+"\\"+foldername+"\\dependency-check-report.html";

            try
            {
                string htmlContent = File.ReadAllText(htmlFilePath);
                BlobClient blobClient = containerClient.GetBlobClient(destinationBlobPath);

                using (MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(htmlContent)))
                {
                    await blobClient.UploadAsync(stream, true);
                }

                BlobProperties properties = await blobClient.GetPropertiesAsync();
                string contentType = properties.ContentType;

                return new ScanReport { FilePath = htmlFilePath, Name = blobFileName, ContentType = contentType };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file to Blob Storage: {ex.Message}");
                return null;
            }
        }
        public async Task<bool> CheckIfFileExistsAsync(string blobFilename, string userId)
        {
            BlobContainerClient client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
            try
            {
                BlobClient file = client.GetBlobClient(userId+"\\"+blobFilename);

                return await file.ExistsAsync();
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                _logger.LogError($"File {blobFilename} was not found.");
            }

            return false;
        }


    }

}
