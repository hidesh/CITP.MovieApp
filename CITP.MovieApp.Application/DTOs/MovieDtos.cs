namespace CITP.MovieApp.Application.DTOs
{
    public class UserBookmarkDto
    {
        public int? BookmarkId { get; set; }
        public bool IsBookmarked { get; set; }
        public string? Note { get; set; }
        public decimal? Rating { get; set; }
    }

    public class TitleDto
    {
        public string Tconst { get; set; } = null!;
        public string PrimaryTitle { get; set; } = null!;
        public string OriginalTitle { get; set; } = null!;
        public string? TitleType { get; set; }
        public bool IsAdult { get; set; }
        public int? StartYear { get; set; }
        public int? EndYear { get; set; }
        public int? RuntimeMinutes { get; set; }
        public string? PosterUrl { get; set; }
    }

    public class TitleCastCrewDto
    {
        public string Nconst { get; set; } = null!; 
        public string Name { get; set; } = null!;

        public string? Job { get; set; } 
        public string? CharacterName { get; set; } 
    }

    public class SeriesDetatailsDto
    {
        public string Tconst { get; set; } = null!;
        public string SeriesTitle { get; set; } = null!;
        public int NumberOfSeasons { get; set; }
        public string Plot { get; set; } = null!;
        public string PosterUrl { get; set; } = null!;
        public string Language { get; set; } = null!;
        public string RatedAge { get; set; } = null!;
        public string ReleaseDate { get; set; } = null!;
        public string WriterNames { get; set; } = null!;
        public string Country { get; set; } = null!;
        public UserBookmarkDto? UserBookmark { get; set; }
    }

    public class EpisodeDetailsDto
    {
        public string Tconst { get; set; } = null!;
        public string EpisodeTitle { get; set; } = null!;
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
        public string Plot { get; set; } = null!;
        public string PosterUrl { get; set; } = null!;
        public string ReleaseDate { get; set; } = null!;
        public string WriterNames { get; set; } = null!;
        public string ParentSeriesId { get; set; } = null!;
        public string ParentSeriesTitle { get; set; } = null!;
        public UserBookmarkDto? UserBookmark { get; set; }
    }

    public class FilmDetailsDto
    {
        public string Tconst { get; set; } = null!;
        public string MovieTitle { get; set; } = null!;
        public string Plot { get; set; } = null!;
        public string PosterUrl { get; set; } = null!;
        public string Language { get; set; } = null!;
        public string RatedAge { get; set; } = null!;
        public string ReleaseDate { get; set; } = null!;
        public string WriterNames { get; set; } = null!;
        public string Country { get; set; } = null!;
        public UserBookmarkDto? UserBookmark { get; set; }
    }

    public class TitleDetailsDto
    {
        // Always included
        public string Tconst { get; set; } = null!;
        public string TitleType { get; set; } = null!;
        public string OriginalTitle { get; set; } = null!;
        public string RatedAge { get; set; } = null!;
        public string Language { get; set; } = null!;
        public string Country { get; set; } = null!;
        public List<string> Genres { get; set; } = new List<string>();
        public string Plot { get; set; } = null!;
        public string PosterUrl { get; set; } = null!;
        public string WriterNames { get; set; } = null!;
        public bool IsAdult { get; set; }
        public UserBookmarkDto? UserBookmark { get; set; }

        // Title field (one of these based on titleType)
        public string? MovieTitle { get; set; }
        public string? SeriesTitle { get; set; }
        public string? EpisodeTitle { get; set; }

        // Date fields
        public string? ReleaseDate { get; set; }  // For movies/episodes
        public int? StartYear { get; set; }       // For series
        public int? EndYear { get; set; }         // For series

        // Series-specific
        public int? NumberOfSeasons { get; set; }

        // Episode-specific
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
        public string? ParentSeriesId { get; set; }
        public string? ParentSeriesTitle { get; set; }

        // Movie-specific
        public int? RuntimeMinutes { get; set; }
    }   
}
