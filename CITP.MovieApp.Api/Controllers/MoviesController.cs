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
        /// <returns>Returns normalized detailed information about the title</returns>
        /// <remarks>
        /// This endpoint returns a normalized response structure for all title types.
        /// The frontend should check `titleType` to determine which fields are populated.
        /// 
        /// **Always included fields:**
        /// - tconst: Title identifier
        /// - titleType: Type of title (movie, tvSeries, tvEpisode, short, video, tvMovie, etc.)
        /// - originalTitle: Original title
        /// - ratedAge: Age rating (e.g., "PG-13", "R")
        /// - language: Primary language
        /// - country: Country of origin
        /// - genres: Array of genre names
        /// - plot: Plot summary
        /// - posterUrl: Poster image URL
        /// - writerNames: Comma-separated list of writers
        /// - isAdult: Boolean flag for adult content
        /// - userBookmark: (authenticated users only) Object with bookmarkId, isBookmarked, note, rating
        /// 
        /// **Title field mapping (one of these will be populated):**
        /// - movieTitle: For movie, short, video, tvMovie
        /// - seriesTitle: For tvSeries, tvMiniSeries
        /// - episodeTitle: For tvEpisode
        /// 
        /// **Date fields:**
        /// - releaseDate: For movies and episodes (formatted date string)
        /// - startYear + endYear: For series (integer years)
        /// 
        /// **Type-specific fields:**
        /// - numberOfSeasons: For series
        /// - seasonNumber, episodeNumber, parentSeriesId, parentSeriesTitle: For episodes
        /// - runtimeMinutes: For movies
        /// 
        /// Sample request:
        /// 
        ///     GET /api/movies/tt0133093
        ///     
        /// </remarks>
        /// <response code="200">Returns the normalized title details</response>
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

            // Return normalized title details
            var details = await repo.GetTitleDetailsAsync(tconst);
            return Ok(details ?? (object)title);
        }

        [HttpGet("{tconst}/cast")]
        public async Task<IActionResult> GetCast(string tconst)
        {
            var cast = await repo.GetCastAndCrewAsync(tconst);
            return Ok(cast);
        }
    }
}