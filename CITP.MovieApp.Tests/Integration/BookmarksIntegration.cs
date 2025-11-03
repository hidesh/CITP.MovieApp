using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Tests_.Integration
{
    /// Simple integration tests for the Bookmarks API endpoints.
    /// Verifies that protected endpoints return Unauthorized for unauthenticated users.
    /// Uses TestApplicationFactory with rollback transaction.

    public class BookmarksIntegrationTests : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;

        public BookmarksIntegrationTests(TestApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        // --------------------------------
        // GET /api/bookmarks
        // --------------------------------
        [Fact(DisplayName = "GET /api/bookmarks requires authentication")]
        public async Task Get_Bookmarks_Returns_Unauthorized()
        {
            var response = await _client.GetAsync("/api/bookmarks");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // --------------------------------
        // GET /api/bookmarks/{id}
        // --------------------------------
        [Fact(DisplayName = "GET /api/bookmarks/{id} requires authentication")]
        public async Task Get_Bookmark_By_Id_Returns_Unauthorized()
        {
            var response = await _client.GetAsync("/api/bookmarks/1");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // --------------------------------
        // POST /api/bookmarks
        // --------------------------------
        [Fact(DisplayName = "POST /api/bookmarks requires authentication")]
        public async Task Post_Bookmark_Returns_Unauthorized()
        {
            var dto = new CreateBookmarkDto { UserId = 1, Tconst = "tt0000001" };
            var response = await _client.PostAsJsonAsync("/api/bookmarks", dto);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // --------------------------------
        // DELETE /api/bookmarks/{id}
        // --------------------------------
        [Fact(DisplayName = "DELETE /api/bookmarks/{id} requires authentication")]
        public async Task Delete_Bookmark_Returns_Unauthorized()
        {
            var response = await _client.DeleteAsync("/api/bookmarks/1");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
