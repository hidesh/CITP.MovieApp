using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CITP.MovieApp.Domain
{
    public class Movie
    {
        public string TConst { get; set; } = default!;     // fx "tt1375666"
        public string PrimaryTitle { get; set; } = default!;
        public int? StartYear { get; set; }
        public float? AverageRating { get; set; }
        public int? NumVotes { get; set; }
        public ICollection<MoviePerson> Principals { get; set; } = new List<MoviePerson>();
    }
}
