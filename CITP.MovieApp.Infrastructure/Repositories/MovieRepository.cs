using Microsoft.EntityFrameworkCore;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;
using CITP.MovieApp.Domain.Entities;
using CITP.MovieApp.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CITP.MovieApp.Infrastructure.Repositories
{
    public class MovieRepository : IMovieRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Title> _dbSet;

        public MovieRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Title>();
        }

        public async Task<IEnumerable<TitleDto>> GetAllAsync()
        {
            return await _dbSet
                .Select(t => new TitleDto
                {
                    Tconst = t.Tconst,
                    PrimaryTitle = t.PrimaryTitle,
                    OriginalTitle = t.OriginalTitle,
                    TitleType = t.TitleType,
                    IsAdult = t.IsAdult,
                    StartYear = t.StartYear,
                    EndYear = t.EndYear,
                    RuntimeMinutes = t.RuntimeMinutes
                })
                .ToListAsync();
        }

        public async Task<TitleDto?> GetByIdAsync(string tconst)
        {
            return await _dbSet
                .Where(t => t.Tconst == tconst)
                .Select(t => new TitleDto
                {
                    Tconst = t.Tconst,
                    PrimaryTitle = t.PrimaryTitle,
                    OriginalTitle = t.OriginalTitle,
                    TitleType = t.TitleType,
                    IsAdult = t.IsAdult,
                    StartYear = t.StartYear,
                    EndYear = t.EndYear,
                    RuntimeMinutes = t.RuntimeMinutes
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TitleCastCrewDto>> GetCastAndCrewAsync(string tconst)
        {
            return await _context.Set<Role>()
                .Where(r => r.Tconst == tconst)
                .Include(r => r.Person)
                .Select(r => new TitleCastCrewDto
                {
                    Nconst = r.Person!.Nconst,
                    Name = r.Person.PrimaryName,
                    Job = r.Job,
                    CharacterName = r.CharacterName
                })
                .ToListAsync();
        }
    }
}
