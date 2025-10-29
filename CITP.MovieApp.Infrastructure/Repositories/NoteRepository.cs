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

        public NoteRepository(AppDbContext db) => _db = db;

        public async Task<IEnumerable<NoteDto>> GetAllForUserAsync(int userId, string? tconst = null, string? nconst = null)
        {
            var q = _db.Notes.AsNoTracking().Where(n => n.UserId == userId);

            if (!string.IsNullOrWhiteSpace(tconst)) q = q.Where(n => n.Tconst == tconst);
            if (!string.IsNullOrWhiteSpace(nconst)) q = q.Where(n => n.Nconst == nconst);

            return await q
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

        public async Task<NoteDto?> GetForUserByIdAsync(int noteId, int userId)
        {
            return await _db.Notes.AsNoTracking()
                .Where(n => n.NoteId == noteId && n.UserId == userId)
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
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateAsync(int userId, NoteCreateDto dto)
        {
            if ((dto.Tconst is null && dto.Nconst is null) ||
                (dto.Tconst is not null && dto.Nconst is not null))
                throw new ArgumentException("Exactly one of Tconst or Nconst must be provided.");

            var entity = new Note
            {
                UserId = userId,
                Tconst = dto.Tconst,
                Nconst = dto.Nconst,
                Content = dto.Content.Trim(),
                NotedAt = DateTime.UtcNow
            };

            _db.Notes.Add(entity);
            await _db.SaveChangesAsync();
            return entity.NoteId;
        }

        public async Task<bool> UpdateAsync(int noteId, int userId, NoteUpdateDto dto)
        {
            var entity = await _db.Notes.FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);
            if (entity is null) return false;

            entity.Content = dto.Content.Trim();
            entity.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int noteId, int userId)
        {
            var entity = await _db.Notes.FirstOrDefaultAsync(n => n.NoteId == noteId && n.UserId == userId);
            if (entity is null) return false;

            _db.Notes.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
