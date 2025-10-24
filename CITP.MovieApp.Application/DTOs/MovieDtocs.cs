using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITP.MovieApp.Application.DTOs;

public record MovieDto(
    string tconst,
    string title,
    int? year,
    float? averageRating,
    int? numVotes,
    string self
);

public record PagedResult<T>(
    int page,
    int pageSize,
    int total,
    IEnumerable<T> data,
    string? next,
    string? prev
);
