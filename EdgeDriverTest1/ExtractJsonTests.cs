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
