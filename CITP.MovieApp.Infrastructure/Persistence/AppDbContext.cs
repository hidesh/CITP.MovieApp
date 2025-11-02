
using Microsoft.EntityFrameworkCore;
using CITP.MovieApp.Domain.Entities;

namespace CITP.MovieApp.Infrastructure.Persistence
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        // DbSets
        public DbSet<User> Users => Set<User>();
        public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
        public DbSet<RatingHistory> RatingHistories => Set<RatingHistory>();
        public DbSet<SearchHistory> SearchHistories => Set<SearchHistory>();
        public DbSet<Title> Titles => Set<Title>();
        public DbSet<Genre> Genres => Set<Genre>();
        public DbSet<TitleGenre> TitleGenres => Set<TitleGenre>();
        public DbSet<Person> Persons => Set<Person>();
        public DbSet<PersonKnownFor> PersonKnownFors => Set<PersonKnownFor>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<AlternateTitle> AlternateTitles => Set<AlternateTitle>();
        public DbSet<Episode> Episodes => Set<Episode>();
        public DbSet<Note> Notes => Set<Note>();
        public DbSet<Rating> Ratings => Set<Rating>();
        public DbSet<TitleMetadata> TitleMetadatas => Set<TitleMetadata>();
        public DbSet<WordIndex> WordIndexes => Set<WordIndex>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /* ------------------------- USER ------------------------- */
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.UserId).HasColumnName("user_id").ValueGeneratedOnAdd();
                entity.Property(e => e.Username).HasColumnName("username").IsRequired();
                entity.Property(e => e.PasswordHash).HasColumnName("password").IsRequired();
                entity.Property(e => e.Email).HasColumnName("email").IsRequired();
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            /* ------------------------- TITLE ------------------------- */
            modelBuilder.Entity<Title>(entity =>
            {
                entity.ToTable("title");
                entity.HasKey(e => e.Tconst);
                entity.Property(e => e.Tconst).HasColumnName("tconst").ValueGeneratedNever();
                entity.Property(e => e.PrimaryTitle).HasColumnName("primarytitle").IsRequired();
                entity.Property(e => e.OriginalTitle).HasColumnName("originaltitle");
                entity.Property(e => e.IsAdult).HasColumnName("isadult");
                entity.Property(e => e.StartYear).HasColumnName("startyear");
                entity.Property(e => e.EndYear).HasColumnName("endyear");
                entity.Property(e => e.RuntimeMinutes).HasColumnName("runtimeminutes");
                entity.Property(e => e.TitleType).HasColumnName("titletype");
                entity.Property(e => e.ParentSeriesId).HasColumnName("parent_series_id");
            });

            /* ------------------------- BOOKMARK ------------------------- */
            modelBuilder.Entity<Bookmark>(entity =>
            {
                entity.ToTable("bookmark");
                entity.HasKey(e => e.BookmarkId);
                entity.Property(e => e.BookmarkId).HasColumnName("bookmark_id").ValueGeneratedOnAdd();
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(e => e.Tconst).HasColumnName("tconst");
                entity.Property(e => e.Nconst).HasColumnName("nconst");
                entity.Property(e => e.BookmarkedAt).HasColumnName("bookmarked_at");

                entity.HasOne(b => b.User).WithMany(u => u.Bookmarks).HasForeignKey(b => b.UserId);
                entity.HasOne(b => b.Title).WithMany(t => t.Bookmarks).HasForeignKey(b => b.Tconst);
                entity.HasOne(b => b.Person).WithMany(p => p.Bookmarks).HasForeignKey(b => b.Nconst);
            });

            /* ------------------------- RATING HISTORY ------------------------- */
            modelBuilder.Entity<RatingHistory>(entity =>
            {
                entity.ToTable("ratinghistory");
                entity.HasKey(e => e.RatingId);
                entity.Property(e => e.RatingId).HasColumnName("rating_id").ValueGeneratedOnAdd();
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(e => e.Tconst).HasColumnName("tconst");
                entity.Property(e => e.Rating).HasColumnName("rating").IsRequired();
                entity.Property(e => e.RatedAt).HasColumnName("rated_at");

                entity.HasOne(r => r.User).WithMany(u => u.RatingHistories).HasForeignKey(r => r.UserId);
                entity.HasOne(r => r.Title).WithMany(t => t.RatingHistories).HasForeignKey(r => r.Tconst);
            });

            /* ------------------------- SEARCH HISTORY ------------------------- */
            modelBuilder.Entity<SearchHistory>(entity =>
            {
                entity.ToTable("searchhistory");
                entity.HasKey(e => e.SearchId);
                entity.Property(e => e.SearchId).HasColumnName("search_id").ValueGeneratedOnAdd();
                entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
                entity.Property(e => e.Tconst).HasColumnName("tconst");
                entity.Property(e => e.VisitedAt).HasColumnName("visited_at");

                entity.HasOne(s => s.User).WithMany(u => u.SearchHistories).HasForeignKey(s => s.UserId);
                entity.HasOne(s => s.Title).WithMany(t => t.SearchHistories).HasForeignKey(s => s.Tconst);
            });

            /* ------------------------- GENRE ------------------------- */
            modelBuilder.Entity<Genre>(entity =>
            {
                entity.ToTable("genre");
                entity.HasKey(e => e.GenreId);
                entity.Property(e => e.GenreId).HasColumnName("genre_id").ValueGeneratedOnAdd();
                entity.Property(e => e.GenreName).HasColumnName("genre_name").IsRequired();
            });

            /* ------------------------- TITLE_GENRE ------------------------- */
            modelBuilder.Entity<TitleGenre>(entity =>
            {
                entity.ToTable("title_genre");
                entity.HasKey(e => new { e.TitleId, e.GenreId });
                entity.Property(e => e.TitleId).HasColumnName("title_id");
                entity.Property(e => e.GenreId).HasColumnName("genre_id");

                entity.HasOne(tg => tg.Title).WithMany(t => t.TitleGenres).HasForeignKey(tg => tg.TitleId);
                entity.HasOne(tg => tg.Genre).WithMany(g => g.TitleGenres).HasForeignKey(tg => tg.GenreId);
            });

            /* ------------------------- PERSON ------------------------- */
            modelBuilder.Entity<Person>(entity =>
            {
                entity.ToTable("person");
                entity.HasKey(e => e.Nconst);
                entity.Property(e => e.Nconst).HasColumnName("nconst");
                entity.Property(e => e.PrimaryName).HasColumnName("primaryname");
                entity.Property(e => e.BirthYear).HasColumnName("birthyear");
                entity.Property(e => e.DeathYear).HasColumnName("deathyear");
                entity.Property(e => e.PrimaryProfession).HasColumnName("primaryprofession");
            });

            /* ------------------------- PERSON_KNOWN_FOR ------------------------- */
            modelBuilder.Entity<PersonKnownFor>(entity =>
            {
                entity.ToTable("person_known_for");
                entity.HasKey(e => new { e.Nconst, e.Tconst });
                entity.Property(e => e.Nconst).HasColumnName("nconst");
                entity.Property(e => e.Tconst).HasColumnName("tconst");

                entity.HasOne(pkf => pkf.Person).WithMany(p => p.KnownFor).HasForeignKey(pkf => pkf.Nconst);
                entity.HasOne(pkf => pkf.Title).WithMany(t => t.KnownForPeople).HasForeignKey(pkf => pkf.Tconst);
            });

            /* ------------------------- ROLE ------------------------- */
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("role");
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.RoleId).HasColumnName("role_id").ValueGeneratedOnAdd();
                entity.Property(e => e.Nconst).HasColumnName("nconst").IsRequired();
                entity.Property(e => e.Tconst).HasColumnName("tconst").IsRequired();
                entity.Property(e => e.Job).HasColumnName("job");
                entity.Property(e => e.CharacterName).HasColumnName("character_name");

                entity.HasOne(r => r.Person).WithMany(p => p.Roles).HasForeignKey(r => r.Nconst);
                entity.HasOne(r => r.Title).WithMany(t => t.Roles).HasForeignKey(r => r.Tconst);
            });

            /* ------------------------- ALTERNATE_TITLE ------------------------- */
            modelBuilder.Entity<AlternateTitle>(entity =>
            {
                entity.ToTable("alternate_title");
                entity.HasKey(e => e.AltId);
                entity.Property(e => e.AltId).HasColumnName("alt_id").ValueGeneratedOnAdd();
                entity.Property(e => e.TitleId).HasColumnName("title_id");
                entity.Property(e => e.Title).HasColumnName("title");
                entity.Property(e => e.Region).HasColumnName("region");
                entity.Property(e => e.Language).HasColumnName("language");
                entity.Property(e => e.Types).HasColumnName("types");
                entity.Property(e => e.Attributes).HasColumnName("attributes");
                entity.Property(e => e.IsOriginalTitle).HasColumnName("isoriginaltitle");

                entity.HasOne(at => at.TitleRef).WithMany(t => t.AlternateTitles).HasForeignKey(at => at.TitleId);
            });

            /* ------------------------- EPISODE ------------------------- */
            modelBuilder.Entity<Episode>(entity =>
            {
                entity.ToTable("episode");
                entity.HasKey(e => e.EpisodeId);
                entity.Property(e => e.EpisodeId).HasColumnName("episode_id").ValueGeneratedOnAdd();
                entity.Property(e => e.Tconst).HasColumnName("tconst");
                entity.Property(e => e.ParentSeriesId).HasColumnName("parent_series_id");
                entity.Property(e => e.SeasonNumber).HasColumnName("season_number");
                entity.Property(e => e.EpisodeNumber).HasColumnName("episode_number");

                entity.HasOne(ep => ep.Title).WithMany(t => t.Episodes).HasForeignKey(ep => ep.Tconst);
            });

            /* ------------------------- NOTE ------------------------- */
            modelBuilder.Entity<Note>(entity =>
            {
                entity.ToTable("note");
                entity.HasKey(e => e.NoteId);
                entity.Property(e => e.NoteId).HasColumnName("note_id").ValueGeneratedOnAdd();
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Tconst).HasColumnName("tconst");
                entity.Property(e => e.Nconst).HasColumnName("nconst");
                entity.Property(e => e.Content).HasColumnName("content");
                entity.Property(e => e.NotedAt).HasColumnName("noted_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasOne(n => n.User).WithMany(u => u.Notes).HasForeignKey(n => n.UserId);
                entity.HasOne(n => n.Title).WithMany(t => t.Notes).HasForeignKey(n => n.Tconst);
                entity.HasOne(n => n.Person).WithMany(p => p.Notes).HasForeignKey(n => n.Nconst);
            });

            /* ------------------------- RATING ------------------------- */
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.ToTable("rating");
                entity.HasKey(e => e.Tconst);
                entity.Property(e => e.Tconst).HasColumnName("tconst");
                entity.Property(e => e.AverageRating).HasColumnName("averagerating");
                entity.Property(e => e.NumVotes).HasColumnName("numvotes");

                entity.HasOne(r => r.Title).WithOne(t => t.Ratings).HasForeignKey<Rating>(r => r.Tconst);
            });

            /* ------------------------- TITLE_METADATA ------------------------- */
            modelBuilder.Entity<TitleMetadata>(entity =>
            {
                entity.ToTable("title_metadata");
                entity.HasKey(e => e.Tconst);
                entity.Property(e => e.Tconst).HasColumnName("tconst");
                entity.Property(e => e.Plot).HasColumnName("plot");
                entity.Property(e => e.Rated).HasColumnName("rated");
                entity.Property(e => e.Language).HasColumnName("language");
                entity.Property(e => e.Released).HasColumnName("released");
                entity.Property(e => e.Writer).HasColumnName("writer");
                entity.Property(e => e.Country).HasColumnName("country");
                entity.Property(e => e.Production).HasColumnName("production");

                entity.HasOne(tm => tm.Title).WithOne(t => t.Metadatas).HasForeignKey<TitleMetadata>(tm => tm.Tconst);
            });

            /* ------------------------- WORDINDEX ------------------------- */
            modelBuilder.Entity<WordIndex>(entity =>
            {
                entity.ToTable("wordindex");
                entity.HasKey(e => e.WordIndexId);
                entity.Property(e => e.WordIndexId).HasColumnName("wordindex_id").ValueGeneratedOnAdd();
                entity.Property(e => e.Tconst).HasColumnName("tconst");
                entity.Property(e => e.Word).HasColumnName("word");
                entity.Property(e => e.Lemma).HasColumnName("lemma");
                entity.Property(e => e.Occurrences).HasColumnName("occurrences");

                entity.HasOne(w => w.Title).WithMany(t => t.WordIndexes).HasForeignKey(w => w.Tconst);
            });
        }
    }
}
