using synapse.Views;

namespace synapse.Services
{
    /// <summary>
    /// Service for managing the main application window lifecycle
    /// </summary>
    public interface IMainWindowService
    {
        /// <summary>
        /// Gets the main window instance, creating it if it doesn't exist
        /// </summary>
        ClipboardHistoryWindow GetMainWindow();
        
        /// <summary>
        /// Gets whether the main window has been created
        /// </summary>
        bool IsMainWindowCreated { get; }
    }
}