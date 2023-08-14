using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DependencyCheckAPI.DAL;


[TestClass]
public class AzureStorageTests
{
    private IConfiguration _configuration;
    private ILogger<AzureStorage> _logger;

    [TestInitialize]
    public void Setup()
    {
        // Setup configuration with test connection string
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("BlobConnectionString", "your_test_connection_string")
            })
            .Build();

        // Setup mock ILogger
        _logger = Mock.Of<ILogger<AzureStorage>>();
    }

    [TestMethod]
    public async Task DownloadAsyncInstantDownload_FileExists_ReturnsBlobDto()
    {
        // Arrange
        string testBlobFilename = "testfile.txt";
        string testContainerName = "test-container";
        var azureStorage = new AzureStorage(_configuration, _logger);

        // Act
        BlobDto result = await azureStorage.DownloadAsyncInstantDownload(testBlobFilename, testContainerName);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual($"C:\\Users\\josse\\source\\repos\\test\\{testBlobFilename}", result.FilePath);
        Assert.AreEqual(testBlobFilename, result.Name);
        // You may need to check the ContentType as well if it is expected to be set in your scenario.
    }

    [TestMethod]
    public async Task DownloadAsyncInstantDownload_FileDoesNotExist_ReturnsNull()
    {
        // Arrange
        string nonExistentBlobFilename = "nonexistentfile.txt";
        string testContainerName = "test-container";
        var azureStorage = new AzureStorage(_configuration, _logger);

        // Act
        BlobDto result = await azureStorage.DownloadAsyncInstantDownload(nonExistentBlobFilename, testContainerName);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task UploadHtmlFileToBlobAsync_SuccessfulUpload_ReturnsBlobDto()
    {
        // Arrange
        string testBlobFileName = "testfile.zip";
        string testContainerName = "test-container";
        var azureStorage = new AzureStorage(_configuration, _logger);

        // Create a temporary test HTML file
        string testHtmlFilePath = @"C:\temp\testfile\dependency-check-report.html";
        Directory.CreateDirectory(Path.GetDirectoryName(testHtmlFilePath));
        File.WriteAllText(testHtmlFilePath, "<html><body>Test HTML content</body></html>");

        // Act
        BlobDto result = await azureStorage.UploadHtmlFileToBlobAsync(testBlobFileName, testContainerName);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(testHtmlFilePath, result.FilePath);
        Assert.AreEqual(testBlobFileName, result.Name);
        // You may need to check the ContentType as well if it is expected to be set in your scenario.

        // Clean up the temporary test HTML file
        File.Delete(testHtmlFilePath);
    }

    [TestMethod]
    public async Task UploadHtmlFileToBlobAsync_UploadError_ReturnsNull()
    {
        // Arrange
        string testBlobFileName = "testfile.zip";
        string testContainerName = "test-container";
        var azureStorage = new AzureStorage(_configuration, _logger);

        // Act
        BlobDto result = await azureStorage.UploadHtmlFileToBlobAsync(testBlobFileName, testContainerName);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CheckIfFileExistsAsync_FileExists_ReturnsTrue()
    {
        // Arrange
        string testBlobFilename = "testfile.txt";
        string testContainerName = "test-container";
        var azureStorage = new AzureStorage(_configuration, _logger);

        // Act
        bool result = await azureStorage.CheckIfFileExistsAsync(testBlobFilename, testContainerName);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task CheckIfFileExistsAsync_FileDoesNotExist_ReturnsFalse()
    {
        // Arrange
        string nonExistentBlobFilename = "nonexistentfile.txt";
        string testContainerName = "test-container";
        var azureStorage = new AzureStorage(_configuration, _logger);

        // Act
        bool result = await azureStorage.CheckIfFileExistsAsync(nonExistentBlobFilename, testContainerName);

        // Assert
        Assert.IsFalse(result);
    }
}
