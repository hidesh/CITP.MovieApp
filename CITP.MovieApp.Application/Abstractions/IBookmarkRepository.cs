using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Application.Abstractions
{
    public interface IBookmarkRepository
    {
        Task<IEnumerable<BookmarkDto>> GetAllAsync(int userId);
        Task<BookmarkDto?> GetByIdAsync(int bookmarkId);
        Task DeleteByIdAsync(int bookmarkId);
        Task<BookmarkDto> AddForMovieAsync(CreateBookmarkDto bookmarkDto);
        Task<BookmarkDto> AddForPersonAsync(CreateBookmarkDto bookmarkDto);
    }
}