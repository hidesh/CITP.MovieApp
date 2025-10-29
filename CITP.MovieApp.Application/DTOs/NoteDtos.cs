namespace CITP.MovieApp.Application.DTOs;

public class NoteDto
{
    public int NoteId { get; set; }
    public int UserId { get; set; }
    public string? Tconst { get; set; }
    public string? Nconst { get; set; }
    public string Content { get; set; } = null!;
    public DateTime NotedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}