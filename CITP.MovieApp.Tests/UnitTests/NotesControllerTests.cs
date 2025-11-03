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
    public class NotesControllerTests
    {
        private readonly Mock<INoteRepository> _repoMock;
        private readonly NotesController _controller;

        public NotesControllerTests()
        {
            _repoMock = new Mock<INoteRepository>();
            _controller = new NotesController(_repoMock.Object);
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
    }
}
