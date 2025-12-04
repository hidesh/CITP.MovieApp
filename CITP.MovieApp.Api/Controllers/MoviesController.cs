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

            return Ok(title);
        }

        [HttpGet("{tconst}/cast")]
        public async Task<IActionResult> GetCast(string tconst)
        {
            var cast = await repo.GetCastAndCrewAsync(tconst);
            return Ok(cast);
        }

        [HttpGet("series/{tconst}")]
        public async Task<IActionResult> GetSeriesDetails(string tconst)
        {
            var seriesDetails = await repo.GetSeriesDetailsAsync(tconst);
            if (seriesDetails == null)
                return NotFound();

            return Ok(seriesDetails);
        }
    }
}