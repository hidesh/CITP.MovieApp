using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITP.MovieApp.Domain.Entities

{
    [Table("wordindex")]
    public class WordIndex
    {
        [Key]
        [Column("wordindex_id")]
        public int WordIndexId { get; set; }

        [Column("tconst")]
        public string Tconst { get; set; } = null!; // References Title

        [Column("word")]
        public string Word { get; set; } = null!;

        [Column("lemma")]
        public string Lemma { get; set; } = null!;

        [Column("occurrences")]
        public int Occurrences { get; set; }

        // Navigation property
        public Title? Title { get; set; }
    }
}