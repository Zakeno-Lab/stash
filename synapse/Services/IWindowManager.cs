namespace synapse.Services
{
    /// <summary>
    /// Interface for managing window operations in an MVVM-compliant way
    /// </summary>
    public interface IWindowManager
    {
        /// <summary>
        /// Shows the main application window
        /// </summary>
        void ShowMainWindow();
        
        /// <summary>
        /// Hides the main application window
        /// </summary>
        void HideMainWindow();
        
        /// <summary>
        /// Gets whether the main window is currently visible
        /// </summary>
        bool IsMainWindowVisible { get; }
        
        /// <summary>
        /// Activates the main window (brings it to front)
        /// </summary>
        void ActivateMainWindow();
        
        /// <summary>
        /// Initializes clipboard service with window handle when window is ready
        /// </summary>
        /// <param name="windowHandle">The window handle for clipboard monitoring</param>
        void InitializeClipboardListener(nint windowHandle);
    }
}