using Microsoft.EntityFrameworkCore;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;
using CITP.MovieApp.Domain.Entities;
using CITP.MovieApp.Infrastructure.Persistence;

namespace CITP.MovieApp.Infrastructure.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Person> _dbSet;

        public PersonRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Person>();
        }

        public async Task<IEnumerable<PersonDto>> GetAllAsync()
        {
            return await _dbSet
                .Select(p => new PersonDto
                {
                    Nconst = p.Nconst,
                    PrimaryName = p.PrimaryName,
                    BirthYear = p.BirthYear,
                    DeathYear = p.DeathYear,
                    PrimaryProfession = p.PrimaryProfession
                })
                .ToListAsync();
        }

        public async Task<PersonDto?> GetByIdAsync(string nconst)
        {
            return await _dbSet
                .Where(p => p.Nconst == nconst)
                .Select(p => new PersonDto
                {
                    Nconst = p.Nconst,
                    PrimaryName = p.PrimaryName,
                    BirthYear = p.BirthYear,
                    DeathYear = p.DeathYear,
                    PrimaryProfession = p.PrimaryProfession
                })
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PersonFilmographyDto>> GetFilmographyAsync(string nconst)
        {
            return await _context.Set<Role>()
                .Where(r => r.Nconst == nconst)
                .Include(r => r.Title)
                .Select(r => new PersonFilmographyDto
                {
                    Tconst = r.Title!.Tconst,
                    Title = r.Title.PrimaryTitle,
                    Job = r.Job,
                    CharacterName = r.CharacterName,
                    StartYear = r.Title.StartYear
                })
                .ToListAsync();
        }
    }
}
