using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITP.MovieApp.Domain.Entities

{
    [Table("title")]
    public class Title
    {
        [Key]
        [Column("tconst")]
        public string Tconst { get; set; } = null!;

        [Column("primarytitle")]
        public string PrimaryTitle { get; set; } = null!;

        [Column("originaltitle")]
        public string OriginalTitle { get; set; } = null!;

        [Column("isadult")]
        public bool IsAdult { get; set; }

        [Column("startyear")]
        public int? StartYear { get; set; }

        [Column("endyear")]
        public int? EndYear { get; set; }

        [Column("runtimeminutes")]
        public int? RuntimeMinutes { get; set; }

        [Column("titletype")]
        public string? TitleType { get; set; }

        [Column("parent_series_id")]
        public string? ParentSeriesId { get; set; } // For episodes belonging to a series

        // Navigation properties
        public ICollection<RatingHistory>? RatingHistories { get; set; }
        public ICollection<SearchHistory>? SearchHistories { get; set; }
        public ICollection<Bookmark>? Bookmarks { get; set; }
        public ICollection<AlternateTitle>? AlternateTitles { get; set; }
        public ICollection<Role>? Roles { get; set; }
        public Rating? Ratings { get; set; }
        public ICollection<PersonKnownFor>? KnownForPeople { get; set; }
        public ICollection<Episode>? Episodes { get; set; }
        public ICollection<TitleGenre>? TitleGenres { get; set; }  
        public ICollection<Note>? Notes { get; set; }
     
        public TitleMetadata? Metadatas { get; set; }    
        public ICollection<WordIndex>? WordIndexes { get; set; }      

    }
}