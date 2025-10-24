using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Domain;
using CITP.MovieApp.Infrastructure.Persistence;
using CITP.MovieApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF InMemory (SKAL skiftes til Postgres senere!!!)
bool useInMemory = builder.Configuration.GetValue("UseInMemory", true);
if (useInMemory)
{
    builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("citp"));
}
else
{
    // senere: builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
}

builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IPersonRepository, PersonRepository>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed lidt data til demo :P
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (useInMemory && !db.Movies.Any())
    {
        var m1 = new Movie { TConst = "tt1375666", PrimaryTitle = "Inception", StartYear = 2010, AverageRating = 8.8f, NumVotes = 2000000 };
        var m2 = new Movie { TConst = "tt0816692", PrimaryTitle = "Interstellar", StartYear = 2014, AverageRating = 8.6f, NumVotes = 1800000 };
        var m3 = new Movie { TConst = "tt0080684", PrimaryTitle = "The Empire Strikes Back", StartYear = 1980, AverageRating = 8.7f, NumVotes = 1400000 };
        var m4 = new Movie { TConst = "tt0086190", PrimaryTitle = "Return of the Jedi", StartYear = 1983, AverageRating = 8.3f, NumVotes = 1300000 };
        db.Movies.AddRange(m1, m2, m3, m4);

        var p1 = new Person { NConst = "nm0634240", PrimaryName = "Christopher Nolan" };
        var p2 = new Person { NConst = "nm0000138", PrimaryName = "Leonardo DiCaprio" };
        db.People.AddRange(p1, p2);

        db.MoviePeople.AddRange(
            new MoviePerson { MovieTConst = m1.TConst, PersonNConst = p1.NConst, Category = "director" },
            new MoviePerson { MovieTConst = m1.TConst, PersonNConst = p2.NConst, Category = "actor" }
        );

        db.SaveChanges();
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();