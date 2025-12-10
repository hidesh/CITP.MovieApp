using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Api.Utils;
using System.Security.Claims;

namespace CITP.MovieApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // Allows public access
    public class MoviesController(IMovieRepository repo) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get(int page = 1, int pageSize = 20)
        {
            if (pageSize <= 0 || pageSize > 200) pageSize = 20;

            var all = await repo.GetAllAsync();
            var total = all.Count();
            var items = all.Skip((page - 1) * pageSize).Take(pageSize);

            string? prev = LinkBuilder.PageLink(Request, page - 1, pageSize, total);
            string? next = LinkBuilder.PageLink(Request, page + 1, pageSize, total);

            return Ok(new
            {
                total,
                page,
                pageSize,
                prev,
                next,
                data = items
            });
        }

        /// <summary>
        /// Get a specific title by its unique identifier (tconst)
        /// </summary>
        /// <param name="tconst">The unique title identifier (e.g., tt0000001)</param>
        /// <returns>Returns detailed information about the title based on its type</returns>
        /// <remarks>
        /// This endpoint intelligently returns different information based on the title type:
        /// 
        /// **For TV Series** (titleType contains "Series"):
        /// - tconst: Title identifier
        /// - seriesTitle: Name of the series
        /// - numberOfSeasons: Total number of seasons
        /// - plot: Series plot summary
        /// - posterUrl: Series poster image URL
        /// - ratedAge: Age rating (e.g., "PG-13", "R")
        /// - language: Primary language
        /// - releaseDate: Original release date
        /// - writerNames: Comma-separated list of writers
        /// - country: Country of origin
        /// - userBookmark: (authenticated users only) Object containing bookmarkId, isBookmarked, note, and rating
        /// 
        /// **For TV Episodes** (titleType = "tvEpisode"):
        /// - tconst: Episode identifier
        /// - episodeTitle: Title of the episode
        /// - seasonNumber: Season number
        /// - episodeNumber: Episode number within season
        /// - plot: Episode plot summary
        /// - posterUrl: Episode poster image URL
        /// - releaseDate: Episode air date
        /// - writerNames: Comma-separated list of writers
        /// - parentSeriesId: The tconst of the parent series
        /// - parentSeriesTitle: The title of the parent series
        /// - userBookmark: (authenticated users only) Object containing bookmarkId, isBookmarked, note, and rating
        /// 
        /// **For Movies/Shorts** (titleType = "movie" or "short"):
        /// - tconst: Title identifier
        /// - movieTitle: Name of the movie
        /// - plot: Movie plot summary
        /// - posterUrl: Movie poster image URL
        /// - ratedAge: Age rating (e.g., "PG-13", "R")
        /// - language: Primary language
        /// - releaseDate: Release date
        /// - writerNames: Comma-separated list of writers
        /// - country: Country of origin
        /// - userBookmark: (authenticated users only) Object containing bookmarkId, isBookmarked, note, and rating
        /// 
        /// **User Bookmark Object** (included only for authenticated users):
        /// - bookmarkId: The ID of the bookmark (null if not bookmarked)
        /// - isBookmarked: Boolean indicating if the title is bookmarked by the user
        /// - note: The user's note content for this title (null if no note exists)
        /// - rating: The user's rating for this title (null if not rated)
        /// 
        /// Sample request:
        /// 
        ///     GET /api/movies/tt0133093
        ///     
        /// </remarks>
        /// <response code="200">Returns the detailed title information</response>
        /// <response code="401">If the title is adult-rated and user is not authenticated</response>
        /// <response code="404">If the title is not found</response>
        [HttpGet("{tconst}")]
        public async Task<IActionResult> GetById(string tconst)
        {
            var title = await repo.GetByIdAsync(tconst);
            if (title == null)
                return NotFound();

            // ✅ Restrict access for adult titles
            if (title.IsAdult)
            {
                var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
                if (!isAuthenticated)
                    return Unauthorized(new { message = "Login required to access adult-rated titles." });
            }

            // Return detailed information based on title type
            if (title.TitleType != null && title.TitleType.Contains("Series", StringComparison.OrdinalIgnoreCase))
            {
                var seriesDetails = await repo.GetSeriesDetailsAsync(tconst);
                return Ok(seriesDetails ?? (object)title);
            }
            else if (title.TitleType == "tvEpisode")
            {
                var episodeDetails = await repo.GetEpisodeDetailsAsync(tconst);
                return Ok(episodeDetails ?? (object)title);
            }
            else if (title.TitleType == "movie" || title.TitleType == "short")
            {
                var filmDetails = await repo.GetFilmDetailsAsync(tconst);
                return Ok(filmDetails ?? (object)title);
            }

            // Fallback to basic title info
            return Ok(title);
        }

        [HttpGet("{tconst}/cast")]
        public async Task<IActionResult> GetCast(string tconst)
        {
            var cast = await repo.GetCastAndCrewAsync(tconst);
            return Ok(cast);
        }
    }
}