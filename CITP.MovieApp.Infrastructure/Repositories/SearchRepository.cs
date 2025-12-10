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

        public async Task<IEnumerable<SearchResultDto>> BestMatchAsync(string keywords)
        {
            // Split keywords into array of words and call best_match function
            // Map snake_case database columns to PascalCase DTO properties
            var keywordArray = keywords.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            // Build PostgreSQL array literal: ARRAY['word1', 'word2', ...]
            var arrayLiteral = string.Join(", ", keywordArray.Select(k => $"'{k.Replace("'", "''")}'"));
            
            var sql = $@"SELECT 
                tconst AS ""Tconst"", 
                primarytitle AS ""PrimaryTitle"", 
                match_count AS ""MatchCount"" 
                FROM best_match(ARRAY[{arrayLiteral}])";
            
            var results = await _context.Database
                .SqlQueryRaw<SearchResultDto>(sql)
                .ToListAsync();

            return results;
        }

        public async Task<IEnumerable<SearchResultDto>> StructuredStringSearchAsync(int userId, string keyword)
        {
            // Call structured_string_search function
            // Map snake_case database columns to PascalCase DTO properties
            var sql = @"SELECT 
                tconst AS ""Tconst"", 
                primarytitle AS ""PrimaryTitle"", 
                NULL AS ""MatchCount"" 
                FROM structured_string_search({0}, {1})";
            
            var results = await _context.Database
                .SqlQueryRaw<SearchResultDto>(sql, userId, keyword)
                .ToListAsync();

            return results;
        }
    }
}
