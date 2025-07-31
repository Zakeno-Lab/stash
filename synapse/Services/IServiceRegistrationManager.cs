using Microsoft.Extensions.DependencyInjection;

namespace synapse.Services
{
    /// <summary>
    /// Manager responsible for registering all application services in the DI container
    /// </summary>
    public interface IServiceRegistrationManager
    {
        /// <summary>
        /// Registers all application services in the provided service collection
        /// </summary>
        /// <param name="services">The service collection to register services in</param>
        void RegisterServices(IServiceCollection services);
    }
} 