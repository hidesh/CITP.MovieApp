namespace CITP.MovieApp.Application.DTOs
{
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
        public int NumberOfSeasons { get; set; }
        public string Plot { get; set; } = null!;
        public string PosterUrl { get; set; } = null!;
        public string Language { get; set; } = null!;
        public string RatedAge { get; set; } = null!;
        public string ReleaseDate { get; set; } = null!;
        public string WriterNames { get; set; } = null!;
        public string Country { get; set; } = null!;
    }

    public class EpisodeDetailsDto
    {
        public string Tconst { get; set; } = null!;
        public int? SeasonNumber { get; set; }
        public int? EpisodeNumber { get; set; }
        public string Plot { get; set; } = null!;
        public string PosterUrl { get; set; } = null!;
        public string ReleaseDate { get; set; } = null!;
        public string WriterNames { get; set; } = null!;
        public string ParentSeriesId { get; set; } = null!;
    }

    public class FilmDetailsDto
    {
        public string Tconst { get; set; } = null!;
        public string Plot { get; set; } = null!;
        public string PosterUrl { get; set; } = null!;
        public string Language { get; set; } = null!;
        public string RatedAge { get; set; } = null!;
        public string ReleaseDate { get; set; } = null!;
        public string WriterNames { get; set; } = null!;
        public string Country { get; set; } = null!;
    }   
}