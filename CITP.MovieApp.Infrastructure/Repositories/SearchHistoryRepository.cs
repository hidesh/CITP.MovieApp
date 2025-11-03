using Microsoft.EntityFrameworkCore;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;
using CITP.MovieApp.Domain.Entities;
using CITP.MovieApp.Infrastructure.Persistence;

namespace CITP.MovieApp.Infrastructure.Repositories
{
    public class SearchHistoryRepository : ISearchHistoryRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<SearchHistory> _dbSet;

        public SearchHistoryRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<SearchHistory>();
        }

        public async Task<IEnumerable<SearchHistoryDto>> GetAllAsync(int userId)
        {
            // call database function get_search_history(user_id integer)
            var searchHistories = await _context.Database
                .SqlQuery<SearchHistoryDto>($"SELECT title AS Title, tconst AS Tconst, visited_at AS VisitedAt FROM get_search_history({userId})")
                .ToListAsync();

            return searchHistories;
        }
        
        public async Task<CreateSearchHistoryDto> AddSearchHistoryAsync(int userId, string? tconst)
        {
            var newSearchHistory = new SearchHistory
            {
                UserId = userId,
                Tconst = tconst,
                VisitedAt = DateTime.UtcNow
            };

            _dbSet.Add(newSearchHistory);
            await _context.SaveChangesAsync();

            return new CreateSearchHistoryDto
            {
                UserId = newSearchHistory.UserId,
                Tconst = newSearchHistory.Tconst
            };
        }
    }
}