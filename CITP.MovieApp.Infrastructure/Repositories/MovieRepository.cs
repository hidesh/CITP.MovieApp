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
        // GET ALL TITLES
        // --------------------------------------------------------------------
        public async Task<IEnumerable<TitleDto>> GetAllAsync()
        {
            return await _dbSet.AsNoTracking()
                .Include(t => t.Ratings)
                .Include(t => t.TitleGenres)!.ThenInclude(g => g.Genre)
                .Where(t => t.TitleType != "tvEpisode")
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
                    PosterUrl = t.Metadatas!.PosterUrl,
                    Genres = t.TitleGenres!.Select(g => g.Genre!.GenreName).ToArray(),
                    AverageRating = t.Ratings != null ? (double)t.Ratings.AverageRating : 0,
                    NumVotes = t.Ratings != null ? t.Ratings.NumVotes : 0
                })
                .ToListAsync();
        }

        // --------------------------------------------------------------------
        // GET TITLE BY ID (LIST DTO)
        // --------------------------------------------------------------------
        public async Task<TitleDto?> GetByIdAsync(string tconst)
        {
            var t = await _dbSet
                .AsNoTracking()
                .Include(x => x.Ratings)
                .Include(x => x.TitleGenres)!.ThenInclude(g => g.Genre)
                .FirstOrDefaultAsync(x => x.Tconst == tconst);

            if (t == null) return null;

            return new TitleDto
            {
                Tconst = t.Tconst,
                PrimaryTitle = t.PrimaryTitle,
                OriginalTitle = t.OriginalTitle ?? t.PrimaryTitle,
                TitleType = t.TitleType,
                IsAdult = t.IsAdult,
                StartYear = t.StartYear,
                EndYear = t.EndYear,
                RuntimeMinutes = t.RuntimeMinutes,
                PosterUrl = t.Metadatas?.PosterUrl,
                Genres = t.TitleGenres!.Select(g => g.Genre!.GenreName).ToArray(),
                AverageRating = t.Ratings != null ? (double)t.Ratings.AverageRating : 0,
                NumVotes = t.Ratings?.NumVotes ?? 0
            };
        }

        // --------------------------------------------------------------------
        // CAST & CREW
        // --------------------------------------------------------------------
        public async Task<IEnumerable<TitleCastCrewDto>> GetCastAndCrewAsync(string tconst)
        {
            return await _context.Set<Role>()
                .AsNoTracking()
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

        // --------------------------------------------------------------------
        // FILM DETAILS
        // --------------------------------------------------------------------
        public async Task<FilmDetailsDto?> GetFilmDetailsAsync(string tconst)
        {
            var film = await _dbSet
                .Include(t => t.Metadatas)
                .Include(t => t.Roles)!.ThenInclude(r => r.Person)
                .FirstOrDefaultAsync(t =>
                    t.Tconst == tconst &&
                    (t.TitleType == "movie" || t.TitleType == "short" || t.TitleType == "tvMovie"));

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
                Country = film.Metadatas?.Country ?? "",
                WriterNames = string.Join(", ",
                    film.Roles!.Where(r => r.Job == "writer")
                               .Select(r => r.Person!.PrimaryName)
                               .Distinct()),
                UserBookmark = await GetUserBookmarkDataAsync(tconst)
            };
        }

        // --------------------------------------------------------------------
        // SERIES DETAILS
        // --------------------------------------------------------------------
        public async Task<SeriesDetatailsDto?> GetSeriesDetailsAsync(string tconst)
        {
            var series = await _dbSet
                .Include(t => t.Metadatas)
                .Include(t => t.Roles)!.ThenInclude(r => r.Person)
                .FirstOrDefaultAsync(t =>
                    t.Tconst == tconst &&
                    (t.TitleType == "tvSeries" || t.TitleType == "tvMiniSeries"));

            if (series == null) return null;

            var seasons = await _context.Episodes
                .Where(e => e.ParentSeriesId == tconst && e.SeasonNumber > 0)
                .Select(e => e.SeasonNumber)
                .Distinct()
                .CountAsync();

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
                NumberOfSeasons = seasons,
                WriterNames = string.Join(", ",
                    series.Roles!.Where(r => r.Job == "writer")
                                 .Select(r => r.Person!.PrimaryName)
                                 .Distinct()),
                UserBookmark = await GetUserBookmarkDataAsync(tconst)
            };
        }

        // --------------------------------------------------------------------
        // EPISODE DETAILS
        // --------------------------------------------------------------------
        public async Task<EpisodeDetailsDto?> GetEpisodeDetailsAsync(string tconst)
        {
            var ep = await _context.Episodes
                .Include(e => e.Title)!.ThenInclude(t => t!.Metadatas)
                .Include(e => e.ParentSeries)
                .Include(e => e.Title)!.ThenInclude(t => t!.Roles)!.ThenInclude(r => r.Person)
                .FirstOrDefaultAsync(e => e.Tconst == tconst);

            if (ep == null) return null;

            return new EpisodeDetailsDto
            {
                Tconst = ep.Tconst,
                EpisodeTitle = ep.Title!.PrimaryTitle,
                SeasonNumber = ep.SeasonNumber,
                EpisodeNumber = ep.EpisodeNumber,
                Plot = ep.Title!.Metadatas?.Plot ?? "",
                PosterUrl = ep.Title!.Metadatas?.PosterUrl ?? "",
                ReleaseDate = ep.Title!.Metadatas?.Released ?? "",
                ParentSeriesId = ep.ParentSeriesId!,
                ParentSeriesTitle = ep.ParentSeries!.PrimaryTitle,
                WriterNames = string.Join(", ",
                    ep.Title!.Roles!.Where(r => r.Job == "writer")
                                     .Select(r => r.Person!.PrimaryName)),
                UserBookmark = await GetUserBookmarkDataAsync(tconst)
            };
        }
        
        // --------------------------------------------------------------------
