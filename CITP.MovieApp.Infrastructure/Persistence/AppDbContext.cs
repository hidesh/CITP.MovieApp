using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CITP.MovieApp.Domain;
using Microsoft.EntityFrameworkCore;

namespace CITP.MovieApp.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<MoviePerson> MoviePeople => Set<MoviePerson>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Movie>().HasKey(x => x.TConst);
        mb.Entity<Person>().HasKey(x => x.NConst);

        mb.Entity<MoviePerson>().HasKey(x => new { x.MovieTConst, x.PersonNConst });

        mb.Entity<MoviePerson>()
            .HasOne(mp => mp.Movie)
            .WithMany(m => m.Principals)
            .HasForeignKey(mp => mp.MovieTConst);

        mb.Entity<MoviePerson>()
            .HasOne(mp => mp.Person)
            .WithMany(p => p.Titles)
            .HasForeignKey(mp => mp.PersonNConst);
    }
}
