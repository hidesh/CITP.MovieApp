using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITP.MovieApp.Domain;

public class MoviePerson
{
    public string MovieTConst { get; set; } = default!;
    public Movie Movie { get; set; } = default!;
    public string PersonNConst { get; set; } = default!;
    public Person Person { get; set; } = default!;
    public string? Category { get; set; }     // actor, director, ...
    public string? Characters { get; set; }
}