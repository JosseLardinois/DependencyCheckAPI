using DependencyCheckAPI.DTO;

namespace DependencyCheckAPI.Interfaces
{
    public interface IAzureBlobStorage
    {
        Task<ScanReportDTO> DownloadAsyncInstantDownload(string blobFilename, string userId);
        Task<bool> CheckIfFileExistsAsync(string blobFilename, string userId);
        Task<ScanReportDTO> UploadHtmlFileToBlobAsync(string blobFileName, string userId);


    }
}
