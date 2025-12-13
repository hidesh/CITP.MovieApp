using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class MoviesController : ControllerBase
    {
        private readonly IMovieRepository _repo;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MoviesController(IMovieRepository repo, IHttpContextAccessor httpContextAccessor)
        {
            _repo = repo;
            _httpContextAccessor = httpContextAccessor;
        }

        private string? BuildPageLink(int page, int pageSize)
        {
            if (page < 1) return null;

            var req = _httpContextAccessor.HttpContext?.Request;
            if (req == null) return null;

            var uriBuilder = new UriBuilder
            {
                Scheme = req.Scheme,
                Host = req.Host.Host,
                Path = req.Path.ToString()
            };
            if (req.Host.Port.HasValue)
                uriBuilder.Port = req.Host.Port.Value;

            var qp = System.Web.HttpUtility.ParseQueryString(req.QueryString.ToString());
            qp.Set("page", page.ToString());
            qp.Set("pageSize", pageSize.ToString());
            uriBuilder.Query = qp.ToString();
            return uriBuilder.Uri.ToString();
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? type = null, [FromQuery] string? genre = null, [FromQuery] string? sort = null)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0 || pageSize > 500) pageSize = 20;

            var paged = await _repo.ListPagedAsync(page, pageSize, type, genre, sort);

            var items = paged?.Items ?? Array.Empty<TitleDto>();
            var total = paged?.Total ?? 0;
            var totalPages = (int)Math.Ceiling((double)total / Math.Max(1, pageSize));

            string? prev = page > 1 ? BuildPageLink(page - 1, pageSize) : null;
            string? next = page < totalPages ? BuildPageLink(page + 1, pageSize) : null;

            return Ok(new
            {
                total,
                totalPages,
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
            var title = await _repo.GetByIdAsync(tconst);
            if (title == null) return NotFound();

            if (title.IsAdult)
            {
                var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
                if (!isAuthenticated) return Unauthorized(new { message = "Login required to access adult-rated titles." });
            }

            var details = await _repo.GetTitleDetailsAsync(tconst);
            return Ok(details ?? (object)title);
        }

        [HttpGet("{tconst}/cast")]
        public async Task<IActionResult> GetCast(string tconst)
        {
            var cast = await _repo.GetCastAndCrewAsync(tconst) ?? Enumerable.Empty<TitleCastCrewDto>();
            return Ok(cast);
        }
        
        [HttpGet("{tconst}/episodes")]
        public async Task<IActionResult> GetEpisodesForSeries(string tconst)
        {
            var episodes = await _repo.GetEpisodesForSeriesAsync(tconst);
            return Ok(episodes);
        }


    }
}
