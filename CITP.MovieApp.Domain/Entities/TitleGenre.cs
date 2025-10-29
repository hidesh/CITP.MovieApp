using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITP.MovieApp.Domain.Entities

{
    [Table("title_genre")]
    public class TitleGenre
    {
        [Key]
        [Column("title_id", Order = 0)]
        public string TitleId { get; set; } = null!; // References Title

        [Key]
        [Column("genre_id", Order = 1)]
        public int GenreId { get; set; } // References Genre

        // Navigation properties
        public Title? Title { get; set; }
        public Genre? Genre { get; set; }
    }
}