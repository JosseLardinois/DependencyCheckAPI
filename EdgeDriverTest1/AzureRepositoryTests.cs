using DependencyCheckAPI.Dto;
using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
namespace AzureStorageTests
{

    [TestClass]
    public class AzureRepositoryTests
    {
        [TestMethod]
        public async Task GetBlobFile_ValidInput_ReturnsBlobDto()
        {
            // Arrange
            var mockStorage = new Mock<IAzureBlobStorage>();
            var azureRepository = new AzureFileRepository(mockStorage.Object);

            string userId = "testuser1";
            string testFilename = "testfile.txt";
            string testContainerName = "test-container";
            var expectedBlobDto = new BlobDto { Name = testFilename, ContentType = "text/plain" };

            // Setup the behavior of the mock IAzureStorage
            mockStorage.Setup(x => x.DownloadAsyncInstantDownload(testFilename, userId)).ReturnsAsync(expectedBlobDto);

            // Act
            var result = await azureRepository.GetBlobFile(testFilename, userId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedBlobDto.Name, result.Name);
            Assert.AreEqual(expectedBlobDto.FilePath, result.FilePath);
            Assert.AreEqual(expectedBlobDto.ContentType, result.ContentType);
        }

        [TestMethod]
        public async Task UploadHtmlReport_ValidInput_ReturnsBlobDto()
        {
            // Arrange
            var mockStorage = new Mock<IAzureBlobStorage>();
            var azureRepository = new AzureFileRepository(mockStorage.Object);

            string testFilename = "testfile.zip";
            string testContainerName = "test-container";
            string testUserName = "abc123";
            var expectedBlobDto = new BlobDto { Name = testFilename, ContentType = "text/html" };

            // Setup the behavior of the mock IAzureStorage
            mockStorage.Setup(x => x.UploadHtmlFileToBlobAsync(testFilename, testUserName)).ReturnsAsync(expectedBlobDto);

            // Act
            var result = await azureRepository.UploadHtmlReport(testFilename, testUserName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedBlobDto.Name, result.Name);
            Assert.AreEqual(expectedBlobDto.FilePath, result.FilePath);
            Assert.AreEqual(expectedBlobDto.ContentType, result.ContentType);
        }

        [TestMethod]
        public async Task DoesFileExistInBlob_FileExists_ReturnsTrue()
        {
            // Arrange
            var mockStorage = new Mock<IAzureBlobStorage>();
            var azureRepository = new AzureFileRepository(mockStorage.Object);

            string userId = "testuser1";
            string testFilename = "testfile.txt";
            string testContainerName = "test-container";

            // Setup the behavior of the mock IAzureStorage
            mockStorage.Setup(x => x.CheckIfFileExistsAsync(testFilename, userId)).ReturnsAsync(true);

            // Act
            var result = await azureRepository.DoesFileExistInBlob(testFilename, userId);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task DoesFileExistInBlob_FileDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var mockStorage = new Mock<IAzureBlobStorage>();
            var azureRepository = new AzureFileRepository(mockStorage.Object);

            string userId = "testuser1";
            string testFilename = "nonexistentfile.txt";
            string testContainerName = "test-container";

            // Setup the behavior of the mock IAzureStorage
            mockStorage.Setup(x => x.CheckIfFileExistsAsync(testFilename, userId)).ReturnsAsync(false);

            // Act
            var result = await azureRepository.DoesFileExistInBlob(testFilename, userId);

            // Assert
            Assert.IsFalse(result);
        }
    }
}