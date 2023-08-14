using DependencyCheckAPI.Interfaces;
using DependencyCheckAPI.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace DependencyCheckAPI.Repositories.Tests
{
    [TestClass] 
    public class ExtractJsonRepositoryTests
    {
        private Mock<ISQLResultsStorage> _sqlStorageMock;
        private Mock<ISQLResultsRepository> _sqlRepositoryMock;

        [TestInitialize]
        public void Initialize()
        {
            _sqlRepositoryMock = new Mock<ISQLResultsRepository>();
            _sqlStorageMock = new Mock<ISQLResultsStorage>();
        }


        [TestMethod]
        public void ExtractJson_ValidFile_ParsesAndInsertsCorrectly()
        {
            // Arrange
            var mockSqlStorage = new Mock<ISQLResultsStorage>();
            var mockSqlRepository = new Mock<ISQLResultsRepository>();
            var extractJsonRepository = new ExtractJsonRepository(mockSqlStorage.Object, mockSqlRepository.Object);

            string fileName = "fuzzy_1689607772400";
            string jsonFilePath = @"c:\Users\josse\source\repos\test\"+fileName+"\\dependency-check-report.json";

            
            // Act
            var result = extractJsonRepository.ExtractJson(fileName);

            // Assert
            NUnit.Framework.Assert.IsNotNull(result);
            NUnit.Framework.Assert.AreEqual(9, result.Count);

            // Assert the parsed DependencyInfo object
            var dependencyInfo = result[0];
            NUnit.Framework.Assert.AreEqual("@sideway/formula:3.0.0", dependencyInfo.DependencyName);
            NUnit.Framework.Assert.AreEqual("pkg:npm/%40sideway%2Fformula@3.0.0", dependencyInfo.PackageName);
            NUnit.Framework.Assert.AreEqual("MEDIUM", dependencyInfo.HighestSeverity);
            NUnit.Framework.Assert.AreEqual(1, dependencyInfo.CveCount);
            NUnit.Framework.Assert.AreEqual(3, dependencyInfo.EvidenceCount);
            NUnit.Framework.Assert.AreEqual(5.5, dependencyInfo.BaseScore);

            // Verify the interactions with the mockSqlStorage
            mockSqlStorage.Verify(repo => repo.InsertIntoDependencyCheckResults(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<double?>()), Times.Exactly(9));
            
        }
        [TestMethod]
        public void MakeNewProject_Should_Insert_New_Project_If_Not_Exists()
        {
            // Arrange
            string userId = "user123";
            string projectName = "project123.zip";

            _sqlStorageMock.Setup(x => x.CheckAndInsertIfNotExistsInProjects(userId, It.IsAny<string>())).Returns(true);
            var repository = new ExtractJsonRepository(_sqlStorageMock.Object, _sqlRepositoryMock.Object);

            // Act
            bool result = repository.MakeNewProject(userId, projectName);

            // Assert
            NUnit.Framework.Assert.IsTrue(result);
        }

        [TestMethod]
        public void MakeNewProject_Should_Not_Insert_Existing_Project()
        {
            // Arrange
            string userId = "user123";
            string projectName = "existing_project.zip";

            _sqlStorageMock.Setup(x => x.CheckAndInsertIfNotExistsInProjects(userId, projectName)).Returns(false);
            var repository = new ExtractJsonRepository(_sqlStorageMock.Object, _sqlRepositoryMock.Object);

            // Act
            bool result = repository.MakeNewProject(userId, projectName);

            // Assert
            NUnit.Framework.Assert.IsFalse(result);
        }
    }
}
