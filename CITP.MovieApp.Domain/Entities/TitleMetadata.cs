using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITP.MovieApp.Domain.Entities

{
    [Table("title_metadata")]
    public class TitleMetadata
    {
        [Key]
        [Column("tconst")]
        public string Tconst { get; set; } = null!; // References Title
    

        [Column("plot")]
        public string? Plot { get; set; }

        [Column("rated")]
        public string? Rated { get; set; } 

        [Column("language")]
        public string? Language { get; set; }

        [Column("released")]
        public string? Released { get; set; }

        [Column("writer")]
        public string? Writer { get; set; }

        [Column("country")]
        public string? Country { get; set; }

        [Column("poster")]
        public string? Poster { get; set; }

        [Column("production")]
        public string? Production { get; set; }
    
        [Column("poster")]
        public string? PosterUrl { get; set; }

        // Navigation property
        [ForeignKey("Tconst")]
        public Title? Title { get; set; }
    }
}