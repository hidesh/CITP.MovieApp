namespace CITP.MovieApp.Application.DTOs
{
    public class PersonDto
    {
        public string Nconst { get; set; } = null!;
        public string PrimaryName { get; set; } = null!;
        public int? BirthYear { get; set; }
        public int? DeathYear { get; set; }
        public string? PrimaryProfession { get; set; }
    }

    public class PersonFilmographyDto
    {
        public string Tconst { get; set; } = null!; 
        public string Title { get; set; } = null!;
        
        public string? Job { get; set; } 
        public string? CharacterName { get; set; } 

        public int? StartYear { get; set; } 
    }
}