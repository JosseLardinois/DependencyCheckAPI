using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Models;
using DependencyCheckAPI.Service;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace DependencyCheckAPI.Tests
{
    [TestFixture]
    public class ExtractJsonServiceTests
    {
        private Mock<ISQLResultsStorageRepository> _mockRepository;
        private IExtractJsonService _extractJsonService;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<ISQLResultsStorageRepository>();
            _extractJsonService = new ExtractJsonService(_mockRepository.Object);
        }

        [Test]
        public void ExtractJson_ValidJson_ReturnsDependencyCheckResults()
        {
            // Arrange
            string fileName = "JsonFiles"; // File will be in the output directory
            Guid scanId = Guid.NewGuid();

            // Act
            var result = _extractJsonService.ExtractJson(fileName, scanId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<DependencyCheckResults>>(result);
            _mockRepository.Verify(r => r.InsertDependencyInfosIntoDatabase(scanId, It.IsAny<List<DependencyCheckResults>>()), Times.Once);
        }

        [Test]
        public void ExtractJson_ValidateCountAndResults()
        {
            // Arrange
            string fileName = "JsonFiles"; // File will be in the output directory
            Guid scanId = Guid.NewGuid();

            // Act
            var result = _extractJsonService.ExtractJson(fileName, scanId);

            // Assert
            Assert.IsNotNull(result);

            Assert.AreEqual(6, result.Count);

            Assert.That(result[0].HighestSeverity, Is.EqualTo("HIGH"));
            Assert.IsInstanceOf<List<DependencyCheckResults>>(result);
            _mockRepository.Verify(r => r.InsertDependencyInfosIntoDatabase(scanId, It.IsAny<List<DependencyCheckResults>>()), Times.Once);
        }


        [Test]
        public void ExtractJson_UnexpectedJsonContent_ReturnsNull()
        {
            // Arrange
            string unexpectedContentFileName = "InvalidJsonFiles";
            Guid scanId = Guid.NewGuid();

            // Act
            Assert.Throws<NullReferenceException>(() => _extractJsonService.ExtractJson(unexpectedContentFileName, scanId));
        }


        [Test]
        public void ExtractJson_InvalidJson_ThrowsException()
        {
            // Arrange
            string invalidFileName = "InvalidPath";
            Guid scanId = Guid.NewGuid();

            // Act & Assert
            Assert.Throws<DirectoryNotFoundException>(() => _extractJsonService.ExtractJson(invalidFileName, scanId));
        }




    }
}
