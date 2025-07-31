using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using synapse.Services;

namespace synapse
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IHost? _host;
        public IServiceProvider Services => _host?.Services ?? throw new InvalidOperationException("Host not initialized");

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                System.Diagnostics.Debug.WriteLine("App: Starting application initialization...");

                // Build the host with service registration
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                        // Use the service registration manager to handle all DI setup
                        var serviceRegistrationManager = new ServiceRegistrationManager();
                        serviceRegistrationManager.RegisterServices(services);
                })
                .Build();

                // Start the host
            await _host.StartAsync();

                // Use the application startup manager to handle all initialization
                var startupManager = _host.Services.GetRequiredService<IApplicationStartupManager>();
                await startupManager.InitializeApplicationAsync(_host);

                System.Diagnostics.Debug.WriteLine("App: Application initialization completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"App: Critical error during startup: {ex.Message}");
                
                // Show error message to user and shutdown gracefully
                MessageBox.Show($"Application failed to start: {ex.Message}", 
                    "Startup Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                
                Shutdown();
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            try
            {
                // Dispose of tray icon first
                if (FindResource("AppTrayIcon") is Hardcodet.Wpf.TaskbarNotification.TaskbarIcon trayIcon)
                {
                    trayIcon.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing tray icon: {ex.Message}");
            }
            
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
            base.OnExit(e);
        }
    }
}
