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
                _storageConnectionString = Environment.GetEnvironmentVariable("BlobConnectionString");
                _storageContainerName = Environment.GetEnvironmentVariable("BlobContainerName");
                _logger = logger;
        }


        public async Task<ScanReport> DownloadAsyncInstantDownload(string blobFilename,string userId)
        {
            BlobContainerClient client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
            string destinationFilePath = blobFilename;
            try
            {
                // Get a reference to the blob uploaded earlier from the API in the container from configuration settings
                BlobClient file = client.GetBlobClient(userId+"\\"+blobFilename);

                // Check if the file exists in the container
                if (await file.ExistsAsync())
                {
                    await file.DownloadToAsync(destinationFilePath);

                    // Retrieve the file properties to populate the BlobDto
                    BlobProperties properties = await file.GetPropertiesAsync();
                    string name = blobFilename;
                    string contentType = properties.ContentType;


                    // Create a new BlobDto with the downloaded file details
                    return new ScanReport { FilePath = destinationFilePath, Name = name, ContentType = contentType };
                }
            }
            catch (RequestFailedException ex)
                when (ex.ErrorCode == BlobErrorCode.BlobNotFound)

            {
                // Log error to console
                _logger.LogError($"File {blobFilename} was not found.");
            }

            // File does not exist, return null and handle that in requesting method
            return null;
        }

        public async Task<ScanReport> UploadHtmlFileToBlobAsync(string blobFileName, string userId)
        {
            string foldername = blobFileName.Replace(".zip", "");
            string htmlFilePath = foldername + @"/dependency-check-report.html";
            //string htmlFilePath = @"C:\Users\josse\source\repos\"+foldername+"\\dependency-check-report.html";
            BlobContainerClient containerClient = new BlobContainerClient(_storageConnectionString, _storageContainerName);
            string destinationBlobPath = userId+"\\"+foldername+"\\dependency-check-report.html"; // The name of the blob in the container

            try
            {
                // Read the HTML file content
                string htmlContent = File.ReadAllText(htmlFilePath);

                // Get a reference to the blob in the container
                BlobClient blobClient = containerClient.GetBlobClient(destinationBlobPath);

                // Upload the HTML content to the blob
                using (MemoryStream stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(htmlContent)))
                {
                    await blobClient.UploadAsync(stream, true);
                }

                // Retrieve the file properties to populate the BlobDto
                BlobProperties properties = await blobClient.GetPropertiesAsync();
                string contentType = properties.ContentType;

                // Create a new BlobDto with the uploaded file details
                return new ScanReport { FilePath = htmlFilePath, Name = blobFileName, ContentType = contentType };
            }
            catch (Exception ex)
            {
                // Handle any errors, log, and return null or rethrow the exception based on your requirements
                Console.WriteLine($"Error uploading file to Blob Storage: {ex.Message}");
                return null;
            }
        }
        public async Task<bool> CheckIfFileExistsAsync(string blobFilename, string userId)
        {
            BlobContainerClient client = new BlobContainerClient(_storageConnectionString, _storageContainerName);
            try
            {
                // Get a reference to the blob in the container
                BlobClient file = client.GetBlobClient(userId+"\\"+blobFilename);

                // Check if the file exists in the container
                return await file.ExistsAsync();
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                // Log error to console
                _logger.LogError($"File {blobFilename} was not found.");
            }

            // File does not exist
            return false;
        }


    }

}
