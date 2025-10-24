using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;
using CITP.MovieApp.Api.Utils;
using Microsoft.AspNetCore.Mvc;

namespace CITP.MovieApp.Api.Controllers;

[ApiController]
[Route("api/movies")]
public class MoviesController : ControllerBase
{
    private readonly IMovieRepository _repo;
    public MoviesController(IMovieRepository repo) { _repo = repo; }

    [HttpGet("{tconst}")]
    public async Task<ActionResult<MovieDto>> GetById([FromRoute] string tconst, CancellationToken ct)
    {
        var m = await _repo.GetByIdAsync(tconst, ct);
        if (m is null) return NotFound();

        var dto = new MovieDto(
            m.TConst, m.PrimaryTitle, m.StartYear, m.AverageRating, m.NumVotes,
            self: $"/api/movies/{m.TConst}"
        );
        return Ok(dto);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<MovieDto>>> Search(
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await _repo.SearchAsync(q, page, pageSize, ct);
        var data = items.Select(m => new MovieDto(
            m.TConst, m.PrimaryTitle, m.StartYear, m.AverageRating, m.NumVotes,
            self: $"/api/movies/{m.TConst}"
        ));

        var next = LinkBuilder.PageLink(Request, page + 1, pageSize, total);
        var prev = LinkBuilder.PageLink(Request, page - 1, pageSize, total);

        return Ok(new PagedResult<MovieDto>(page, pageSize, total, data, next, prev));
    }
}