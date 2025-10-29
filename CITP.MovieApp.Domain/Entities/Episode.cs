using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITP.MovieApp.Domain.Entities
{

[Table("episode")]
public class Episode
{
    [Key]
    [Column("episode_id")]
    public int EpisodeId { get; set; }

    [Column("tconst")]
    public string Tconst { get; set; } = null!; // Episode's own title ID

    [Column("parent_series_id")]
    public string ParentSeriesId { get; set; } = null!; // References the series title

    [Column("season_number")]
    public int SeasonNumber { get; set; }

    [Column("episode_number")]
    public int EpisodeNumber { get; set; }

    // Navigation properties
    public Title? Title { get; set; } // The episode itself
    public Title? ParentSeries { get; set; } // The series it belongs to
}
}