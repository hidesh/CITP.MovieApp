using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CITP.MovieApp.Infrastructure.Persistence;

namespace CITP.MovieApp.Tests_.Integration
{

    /// Simple WebApplicationFactory that uses a local PostgreSQL for testing
    /// and rolls back all changes after each test.
    
    public class TestApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private IServiceScope? _scope;
        private AppDbContext? _db;
        private IDbContextTransaction? _transaction;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true);
            });

            builder.ConfigureServices((context, services) =>
            {
                // Remove existing DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                var connectionString = context.Configuration.GetConnectionString("TestConnection");

                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException("TestConnection not found in appsettings.Test.json.");

                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(connectionString));
            });
        }

        public async Task InitializeAsync()
        {
            _scope = Services.CreateScope();
            _db = _scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Ensure database exists before opening connection
            await _db.Database.EnsureCreatedAsync();

            await _db.Database.OpenConnectionAsync();
            _transaction = await _db.Database.BeginTransactionAsync();
        }

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
