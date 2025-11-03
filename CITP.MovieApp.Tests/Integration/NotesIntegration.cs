using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Tests_.Integration
{
   
    /// Simple tests to make sure unauthenticated users 
    /// cannot access protected note endpoints. 
 
    public class NotesIntegrationTests : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;

        public NotesIntegrationTests(TestApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Post_Note_For_Movie_Returns_Unauthorized()
        {
            var dto = new NoteCreateDto { Content = "Test note" };
            var response = await _client.PostAsJsonAsync("/api/notes/movie/tt0000001", dto);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Get_User_Notes_Returns_Unauthorized()
        {
            var response = await _client.GetAsync("/api/notes/user");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}