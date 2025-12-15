using Microsoft.EntityFrameworkCore;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;
using CITP.MovieApp.Infrastructure.Persistence;

namespace CITP.MovieApp.Infrastructure.Repositories
{
    public class SearchRepository : ISearchRepository
    {
        private readonly AppDbContext _context;

        public SearchRepository(AppDbContext context)
        {
            _context = context;
        }

        // 🔕 Can keep this for future experiments
        public async Task<IEnumerable<SearchResultDto>> BestMatchAsync(string keywords)
        {
            var keywordArray = keywords.Split(
                ' ', StringSplitOptions.RemoveEmptyEntries);

            var arrayLiteral = string.Join(
                ", ",
                keywordArray.Select(k => $"'{k.Replace("'", "''")}'"));

            var sql = $@"
                SELECT 
                    tconst AS ""Tconst"", 
                    primarytitle AS ""PrimaryTitle"", 
                    match_count AS ""MatchCount""
                FROM best_match(ARRAY[{arrayLiteral}])";

            return await _context.Database
                .SqlQueryRaw<SearchResultDto>(sql)
                .ToListAsync();
        }

        // ✅ This now powers BOTH logged-in and anonymous search
        public async Task<IEnumerable<SearchResultDto>> StructuredStringSearchAsync(
            int userId,
            string keyword)
        {
            var sql = @"
                SELECT 
                    tconst AS ""Tconst"", 
                    primarytitle AS ""PrimaryTitle"", 
                    NULL AS ""MatchCount""
                FROM structured_string_search({0}, {1})";

            return await _context.Database
                .SqlQueryRaw<SearchResultDto>(sql, userId, keyword)
                .ToListAsync();
        }
    }
}