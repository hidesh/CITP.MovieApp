using System.Threading.Tasks;
using CITP.MovieApp.Application.DTOs;
using System.Collections.Generic;

namespace CITP.MovieApp.Application.Abstractions
{
    public interface IMovieRepository
    {
        Task<IEnumerable<TitleDto>> GetAllAsync();
        Task<TitleDto?> GetByIdAsync(string tconst);
        Task<IEnumerable<TitleCastCrewDto>> GetCastAndCrewAsync(string tconst);
        Task<SeriesDetatailsDto?> GetSeriesDetailsAsync(string tconst);
        Task<EpisodeDetailsDto?> GetEpisodeDetailsAsync(string tconst);
        Task<FilmDetailsDto?> GetFilmDetailsAsync(string tconst);
        Task<TitleDetailsDto?> GetTitleDetailsAsync(string tconst);

        // signature: page, pageSize, type, genre, sort
        Task<PagedResult<TitleDto>> ListPagedAsync(int page, int pageSize, string? type = null, string? genre = null, string? sort = null);
    }
}