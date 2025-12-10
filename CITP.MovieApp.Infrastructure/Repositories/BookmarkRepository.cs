using Microsoft.EntityFrameworkCore;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;
using CITP.MovieApp.Domain.Entities;
using CITP.MovieApp.Infrastructure.Persistence;
using CITP.MovieApp.Infrastructure.Utils;

namespace CITP.MovieApp.Infrastructure.Repositories
{
    public class BookmarkRepository : IBookmarkRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Bookmark> _dbSet;

        public BookmarkRepository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Bookmark>();
        }

        private static BookmarkDto CreateBookmarkDto(Bookmark b, AppDbContext context)
        {
            // Fetch the title and titleType first
            var title = b.Tconst != null 
                ? context.Titles.FirstOrDefault(t => t.Tconst == b.Tconst)
                : null;
            
            var titleType = title?.TitleType;
            
            return new BookmarkDto
            {
                BookmarkId = b.BookmarkId,
                UserId = b.UserId,
                Tconst = b.Tconst,
                Nconst = b.Nconst,
                BookmarkedAt = b.BookmarkedAt,
                
                // For title bookmarks - use helper methods in memory after data is loaded
                TitleType = titleType,
                MovieTitle = TitleTypeHelper.IsMovie(titleType) ? title?.PrimaryTitle : null,
                SeriesTitle = TitleTypeHelper.IsSeries(titleType) ? title?.PrimaryTitle : null,
                EpisodeTitle = TitleTypeHelper.IsEpisode(titleType) ? title?.PrimaryTitle : null,
                
                // For person bookmarks
                PersonName = b.Nconst != null 
                    ? context.Persons.Where(p => p.Nconst == b.Nconst).Select(p => p.PrimaryName).FirstOrDefault()
                    : null,
                
                // Get poster only for titles (movies/series) from title_metadata, null for persons
                PosterUrl = b.Tconst != null
                    ? context.TitleMetadatas.Where(m => m.Tconst == b.Tconst).Select(m => m.PosterUrl).FirstOrDefault()
                    : null
            };
        }

        public async Task<IEnumerable<BookmarkDto>> GetAllAsync(int userId)
        {
            var bookmarks = await _dbSet
                .Where(b => b.UserId == userId)
                .ToListAsync();

            return bookmarks.Select(b => CreateBookmarkDto(b, _context)).ToList();
        }

        public async Task<BookmarkDto?> GetByIdAsync(int bookmarkId)
        {
            var bookmark = await _dbSet
                .Where(b => b.BookmarkId == bookmarkId)
                .FirstOrDefaultAsync();

            return bookmark != null ? CreateBookmarkDto(bookmark, _context) : null;
        }

        public async Task<bool> DeleteByIdAsync(int bookmarkId)
        {
            var bookmark = await _dbSet.FindAsync(bookmarkId);
            if (bookmark == null)
                return false; // Bookmark not found
            
            _dbSet.Remove(bookmark);
            await _context.SaveChangesAsync();
            return true; // Successfully deleted
        }

        public async Task<BookmarkDto> AddBookmarkAsync(int userId, string? tconst, string? nconst)
        {
            // Call the database function add_bookmark(userId, tconst, nconst)
            await _context.Database.ExecuteSqlInterpolatedAsync($"SELECT add_bookmark({userId}, {tconst}, {nconst})");

            // Fetch the newly created bookmark
            var bookmark = await _dbSet
                .Where(b => b.UserId == userId && 
                           ((tconst != null && b.Tconst == tconst) || 
                            (nconst != null && b.Nconst == nconst)))
                .OrderByDescending(b => b.BookmarkedAt)
                .FirstOrDefaultAsync();

            if (bookmark == null)
                throw new InvalidOperationException("Failed to create bookmark or bookmark not found");

            return new BookmarkDto
            {
                BookmarkId = bookmark.BookmarkId,
                UserId = bookmark.UserId,
                Tconst = bookmark.Tconst,
                Nconst = bookmark.Nconst,
                BookmarkedAt = bookmark.BookmarkedAt
            };
        }
       
    }
}