using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using synapse.Data;

namespace synapse.Services
{
    /// <summary>
    /// Manager responsible for database initialization and migrations
    /// </summary>
    public class DatabaseInitializationManager : IDatabaseInitializationManager
    {
        public async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("DatabaseInitializationManager: Starting database initialization...");

                // Apply database migrations
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                System.Diagnostics.Debug.WriteLine("DatabaseInitializationManager: Applying migrations...");
                await dbContext.Database.MigrateAsync();
                
                System.Diagnostics.Debug.WriteLine("DatabaseInitializationManager: Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DatabaseInitializationManager: Error during database initialization: {ex.Message}");
                
                // Log the error but don't crash the application
                // In a production app, you might want to show a user-friendly error message
                throw new InvalidOperationException("Failed to initialize database", ex);
            }
        }
    }
} 