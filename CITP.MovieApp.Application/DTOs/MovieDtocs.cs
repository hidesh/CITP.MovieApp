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
}