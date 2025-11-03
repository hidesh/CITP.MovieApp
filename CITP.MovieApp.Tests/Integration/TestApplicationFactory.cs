using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CITP.MovieApp.Infrastructure.Persistence;

namespace CITP.MovieApp.Tests_.Integration
{
   
    /// Small WebApplicationFactory for integration testing.
    /// Uses DB environmental keys for PostgreSQL from appsettings.test.json (like we have the appsettings.json before .env,
    /// you'll have to set this up locally yourselves)
    
    public class TestApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private IServiceScope? _scope;
        private AppDbContext? _db;
        private IDbContextTransaction? _transaction;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Load the regular and test-specific appsettings
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: true)
                      .AddEnvironmentVariables();
            });

            builder.ConfigureServices((context, services) =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Determine which connection string to use
                var connectionString =
                    context.Configuration.GetConnectionString("TestConnection") ??
                    context.Configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException("No connection string found for integration testing.");

                // Register the real PostgreSQL context
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(connectionString));
            });
        }

        // ------------------------
        //  Transaction Management
        // ------------------------
        
        /// Begins a new database transaction before each test.
        public async Task InitializeAsync()
        {
            _scope = Services.CreateScope();
            _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await _db.Database.OpenConnectionAsync();
            _transaction = await _db.Database.BeginTransactionAsync();
        }

     
        /// Rolls back the active database transaction after each test.
        public async Task DisposeAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
            if (_db != null)
                await _db.Database.CloseConnectionAsync();

            _scope?.Dispose();
        }
    }
}
