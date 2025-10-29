using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Application.Abstractions
{
    public interface INoteRepository
    {
        Task<IEnumerable<NoteDto>> GetAllForUserAsync(int userId, string? tconst = null, string? nconst = null);
        Task<NoteDto?> GetForUserByIdAsync(int noteId, int userId);
        Task<int> CreateAsync(int userId, NoteCreateDto dto);
        Task<bool> UpdateAsync(int noteId, int userId, NoteUpdateDto dto);
        Task<bool> DeleteAsync(int noteId, int userId);
    }
}