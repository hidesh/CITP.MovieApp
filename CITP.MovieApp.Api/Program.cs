using System.Collections;
using System.Text;
using CITP.MovieApp.Application.Abstractions;
using CITP.MovieApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using CITP.MovieApp.Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using DotNetEnv; 

var builder = WebApplication.CreateBuilder(args);

// Load environment variables first
DotNetEnv.Env.Load();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();



// Manually substitute ${VAR} placeholders in connection string
var config = builder.Configuration;
string connStr = config.GetConnectionString("DefaultConnection");

foreach (var kvp in Environment.GetEnvironmentVariables().Cast<DictionaryEntry>())
{
    connStr = connStr.Replace($"${{{kvp.Key}}}", kvp.Value?.ToString());
}

builder.Configuration["ConnectionStrings:DefaultConnection"] = connStr;

// Substitute ${VAR} placeholders in Jwt config
var jwtConfig = config.GetSection("Jwt");
foreach (var kvp in Environment.GetEnvironmentVariables().Cast<DictionaryEntry>())
{
    foreach (var jwtKeyName in new[] { "Key", "Issuer", "Audience", "ExpiryMinutes" })
    {
        var currentValue = jwtConfig[jwtKeyName];
        if (!string.IsNullOrWhiteSpace(currentValue))
            jwtConfig[jwtKeyName] = currentValue.Replace($"${{{kvp.Key}}}", kvp.Value?.ToString());
    }
}

// Convert ExpiryMinutes to int safely
int expiryMinutes = int.TryParse(jwtConfig["ExpiryMinutes"], out var val) ? val : 60;

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor(); // For accessing HTTP context in repositories


// Swagger configuration with JWT support
builder.Services.AddSwaggerGen(c =>
{
    // Enable XML documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// DbContext - PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IMovieRepository, MovieRepository>();
builder.Services.AddScoped<IBookmarkRepository, BookmarkRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddScoped<ISearchHistoryRepository, SearchHistoryRepository>();

// JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSection.GetValue<string>("Key")!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSection.GetValue<string>("Issuer"),
        ValidateAudience = true,
        ValidAudience = jwtSection.GetValue<string>("Audience"),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Remove default 5 minute clock skew
    };
});

// Add authorization fallback so all endpoints require login unless [AllowAnonymous]
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// CORS policy for local frontend during development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowFrontend");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
public partial class Program { }