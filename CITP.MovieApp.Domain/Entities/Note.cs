using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CITP.MovieApp.Domain.Entities

{
    [Table("note")]
    public class Note
    {
        [Key]
        [Column("note_id")]
        public int NoteId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("tconst")]
        public string? Tconst { get; set; } // Nullable if note is about a person

        [Column("nconst")]
        public string? Nconst { get; set; } // Nullable if note is about a title

        [Column("content")]
        public string Content { get; set; } = null!;

        [Column("noted_at")]
        public DateTime NotedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public Title? Title { get; set; }
        public Person? Person { get; set; }
    }
}