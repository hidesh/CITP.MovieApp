namespace CITP.MovieApp.Application.DTOs
{
    public class BookmarkDto
    {
        public int BookmarkId { get; set; }
        public int UserId { get; set; }
        public string? Tconst { get; set; }
        public string? Nconst { get; set; }
        public DateTime BookmarkedAt { get; set; }
        public string? Title { get; set; }
        public string? PosterUrl { get; set; }
    }
    public class CreateBookmarkDto
    {
        public int UserId { get; set; }
        public string? Tconst { get; set; }
        public string? Nconst { get; set; }
    }
}