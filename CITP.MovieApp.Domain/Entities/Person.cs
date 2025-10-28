using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITP.MovieApp.Domain.Entities

{
    [Table("person")]
    public class Person
    {
        [Key]
        [Column("nconst")]
        public string Nconst { get; set; } = null!; // Unique identifier

        [Column("primaryname")]
        public string PrimaryName { get; set; } = null!;

        [Column("birthyear")]
        public int? BirthYear { get; set; }

        [Column("deathyear")]
        public int? DeathYear { get; set; }

        [Column("primaryprofession")]
        public string? PrimaryProfession { get; set; }

        // Navigation properties
        public ICollection<Role>? Roles { get; set; } // Roles this person has played
        public ICollection<PersonKnownFor>? KnownFor { get; set; } // Titles this person is known for
        public ICollection<Bookmark>? Bookmarks { get; set; } // Bookmarks of this person
        public ICollection<Note>? Notes { get; set; } // Notes about this person
        
    }
}