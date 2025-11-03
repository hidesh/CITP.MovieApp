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
    public class BookmarkControllerTests
    {
        private readonly Mock<IBookmarkRepository> _repoMock;
        private readonly BookmarksController _controller;

        public BookmarkControllerTests()
        {
            _repoMock = new Mock<IBookmarkRepository>();
            _controller = new BookmarksController(_repoMock.Object);
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
        // GET /api/bookmarks
        // --------------------------------
        [Fact]
        public async Task Get_ReturnsOk_WithPagedBookmarks()
        {
            SetUser(1);
            var bookmarks = Enumerable.Range(1, 30)
                .Select(i => new BookmarkDto
                {
                    BookmarkId = i,
                    UserId = 1,
                    Tconst = $"tt{i:D4}",
                    BookmarkedAt = DateTime.UtcNow
                }).ToList();

            _repoMock.Setup(r => r.GetAllAsync(1)).ReturnsAsync(bookmarks);

            var result = await _controller.Get(page: 2, pageSize: 10);

            var ok = Assert.IsType<OkObjectResult>(result);
            var json = JObject.FromObject(ok.Value!);

            Assert.Equal(30, (int)json["total"]!);
            Assert.Equal(2, (int)json["page"]!);
            Assert.Equal(10, (int)json["pageSize"]!);

            var data = json["data"]!.ToObject<List<BookmarkDto>>();
            Assert.Equal(10, data!.Count);
            Assert.Equal("tt0011", data.First().Tconst);
        }


        [Fact]
        public async Task Get_ThrowsUnauthorized_WhenNoUser()
        {
            SetUser(authenticated: false);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.Get());
        }

        // --------------------------------
        // GET /api/bookmarks/{id}
        // --------------------------------
        [Fact]
        public async Task GetById_ReturnsOk_WhenBookmarkExists()
        {
            SetUser(1);
            var dto = new BookmarkDto { BookmarkId = 99, UserId = 1, Tconst = "tt999" };
            _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync(dto);

            var result = await _controller.GetById(99);

            var ok = Assert.IsType<OkObjectResult>(result);
            var bookmark = Assert.IsAssignableFrom<BookmarkDto>(ok.Value);
            Assert.Equal(99, bookmark.BookmarkId);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenBookmarkMissing()
        {
            SetUser(1);
            _repoMock.Setup(r => r.GetByIdAsync(50)).ReturnsAsync((BookmarkDto?)null);

            var result = await _controller.GetById(50);

            Assert.IsType<NotFoundResult>(result);
        }

        // --------------------------------
        // POST /api/bookmarks
        // --------------------------------
        [Fact]
        public async Task Add_ReturnsOk_WhenValid()
        {
            SetUser(2);
            var dto = new CreateBookmarkDto { UserId = 2, Tconst = "tt777" };

            var newBookmark = new BookmarkDto
            {
                BookmarkId = 1,
                UserId = 2,
                Tconst = "tt777",
                BookmarkedAt = DateTime.UtcNow
            };

            _repoMock.Setup(r => r.AddBookmarkAsync(2, "tt777", null))
                .ReturnsAsync(newBookmark);

            var result = await _controller.Add(dto);

            var ok = Assert.IsType<OkObjectResult>(result);
            var bookmark = Assert.IsAssignableFrom<BookmarkDto>(ok.Value);
            Assert.Equal(2, bookmark.UserId);
            Assert.Equal("tt777", bookmark.Tconst);
        }

        [Fact]
        public async Task Add_ReturnsBadRequest_WhenUserIdMismatch()
        {
            SetUser(5);
            var dto = new CreateBookmarkDto { UserId = 10, Tconst = "tt555" };

            var result = await _controller.Add(dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var json = JObject.FromObject(bad.Value!);
            Assert.Equal("User ID mismatch", (string)json["message"]!);
        }

        // --------------------------------
        // DELETE /api/bookmarks/{id}
        // --------------------------------
        [Fact]
        public async Task Delete_ReturnsNoContent_WhenSuccessful()
        {
            SetUser(3);
            var bookmark = new BookmarkDto { BookmarkId = 7, UserId = 3, Tconst = "tt007" };
            _repoMock.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(bookmark);
            _repoMock.Setup(r => r.DeleteByIdAsync(7)).ReturnsAsync(true);

            var result = await _controller.Delete(7);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenBookmarkMissing()
        {
            SetUser(3);
            _repoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((BookmarkDto?)null);

            var result = await _controller.Delete(10);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Bookmark not found", notFound.Value);
        }

        [Fact]
        public async Task Delete_ReturnsBadRequest_WhenNotUserBookmark()
        {
            SetUser(4);
            var bookmark = new BookmarkDto { BookmarkId = 8, UserId = 9, Tconst = "tt008" };
            _repoMock.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(bookmark);

            var result = await _controller.Delete(8);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var json = JObject.FromObject(bad.Value!);
            Assert.Equal("You can only delete your own bookmarks", (string)json["message"]!);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenDeleteFails()
        {
            SetUser(3);
            var bookmark = new BookmarkDto { BookmarkId = 11, UserId = 3 };
            _repoMock.Setup(r => r.GetByIdAsync(11)).ReturnsAsync(bookmark);
            _repoMock.Setup(r => r.DeleteByIdAsync(11)).ReturnsAsync(false);

            var result = await _controller.Delete(11);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Bookmark not found", notFound.Value);
        }
    }
}
