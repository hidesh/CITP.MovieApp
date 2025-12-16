using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RatingsController : ControllerBase
    {
        private readonly IRatingRepository _ratings;

        public RatingsController(IRatingRepository ratings)
        {
            _ratings = ratings;
        }

        private int GetUserIdOrThrow()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(id))
                throw new UnauthorizedAccessException();
            return int.Parse(id);
        }

        // Get all ratings for the logged-in user
        [HttpGet("user")]
        [Authorize] // Requires login
        public async Task<IActionResult> GetMyRatings()
        {
            var userId = GetUserIdOrThrow();
            var ratings = await _ratings.GetAllForUserAsync(userId);
            return Ok(ratings);
        }

        // Get all ratings for a movie by anyone (public)
        [HttpGet("movie/{tconst}")]
        [AllowAnonymous] // Public access
        public async Task<IActionResult> GetMovieRatings(string tconst)
        {
            // This endpoint is now public - returns all ratings for a movie
            // We'll need to update the repository method to not require userId
            var ratings = await _ratings.GetAllByMovieAsync(tconst);
            return Ok(ratings);
        }

        // Create rating for a movie
        [HttpPost("movie/{tconst}")]
        [Authorize] // Requires login
        public async Task<IActionResult> CreateForMovie(string tconst, [FromBody] RatingCreateDto dto)
        {
            if (dto.Rating < 0 || dto.Rating > 10)
                return BadRequest(new { message = "Rating must be between 0 and 10." });

            var userId = GetUserIdOrThrow();
            var id = await _ratings.CreateOrUpdateForMovieAsync(userId, tconst, dto);
            return CreatedAtAction(nameof(GetMovieRatings), new { tconst }, new { id });
        }

        // Update a rating
        [HttpPut("{id:int}")]
        [Authorize] // Requires login
        public async Task<IActionResult> Update(int id, [FromBody] RatingUpdateDto dto)
        {
            if (dto.Rating < 0 || dto.Rating > 10)
                return BadRequest(new { message = "Rating must be between 0 and 10." });

            var userId = GetUserIdOrThrow();
            var ok = await _ratings.UpdateAsync(id, userId, dto);
            return ok ? NoContent() : NotFound();
        }

        // Delete a rating
        [HttpDelete("{id:int}")]
        [Authorize] // Requires login
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserIdOrThrow();
            var ok = await _ratings.DeleteAsync(id, userId);
            return ok ? NoContent() : NotFound();
        }
    }
}
