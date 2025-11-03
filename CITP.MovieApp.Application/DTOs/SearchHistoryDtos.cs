namespace CITP.MovieApp.Application.DTOs
{
    public class SearchHistoryDto
    {
        public string? Title { get; set; }
        public string? Tconst { get; set; }
        public DateTime VisitedAt { get; set; }
    }

    public class CreateSearchHistoryDto
    {
        public int UserId { get; set; }
        public string? Tconst { get; set; }
    }
}