namespace CITP.MovieApp.Infrastructure.Utils
{
    public static class TitleTypeHelper
    {
        public static bool IsSeries(string? type)
        {
            if (string.IsNullOrWhiteSpace(type)) return false;
            type = type.ToLower();

            return type.Contains("series") || type == "tvseries" || type == "tvminiseries";
        }

        public static bool IsEpisode(string? type)
        {
            if (string.IsNullOrWhiteSpace(type)) return false;
            type = type.ToLower();

            return type == "tvepisode" || type.Contains("episode");
        }

        public static bool IsMovie(string? type)
        {
            if (string.IsNullOrWhiteSpace(type)) return false;
            type = type.ToLower();

            return type == "movie" || type == "short" || type == "tvmovie" || type == "video";
        }
    }
}