using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CITP.MovieApp.Api.Controllers;
using CITP.MovieApp.Application.DTOs;
using CITP.MovieApp.Domain.Entities;
using CITP.MovieApp.Infrastructure.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CITP.MovieApp.Tests_.UnitTests
{
    public class AuthControllerTests
    {
        private readonly Mock<UserRepository> _repoMock;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly IConfiguration _config;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            // IMPORTANT: UserRepository methods must be virtual for Moq
            _repoMock = new Mock<UserRepository>(null!);

            var jwtConfig = new Dictionary<string, string?>
            {
                { "Jwt:Key", "this_is_a_long_enough_test_key_for_jwt_1234567890" },
                { "Jwt:Issuer", "test_issuer" },
                { "Jwt:Audience", "test_audience" },
                { "Jwt:ExpiryMinutes", "60" }
            };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(jwtConfig!)
                .Build();

            // Mock IWebHostEnvironment for avatar uploads
            _envMock = new Mock<IWebHostEnvironment>();
            _envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

            _controller = new AuthController(
                _repoMock.Object,
                _config,
                _envMock.Object
            );
        }

        // -------------------------------
        // REGISTER TESTS
        // -------------------------------

        [Fact]
        public async Task Register_ReturnsOk_WhenValid()
        {
            var req = new RegisterRequest
            {
                Username = "newuser",
                Email = "newuser@test.com",
                Password = "password123"
            };

            _repoMock.Setup(r => r.GetByUsernameAsync(req.Username))
                .ReturnsAsync((User?)null);

            _repoMock.Setup(r => r.GetByEmailAsync(req.Email))
                .ReturnsAsync((User?)null);

            var result = await _controller.Register(req, null);

            var ok = Assert.IsType<OkObjectResult>(result);
            var message = ok.Value?.GetType()
                .GetProperty("message")?
                .GetValue(ok.Value)?
                .ToString();

            Assert.Equal("User registered successfully", message);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenUsernameExists()
        {
            var req = new RegisterRequest
            {
                Username = "existinguser",
                Email = "unique@test.com",
                Password = "password123"
            };

            _repoMock.Setup(r => r.GetByUsernameAsync(req.Username))
                .ReturnsAsync(new User { Username = req.Username });

            var result = await _controller.Register(req, null);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var message = bad.Value?.GetType()
                .GetProperty("message")?
                .GetValue(bad.Value)?
                .ToString();

            Assert.Equal("Username already exists", message);
        }

        [Fact]
        public async Task Register_ReturnsBadRequest_WhenEmailExists()
        {
            var req = new RegisterRequest
            {
                Username = "uniqueuser",
                Email = "existing@test.com",
                Password = "password123"
            };

            _repoMock.Setup(r => r.GetByUsernameAsync(req.Username))
                .ReturnsAsync((User?)null);

            _repoMock.Setup(r => r.GetByEmailAsync(req.Email))
                .ReturnsAsync(new User { Email = req.Email });

            var result = await _controller.Register(req, null);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var message = bad.Value?.GetType()
                .GetProperty("message")?
                .GetValue(bad.Value)?
                .ToString();

            Assert.Equal("Email already exists", message);
        }

        // -------------------------------
        // LOGIN TESTS
        // -------------------------------

        [Fact]
        public async Task Login_ReturnsOk_WithJwtToken_WhenCredentialsValid()
        {
            var req = new LoginRequest
            {
                Username = "user1",
                Password = "password123"
            };

            var hashed = Convert.ToBase64String(
                SHA256.HashData(Encoding.UTF8.GetBytes(req.Password))
            );

            var user = new User
            {
                UserId = 1,
                Username = req.Username,
                Email = "user1@test.com",
                PasswordHash = hashed
            };

            _repoMock.Setup(r => r.GetByUsernameAsync(req.Username))
                .ReturnsAsync(user);

            var result = await _controller.Login(req);

            var ok = Assert.IsType<OkObjectResult>(result);

            // Extract token safely via JSON
            var json = JsonSerializer.Serialize(ok.Value);
            using var doc = JsonDocument.Parse(json);

            var token = doc.RootElement.GetProperty("token").GetString();

            Assert.False(string.IsNullOrWhiteSpace(token));
            Assert.Contains(".", token); // basic JWT sanity check
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenUserNotFound()
        {
            var req = new LoginRequest
            {
                Username = "ghost",
                Password = "anything"
            };

            _repoMock.Setup(r => r.GetByUsernameAsync(req.Username))
                .ReturnsAsync((User?)null);

            var result = await _controller.Login(req);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var message = unauthorized.Value?.GetType()
                .GetProperty("message")?
                .GetValue(unauthorized.Value)?
                .ToString();

            Assert.Equal("Invalid username or password", message);
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenWrongPassword()
        {
            var req = new LoginRequest
            {
                Username = "user1",
                Password = "wrongpass"
            };

            var correctHash = Convert.ToBase64String(
                SHA256.HashData(Encoding.UTF8.GetBytes("correctpass"))
            );

            var user = new User
            {
                UserId = 2,
                Username = req.Username,
                Email = "x@test.com",
                PasswordHash = correctHash
            };

            _repoMock.Setup(r => r.GetByUsernameAsync(req.Username))
                .ReturnsAsync(user);

            var result = await _controller.Login(req);

            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            var message = unauthorized.Value?.GetType()
                .GetProperty("message")?
                .GetValue(unauthorized.Value)?
                .ToString();

            Assert.Equal("Invalid username or password", message);
        }
    }
}
