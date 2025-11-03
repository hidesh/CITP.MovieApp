using System.Security.Claims;
using CITP.MovieApp.Api.Controllers;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;

namespace CITP.MovieApp.Tests_.UnitTests
{
    public class MoviesControllerTests
    {
        private readonly Mock<IMovieRepository> _repoMock;
        private readonly MoviesController _controller;

        public MoviesControllerTests()
        {
            _repoMock = new Mock<IMovieRepository>();
            _controller = new MoviesController(_repoMock.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        // -------------------------------
        // GET /api/movies
        // -------------------------------

        [Fact]
        public async Task Get_ReturnsOk_WithPagedData()
        {
            // Arrange
            var titles = Enumerable.Range(1, 50).Select(i => new TitleDto
            {
                Tconst = $"tt{i:D5}",
                PrimaryTitle = $"Movie {i}",
                IsAdult = false
            }).ToList();

            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(titles);

            // Act
            var result = await _controller.Get(page: 2, pageSize: 10);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var json = JObject.FromObject(ok.Value!);

            Assert.Equal(50, (int)json["total"]!);
            Assert.Equal(2, (int)json["page"]!);
            Assert.Equal(10, (int)json["pageSize"]!);
            Assert.Equal(10, json["data"]!.Count());
        }

        [Fact]
        public async Task Get_ReturnsDefaultPageSize_WhenInvalidPageSize()
        {
            // Arrange
            var titles = new List<TitleDto>
            {
                new() { Tconst = "tt0001", PrimaryTitle = "Test Movie" }
            };

            _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(titles);

            // Act
            var result = await _controller.Get(page: 1, pageSize: -5);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var json = JObject.FromObject(ok.Value!);

            Assert.Equal(20, (int)json["pageSize"]!); // defaulted
        }

        // -------------------------------
        // GET /api/movies/{tconst}
        // -------------------------------

        [Fact]
        public async Task GetById_ReturnsOk_WhenTitleExists_AndNotAdult()
        {
            // Arrange
            var title = new TitleDto { Tconst = "tt0001", PrimaryTitle = "Normal Movie", IsAdult = false };
            _repoMock.Setup(r => r.GetByIdAsync("tt0001")).ReturnsAsync(title);

            // Act
            var result = await _controller.GetById("tt0001");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<TitleDto>(ok.Value);
            Assert.Equal("Normal Movie", returned.PrimaryTitle);
        }

        [Fact]
        public async Task GetById_ReturnsUnauthorized_WhenAdultAndUserNotAuthenticated()
        {
            // Arrange
            var adultTitle = new TitleDto { Tconst = "tt0999", PrimaryTitle = "Adult Movie", IsAdult = true };
            _repoMock.Setup(r => r.GetByIdAsync("tt0999")).ReturnsAsync(adultTitle);

            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity()) // not logged in
            };
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.GetById("tt0999");

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var json = JObject.FromObject(unauthorized.Value!);
            Assert.Equal("Login required to access adult-rated titles.", (string)json["message"]!);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenAdultAndUserAuthenticated()
        {
            // Arrange
            var adultTitle = new TitleDto { Tconst = "tt0999", PrimaryTitle = "Adult Movie", IsAdult = true };
            _repoMock.Setup(r => r.GetByIdAsync("tt0999")).ReturnsAsync(adultTitle);

            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }, "TestAuth");
            var httpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            };
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.GetById("tt0999");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<TitleDto>(ok.Value);
            Assert.Equal("Adult Movie", returned.PrimaryTitle);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenMissing()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync("tt9999")).ReturnsAsync((TitleDto?)null);

            // Act
            var result = await _controller.GetById("tt9999");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // -------------------------------
        // GET /api/movies/{tconst}/cast
        // -------------------------------

        [Fact]
        public async Task GetCast_ReturnsOk_WithCastList()
        {
            // Arrange
            var cast = new List<TitleCastCrewDto>
            {
                new() { Nconst = "nm0001", Name = "Actor One", CharacterName = "Hero" },
                new() { Nconst = "nm0002", Name = "Actor Two", CharacterName = "Villain" }
            };

            _repoMock.Setup(r => r.GetCastAndCrewAsync("tt0001")).ReturnsAsync(cast);

            // Act
            var result = await _controller.GetCast("tt0001");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result);
            var returnedCast = Assert.IsAssignableFrom<IEnumerable<TitleCastCrewDto>>(ok.Value);
            Assert.Equal(2, returnedCast.Count());
        }
    }
}
