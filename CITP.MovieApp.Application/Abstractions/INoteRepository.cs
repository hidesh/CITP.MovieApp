using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Application.Abstractions
{
    public interface INoteRepository
    {
        Task<IEnumerable<NoteDto>> GetAllForUserAsync(int userId);
        Task<IEnumerable<NoteDto>> GetAllForUserByMovieAsync(int userId, string tconst);
        Task<IEnumerable<NoteDto>> GetAllForUserByPersonAsync(int userId, string nconst);
        Task<int> CreateForMovieAsync(int userId, string tconst, NoteCreateDto dto);
        Task<int> CreateForPersonAsync(int userId, string nconst, NoteCreateDto dto);
        Task<bool> UpdateAsync(int noteId, int userId, NoteUpdateDto dto);
        Task<bool> DeleteAsync(int noteId, int userId);
    }
}
