using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DependencyCheckAPI.DAL;
using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Models;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DependencyCheckAPI.Tests
{
    [TestFixture]
    public class ReportRepositoryTests
    {
        private Mock<ILogger<ReportRepository>> _mockLogger;
        private IReportRepository _reportRepository;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<ReportRepository>>();

            // Setup your mocks here
            // ...

            _reportRepository = new ReportRepository(_mockLogger.Object);
        }

        [Test]
        public async Task DownloadAsyncInstantDownload_FileExists_ReturnsScanReport()
        {
            // Arrange
            string blobFilename = "circustrein_1699625745679.zip";
            string userId = "josselard";
            string downloadedFilePath = Path.Combine(Directory.GetCurrentDirectory(), blobFilename); // Adjust the path as needed

            try
            {
                // Act
                var result = await _reportRepository.DownloadAsyncInstantDownload(blobFilename, userId);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(blobFilename, result.Name);
                Assert.AreEqual("application/octet-stream", result.ContentType);
            }
            finally
            {
                // Cleanup: Delete the downloaded file
                if (File.Exists(downloadedFilePath))
                {
                    File.Delete(downloadedFilePath);
                }
            }
        }





        [Test]
        public async Task DownloadAsyncInstantDownload_FileDoesNotExist_ReturnsNull()
        {
            // Arrange
            string blobFilename = "nonexistent.zip";
            string userId = "";
     

            // Act
            var result = await _reportRepository.DownloadAsyncInstantDownload(blobFilename, userId);

            // Assert
            Assert.IsNull(result);
        }
    }
}
