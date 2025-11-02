using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Application.Abstractions
{
    public interface IBookmarkRepository
    {
        Task<IEnumerable<BookmarkDto>> GetAllAsync(int userId);
        Task<BookmarkDto?> GetByIdAsync(int bookmarkId);
        Task<bool> DeleteByIdAsync(int bookmarkId);
        Task<BookmarkDto> AddBookmarkAsync(int userId, string? tconst, string? nconst);
    }
}