using DependencyCheckAPI.Service;
using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.DTO;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DependencyCheckAPI.Models;

namespace DependencyCheckAPI.Tests
{
    [TestFixture]
    public class SQLResultsServiceTests
    {
        private Mock<ISQLResultsStorageRepository> _mockStorage;
        private ISQLResultsService _sqlResultsService;

        [SetUp]
        public void Setup()
        {
            _mockStorage = new Mock<ISQLResultsStorageRepository>();
            _sqlResultsService = new SQLResultsService(_mockStorage.Object);
        }

        [Test]
        public async Task CreateScan_ValidInput_ReturnsGuid()
        {
            // Arrange
            var createdBy = Guid.NewGuid();
            var projectName = "TestProject";
            var expectedScanId = Guid.NewGuid();
            _mockStorage.Setup(x => x.CreateScan(projectName, createdBy)).ReturnsAsync(expectedScanId);

            // Act
            var result = await _sqlResultsService.CreateScan(createdBy, projectName);

            // Assert
            Assert.AreEqual(expectedScanId, result);
        }

        [Test]
        public void CreateScan_StorageThrowsException_ThrowsException()
        {
            // Arrange
            var createdBy = Guid.NewGuid();
            var projectName = "TestProject";
            _mockStorage.Setup(x => x.CreateScan(projectName, createdBy)).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await _sqlResultsService.CreateScan(createdBy, projectName));
        }

        [Test]
        public async Task GetResults_EmptyValidInput_ReturnsEmptyResult()
        {
            // Arrange
            var projectName = "TestProject";
            var mockResults = new List<DependencyCheckResults> { /*No data*/ };
            var mockResultsDTO = mockResults.Select(r => new DependencyCheckResultsDTO
            {
                // Map properties from DependencyCheckResults to DependencyCheckResultsDTO
            }).ToList();

            _mockStorage.Setup(x => x.RetrieveDependencyCheckResults(projectName)).ReturnsAsync(mockResults);

            // Act
            var result = await _sqlResultsService.GetResults(projectName);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<DependencyCheckResultsDTO>>(result);
            Assert.IsEmpty(result);
        }


        [Test]
        public async Task GetResults_EmptyValidInput_ReturnsResult()
        {
            // Arrange
            var projectName = "TestProject";
            var mockResults = new List<DependencyCheckResults> { new DependencyCheckResults
        {
            Id = Guid.NewGuid(),
            ScanId = Guid.NewGuid(),
            PackageName = "Package1",
            HighestSeverity = "HIGH",
            CveCount = 5,
            EvidenceCount = 10,
            BaseScore = 7.5
        },
        new DependencyCheckResults
        {
            Id = Guid.NewGuid(),
            ScanId = Guid.NewGuid(),
            PackageName = "Package2",
            HighestSeverity = "MEDIUM",
            CveCount = 3,
            EvidenceCount = 8,
            BaseScore = 5.0
        } };
            var mockResultsDTO = mockResults.Select(r => new DependencyCheckResultsDTO
            {
                // Map properties from DependencyCheckResults to DependencyCheckResultsDTO
            }).ToList();

            _mockStorage.Setup(x => x.RetrieveDependencyCheckResults(projectName)).ReturnsAsync(mockResults);

            // Act
            var result = await _sqlResultsService.GetResults(projectName);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<DependencyCheckResultsDTO>>(result);
            //assert count
            Assert.That(result.Count, Is.EqualTo(2));

            //Assert results
            Assert.That(result[0].PackageName, Is.EqualTo("Package1"));
            Assert.That(result[0].HighestSeverity, Is.EqualTo("HIGH"));
            Assert.That(result[0].CveCount, Is.EqualTo(5));
            Assert.That(result[0].EvidenceCount, Is.EqualTo(10));
            Assert.That(result[0].BaseScore, Is.EqualTo(7.5));

            //Assert second package
            Assert.That(result[1].PackageName, Is.EqualTo("Package2"));
            Assert.That(result[1].BaseScore, Is.EqualTo(5.0));

        }


        [Test]
        public void GetResults_StorageThrowsException_ThrowsException()
        {
            // Arrange
            var projectName = "TestProject";
            _mockStorage.Setup(x => x.RetrieveDependencyCheckResults(projectName)).ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            Assert.ThrowsAsync<Exception>(async () => await _sqlResultsService.GetResults(projectName));
        }
    }
}
