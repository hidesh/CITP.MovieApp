using Microsoft.AspNetCore.Mvc;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Api.Utils;

namespace CITP.MovieApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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