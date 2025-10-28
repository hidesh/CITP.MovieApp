using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITP.MovieApp.Domain.Entities
{
    [Table("person_known_for")]
    public class PersonKnownFor
    {
        [Key]
        [Column("nconst", Order = 0)]
        public string Nconst { get; set; } = null!; // References Person

        [Key]
        [Column("tconst", Order = 1)]
        public string Tconst { get; set; } = null!; // References Title

        // Navigation properties
        public Person? Person { get; set; }
        public Title? Title { get; set; }
    }
}
