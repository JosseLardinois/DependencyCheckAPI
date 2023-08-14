using DependencyCheckAPI.Dto;
using DependencyCheckAPI.Interfaces;

namespace DependencyCheckAPI.Repositories
{
    public class AzureFileRepository : IAzureFileRepository
    {
        private readonly IAzureBlobStorage _storage;

        public AzureFileRepository(IAzureBlobStorage storage)
        {
            _storage = storage;
        }

        public async Task<BlobDto> GetBlobFile(string filename, string userId)
        {
            BlobDto? file = await _storage.DownloadAsyncInstantDownload(filename, userId);
            return file;
        }
        public async Task<BlobDto> UploadHtmlReport(string filename, string userId)
        {
            BlobDto? file = await _storage.UploadHtmlFileToBlobAsync(filename, userId);
            return file;
        }
        public async Task<bool> DoesFileExistInBlob(string filename, string userId)
        {
            if(await _storage.CheckIfFileExistsAsync(filename, userId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}
