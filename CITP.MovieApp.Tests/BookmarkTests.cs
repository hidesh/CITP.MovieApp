using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CITP.MovieApp.Tests
{
    public class BookmarkTests
    {
        private const string BookmarksApi = "http://localhost:5079/api/bookmarks";
        private const string AuthApi = "http://localhost:5079/api/auth";

        // for testing purposes, create test user manually via /api/auth/register
        //  Username = "testuser",
        //  Email = "testuser@test.com",
        //  Password = "test12345"

        /* /api/bookmarks */

        [Fact]
        public void ApiBookmarks_GetWithAuthentication_OkAndBookmarks()
        {
            var token = GetAuthToken();
            var (data, statusCode) = GetArrayWithAuth(BookmarksApi, token);

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.True(data.Count >= 0);
            
            if (data.Count > 0)
            {
                Assert.NotNull(data.First()["bookmarkId"]);
                Assert.NotNull(data.First()["userId"]);
                Assert.NotNull(data.First()["bookmarkedAt"]);
            }
        }

        [Fact]
        public void ApiBookmarks_GetWithoutAuthentication_Unauthorized()
        {
            var (_, statusCode) = GetArray(BookmarksApi);
            Assert.Equal(HttpStatusCode.Unauthorized, statusCode);
        }

        [Fact]
        public void ApiBookmarks_PostMovieBookmark_Created()
        {
            var token = GetAuthToken();
            var userId = GetCurrentUserId(token);
            
            // First get a valid movie ID from the movies endpoint
            var (moviesData, moviesStatus) = GetArrayWithAuth("http://localhost:5079/api/movies?page=1&pageSize=1", token);
            if (moviesStatus != HttpStatusCode.OK || moviesData.Count == 0)
            {
                // If no movies available, we can't test movie bookmarks
                Assert.Fail("No movies available in database for testing");
                return;
            }
            
            var validTconst = moviesData[0]["tconst"]?.ToString();
            
            var newBookmark = new
            {
                UserId = userId,
                Tconst = validTconst,
                Nconst = (string?)null
            };
            
            var (bookmark, statusCode) = PostDataWithAuth(BookmarksApi, newBookmark, token);
            
            // If we get a 500 error, log the error message for debugging
            if (statusCode == HttpStatusCode.InternalServerError && bookmark["error"] != null)
            {
                throw new Exception($"Server error: {bookmark["error"]}");
            }
            
            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(validTconst, bookmark["tconst"]);
            
            // Cleanup
            if (bookmark["bookmarkId"] != null)
                DeleteDataWithAuth($"{BookmarksApi}/{bookmark["bookmarkId"]}", token);
        }

        [Fact]
        public void ApiBookmarks_PostPersonBookmark_Created()
        {
            var token = GetAuthToken();
            var userId = GetCurrentUserId(token);
            
            // First get a valid person ID from the person endpoint  
            var (personsData, personsStatus) = GetArrayWithAuth("http://localhost:5079/api/person?page=1&pageSize=1", token);
            if (personsStatus != HttpStatusCode.OK || personsData.Count == 0)
            {
                Assert.Fail("No persons available in database for testing");
                return;
            }
            
            var validNconst = personsData[0]["nconst"]?.ToString();
            
            var newBookmark = new
            {
                UserId = userId,
                Tconst = (string?)null,
                Nconst = validNconst
            };
            
            var (bookmark, statusCode) = PostDataWithAuth(BookmarksApi, newBookmark, token);
            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.Equal(validNconst, bookmark["nconst"]);
            
            // Cleanup
            if (bookmark["bookmarkId"] != null)
                DeleteDataWithAuth($"{BookmarksApi}/{bookmark["bookmarkId"]}", token);
        }

        [Fact]
        public void ApiBookmarks_DeleteValidBookmark_NoContent()
        {
            var token = GetAuthToken();
            var userId = GetCurrentUserId(token);
            
            // Get a valid movie ID from the movies endpoint
            var (moviesData, moviesStatus) = GetArrayWithAuth("http://localhost:5079/api/movies?page=1&pageSize=1", token);
            if (moviesStatus != HttpStatusCode.OK || moviesData.Count == 0)
            {
                Assert.Fail("No movies available in database for testing");
                return;
            }
            
            var validTconst = moviesData[0]["tconst"]?.ToString();
            
            // Create a bookmark to delete
            var newBookmark = new
            {
                UserId = userId,
                Tconst = validTconst,
                Nconst = (string?)null
            };
            
            var (bookmark, createStatusCode) = PostDataWithAuth(BookmarksApi, newBookmark, token);
            
            // Only proceed if bookmark creation succeeded
            Assert.Equal(HttpStatusCode.OK, createStatusCode);
            Assert.NotNull(bookmark["bookmarkId"]);
            
            var bookmarkId = bookmark["bookmarkId"];
            
            // Delete it
            var statusCode = DeleteDataWithAuth($"{BookmarksApi}/{bookmarkId}", token);
            Assert.Equal(HttpStatusCode.NoContent, statusCode);
        }

        [Fact]
        public void ApiBookmarks_DeleteInvalidBookmark_NotFound()
        {
            var token = GetAuthToken();
            var statusCode = DeleteDataWithAuth($"{BookmarksApi}/99999", token);
            Assert.Equal(HttpStatusCode.NotFound, statusCode);
        }

        [Fact]
        public void ApiBookmarks_GetWithPagination_CorrectPageSize()
        {
            var token = GetAuthToken();
            var (data, statusCode) = GetArrayWithAuth($"{BookmarksApi}?page=1&pageSize=5", token);

            Assert.Equal(HttpStatusCode.OK, statusCode);
            Assert.True(data.Count <= 5);
        }

        // Helper methods

        string GetAuthToken()
        {
            // Login with the test user (must be created manually first)
            var loginData = new
            {
                Username = "testuser",
                Password = "test12345"
            };
            
            var (response, statusCode) = PostData($"{AuthApi}/login", loginData);
            if (statusCode != HttpStatusCode.OK)
                throw new Exception("Failed to get auth token - ensure test user exists");
                
            return response["token"]?.ToString() ?? throw new Exception("No token received");
        }

        int GetCurrentUserId(string token)
        {
            // Decode JWT token to get user ID (simple base64 decode of payload)
            var parts = token.Split('.');
            if (parts.Length != 3)
                throw new Exception("Invalid JWT token format");
                
            // Decode the payload (second part)
            var payload = parts[1];
            // Add padding if needed
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }
            
            var payloadBytes = Convert.FromBase64String(payload);
            var payloadJson = Encoding.UTF8.GetString(payloadBytes);
            var payloadObject = JsonConvert.DeserializeObject<JObject>(payloadJson);
            
            // Look for nameid (which is ClaimTypes.NameIdentifier) or userId
            return int.Parse(payloadObject?["nameid"]?.ToString() ?? payloadObject?["userId"]?.ToString() ?? "0");
        }

        (JArray, HttpStatusCode) GetArray(string url)
        {
            var client = new HttpClient();
            var response = client.GetAsync(url).Result;
            var data = response.Content.ReadAsStringAsync().Result;
            
            if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(data))
            {
                var jsonResponse = JObject.Parse(data);
                var dataArray = (JArray?)jsonResponse["data"];
                return (dataArray ?? new JArray(), response.StatusCode);
            }
            
            return (new JArray(), response.StatusCode);
        }

        (JArray, HttpStatusCode) GetArrayWithAuth(string url, string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = client.GetAsync(url).Result;
            var data = response.Content.ReadAsStringAsync().Result;
            
            if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(data))
            {
                var jsonResponse = JObject.Parse(data);
                var dataArray = (JArray?)jsonResponse["data"];
                return (dataArray ?? new JArray(), response.StatusCode);
            }
            
            return (new JArray(), response.StatusCode);
        }

        (JObject, HttpStatusCode) GetObject(string url)
        {
            var client = new HttpClient();
            var response = client.GetAsync(url).Result;
            var data = response.Content.ReadAsStringAsync().Result;
            return ((JObject?)JsonConvert.DeserializeObject(data) ?? new JObject(), response.StatusCode);
        }

        (JObject, HttpStatusCode) PostData(string url, object content)
        {
            var client = new HttpClient();
            var requestContent = new StringContent(
                JsonConvert.SerializeObject(content),
                Encoding.UTF8,
                "application/json");
            var response = client.PostAsync(url, requestContent).Result;
            var data = response.Content.ReadAsStringAsync().Result;
            
            // Handle non-JSON responses gracefully
            if (string.IsNullOrEmpty(data) || !response.IsSuccessStatusCode)
            {
                return (new JObject { ["error"] = data }, response.StatusCode);
            }
            
            try
            {
                return ((JObject?)JsonConvert.DeserializeObject(data) ?? new JObject(), response.StatusCode);
            }
            catch (JsonReaderException)
            {
                return (new JObject { ["error"] = data }, response.StatusCode);
            }
        }

        (JObject, HttpStatusCode) PostDataWithAuth(string url, object content, string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var requestContent = new StringContent(
                JsonConvert.SerializeObject(content),
                Encoding.UTF8,
                "application/json");
            var response = client.PostAsync(url, requestContent).Result;
            var data = response.Content.ReadAsStringAsync().Result;
            
            // Handle non-JSON responses gracefully
            if (string.IsNullOrEmpty(data) || !response.IsSuccessStatusCode)
            {
                return (new JObject { ["error"] = data }, response.StatusCode);
            }
            
            try
            {
                return ((JObject?)JsonConvert.DeserializeObject(data) ?? new JObject(), response.StatusCode);
            }
            catch (JsonReaderException)
            {
                return (new JObject { ["error"] = data }, response.StatusCode);
            }
        }

        HttpStatusCode DeleteDataWithAuth(string url, string token)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var response = client.DeleteAsync(url).Result;
            return response.StatusCode;
        }
    }
}