using Microsoft.AspNetCore.Mvc;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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