using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;
using CITP.MovieApp.Domain.Entities;
using CITP.MovieApp.Infrastructure.Persistence;
using CITP.MovieApp.Infrastructure.Utils;

namespace CITP.MovieApp.Infrastructure.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Title> _dbSet;
        private readonly ISearchHistoryRepository _searchHistoryRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MovieRepository(
            AppDbContext context,
            ISearchHistoryRepository searchHistoryRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _dbSet = context.Set<Title>();
            _searchHistoryRepository = searchHistoryRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        // --------------------------------------------------------------------
        // GET ALL TITLES (simple)
        // --------------------------------------------------------------------

        public async Task<IEnumerable<TitleDto>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking()
                .Include(t => t.Ratings)
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
                    PosterUrl = t.Metadatas != null ? t.Metadatas.PosterUrl : null,
                    Genres = t.TitleGenres != null 
                        ? t.TitleGenres.Select(g => g.Genre.GenreName).ToArray()
                        : Array.Empty<string>(),
                    AverageRating = t.Ratings != null ? (double)t.Ratings.AverageRating : 0,
                    NumVotes = t.Ratings != null ? t.Ratings.NumVotes : 0

                })
                .ToListAsync();
        }

        // --------------------------------------------------------------------
        // GET TITLE BY ID (simple)
        // --------------------------------------------------------------------

        public async Task<TitleDto?> GetByIdAsync(string tconst)
        {
            var title = await _dbSet
                .AsNoTracking()
                .Where(t => t.Tconst == tconst)
                .Include(t => t.Ratings)
                .FirstOrDefaultAsync();

            if (title == null) return null;

            return new TitleDto
            {
                Tconst = title.Tconst,
                PrimaryTitle = title.PrimaryTitle,
                OriginalTitle = title.OriginalTitle ?? title.PrimaryTitle,
                TitleType = title.TitleType,
                IsAdult = title.IsAdult,
                StartYear = title.StartYear,
                EndYear = title.EndYear,
                RuntimeMinutes = title.RuntimeMinutes,
                PosterUrl = title.Metadatas?.PosterUrl,
                Genres = title.TitleGenres?.Select(g => g.Genre!.GenreName).ToArray(),
                AverageRating = title.Ratings != null ? (double)title.Ratings.AverageRating : 0,
                NumVotes = title.Ratings?.NumVotes ?? 0
            };
        }

        // --------------------------------------------------------------------
        // CAST & CREW
        // --------------------------------------------------------------------

        public async Task<IEnumerable<TitleCastCrewDto>> GetCastAndCrewAsync(string tconst)
        {
            var roles = await _context.Set<Role>()
                .AsNoTracking()
                .Where(r => r.Tconst == tconst)
                .Include(r => r.Person)
                .ToListAsync();

            return roles.Select(r => new TitleCastCrewDto
            {
                Nconst = r.Person!.Nconst,
                Name = r.Person.PrimaryName,
                Job = r.Job,
                CharacterName = r.CharacterName
            });
        }

        // --------------------------------------------------------------------
        // DETAILS — OPTION B (rewritten, robust, navigation-safe)
        // --------------------------------------------------------------------

        public async Task<FilmDetailsDto?> GetFilmDetailsAsync(string tconst)
        {
            var film = await _dbSet
                .Where(t => t.Tconst == tconst && (t.TitleType == "movie" || t.TitleType == "short"))
                .Include(t => t.Metadatas)
                .Include(t => t.Roles).ThenInclude(r => r.Person)
                .FirstOrDefaultAsync();

            if (film == null) return null;

            return new FilmDetailsDto
            {
                Tconst = film.Tconst,
                MovieTitle = film.PrimaryTitle,
                Plot = film.Metadatas?.Plot ?? "",
                PosterUrl = film.Metadatas?.PosterUrl ?? "",
                Language = film.Metadatas?.Language ?? "",
                RatedAge = film.Metadatas?.Rated ?? "",
                ReleaseDate = film.Metadatas?.Released ?? "",
                WriterNames = string.Join(", ", film.Roles!.Where(r => r.Job == "writer").Select(r => r.Person!.PrimaryName).Distinct()),
                Country = film.Metadatas?.Country ?? "",
                UserBookmark = await GetUserBookmarkDataAsync(tconst)
            };
        }

        public async Task<SeriesDetatailsDto?> GetSeriesDetailsAsync(string tconst)
        {
            var series = await _dbSet
                .Where(t => t.Tconst == tconst && EF.Functions.Like(t.TitleType!, "%series%"))
                .Include(t => t.Metadatas)
                .Include(t => t.Roles).ThenInclude(r => r.Person)
                .FirstOrDefaultAsync();

            if (series == null) return null;

            var seasonCount = await _context.Episodes
                .Where(e => e.ParentSeriesId == tconst && e.SeasonNumber > 0)
                .OrderByDescending(e => e.SeasonNumber)
                .Select(e => (int?)e.SeasonNumber)
                .FirstOrDefaultAsync() ?? 0;

            return new SeriesDetatailsDto
            {
                Tconst = series.Tconst,
                SeriesTitle = series.PrimaryTitle,
                Plot = series.Metadatas?.Plot ?? "",
                PosterUrl = series.Metadatas?.PosterUrl ?? "",
                Language = series.Metadatas?.Language ?? "",
                RatedAge = series.Metadatas?.Rated ?? "",
                ReleaseDate = series.Metadatas?.Released ?? "",
                Country = series.Metadatas?.Country ?? "",
                NumberOfSeasons = seasonCount,
                WriterNames = string.Join(", ", series.Roles!.Where(r => r.Job == "writer").Select(r => r.Person!.PrimaryName).Distinct()),
                UserBookmark = await GetUserBookmarkDataAsync(tconst)
            };
        }

        public async Task<EpisodeDetailsDto?> GetEpisodeDetailsAsync(string tconst)
        {
            var episode = await _context.Episodes
                .Where(e => e.Tconst == tconst)
                .Include(e => e.Title)!.ThenInclude(t => t!.Metadatas)
                .Include(e => e.ParentSeries)
                .Include(e => e.Title)!.ThenInclude(t => t!.Roles)!.ThenInclude(r => r.Person)
                .FirstOrDefaultAsync();

            if (episode == null) return null;

            return new EpisodeDetailsDto
            {
                Tconst = episode.Tconst,
                EpisodeTitle = episode.Title?.PrimaryTitle ?? "",
                SeasonNumber = episode.SeasonNumber,
                EpisodeNumber = episode.EpisodeNumber,
                Plot = episode.Title?.Metadatas?.Plot ?? "",
                PosterUrl = episode.Title?.Metadatas?.PosterUrl ?? "",
                ReleaseDate = episode.Title?.Metadatas?.Released ?? "",
                WriterNames = string.Join(", ", episode.Title!.Roles!.Where(r => r.Job == "writer").Select(r => r.Person!.PrimaryName)),
                ParentSeriesId = episode.ParentSeriesId!,
                ParentSeriesTitle = episode.ParentSeries?.PrimaryTitle ?? "",
                UserBookmark = await GetUserBookmarkDataAsync(tconst)
            };
        }

        // --------------------------------------------------------------------
        // TITLE DETAILS (main details endpoint)
        // --------------------------------------------------------------------

        public async Task<TitleDetailsDto?> GetTitleDetailsAsync(string tconst)
        {
            var title = await _dbSet
                .Where(t => t.Tconst == tconst)
                .Include(t => t.Metadatas)
                .Include(t => t.Roles)!.ThenInclude(r => r.Person)
                .Include(t => t.TitleGenres)!.ThenInclude(tg => tg.Genre)
                .Include(t => t.Ratings)
                .FirstOrDefaultAsync();

            if (title == null) return null;

            var dto = new TitleDetailsDto
            {
                Tconst = title.Tconst,
                TitleType = title.TitleType ?? "",
                OriginalTitle = title.OriginalTitle ?? title.PrimaryTitle,
                RatedAge = title.Metadatas?.Rated ?? "",
                Language = title.Metadatas?.Language ?? "",
                Country = title.Metadatas?.Country ?? "",
                Plot = title.Metadatas?.Plot ?? "",
                PosterUrl = title.Metadatas?.PosterUrl ?? "",
                Genres = title.TitleGenres!.Select(g => g.Genre!.GenreName).ToList(),
                IsAdult = title.IsAdult,
                WriterNames = string.Join(", ", title.Roles!.Where(r => r.Job == "writer").Select(r => r.Person!.PrimaryName)),
                UserBookmark = await GetUserBookmarkDataAsync(tconst)
            };

            // Movie
            if (TitleTypeHelper.IsMovie(title.TitleType))
            {
                dto.MovieTitle = title.PrimaryTitle;
                dto.ReleaseDate = title.Metadatas?.Released ?? "";
                dto.RuntimeMinutes = title.RuntimeMinutes;
            }
            // Series
            else if (TitleTypeHelper.IsSeries(title.TitleType))
            {
                dto.SeriesTitle = title.PrimaryTitle;
                dto.StartYear = title.StartYear;
                dto.EndYear = title.EndYear;
                dto.NumberOfSeasons =
                    await _context.Episodes
                        .Where(e => e.ParentSeriesId == tconst)
                        .OrderByDescending(e => e.SeasonNumber)
                        .Select(e => (int?)e.SeasonNumber)
                        .FirstOrDefaultAsync() ?? 0;
            }
            // Episode
            else if (TitleTypeHelper.IsEpisode(title.TitleType))
            {
                var ep = await _context.Episodes
                    .Where(e => e.Tconst == tconst)
                    .Include(e => e.ParentSeries)
                    .FirstOrDefaultAsync();

                dto.EpisodeTitle = title.PrimaryTitle;
                dto.ReleaseDate = title.Metadatas?.Released ?? "";
                dto.SeasonNumber = ep?.SeasonNumber;
                dto.EpisodeNumber = ep?.EpisodeNumber;
                dto.ParentSeriesId = ep?.ParentSeriesId ?? "";
                dto.ParentSeriesTitle = ep?.ParentSeries?.PrimaryTitle ?? "";
            }

            return dto;
        }

        // --------------------------------------------------------------------
        // PAGED LISTING WITH SORTING / FILTERING
        // --------------------------------------------------------------------

        public async Task<PagedResult<TitleDto>> ListPagedAsync(
            int page,
            int pageSize,
            string? type = null,
            string? genre = null,
            string? sort = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var q = _dbSet.AsNoTracking()
                          .Include(t => t.Ratings)
                          .Include(t => t.TitleGenres)!.ThenInclude(g => g.Genre)
                          .AsQueryable();

            // Type filter
            if (!string.IsNullOrWhiteSpace(type))
            {
                var tnorm = type.Trim().ToLower();
                if (tnorm == "movie" || tnorm == "movies")
                    q = q.Where(t => t.TitleType!.ToLower() == "movie");
                else if (tnorm == "series")
                    q = q.Where(t => EF.Functions.Like(t.TitleType!, "%series%"));
            }

            // Genre filter
            if (!string.IsNullOrWhiteSpace(genre))
            {
                var gl = genre.ToLower().Trim();
                q = q.Where(t => t.TitleGenres!.Any(g => g.Genre!.GenreName.ToLower() == gl));
            }

            // Sorting
            switch (sort?.Trim().ToLower())
            {
                case "top-rated":
                    q = q.Where(t => t.Ratings != null && t.Ratings.NumVotes >= 50)
                         .OrderByDescending(t => t.Ratings!.AverageRating)
                         .ThenBy(t => t.PrimaryTitle);
                    break;

                case "newest":
                    q = q.OrderByDescending(t => t.StartYear ?? 0);
                    break;

                case "oldest":
                    q = q.OrderBy(t => t.StartYear ?? 999999);
                    break;

                default:
                    q = q.OrderBy(t => t.PrimaryTitle);
                    break;
            }

            var total = await q.CountAsync();

            var items = await q.Skip((page - 1) * pageSize)
                               .Take(pageSize)
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
                                   PosterUrl = t.Metadatas != null ? t.Metadatas.PosterUrl : null,
                                   Genres = t.TitleGenres != null 
                                       ? t.TitleGenres.Select(g => g.Genre.GenreName).ToArray()
                                       : Array.Empty<string>(),
                                   AverageRating = t.Ratings != null ? (double)t.Ratings.AverageRating : 0,
                                   NumVotes = t.Ratings != null ? t.Ratings.NumVotes : 0

                               })
                               .ToArrayAsync();

            return new PagedResult<TitleDto>
            {
                Items = items,
                Total = total
            };
        }

        // --------------------------------------------------------------------
        // USER BOOKMARK HELPER
        // --------------------------------------------------------------------

        private async Task<UserBookmarkDto?> GetUserBookmarkDataAsync(string tconst)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
                return null;

            try
            {
                var uid = int.Parse(user.Claims
                    .First(c => c.Type == ClaimTypes.NameIdentifier).Value);

                var bookmark = await _context.Set<Bookmark>()
                    .Where(b => b.UserId == uid && b.Tconst == tconst)
                    .FirstOrDefaultAsync();

                var note = await _context.Set<Note>()
                    .Where(n => n.UserId == uid && n.Tconst == tconst)
                    .OrderByDescending(n => n.UpdatedAt ?? n.NotedAt)
                    .FirstOrDefaultAsync();

                var rating = await _context.Set<RatingHistory>()
                    .Where(r => r.UserId == uid && r.Tconst == tconst)
                    .OrderByDescending(r => r.RatedAt)
                    .FirstOrDefaultAsync();

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
                return null;
            }
        }
    }
}
