using Microsoft.EntityFrameworkCore;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;
using CITP.MovieApp.Domain.Entities;
using CITP.MovieApp.Infrastructure.Persistence;

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

        public async Task<IEnumerable<BookmarkDto>> GetAllAsync(int userId)
        {
            return await _dbSet
                .Where(b => b.UserId == userId)
                .Select(b => new BookmarkDto
                {
                    BookmarkId = b.BookmarkId,
                    UserId = b.UserId,
                    Tconst = b.Tconst,
                    Nconst = b.Nconst,
                    BookmarkedAt = b.BookmarkedAt
                })
                .ToListAsync();
        }

        public async Task<BookmarkDto?> GetByIdAsync(int bookmarkId)
        {
            return await _dbSet
                .Where(b => b.BookmarkId == bookmarkId)
                .Select(b => new BookmarkDto
                {
                    BookmarkId = b.BookmarkId,
                    UserId = b.UserId,
                    Tconst = b.Tconst,
                    Nconst = b.Nconst,
                    BookmarkedAt = b.BookmarkedAt
                })
                .FirstOrDefaultAsync();
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