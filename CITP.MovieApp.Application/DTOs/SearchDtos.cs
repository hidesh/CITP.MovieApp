namespace CITP.MovieApp.Application.DTOs
{
    public class SearchResultDto
    {
        public string Tconst { get; set; } = null!;
        public string PrimaryTitle { get; set; } = null!;
        public long? MatchCount { get; set; } // Only for best_match results
    }

    public class SearchRequestDto
    {
        public string Query { get; set; } = null!;
    }
}
