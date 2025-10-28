using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITP.MovieApp.Domain.Entities

{
    [Table("genre")]
    public class Genre
    {
        [Key]
        [Column("genre_id")]
        public int GenreId { get; set; }

        [Column("genre_name")]
        public string GenreName { get; set; } = null!;

        // Navigation property for many-to-many with Title
        public ICollection<TitleGenre>? TitleGenres { get; set; }
    }
}
