using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Application.DTOs;
using CITP.MovieApp.Domain.Entities;
using CITP.MovieApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CITP.MovieApp.Infrastructure.Repositories
{
    public class RatingRepository : IRatingRepository
    {
        private readonly AppDbContext _db;

        public RatingRepository(AppDbContext db)
        {
            _db = db;
        }

        // ---------------- READ ----------------

        public async Task<IEnumerable<RatingDto>> GetAllForUserAsync(int userId)
        {
            var ratings = await _db.RatingHistories.AsNoTracking()
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RatedAt)
                .ToListAsync();

            var result = new List<RatingDto>();

            foreach (var rating in ratings)
            {
                var title = await _db.Titles
                    .Include(t => t.Metadatas)
                    .FirstOrDefaultAsync(t => t.Tconst == rating.Tconst);

                result.Add(new RatingDto
                {
                    RatingId = rating.RatingId,
                    UserId = rating.UserId,
                    Tconst = rating.Tconst,
                    Rating = rating.Rating,
                    RatedAt = rating.RatedAt,
                    TitleType = title?.TitleType,
                    Title = title?.PrimaryTitle,
                    PosterUrl = title?.Metadatas?.PosterUrl
                });
            }

            return result;
        }

        public async Task<IEnumerable<RatingDto>> GetAllForUserByMovieAsync(int userId, string tconst)
        {
            return await _db.RatingHistories.AsNoTracking()
                .Where(r => r.UserId == userId && r.Tconst == tconst)
                .Select(r => new RatingDto
                {
                    RatingId = r.RatingId,
                    UserId = r.UserId,
                    Tconst = r.Tconst,
                    Rating = r.Rating,
                    RatedAt = r.RatedAt
                })
                .ToListAsync();
        }

        public async Task<MovieRatingsDto> GetAllByMovieAsync(string tconst)
        {
            var imdbRating = await _db.Ratings.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Tconst == tconst);

            var userRatings = await _db.RatingHistories.AsNoTracking()
                .Where(r => r.Tconst == tconst)
                .OrderByDescending(r => r.RatedAt)
                .Select(r => new RatingDto
                {
                    RatingId = r.RatingId,
                    UserId = r.UserId,
                    Tconst = r.Tconst,
                    Rating = r.Rating,
                    RatedAt = r.RatedAt
                })
                .ToListAsync();

            return new MovieRatingsDto
            {
                Tconst = tconst,
                ImdbAverageRating = imdbRating?.AverageRating,
                ImdbNumVotes = imdbRating?.NumVotes,
                UserRatings = userRatings
            };
        }

        // ---------------- WRITE ----------------

        // ✅ NEW: Create OR update rating (UPSERT)
        public async Task<int> CreateOrUpdateForMovieAsync(
            int userId,
            string tconst,
            RatingCreateDto dto)
        {
            var existing = await _db.RatingHistories
                .FirstOrDefaultAsync(r =>
                    r.UserId == userId &&
                    r.Tconst == tconst);

            if (existing != null)
            {
                existing.Rating = dto.Rating;
                existing.RatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return existing.RatingId;
            }

            var entity = new RatingHistory
            {
                UserId = userId,
                Tconst = tconst,
                Rating = dto.Rating,
                RatedAt = DateTime.UtcNow
            };

            _db.RatingHistories.Add(entity);
            await _db.SaveChangesAsync();

            return entity.RatingId;
        }

        // Legacy methods (still valid)

        public async Task<int> CreateForMovieAsync(int userId, string tconst, RatingCreateDto dto)
        {
            return await CreateOrUpdateForMovieAsync(userId, tconst, dto);
        }

        public async Task<bool> UpdateAsync(int ratingId, int userId, RatingUpdateDto dto)
        {
            var entity = await _db.RatingHistories
                .FirstOrDefaultAsync(r => r.RatingId == ratingId && r.UserId == userId);

            if (entity == null) return false;

            entity.Rating = dto.Rating;
            entity.RatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int ratingId, int userId)
        {
            var entity = await _db.RatingHistories
                .FirstOrDefaultAsync(r => r.RatingId == ratingId && r.UserId == userId);

            if (entity == null) return false;

            _db.RatingHistories.Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
