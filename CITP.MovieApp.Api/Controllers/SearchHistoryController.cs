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
    public class SearchHistoryController (ISearchHistoryRepository repo) : ControllerBase
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Add([FromBody] CreateSearchHistoryDto searchHistoryDto)
        {
            int tokenId = GetCurrentUserId();
            if (searchHistoryDto.UserId != tokenId)
                return BadRequest(new { message = "User ID mismatch" });

            var created = await repo.AddSearchHistoryAsync(searchHistoryDto.UserId, searchHistoryDto.Tconst);
            return Ok(created);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                throw new Exception("User ID claim not found");

            return int.Parse(userIdClaim.Value);
        }
    }
}