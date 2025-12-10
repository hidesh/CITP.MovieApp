using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Application.Abstractions
{
    public interface ISearchRepository
    {
        /// <summary>
        /// Search for titles using best match algorithm (for anonymous users)
        /// </summary>
        Task<IEnumerable<SearchResultDto>> BestMatchAsync(string keywords);

        /// <summary>
        /// Search for titles using structured string search (for authenticated users)
        /// </summary>
        Task<IEnumerable<SearchResultDto>> StructuredStringSearchAsync(int userId, string keyword);
    }
}
