using DependencyCheckAPI.Dto;

namespace DependencyCheckAPI.Interfaces
{
    public interface IAzureBlobStorage
    {
        Task<BlobDto> DownloadAsyncInstantDownload(string blobFilename, string userId);
        Task<bool> CheckIfFileExistsAsync(string blobFilename, string userId);
        Task<BlobDto> UploadHtmlFileToBlobAsync(string blobFileName, string userId);


    }
}
