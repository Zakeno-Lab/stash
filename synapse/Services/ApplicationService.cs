using System;
using System.Threading.Tasks;
using System.Windows;

namespace synapse.Services
{
    /// <summary>
    /// Service for managing application lifecycle operations
    /// </summary>
    public class ApplicationService : IApplicationService
    {
        private readonly IWindowManager _windowManager;

        public ApplicationService(IWindowManager windowManager)
        {
            _windowManager = windowManager;
            System.Diagnostics.Debug.WriteLine("ApplicationService: Created successfully");
        }

        public void Shutdown()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ApplicationService: Initiating shutdown...");
                
                // For WPF applications, we need to shutdown the Application directly
                // The host lifetime will be handled by the App.xaml.cs OnExit method
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during application shutdown: {ex.Message}");
                // Fallback - try direct shutdown
                try
                {
                    Application.Current?.Shutdown();
                }
                catch (Exception fallbackEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Fallback shutdown also failed: {fallbackEx.Message}");
                }
            }
        }

        public async Task ShutdownAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ApplicationService: Initiating async shutdown...");
                
                // Wait a short time for any pending operations
                await Task.Delay(500);
                
                // Shutdown the application on the UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during async application shutdown: {ex.Message}");
                // Fallback to direct shutdown
                try
                {
                    Application.Current?.Shutdown();
                }
                catch (Exception fallbackEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Async fallback shutdown also failed: {fallbackEx.Message}");
                }
            }
        }

        public void ShowMainWindow()
        {
            _windowManager.ShowMainWindow();
        }

        public void HideMainWindow()
        {
            _windowManager.HideMainWindow();
        }

        public bool IsMainWindowVisible => _windowManager.IsMainWindowVisible;
    }
} 