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
                    RuntimeMinutes = t.RuntimeMinutes
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

        public async Task<IEnumerable<TitleCastCrewDto>> GetCastAndCrewAsync(string tconst)
        {
            return await _context.Set<Role>()
                .Where(r => r.Tconst == tconst)
                .Include(r => r.Person)
                .Select(r => new TitleCastCrewDto
                {
                    Nconst = r.Person!.Nconst,
                    Name = r.Person.PrimaryName,
                    Job = r.Job,
                    CharacterName = r.CharacterName
                })
                .ToListAsync();
        }

        public async Task<SeriesDetatailsDto?> GetSeriesDetailsAsync(string tconst)
        {
            var series = await _dbSet
                .Where(t => t.Tconst == tconst && EF.Functions.Like(t.TitleType, "%Series%"))
                .Select(t => new SeriesDetatailsDto
                {
                    Tconst = t.Tconst,
                    NumberOfSeasons = _context.Episodes
                        .Where(e => e.ParentSeriesId == t.Tconst && e.SeasonNumber > 0)
                        .OrderByDescending(e => e.SeasonNumber)
                        .Select(e => (int?)e.SeasonNumber)
                        .FirstOrDefault() ?? 0,
                    Plot = t.Metadatas!.Plot,
                    PosterUrl = t.Metadatas.PosterUrl,
                    Language = t.Metadatas.Language,
                    RatedAge = t.Metadatas.Rated,
                    ReleaseDate = t.Metadatas.Released,
                    WriterNames = string.Join(", ", t.Roles!
                        .Where(r => r.Job == "writer")
                        .Select(r => r.Person!.PrimaryName)
                        .Distinct()),
                    Country = t.Metadatas.Country
                })
                .FirstOrDefaultAsync();

            return series;
        }
    }
}
