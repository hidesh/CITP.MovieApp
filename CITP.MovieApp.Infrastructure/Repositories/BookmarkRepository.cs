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

        public async Task DeleteByIdAsync(int bookmarkId)
        {
            var bookmark = await _dbSet.FindAsync(bookmarkId);
            if (bookmark != null)
            {
                _dbSet.Remove(bookmark);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<BookmarkDto> AddForMovieAsync(CreateBookmarkDto bookmarkDto)
        {
            var bookmark = new Bookmark
            {
                UserId = bookmarkDto.UserId,
                Tconst = bookmarkDto.Tconst
            };
            _dbSet.Add(bookmark);
            await _context.SaveChangesAsync();
            return new BookmarkDto
            {
                BookmarkId = bookmark.BookmarkId,
                UserId = bookmark.UserId,
                Tconst = bookmark.Tconst,
                BookmarkedAt = bookmark.BookmarkedAt
            };
        }

        public async Task<BookmarkDto> AddForPersonAsync(CreateBookmarkDto bookmarkDto)
        {
            var bookmark = new Bookmark
            {
                UserId = bookmarkDto.UserId,
                Nconst = bookmarkDto.Nconst
            };
            _dbSet.Add(bookmark);
            await _context.SaveChangesAsync();
            return new BookmarkDto
            {
                BookmarkId = bookmark.BookmarkId,
                UserId = bookmark.UserId,
                Nconst = bookmark.Nconst,
                BookmarkedAt = bookmark.BookmarkedAt
            };
        }
     
    }
}