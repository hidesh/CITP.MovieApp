using CITP.MovieApp.Domain;

namespace CITP.MovieApp.Application.Abstractions;

public interface IPersonRepository
{
    Task<Person?> GetByIdAsync(string nconst, CancellationToken ct);
    Task<(IEnumerable<Person> items, int total)> SearchAsync(string? q, int page, int size, CancellationToken ct);
}