using DependencyCheckAPI.DTO;

namespace DependencyCheckAPI.Interfaces
{
    public interface IReportService
    {
        Task<ScanReportDTO> GetBlobFile(string filename, string userId);
        Task<ScanReportDTO> UploadHtmlReport(string filename, string userId);
        Task<bool> DoesFileExistInBlob(string filename, string userId);
    }
}