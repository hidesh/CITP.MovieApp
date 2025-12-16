using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Application.Abstractions
{
    public interface IRatingRepository
    {
        Task<IEnumerable<RatingDto>> GetAllForUserAsync(int userId);
        Task<MovieRatingsDto> GetAllByMovieAsync(string tconst);
        Task<IEnumerable<RatingDto>> GetAllForUserByMovieAsync(int userId, string tconst);

        Task<int> CreateForMovieAsync(int userId, string tconst, RatingCreateDto dto);
        Task<bool> UpdateAsync(int ratingId, int userId, RatingUpdateDto dto);
        Task<bool> DeleteAsync(int ratingId, int userId);
        
        Task<int> CreateOrUpdateForMovieAsync(int userId, string tconst, RatingCreateDto dto);
    }

}
