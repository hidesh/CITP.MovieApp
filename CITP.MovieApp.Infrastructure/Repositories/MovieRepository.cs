using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Domain;
using CITP.MovieApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CITP.MovieApp.Infrastructure.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly AppDbContext _db;
    public MovieRepository(AppDbContext db) => _db = db;

    public async Task<Movie?> GetByIdAsync(string tconst, CancellationToken ct)
        => await _db.Movies.AsNoTracking().FirstOrDefaultAsync(m => m.TConst == tconst, ct);

    public async Task<(IEnumerable<Movie> items, int total)> SearchAsync(string? q, int page, int size, CancellationToken ct)
    {
        var query = _db.Movies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = query.Where(m => m.PrimaryTitle.ToLower().Contains(s));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(m => m.PrimaryTitle)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (items, total);
    }
}
