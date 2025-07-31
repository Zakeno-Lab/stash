using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace synapse.Services
{
    /// <summary>
    /// Manager responsible for orchestrating the application startup process
    /// </summary>
    public interface IApplicationStartupManager
    {
        /// <summary>
        /// Initializes all application components in the correct order
        /// </summary>
        /// <param name="host">The application host</param>
        Task InitializeApplicationAsync(IHost host);
    }
} 