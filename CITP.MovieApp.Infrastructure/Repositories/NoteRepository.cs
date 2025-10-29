using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;
using CITP.MovieApp.Domain.Entities;
using CITP.MovieApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CITP.MovieApp.Infrastructure.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly AppDbContext _db;

        public NoteRepository(AppDbContext db)
        {
            _db = db;
        }

        // Get all notes for a user
        public async Task<IEnumerable<NoteDto>> GetAllForUserAsync(int userId)
        {
            return await _db.Notes.AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.UpdatedAt ?? n.NotedAt)
                .Select(n => new NoteDto
                {
                    NoteId = n.NoteId,
                    UserId = n.UserId,
                    Tconst = n.Tconst,
                    Nconst = n.Nconst,
                    Content = n.Content,
                    NotedAt = n.NotedAt,
                    UpdatedAt = n.UpdatedAt
                })
                .ToListAsync();
        }

        // Get all notes for a user on a specific movie
        public async Task<IEnumerable<NoteDto>> GetAllForUserByMovieAsync(int userId, string tconst)
        {
            return await _db.Notes.AsNoTracking()
                .Where(n => n.UserId == userId && n.Tconst == tconst)
                .OrderByDescending(n => n.UpdatedAt ?? n.NotedAt)
                .Select(n => new NoteDto
                {
                    NoteId = n.NoteId,
                    Tconst = n.Tconst,
                    Content = n.Content,
                    NotedAt = n.NotedAt,
                    UpdatedAt = n.UpdatedAt
                })
                .ToListAsync();
        }

        // Get all notes for a user on a specific person
        public async Task<IEnumerable<NoteDto>> GetAllForUserByPersonAsync(int userId, string nconst)
        {
            return await _db.Notes.AsNoTracking()
                .Where(n => n.UserId == userId && n.Nconst == nconst)
                .OrderByDescending(n => n.UpdatedAt ?? n.NotedAt)
                .Select(n => new NoteDto
                {
                    NoteId = n.NoteId,
                    Nconst = n.Nconst,
                    Content = n.Content,
                    NotedAt = n.NotedAt,
                    UpdatedAt = n.UpdatedAt
                })
                .ToListAsync();
        }

        // Create note for a movie
        public async Task<int> CreateForMovieAsync(int userId, string tconst, NoteCreateDto dto)
        {
            var entity = new Note
            {
                UserId = userId,
                Tconst = tconst,
                Content = dto.Content.Trim(),
                NotedAt = DateTime.UtcNow
            };

            _db.Notes.Add(entity);
            await _db.SaveChangesAsync();
            return entity.NoteId;
        }

        //  Create note for a person
        public async Task<int> CreateForPersonAsync(int userId, string nconst, NoteCreateDto dto)
        {
            var entity = new Note
            {
                UserId = userId,
                Nconst = nconst,
                Content = dto.Content.Trim(),
                NotedAt = DateTime.UtcNow
            };

            _db.Notes.Add(entity);
            await _db.SaveChangesAsync();
            return entity.NoteId;
        }

        // Update note content (only if user owns it)
        public async Task<bool> UpdateAsync(int noteId, int userId, NoteUpdateDto dto)
        {
            var entity = await _db.Notes.FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);
            if (entity == null) return false;

            entity.Content = dto.Content.Trim();
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        //  Delete a note (only if user owns it)
        public async Task<bool> DeleteAsync(int noteId, int userId)
        {
            var entity = await _db.Notes.FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);
            if (entity == null) return false;

            _db.Notes.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
