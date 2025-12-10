namespace CITP.MovieApp.Infrastructure.Utils
{
    /// <summary>
    /// Helper class for determining title type categories based on IMDb titleType values.
    /// Centralizes the logic for categorizing titles as movies, series, or episodes.
    /// </summary>
    public static class TitleTypeHelper
    {
        /// <summary>
        /// Determines if the titleType represents a movie (including shorts, videos, TV movies, and video games).
        /// </summary>
        public static bool IsMovie(string? titleType)
        {
            if (string.IsNullOrEmpty(titleType))
                return false;

            var type = titleType.ToLower();
            return type == "movie" || type == "short" || type == "video" || type == "tvmovie" || type == "videogame";
        }

        /// <summary>
        /// Determines if the titleType represents a series (excluding episodes).
        /// </summary>
        public static bool IsSeries(string? titleType)
        {
            if (string.IsNullOrEmpty(titleType))
                return false;

            var type = titleType.ToLower();
            return type.Contains("series") && type != "tvepisode";
        }

        /// <summary>
        /// Determines if the titleType represents an episode.
        /// </summary>
        public static bool IsEpisode(string? titleType)
        {
            if (string.IsNullOrEmpty(titleType))
                return false;

            return titleType.ToLower() == "tvepisode";
        }

        /// <summary>
        /// Gets the category name for a given titleType (Movie, Series, Episode, or Unknown).
        /// </summary>
        public static string GetCategory(string? titleType)
        {
            if (IsMovie(titleType)) return "Movie";
            if (IsSeries(titleType)) return "Series";
            if (IsEpisode(titleType)) return "Episode";
            return "Unknown";
        }
    }
}
