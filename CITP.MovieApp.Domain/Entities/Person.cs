using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITP.MovieApp.Domain;

public class Person
{
    public string NConst { get; set; } = default!;     // fx "nm0000229"
    public string PrimaryName { get; set; } = default!;
    public ICollection<MoviePerson> Titles { get; set; } = new List<MoviePerson>();
}
