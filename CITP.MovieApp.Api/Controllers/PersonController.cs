using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Api.Utils;

namespace CITP.MovieApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] //  Allows public access
    public class PersonController(IPersonRepository repo) : ControllerBase
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
        /// Get a specific person by their unique identifier (nconst)
        /// </summary>
        /// <param name="nconst">The unique person identifier (e.g., nm0000001)</param>
        /// <returns>Returns detailed information about the person</returns>
        /// <remarks>
        /// This endpoint returns person information including:
        /// - nconst: Person identifier
        /// - primaryName: Name of the person
        /// - birthYear: Year of birth
        /// - deathYear: Year of death (null if still alive)
        /// - primaryProfession: Primary profession(s)
        /// - userBookmark: (authenticated users only) Object containing bookmarkId, isBookmarked, and note
        /// 
        /// **User Bookmark Object** (included only for authenticated users):
        /// - bookmarkId: The ID of the bookmark (null if not bookmarked)
        /// - isBookmarked: Boolean indicating if the person is bookmarked by the user
        /// - note: The user's note content for this person (null if no note exists)
        /// 
        /// Sample request:
        /// 
        ///     GET /api/person/nm0000001
        ///     
        /// </remarks>
        /// <response code="200">Returns the person information</response>
        /// <response code="404">If the person is not found</response>
        [HttpGet("{nconst}")]
        public async Task<IActionResult> GetById(string nconst)
        {
            var person = await repo.GetByIdAsync(nconst);
            if (person == null) return NotFound();
            return Ok(person);
        }

        [HttpGet("{nconst}/filmography")]
        public async Task<IActionResult> GetFilmography(string nconst)
        {
            var filmography = await repo.GetFilmographyAsync(nconst);
            return Ok(filmography);
        }
    }
}