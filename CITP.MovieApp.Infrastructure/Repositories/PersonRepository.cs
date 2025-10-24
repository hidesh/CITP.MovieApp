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

public class PersonRepository : IPersonRepository
{
    private readonly AppDbContext _db;
    public PersonRepository(AppDbContext db) => _db = db;

    public async Task<Person?> GetByIdAsync(string nconst, CancellationToken ct)
        => await _db.People.AsNoTracking().FirstOrDefaultAsync(p => p.NConst == nconst, ct);

    public async Task<(IEnumerable<Person> items, int total)> SearchAsync(string? q, int page, int size, CancellationToken ct)
    {
        var query = _db.People.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var s = q.Trim().ToLower();
            query = query.Where(p => p.PrimaryName.ToLower().Contains(s));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.PrimaryName)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        return (items, total);
    }
}
