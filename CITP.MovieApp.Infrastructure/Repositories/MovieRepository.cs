using Microsoft.EntityFrameworkCore;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;
using CITP.MovieApp.Domain.Entities;
using CITP.MovieApp.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CITP.MovieApp.Infrastructure.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Title> _dbSet;
        private readonly ISearchHistoryRepository _searchHistoryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MovieRepository(AppDbContext context, ISearchHistoryRepository searchHistoryRepository, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _dbSet = _context.Set<Title>();
            _searchHistoryRepository = searchHistoryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<TitleDto>> GetAllAsync()
        {
            return await _dbSet
                .Select(t => new TitleDto
                {
                    Tconst = t.Tconst,
                    PrimaryTitle = t.PrimaryTitle,
                    OriginalTitle = t.OriginalTitle ?? t.PrimaryTitle,
                    TitleType = t.TitleType,
                    IsAdult = t.IsAdult,
                    StartYear = t.StartYear,
                    EndYear = t.EndYear,
                    RuntimeMinutes = t.RuntimeMinutes,
                    PosterUrl = t.Metadatas != null ? t.Metadatas.PosterUrl : null
                })
                .ToListAsync();
        }

        public async Task<TitleDto?> GetByIdAsync(string tconst)
        {
            var title = await _dbSet
                .Where(t => t.Tconst == tconst)
                .Select(t => new TitleDto
                {
                    Tconst = t.Tconst,
                    PrimaryTitle = t.PrimaryTitle,
                    // ✅ Same fallback for individual fetches
                    OriginalTitle = t.OriginalTitle ?? t.PrimaryTitle,
                    TitleType = t.TitleType,
                    IsAdult = t.IsAdult,
                    StartYear = t.StartYear,
                    EndYear = t.EndYear,
                    RuntimeMinutes = t.RuntimeMinutes
                })
                .FirstOrDefaultAsync();

            // Add to search history if user is authenticated and title exists
            if (title != null && IsUserAuthenticated())
            {
                try
                {
                    var userId = GetCurrentUserId();
                    await _searchHistoryRepository.AddSearchHistoryAsync(userId, tconst);
                }
                catch
                {
                    // Silently ignore search history errors - don't break the main functionality
                }
            }

            return title;
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

        private string? CleanCharacterName(string? characterName)
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

        private async Task<UserBookmarkDto?> GetUserBookmarkDataAsync(string tconst)
        {
            if (!IsUserAuthenticated())
                return null;

            try
            {
                var userId = GetCurrentUserId();

                // Get bookmark
                var bookmark = await _context.Set<Bookmark>()
                    .Where(b => b.UserId == userId && b.Tconst == tconst)
                    .FirstOrDefaultAsync();

                // Get note
                var note = await _context.Set<Note>()
                    .Where(n => n.UserId == userId && n.Tconst == tconst)
                    .OrderByDescending(n => n.UpdatedAt ?? n.NotedAt)
                    .FirstOrDefaultAsync();

                // Get rating
                var rating = await _context.Set<RatingHistory>()
                    .Where(r => r.UserId == userId && r.Tconst == tconst)
                    .OrderByDescending(r => r.RatedAt)
                    .FirstOrDefaultAsync();

                // Only return data if at least one of them exists
                if (bookmark == null && note == null && rating == null)
                    return null;

                return new UserBookmarkDto
                {
                    BookmarkId = bookmark?.BookmarkId,
                    IsBookmarked = bookmark != null,
                    Note = note?.Content,
                    Rating = rating?.Rating
                };
            }
            catch
            {
                // If anything goes wrong, just return null (don't break the main request)
                return null;
            }
        }

        public async Task<IEnumerable<TitleCastCrewDto>> GetCastAndCrewAsync(string tconst)
        {
            var roles = await _context.Set<Role>()
                .Where(r => r.Tconst == tconst)
                .Include(r => r.Person)
                .ToListAsync();

            return roles.Select(r => new TitleCastCrewDto
            {
                Nconst = r.Person!.Nconst,
                Name = r.Person.PrimaryName,
                Job = r.Job,
                CharacterName = CleanCharacterName(r.CharacterName)
            });
        }

        public async Task<SeriesDetatailsDto?> GetSeriesDetailsAsync(string tconst)
        {
            var series = await _dbSet
                .Where(t => t.Tconst == tconst && EF.Functions.Like(t.TitleType, "%Series%"))
                .Select(t => new SeriesDetatailsDto
                {
                    Tconst = t.Tconst,
                    SeriesTitle = t.PrimaryTitle,
                    NumberOfSeasons = _context.Episodes
                        .Where(e => e.ParentSeriesId == t.Tconst && e.SeasonNumber > 0)
                        .OrderByDescending(e => e.SeasonNumber)
                        .Select(e => (int?)e.SeasonNumber)
                        .FirstOrDefault() ?? 0,
                    Plot = t.Metadatas!.Plot ?? "",
                    PosterUrl = t.Metadatas.PosterUrl ?? "",
                    Language = t.Metadatas.Language ?? "",
                    RatedAge = t.Metadatas.Rated ?? "",
                    ReleaseDate = t.Metadatas.Released ?? "",
                    WriterNames = string.Join(", ", t.Roles!
                        .Where(r => r.Job == "writer")
                        .Select(r => r.Person!.PrimaryName)
                        .Distinct()),
                    Country = t.Metadatas.Country ?? ""
                })
                .FirstOrDefaultAsync();

            if (series != null)
            {
                series.UserBookmark = await GetUserBookmarkDataAsync(tconst);
            }

            return series;
        }

        public async Task<EpisodeDetailsDto?> GetEpisodeDetailsAsync(string tconst)
        {
            var episode = await _context.Episodes
                .Where(e => e.Tconst == tconst)
                .Select(e => new EpisodeDetailsDto
                {
                    Tconst = e.Tconst,
                    EpisodeTitle = e.Title!.PrimaryTitle,
                    SeasonNumber = e.SeasonNumber,
                    EpisodeNumber = e.EpisodeNumber,
                    Plot = e.Title!.Metadatas!.Plot ?? "",
                    PosterUrl = e.Title.Metadatas.PosterUrl ?? "",
                    ReleaseDate = e.Title.Metadatas.Released ?? "",
                    WriterNames = string.Join(", ", e.Title.Roles!
                        .Where(r => r.Job == "writer")
                        .Select(r => r.Person!.PrimaryName)
                        .Distinct()),
                    ParentSeriesId = e.ParentSeriesId,
                    ParentSeriesTitle = _context.Titles
                        .Where(t => t.Tconst == e.ParentSeriesId)
                        .Select(t => t.PrimaryTitle)
                        .FirstOrDefault() ?? ""
                })
                .FirstOrDefaultAsync();

            if (episode != null)
            {
                episode.UserBookmark = await GetUserBookmarkDataAsync(tconst);
            }

            return episode;
        }

        public async Task<FilmDetailsDto?> GetFilmDetailsAsync(string tconst)
        {
            var film = await _dbSet
                .Where(t => t.Tconst == tconst && (t.TitleType == "movie" || t.TitleType == "short"))
                .Select(t => new FilmDetailsDto
                {
                    Tconst = t.Tconst,
                    MovieTitle = t.PrimaryTitle,
                    Plot = t.Metadatas!.Plot ?? "",
                    PosterUrl = t.Metadatas.PosterUrl ?? "",
                    Language = t.Metadatas.Language ?? "",
                    RatedAge = t.Metadatas.Rated ?? "",
                    ReleaseDate = t.Metadatas.Released ?? "",
                    WriterNames = string.Join(", ", t.Roles!
                        .Where(r => r.Job == "writer")
                        .Select(r => r.Person!.PrimaryName)
                        .Distinct()),
                    Country = t.Metadatas.Country ?? ""
                })
                .FirstOrDefaultAsync();

            if (film != null)
            {
                film.UserBookmark = await GetUserBookmarkDataAsync(tconst);
            }

            return film;
        }
    }
}
