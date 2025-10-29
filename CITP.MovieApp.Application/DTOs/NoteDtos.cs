namespace CITP.MovieApp.Application.DTOs
{
    // For creating notes
    public class NoteCreateDto
    {
        public string Content { get; set; } = string.Empty;
    }

    // For updating notes
    public class NoteUpdateDto
    {
        public string Content { get; set; } = string.Empty;
    }

    // For reading notes
    public class NoteDto
    {
        public int NoteId { get; set; }
        public int UserId { get; set; }
        public string? Tconst { get; set; }
        public string? Nconst { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime NotedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}