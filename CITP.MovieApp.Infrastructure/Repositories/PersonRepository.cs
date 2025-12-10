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
            var roles = await _context.Set<Role>()
                .Where(r => r.Nconst == nconst)
                .Include(r => r.Title)
                .ThenInclude(t => t!.Metadatas)
                .ToListAsync();

            return roles
                .GroupBy(r => r.Tconst)
                .Select(g => new PersonFilmographyDto
                {
                    Tconst = g.Key,
                    Title = g.First().Title!.PrimaryTitle,
                    Jobs = g.Where(r => !string.IsNullOrEmpty(r.Job))
                           .Select(r => r.Job!)
                           .Distinct()
                           .ToList(),
                    Characters = g.Where(r => !string.IsNullOrEmpty(r.CharacterName))
                                 .Select(r => CleanCharacterName(r.CharacterName!))
                                 .Distinct()
                                 .ToList(),
                    StartYear = g.First().Title!.StartYear,
                    PosterUrl = g.First().Title!.Metadatas?.PosterUrl
                })
                .ToList();
        }

        private string CleanCharacterName(string characterName)
        {
            if (string.IsNullOrEmpty(characterName))
                return characterName;

            // Remove Python-style list formatting: ['name'] -> name
            var cleaned = characterName.Trim();
            
            // Check if it starts with [' and ends with ']
            if (cleaned.StartsWith("['") && cleaned.EndsWith("']"))
            {
                // Remove [' from start and '] from end
                cleaned = cleaned.Substring(2, cleaned.Length - 4);
            }
            
            return cleaned;
        }
    }
}