// TITLE DETAILS (UNIFIED MOVIE / SERIES / EPISODE)
// --------------------------------------------------------------------
public async Task<TitleDetailsDto?> GetTitleDetailsAsync(string tconst)
{
    var title = await _dbSet
        .AsNoTracking()
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
        WriterNames = string.Join(", ",
            title.Roles!.Where(r => r.Job == "writer")
                        .Select(r => r.Person!.PrimaryName)),
        UserBookmark = await GetUserBookmarkDataAsync(tconst)
    };

    // -----------------------
    // Movie
    // -----------------------
    if (TitleTypeHelper.IsMovie(title.TitleType))
    {
        dto.MovieTitle = title.PrimaryTitle;
        dto.ReleaseDate = title.Metadatas?.Released ?? "";
        dto.RuntimeMinutes = title.RuntimeMinutes;
    }
    // -----------------------
    // Series
    // -----------------------
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
    // -----------------------
    // Episode
    // -----------------------
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
        // PAGED LISTING (FILTERED, FIXED)
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
                .Where(t => t.TitleType != "tvEpisode");

            if (!string.IsNullOrWhiteSpace(type))
            {
                var tnorm = type.ToLower();
                if (tnorm == "movie")
                    q = q.Where(t => t.TitleType == "movie" || t.TitleType == "tvMovie" || t.TitleType == "short");
                else if (tnorm == "series")
                    q = q.Where(t => t.TitleType == "tvSeries" || t.TitleType == "tvMiniSeries");
            }

            if (!string.IsNullOrWhiteSpace(genre))
            {
                var g = genre.ToLower();
                q = q.Where(t => t.TitleGenres!.Any(x => x.Genre!.GenreName.ToLower() == g));
            }

            q = sort switch
            {
                "top-rated" => q.Where(t => t.Ratings!.NumVotes >= 50)
                                .OrderByDescending(t => t.Ratings!.AverageRating),
                "newest" => q.OrderByDescending(t => t.StartYear),
                "oldest" => q.OrderBy(t => t.StartYear),
                _ => q.OrderBy(t => t.PrimaryTitle)
            };

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
                    PosterUrl = t.Metadatas!.PosterUrl,
                    Genres = t.TitleGenres!.Select(g => g.Genre!.GenreName).ToArray(),
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
// EPISODES FOR SERIES
// --------------------------------------------------------------------
        public async Task<IEnumerable<EpisodeDetailsDto>> GetEpisodesForSeriesAsync(string seriesTconst)
        {
            return await _context.Episodes
                .AsNoTracking()
                .Where(e => e.ParentSeriesId == seriesTconst)
                .Include(e => e.Title)!.ThenInclude(t => t!.Metadatas)
                .OrderBy(e => e.SeasonNumber)
                .ThenBy(e => e.EpisodeNumber)
                .Select(e => new EpisodeDetailsDto
                {
                    Tconst = e.Tconst,
                    EpisodeTitle = e.Title!.PrimaryTitle,
                    SeasonNumber = e.SeasonNumber,
                    EpisodeNumber = e.EpisodeNumber,
                    Plot = e.Title!.Metadatas!.Plot ?? "",
                    PosterUrl = e.Title!.Metadatas!.PosterUrl ?? "",
                    ReleaseDate = e.Title!.Metadatas!.Released ?? "",
                    ParentSeriesId = e.ParentSeriesId!,
                    ParentSeriesTitle = "",
                    WriterNames = ""
                })
                .ToListAsync();
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
                var uid = int.Parse(user.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);

                var bookmark = await _context.Set<Bookmark>()
                    .FirstOrDefaultAsync(b => b.UserId == uid && b.Tconst == tconst);

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
                    NoteId = note?.NoteId,
                    Note = note?.Content,
                    RatingId = rating?.RatingId,
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
