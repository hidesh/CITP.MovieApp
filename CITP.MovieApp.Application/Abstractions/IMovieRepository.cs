using CITP.MovieApp.Application.DTOs;

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
    }
}