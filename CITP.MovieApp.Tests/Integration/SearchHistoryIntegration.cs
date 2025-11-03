using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Tests_.Integration
{
    public class SearchHistoryIntegrationTests : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;

        public SearchHistoryIntegrationTests(TestApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        // --------------------------------
        // GET /api/searchhistory
        // --------------------------------
        [Fact(DisplayName = "GET /api/searchhistory requires authentication")]
        public async Task Get_SearchHistory_Returns_Unauthorized()
        {
            var response = await _client.GetAsync("/api/searchhistory");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // --------------------------------
        // POST /api/searchhistory
        // --------------------------------
        [Fact(DisplayName = "POST /api/searchhistory requires authentication")]
        public async Task Post_SearchHistory_Returns_Unauthorized()
        {
            var dto = new CreateSearchHistoryDto { UserId = 1, Tconst = "tt0000001" };
            var response = await _client.PostAsJsonAsync("/api/searchhistory", dto);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}