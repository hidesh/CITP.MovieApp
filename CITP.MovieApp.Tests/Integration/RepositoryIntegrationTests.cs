using System;
using System.Linq;
using System.Threading.Tasks;
using CITP.MovieApp.Domain.Entities;
using CITP.MovieApp.Infrastructure.Persistence;
using CITP.MovieApp.Infrastructure.Repositories;
using CITP.MovieApp.Application.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace CITP.MovieApp.Tests_.Integration
{
    /// Simple integration tests for repository layer.
    /// - UserRepository: add & fetch (write + read)
    /// - MovieRepository: read existing titles
    /// - PersonRepository: read existing people
    /// Each test runs in a rollback transaction (safe for the database).
    
    public class RepositoryIntegrationTests : IClassFixture<TestApplicationFactory>
    {
        private readonly AppDbContext _db;
        private readonly UserRepository _userRepo;
        private readonly MovieRepository _movieRepo;
        private readonly PersonRepository _personRepo;

        public RepositoryIntegrationTests(TestApplicationFactory factory)
        {
            var scope = factory.Services.CreateScope();
            _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            var mockSearchHistoryRepo = new Mock<ISearchHistoryRepository>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

            _userRepo = new UserRepository(_db);
            _movieRepo = new MovieRepository(_db, mockSearchHistoryRepo.Object, mockHttpContextAccessor.Object);
            _personRepo = new PersonRepository(_db, mockHttpContextAccessor.Object);
        }

        // ----------------------------
        // USER REPOSITORY (WRITE + READ)
        // ----------------------------
        [Fact(DisplayName = "UserRepository can add and fetch user")]
        public async Task Add_And_Get_User()
        {
            // Use unique username/email to avoid key collisions
            var username = $"test_user_{Guid.NewGuid()}";
            var email = $"{Guid.NewGuid()}@example.com";

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = "hashed_pw",
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveAsync();

            var fetched = await _userRepo.GetByUsernameAsync(username);

            Assert.NotNull(fetched);
            Assert.Equal(email, fetched!.Email);
        }

        // ----------------------------
        // MOVIE REPOSITORY (READ ONLY)
        // ----------------------------
        [Fact(DisplayName = "MovieRepository can read movies from database")]
        public async Task Get_All_Movies()
        {
            var movies = await _movieRepo.GetAllAsync();

            Assert.NotNull(movies);
            Assert.True(movies.Any(), "Expected at least one movie in database.");
        }

        [Fact(DisplayName = "MovieRepository can fetch a movie by ID")]
        public async Task Get_Movie_By_Id()
        {
            var movies = await _movieRepo.GetAllAsync();
            var first = movies.FirstOrDefault();

            if (first != null)
            {
                var fetched = await _movieRepo.GetByIdAsync(first.Tconst);
                Assert.NotNull(fetched);
                Assert.Equal(first.Tconst, fetched!.Tconst);
            }
            else
            {
                // Skip test if no data
                Assert.True(true, "No movie data to test against.");
            }
        }

        // ----------------------------
        // PERSON REPOSITORY (READ ONLY)
        // ----------------------------
        [Fact(DisplayName = "PersonRepository can read people from database")]
        public async Task Get_All_People()
        {
            var people = await _personRepo.GetAllAsync();

            Assert.NotNull(people);
            Assert.True(people.Any(), "Expected at least one person in database.");
        }

        [Fact(DisplayName = "PersonRepository can fetch a person by ID")]
        public async Task Get_Person_By_Id()
        {
            var allPeople = await _personRepo.GetAllAsync();
            var first = allPeople.FirstOrDefault();

            if (first != null)
            {
                var fetched = await _personRepo.GetByIdAsync(first.Nconst);
                Assert.NotNull(fetched);
                Assert.Equal(first.Nconst, fetched!.Nconst);
            }
            else
            {
                Assert.True(true, "No person data to test against.");
            }
        }
    }
}
