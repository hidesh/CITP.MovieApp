using System.Security.Claims;
using CITP.MovieApp.Api.Controllers;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CITP.MovieApp.Tests_.UnitTests
{
    public class SearchHistoryControllerTests
    {
        private readonly Mock<ISearchHistoryRepository> _repoMock;
        private readonly SearchHistoryController _controller;

        public SearchHistoryControllerTests()
        {
            _repoMock = new Mock<ISearchHistoryRepository>();
            _controller = new SearchHistoryController(_repoMock.Object);
        }

        private void SetUser(int userId = 1, bool authenticated = true)
        {
            var claims = new List<Claim>();
            if (authenticated)
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

            var identity = new ClaimsIdentity(claims, authenticated ? "TestAuth" : null);
            var user = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        // --------------------------------
        // GET /api/SearchHistory
        // --------------------------------

        [Fact]
        public async Task Get_WithValidUser_ReturnsOkWithPaginatedData()
        {
            // Arrange
            SetUser(userId: 123);
            var searchHistoryData = CreateTestSearchHistoryData();
            _repoMock.Setup(r => r.GetAllAsync(123))
                    .ReturnsAsync(searchHistoryData);

            // Act
            var result = await _controller.Get(page: 1, pageSize: 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = JObject.FromObject(okResult.Value!);
            
            Assert.Equal(3, response["total"]!.Value<int>());
            Assert.Equal(1, response["page"]!.Value<int>());
            Assert.Equal(10, response["pageSize"]!.Value<int>());
            Assert.NotNull(response["data"]);
            
            _repoMock.Verify(r => r.GetAllAsync(123), Times.Once);
        }

        [Theory]
        [InlineData(0, 20)]      // Invalid: zero -> default to 20
        [InlineData(-5, 20)]     // Invalid: negative -> default to 20  
        [InlineData(300, 20)]    // Invalid: too large -> default to 20
        [InlineData(50, 50)]     // Valid: within range
        [InlineData(200, 200)]   // Valid: max allowed
        public async Task Get_WithVariousPageSizes_HandlesCorrectly(int inputPageSize, int expectedPageSize)
        {
            // Arrange
            SetUser(userId: 123);
            var searchHistoryData = CreateTestSearchHistoryData();
            _repoMock.Setup(r => r.GetAllAsync(123))
                    .ReturnsAsync(searchHistoryData);

            // Act
            var result = await _controller.Get(page: 1, pageSize: inputPageSize);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = JObject.FromObject(okResult.Value!);
            Assert.Equal(expectedPageSize, response["pageSize"]!.Value<int>());
        }

        [Fact]
        public async Task Get_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            SetUser(userId: 123);
            var searchHistoryData = CreateLargeTestSearchHistoryData(25); // 25 items
            _repoMock.Setup(r => r.GetAllAsync(123))
                    .ReturnsAsync(searchHistoryData);

            // Act
            var result = await _controller.Get(page: 2, pageSize: 10);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = JObject.FromObject(okResult.Value!);
            var dataArray = response["data"] as JArray;
            
            Assert.Equal(25, response["total"]!.Value<int>());
            Assert.Equal(2, response["page"]!.Value<int>());
            Assert.Equal(10, dataArray!.Count); // Should have 10 items on page 2
        }

        [Fact]
        public async Task Get_WhenUserNotLoggedIn_ThrowsUserIdClaimNotFoundException()
        {
            // Arrange
            SetUser(authenticated: false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _controller.Get());
            Assert.Equal("User ID claim not found", exception.Message);
        }

        [Fact]
        public async Task Get_WithEmptySearchHistory_ReturnsEmptyData()
        {
            // Arrange
            SetUser(userId: 123);
            _repoMock.Setup(r => r.GetAllAsync(123))
                    .ReturnsAsync(new List<SearchHistoryDto>());

            // Act
            var result = await _controller.Get();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = JObject.FromObject(okResult.Value!);
            var dataArray = response["data"] as JArray;
            
            Assert.Equal(0, response["total"]!.Value<int>());
            Assert.Empty(dataArray!);
        }

        // --------------------------------
        // POST /api/SearchHistory
        // --------------------------------

        [Fact]
        public async Task Add_WithValidData_ReturnsOkWithCreatedItem()
        {
            // Arrange
            SetUser(userId: 123);
            var createDto = new CreateSearchHistoryDto
            {
                UserId = 123,
                Tconst = "tt0111161"
            };

            var createdDto = new CreateSearchHistoryDto
            {
                UserId = 123,
                Tconst = "tt0111161"
            };

            _repoMock.Setup(r => r.AddSearchHistoryAsync(123, "tt0111161"))
                    .ReturnsAsync(createdDto);

            // Act
            var result = await _controller.Add(createDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(createdDto, okResult.Value);
            _repoMock.Verify(r => r.AddSearchHistoryAsync(123, "tt0111161"), Times.Once);
        }

        [Fact]
        public async Task Add_WhenUserTriesToCreateForAnotherUser_ReturnsBadRequest()
        {
            // Arrange
            SetUser(userId: 123);
            var createDto = new CreateSearchHistoryDto
            {
                UserId = 999, // Different from authenticated user ID (123)
                Tconst = "tt0111161"
            };

            // Act
            var result = await _controller.Add(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = JObject.FromObject(badRequestResult.Value!);
            Assert.Equal("User ID mismatch", response["message"]!.Value<string>());
            
            _repoMock.Verify(r => r.AddSearchHistoryAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Add_WhenUserNotLoggedIn_ThrowsUserIdClaimNotFoundException()
        {
            // Arrange
            SetUser(authenticated: false);
            var createDto = new CreateSearchHistoryDto
            {
                UserId = 123,
                Tconst = "tt0111161"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _controller.Add(createDto));
            Assert.Equal("User ID claim not found", exception.Message);
        }

        [Fact]
        public async Task Add_WhenDatabaseErrorOccurs_PropagatesDatabaseException()
        {
            // Arrange
            SetUser(userId: 123);
            var createDto = new CreateSearchHistoryDto
            {
                UserId = 123,
                Tconst = "tt0111161"
            };

            _repoMock.Setup(r => r.AddSearchHistoryAsync(123, "tt0111161"))
                    .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _controller.Add(createDto));
            Assert.Equal("Database error", exception.Message);
        }

        // --------------------------------
        // Helper Methods
        // --------------------------------

        private List<SearchHistoryDto> CreateTestSearchHistoryData()
        {
            return new List<SearchHistoryDto>
            {
                new SearchHistoryDto
                {
                    Title = "The Shawshank Redemption",
                    Tconst = "tt0111161",
                    VisitedAt = DateTime.UtcNow.AddDays(-1)
                },
                new SearchHistoryDto
                {
                    Title = "The Godfather",
                    Tconst = "tt0068646",
                    VisitedAt = DateTime.UtcNow.AddDays(-2)
                },
                new SearchHistoryDto
                {
                    Title = "The Dark Knight",
                    Tconst = "tt0468569",
                    VisitedAt = DateTime.UtcNow.AddDays(-3)
                }
            };
        }

        private List<SearchHistoryDto> CreateLargeTestSearchHistoryData(int count)
        {
            var data = new List<SearchHistoryDto>();
            for (int i = 0; i < count; i++)
            {
                data.Add(new SearchHistoryDto
                {
                    Title = $"Movie {i + 1}",
                    Tconst = $"tt{i:D7}",
                    VisitedAt = DateTime.UtcNow.AddDays(-i)
                });
            }
            return data;
        }
        
    }
}