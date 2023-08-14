using DependencyCheckAPI.Dto;

namespace DependencyCheckAPI.Interfaces
{
    public interface IAzureFileRepository
    {
        Task<BlobDto> GetBlobFile(string filename, string userId);
        Task<BlobDto> UploadHtmlReport(string filename, string userId);
        Task<bool> DoesFileExistInBlob(string filename, string userId);
    }
}