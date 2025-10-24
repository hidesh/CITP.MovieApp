using CITP.MovieApp.Domain;

namespace CITP.MovieApp.Application.Abstractions;

public interface IMovieRepository
{
    Task<Movie?> GetByIdAsync(string tconst, CancellationToken ct);
    Task<(IEnumerable<Movie> items, int total)> SearchAsync(string? q, int page, int size, CancellationToken ct);
}