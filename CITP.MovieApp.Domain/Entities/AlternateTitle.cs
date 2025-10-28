using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITP.MovieApp.Domain.Entities;

[Table("alternate_title")]
public class AlternateTitle
{
    [Key]
    [Column("alt_id")]
    public int AltId { get; set; }

    [Column("title_id")]
    public string TitleId { get; set; } = null!;

    [Column("title")]
    public string? Title { get; set; }

    [Column("region")]
    public string? Region { get; set; }

    [Column("language")]
    public string? Language { get; set; }

    [Column("types")]
    public string? Types { get; set; }

    [Column("attributes")]
    public string? Attributes { get; set; }

    [Column("isoriginaltitle")]
    public bool? IsOriginalTitle { get; set; }

    // Navigation property
    public Title? TitleRef { get; set; }  
}

