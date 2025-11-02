using Microsoft.AspNetCore.Mvc;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookmarksController(IBookmarkRepository repo) : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get(int page = 1, int pageSize = 20)
        {
            if (pageSize <= 0 || pageSize > 200) pageSize = 20;

            int userId = GetCurrentUserId();

            var all = await repo.GetAllAsync(userId);
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

        [HttpGet("{bookmarkId}")]
        [Authorize]
        public async Task<IActionResult> GetById(int bookmarkId)
        {
            var bookmark = await repo.GetByIdAsync(bookmarkId);
            if (bookmark == null) return NotFound();

            return Ok(bookmark);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] CreateBookmarkDto bookmarkDto)
        {
            // Get user ID from JWT token
            int tokenUserId = GetCurrentUserId();
            
            // Validate that client-provided userId matches the token
            if (bookmarkDto.UserId != tokenUserId)
                return BadRequest(new { message = "User ID mismatch" });
            
            var newBookmark = await repo.AddBookmarkAsync(bookmarkDto.UserId, bookmarkDto.Tconst, bookmarkDto.Nconst);
            return Ok(newBookmark);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            // First check if the bookmark exists and belongs to the current user
            var bookmark = await repo.GetByIdAsync(id);
            if (bookmark == null)
                return NotFound("Bookmark not found");

            int userId = GetCurrentUserId();
            if (bookmark.UserId != userId)
                return BadRequest(new { message = "You can only delete your own bookmarks" });

            // Attempt to delete the bookmark
            var deleted = await repo.DeleteByIdAsync(id);
            if (!deleted)
                return NotFound("Bookmark not found");

            return NoContent(); // 204 No Content - successful deletion
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
            if (userIdClaim == null)
                throw new UnauthorizedAccessException("User ID claim not found in token");
            
            if (!int.TryParse(userIdClaim.Value, out int userId))
                throw new UnauthorizedAccessException($"Invalid user ID format: {userIdClaim.Value}");
            
            return userId;
        }
    }
}