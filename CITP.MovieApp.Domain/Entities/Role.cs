using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITP.MovieApp.Domain.Entities

{
    [Table("role")]
    public class Role
    {
        [Key]
        [Column("role_id")]
        public int RoleId { get; set; }

        [Column("nconst")]
        public string Nconst { get; set; } = null!; // References Person

        [Column("tconst")]
        public string Tconst { get; set; } = null!; // References Title

        [Column("job")]
        public string? Job { get; set; } 

        [Column("character_name")]
        public string? CharacterName { get; set; } 

        // Navigation properties
        public Person? Person { get; set; }
        public Title? Title { get; set; }
    }
}