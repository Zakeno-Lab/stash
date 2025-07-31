using System.Threading.Tasks;

namespace synapse.Services
{
    /// <summary>
    /// Service for managing application lifecycle operations
    /// </summary>
    public interface IApplicationService
    {
        /// <summary>
        /// Initiates application shutdown
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Initiates application shutdown asynchronously
        /// </summary>
        Task ShutdownAsync();

        /// <summary>
        /// Shows the main application window
        /// </summary>
        void ShowMainWindow();

        /// <summary>
        /// Hides the main application window
        /// </summary>
        void HideMainWindow();

        /// <summary>
        /// Checks if the main window is currently visible
        /// </summary>
        bool IsMainWindowVisible { get; }
    }
} 