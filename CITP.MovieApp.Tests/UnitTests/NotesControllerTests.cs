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
        
        // --------------------------------
        // GET /api/notes/person/{nconst}
        // --------------------------------
        [Fact]
        public async Task GetPersonNotes_ReturnsOk_WithNotes()
        {
            SetUser(3);
            var nconst = "nm123";
            var notes = new List<NoteDto> { new() { NoteId = 5, Nconst = nconst, Content = "Actor note" } };
            _repoMock.Setup(r => r.GetAllForUserByPersonAsync(3, nconst)).ReturnsAsync(notes);

            var result = await _controller.GetPersonNotes(nconst);

            var ok = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsAssignableFrom<IEnumerable<NoteDto>>(ok.Value);
            Assert.Single(returned);
        }
        
        // --------------------------------
        // POST /api/notes/movie/{tconst}
        // --------------------------------
        [Fact]
        public async Task CreateForMovie_ReturnsCreated_WhenValid()
        {
            SetUser(10);
            var dto = new NoteCreateDto { Content = "Good movie!" };
            _repoMock.Setup(r => r.CreateForMovieAsync(10, "tt999", dto)).ReturnsAsync(123);

            var result = await _controller.CreateForMovie("tt999", dto);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            var json = JObject.FromObject(created.Value!);
            Assert.Equal(123, (int)json["id"]!);
        }
        
        [Fact]
        public async Task CreateForMovie_ReturnsBadRequest_WhenEmptyContent()
        {
            SetUser(10);
            var dto = new NoteCreateDto { Content = " " };

            var result = await _controller.CreateForMovie("tt999", dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var json = JObject.FromObject(bad.Value!);
            Assert.Equal("Content is required.", (string)json["message"]!);
        }
        
        // --------------------------------
        // POST /api/notes/person/{nconst}
        // --------------------------------
        [Fact]
        public async Task CreateForPerson_ReturnsCreated_WhenValid()
        {
            SetUser(7);
            var dto = new NoteCreateDto { Content = "Cool actor!" };
            _repoMock.Setup(r => r.CreateForPersonAsync(7, "nm777", dto)).ReturnsAsync(321);

            var result = await _controller.CreateForPerson("nm777", dto);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            var json = JObject.FromObject(created.Value!);
            Assert.Equal(321, (int)json["id"]!);
        }
        
        [Fact]
        public async Task CreateForPerson_ReturnsBadRequest_WhenContentMissing()
        {
            SetUser(7);
            var dto = new NoteCreateDto { Content = "" };

            var result = await _controller.CreateForPerson("nm777", dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var json = JObject.FromObject(bad.Value!);
            Assert.Equal("Content is required.", (string)json["message"]!);
        }
        
        // --------------------------------
        // PUT /api/notes/{id}
        // --------------------------------
        [Fact]
        public async Task Update_ReturnsNoContent_WhenSuccessful()
        {
            SetUser(5);
            var dto = new NoteUpdateDto { Content = "Updated note!" };
            _repoMock.Setup(r => r.UpdateAsync(42, 5, dto)).ReturnsAsync(true);

            var result = await _controller.Update(42, dto);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsNotFound_WhenRepositoryReturnsFalse()
        {
            SetUser(5);
            var dto = new NoteUpdateDto { Content = "Updated note!" };
            _repoMock.Setup(r => r.UpdateAsync(99, 5, dto)).ReturnsAsync(false);

            var result = await _controller.Update(99, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsBadRequest_WhenContentEmpty()
        {
            SetUser(5);
            var dto = new NoteUpdateDto { Content = "" };

            var result = await _controller.Update(42, dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var json = JObject.FromObject(bad.Value!);
            Assert.Equal("Content is required.", (string)json["message"]!);
        }
        
        // --------------------------------
        // DELETE /api/notes/{id}
        // --------------------------------
        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSuccessful()
        {
            SetUser(4);
            _repoMock.Setup(r => r.DeleteAsync(55, 4)).ReturnsAsync(true);

            var result = await _controller.Delete(55);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenRepositoryReturnsFalse()
        {
            SetUser(4);
            _repoMock.Setup(r => r.DeleteAsync(77, 4)).ReturnsAsync(false);

            var result = await _controller.Delete(77);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
