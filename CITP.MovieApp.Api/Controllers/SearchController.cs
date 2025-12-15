using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchRepository _searchRepository;

        public SearchController(ISearchRepository searchRepository)
        {
            _searchRepository = searchRepository;
        }

        [AllowAnonymous]
        [HttpGet("best-match")]
        public async Task<ActionResult<IEnumerable<SearchResultDto>>> BestMatch(
            [FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Search query cannot be empty");

            // ✅ Anonymous search uses best_match
            var results = await _searchRepository.BestMatchAsync(query);
            return Ok(results);
        }


        // 🔒 Authenticated structured search
        [Authorize]
        [HttpGet("structured")]
        public async Task<ActionResult<IEnumerable<SearchResultDto>>> StructuredSearch(
            [FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Search query cannot be empty");

            var userId = GetCurrentUserId();
            var results = await _searchRepository
                .StructuredStringSearchAsync(userId, query);

            return Ok(results);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim!);
        }
    }
}