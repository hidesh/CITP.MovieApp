using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Api.Controllers
{
    /// <summary>
    /// Handles search operations for titles using different search strategies
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchRepository _searchRepository;

        public SearchController(ISearchRepository searchRepository)
        {
            _searchRepository = searchRepository;
        }

        /// <summary>
        /// Searches for titles using best match algorithm (anonymous access)
        /// </summary>
        /// <param name="query">Search query containing keywords</param>
        /// <returns>List of matching titles with match counts</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/search/best-match?query=star wars
        ///     
        /// </remarks>
        /// <response code="200">Returns matching titles with match counts</response>
        /// <response code="400">If the search query is invalid</response>
        [AllowAnonymous]
        [HttpGet("best-match")]
        public async Task<ActionResult<IEnumerable<SearchResultDto>>> BestMatch([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }

            var results = await _searchRepository.BestMatchAsync(query);
            return Ok(results);
        }

        /// <summary>
        /// Searches for titles using structured string search with user context (requires authentication)
        /// </summary>
        /// <param name="query">Search query containing keyword</param>
        /// <returns>List of personalized matching titles</returns>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/search/structured?query=inception
        ///     
        /// Requires valid JWT token in Authorization header
        /// </remarks>
        /// <response code="200">Returns personalized matching titles</response>
        /// <response code="400">If the search query is invalid</response>
        /// <response code="401">If the user is not authenticated</response>
        [Authorize]
        [HttpGet("structured")]
        public async Task<ActionResult<IEnumerable<SearchResultDto>>> StructuredSearch([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Search query cannot be empty");
            }

            var userId = GetCurrentUserId();
            var results = await _searchRepository.StructuredStringSearchAsync(userId, query);
            return Ok(results);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim!);
        }
    }
}
