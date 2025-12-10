using Microsoft.EntityFrameworkCore;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;
using CITP.MovieApp.Domain.Entities;
using CITP.MovieApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CITP.MovieApp.Infrastructure.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Person> _dbSet;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PersonRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _dbSet = _context.Set<Person>();
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<PersonDto>> GetAllAsync()
        {
            return await _dbSet
                .Select(p => new PersonDto
                {
                    Nconst = p.Nconst,
                    PrimaryName = p.PrimaryName,
                    BirthYear = p.BirthYear,
                    DeathYear = p.DeathYear,
                    PrimaryProfession = p.PrimaryProfession
                })
                .ToListAsync();
        }

        public async Task<PersonDto?> GetByIdAsync(string nconst)
        {
            var person = await _dbSet
                .Where(p => p.Nconst == nconst)
                .Select(p => new PersonDto
                {
                    Nconst = p.Nconst,
                    PrimaryName = p.PrimaryName,
                    BirthYear = p.BirthYear,
                    DeathYear = p.DeathYear,
                    PrimaryProfession = p.PrimaryProfession
                })
                .FirstOrDefaultAsync();

            if (person != null)
            {
                person.UserBookmark = await GetUserPersonBookmarkDataAsync(nconst);
            }

            return person;
        }

        public async Task<IEnumerable<PersonFilmographyDto>> GetFilmographyAsync(string nconst)
        {
            var roles = await _context.Set<Role>()
                .Where(r => r.Nconst == nconst)
                .Include(r => r.Title)
                .ThenInclude(t => t!.Metadatas)
                .ToListAsync();

            return roles
                .GroupBy(r => r.Tconst)
                .Select(g => new PersonFilmographyDto
                {
                    Tconst = g.Key,
                    Title = g.First().Title!.PrimaryTitle,
                    Jobs = g.Where(r => !string.IsNullOrEmpty(r.Job))
                           .Select(r => r.Job!)
                           .Distinct()
                           .ToList(),
                    Characters = g.Where(r => !string.IsNullOrEmpty(r.CharacterName))
                                 .Select(r => CleanCharacterName(r.CharacterName!))
                                 .Distinct()
                                 .ToList(),
                    StartYear = g.First().Title!.StartYear,
                    PosterUrl = g.First().Title!.Metadatas?.PosterUrl
                })
                .ToList();
        }

        private string CleanCharacterName(string characterName)
        {
            if (string.IsNullOrEmpty(characterName))
                return characterName;

            // Remove Python-style list formatting: ['name'] -> name
            var cleaned = characterName.Trim();
            
            // Check if it starts with [' and ends with ']
            if (cleaned.StartsWith("['") && cleaned.EndsWith("']"))
            {
                // Remove [' from start and '] from end
                cleaned = cleaned.Substring(2, cleaned.Length - 4);
            }
            
            return cleaned;
        }

        private bool IsUserAuthenticated()
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User ID claim not found");

            return int.Parse(userIdClaim.Value);
        }

        private async Task<UserPersonBookmarkDto?> GetUserPersonBookmarkDataAsync(string nconst)
        {
            if (!IsUserAuthenticated())
                return null;

            try
            {
                var userId = GetCurrentUserId();

                // Get bookmark
                var bookmark = await _context.Set<Bookmark>()
                    .Where(b => b.UserId == userId && b.Nconst == nconst)
                    .FirstOrDefaultAsync();

                // Get note
                var note = await _context.Set<Note>()
                    .Where(n => n.UserId == userId && n.Nconst == nconst)
                    .OrderByDescending(n => n.UpdatedAt ?? n.NotedAt)
                    .FirstOrDefaultAsync();

                // Only return data if at least one of them exists
                if (bookmark == null && note == null)
                    return null;

                return new UserPersonBookmarkDto
                {
                    BookmarkId = bookmark?.BookmarkId,
                    IsBookmarked = bookmark != null,
                    Note = note?.Content
                };
            }
            catch
            {
                // If anything goes wrong, just return null (don't break the main request)
                return null;
            }
        }
    }
}
