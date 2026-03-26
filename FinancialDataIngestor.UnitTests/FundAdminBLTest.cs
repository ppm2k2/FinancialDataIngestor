using FinancialDataIngestor.Interfaces.DataAccess;
using FinancialDataIngestor.Models.Type;
using FundAdminRestAPI.Interfaces.DataAccess;
using FundAdminRestAPI.Models;
using Moq;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Xunit;

namespace FinancialDataIngestor.BusinessLogic.Tests
{
    public class FundAdminBLTests
    {
        private readonly Mock<IFundRepository> _fundRepositoryMock;
        private readonly Mock<IAuditService> _auditServiceMock;
        private readonly FundAdminBL _sut;

        public FundAdminBLTests()
        {
            _fundRepositoryMock = new Mock<IFundRepository>();
            _auditServiceMock = new Mock<IAuditService>();
            _sut = new FundAdminBL(_fundRepositoryMock.Object, _auditServiceMock.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_NullFundRepository_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FundAdminBL(null, _auditServiceMock.Object));
        }

        [Fact]
        public void Constructor_NullAuditService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new FundAdminBL(_fundRepositoryMock.Object, null));
        }

        [Fact]
        public void Constructor_SingleParam_DoesNotThrow()
        {
            var bl = new FundAdminBL(_fundRepositoryMock.Object);
            Assert.NotNull(bl);
        }

        #endregion

        #region GetFundData Tests

        [Fact]
        public async Task GetFundData_Success_ReturnsSuccessfulResponse()
        {
            // Arrange
            var expectedDto = new ClientAccountDTO
            {
                ClientId = "CLT_12345"
            };
            _fundRepositoryMock
                .Setup(r => r.GetFundData())
                .Returns(expectedDto);

            // Act
            var result = await _sut.GetFundData();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ServiceReponse);
            Assert.True(result.ServiceReponse.IsSuccessful);
            Assert.NotNull(result.Result);
            Assert.Single(result.Result);
            Assert.Equal("CLT_12345", result.Result[0].ClientId);
            Assert.Contains("Fund data retrieved successfully.", result.ServiceReponse.Message);
        }

        [Fact]
        public async Task GetFundData_RepositoryThrows_ReturnsFailureResponse()
        {
            // Arrange
            _fundRepositoryMock
                .Setup(r => r.GetFundData())
                .Throws(new Exception("Database error"));

            // Act
            var result = await _sut.GetFundData();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.ServiceReponse);
            Assert.False(result.ServiceReponse.IsSuccessful);
            Assert.Equal("Database error", result.ServiceReponse.ErrorMessage);
            Assert.Contains("Failed to retrieve fund data.", result.ServiceReponse.Message);
        }

        [Fact]
        public async Task GetFundData_RepositoryReturnsNull_StillReturnsSuccessWithNullInList()
        {
            // Arrange
            _fundRepositoryMock
                .Setup(r => r.GetFundData())
                .Returns((ClientAccountDTO)null);

            // Act
            var result = await _sut.GetFundData();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ServiceReponse.IsSuccessful);
            Assert.NotNull(result.Result);
            Assert.Single(result.Result);
            Assert.Null(result.Result[0]);
        }

        #endregion

        #region InsertFundDataAsync Tests

        [Fact]
        public async Task InsertFundDataAsync_WhenDownloadFails_ReturnsFailureAndLogsAudit()
        {
            // Arrange
            // Download_File and ReadFundData depend on file system / HTTP,
            // so this will fail at the file I/O level, exercising the catch block.
            _auditServiceMock
                .Setup(a => a.LogChangeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<object>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.InsertFundDataAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.ServiceReponse.IsSuccessful);
            //Assert.NotNull(result.ServiceReponse.ErrorMessage);

            // Verify audit was called at least for the INSERT action
            _auditServiceMock.Verify(a => a.LogChangeAsync(
                "INSERT",
                It.IsAny<string>(),
                "INSERT",
                null,
                It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task InsertFundDataAsync_WithZipUrl_WhenDownloadFails_ReturnsFailureAndLogsAudit()
        {
            // Arrange
            _auditServiceMock
                .Setup(a => a.LogChangeAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<object>(),
                    It.IsAny<object>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _sut.InsertFundDataAsync("https://example.com/test.zip");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.ServiceReponse.IsSuccessful);

            _auditServiceMock.Verify(a => a.LogChangeAsync(
                "INSERT",
                It.IsAny<string>(),
                "INSERT",
                null,
                It.IsAny<object>()), Times.Once);
        }

        #endregion

        
    }
}