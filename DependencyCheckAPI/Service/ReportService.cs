using DependencyCheckAPI.DTO;
using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Models;

namespace DependencyCheckAPI.Service
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _storage;

        public ReportService(IReportRepository storage)
        {
            _storage = storage;
        }

        public async Task<ScanReportDTO> GetBlobFile(string filename, string userId)
        {
            ScanReport? file = await _storage.DownloadAsyncInstantDownload(filename, userId);
            return MapToDTO(file);
        }
        public async Task<ScanReportDTO> UploadHtmlReport(string filename, string userId)
        {
            ScanReport? file = await _storage.UploadHtmlFileToBlobAsync(filename, userId);
            return MapToDTO(file);
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

        private ScanReport MapToModel(ScanReportDTO scanReportDTO) {
            return new ScanReport
            {
                Uri = scanReportDTO.Uri,
                Name = scanReportDTO.Name,
                Content = scanReportDTO.Content,
                ContentType = scanReportDTO.ContentType,
                FilePath = scanReportDTO.FilePath,
            };
    }
        private ScanReportDTO MapToDTO(ScanReport scanReport)
        {
            return new ScanReportDTO
            {
                Uri = scanReport.Uri,
                Name = scanReport.Name,
                Content = scanReport.Content,
                ContentType = scanReport.ContentType,
                FilePath = scanReport.FilePath,
            };
        }
    }
}
