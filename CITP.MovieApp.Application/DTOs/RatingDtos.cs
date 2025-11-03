namespace CITP.MovieApp.Application.DTOs
{
    // For creating ratings
    public class RatingCreateDto
    {
        public decimal Rating { get; set; }
    }

    // For updating ratings
    public class RatingUpdateDto
    {
        public decimal Rating { get; set; }
    }

    // For reading ratings
    public class RatingDto
    {
        public int RatingId { get; set; }
        public int UserId { get; set; }
        public string Tconst { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public DateTime RatedAt { get; set; }
    }

    // For reading movie ratings (combines IMDb + user ratings)
    public class MovieRatingsDto
    {
        public string Tconst { get; set; } = string.Empty;
        public decimal? ImdbAverageRating { get; set; }
        public int? ImdbNumVotes { get; set; }
        public IEnumerable<RatingDto> UserRatings { get; set; } = new List<RatingDto>();
    }
}
