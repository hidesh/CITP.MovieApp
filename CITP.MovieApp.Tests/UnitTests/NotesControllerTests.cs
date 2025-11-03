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
        
        // --------------------------------
        // GET /api/notes/user
        // --------------------------------
        [Fact]
        public async Task GetMyNotes_ReturnsOk_WithNotes()
        {
            SetUser(1);

            var notes = new List<NoteDto>
            {
                new() { NoteId = 1, UserId = 1, Content = "Note A" },
                new() { NoteId = 2, UserId = 1, Content = "Note B" }
            };
            _repoMock.Setup(r => r.GetAllForUserAsync(1)).ReturnsAsync(notes);

            var result = await _controller.GetMyNotes();

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<NoteDto>>(ok.Value);
            Assert.Equal(2, ((List<NoteDto>)returned).Count);
        }
        
        [Fact]
        public async Task GetMyNotes_ThrowsUnauthorized_WhenUserMissing()
        {
            SetUser(authenticated: false);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.GetMyNotes());
        }
        
        // --------------------------------
        // GET /api/notes/movie/{tconst}
        // --------------------------------
        [Fact]
        public async Task GetMovieNotes_ReturnsOk_WithNotes()
        {
            SetUser(2);
            var tconst = "tt001";
            var notes = new List<NoteDto> { new() { NoteId = 1, Tconst = tconst, Content = "Movie note" } };
            _repoMock.Setup(r => r.GetAllForUserByMovieAsync(2, tconst)).ReturnsAsync(notes);

            var result = await _controller.GetMovieNotes(tconst);

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<NoteDto>>(ok.Value);
            Assert.Single(returned);
        }
    }
}
