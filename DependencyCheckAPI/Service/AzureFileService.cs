using DependencyCheckAPI.DTO;
using DependencyCheckAPI.Interfaces;

namespace DependencyCheckAPI.Service
{
    public class AzureFileService : IAzureFileService
    {
        private readonly IAzureBlobStorage _storage;

        public AzureFileService(IAzureBlobStorage storage)
        {
            _storage = storage;
        }

        public async Task<ScanReportDTO> GetBlobFile(string filename, string userId)
        {
            ScanReportDTO? file = await _storage.DownloadAsyncInstantDownload(filename, userId);
            return file;
        }
        public async Task<ScanReportDTO> UploadHtmlReport(string filename, string userId)
        {
            ScanReportDTO? file = await _storage.UploadHtmlFileToBlobAsync(filename, userId);
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
