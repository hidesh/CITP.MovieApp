using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Application.Abstractions
{
    public interface ISearchHistoryRepository
    {
        Task<IEnumerable<SearchHistoryDto>> GetAllAsync(int userId);
    
        Task<CreateSearchHistoryDto> AddSearchHistoryAsync(int userId, string? tconst);
    }
}