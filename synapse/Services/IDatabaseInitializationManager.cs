using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace synapse.Services
{
    /// <summary>
    /// Manager responsible for database initialization and migrations
    /// </summary>
    public interface IDatabaseInitializationManager
    {
        /// <summary>
        /// Initializes the database and applies any pending migrations
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve dependencies</param>
        Task InitializeDatabaseAsync(IServiceProvider serviceProvider);
    }
} 