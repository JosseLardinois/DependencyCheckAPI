﻿using DependencyCheckAPI.DTO;
using DependencyCheckAPI.Models;

namespace DependencyCheckAPI.Interfaces
{
    public interface IAzureBlobStorageRepository
    {
        Task<ScanReport> DownloadAsyncInstantDownload(string blobFilename, string userId);
        Task<bool> CheckIfFileExistsAsync(string blobFilename, string userId);
        Task<ScanReport> UploadHtmlFileToBlobAsync(string blobFileName, string userId);


    }
}
