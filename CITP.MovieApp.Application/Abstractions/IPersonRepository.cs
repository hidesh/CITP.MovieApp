using CITP.MovieApp.Application.DTOs;

namespace CITP.MovieApp.Application.Abstractions
{
    public interface IPersonRepository
    {
        Task<IEnumerable<PersonDto>> GetAllAsync();
        Task<PersonDto?> GetByIdAsync(string nconst);
        Task<IEnumerable<PersonFilmographyDto>> GetFilmographyAsync(string nconst);
    }
}