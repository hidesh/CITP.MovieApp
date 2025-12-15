namespace CITP.MovieApp.Application.DTOs
{
    public class NoteCreateDto
    {
        public string Content { get; set; } = string.Empty;
    }

    public class NoteUpdateDto
    {
        public string Content { get; set; } = string.Empty;
    }

    public class NoteDto
    {
        public int NoteId { get; set; }
        public int UserId { get; set; }

        public string? Tconst { get; set; }
        public string? Nconst { get; set; }
        
        public string? TitleName { get; set; }
        public string? PersonName { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime NotedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}