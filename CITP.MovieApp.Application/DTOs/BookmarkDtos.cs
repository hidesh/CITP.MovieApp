namespace CITP.MovieApp.Application.DTOs
{
    public class BookmarkDto
    {
        public int BookmarkId { get; set; }
        public int UserId { get; set; }
        public string? Tconst { get; set; }
        public string? Nconst { get; set; }
        public DateTime BookmarkedAt { get; set; }
        
        // For title bookmarks - normalized title fields
        public string? TitleType { get; set; }
        public string? MovieTitle { get; set; }
        public string? SeriesTitle { get; set; }
        public string? EpisodeTitle { get; set; }
        
        // For person bookmarks
        public string? PersonName { get; set; }
        
        public string? PosterUrl { get; set; }
    }
    public class CreateBookmarkDto
    {
        public int UserId { get; set; }
        public string? Tconst { get; set; }
        public string? Nconst { get; set; }
    }
}