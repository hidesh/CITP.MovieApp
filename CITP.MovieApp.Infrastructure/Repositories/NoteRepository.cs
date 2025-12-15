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

        // ------------------------------------------------------------
        // Get all notes for a user (with context)
        // ------------------------------------------------------------
        public async Task<IEnumerable<NoteDto>> GetAllForUserAsync(int userId)
        {
            return await _db.Notes
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .Select(n => new NoteDto
                {
                    NoteId = n.NoteId,
                    UserId = n.UserId,
                    Tconst = n.Tconst,
                    Nconst = n.Nconst,

                    TitleName = n.Title != null ? n.Title.PrimaryTitle : null,
                    PersonName = n.Person != null ? n.Person.PrimaryName : null,

                    Content = n.Content,
                    NotedAt = n.NotedAt,
                    UpdatedAt = n.UpdatedAt
                })
                .OrderByDescending(n => n.UpdatedAt ?? n.NotedAt)
                .ToListAsync();
        }

        // ------------------------------------------------------------
        // Get notes for a movie
        // ------------------------------------------------------------
        public async Task<IEnumerable<NoteDto>> GetAllForUserByMovieAsync(
            int userId,
            string tconst
        )
        {
            return await _db.Notes
                .AsNoTracking()
                .Where(n => n.UserId == userId && n.Tconst == tconst)
                .Select(n => new NoteDto
                {
                    NoteId = n.NoteId,
                    UserId = n.UserId,
                    Tconst = n.Tconst,

                    TitleName = n.Title != null ? n.Title.PrimaryTitle : null,

                    Content = n.Content,
                    NotedAt = n.NotedAt,
                    UpdatedAt = n.UpdatedAt
                })
                .OrderByDescending(n => n.UpdatedAt ?? n.NotedAt)
                .ToListAsync();
        }

        // ------------------------------------------------------------
        // Get notes for a person
        // ------------------------------------------------------------
        public async Task<IEnumerable<NoteDto>> GetAllForUserByPersonAsync(
            int userId,
            string nconst
        )
        {
            return await _db.Notes
                .AsNoTracking()
                .Where(n => n.UserId == userId && n.Nconst == nconst)
                .Select(n => new NoteDto
                {
                    NoteId = n.NoteId,
                    UserId = n.UserId,
                    Nconst = n.Nconst,

                    PersonName = n.Person != null ? n.Person.PrimaryName : null,

                    Content = n.Content,
                    NotedAt = n.NotedAt,
                    UpdatedAt = n.UpdatedAt
                })
                .OrderByDescending(n => n.UpdatedAt ?? n.NotedAt)
                .ToListAsync();
        }

        // ------------------------------------------------------------
        // Create note for a movie
        // ------------------------------------------------------------
        public async Task<int> CreateForMovieAsync(
            int userId,
            string tconst,
            NoteCreateDto dto
        )
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

        // ------------------------------------------------------------
        // Create note for a person
        // ------------------------------------------------------------
        public async Task<int> CreateForPersonAsync(
            int userId,
            string nconst,
            NoteCreateDto dto
        )
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

        // ------------------------------------------------------------
        // Update note
        // ------------------------------------------------------------
        public async Task<bool> UpdateAsync(
            int noteId,
            int userId,
            NoteUpdateDto dto
        )
        {
            var entity = await _db.Notes
                .FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);

            if (entity == null)
                return false;

            entity.Content = dto.Content.Trim();
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        // ------------------------------------------------------------
        // Delete note
        // ------------------------------------------------------------
        public async Task<bool> DeleteAsync(int noteId, int userId)
        {
            var entity = await _db.Notes
                .FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);

            if (entity == null)
                return false;

            _db.Notes.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
