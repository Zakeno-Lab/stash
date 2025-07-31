using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using synapse.ViewModels;
using synapse.Views;

namespace synapse.Services
{
    /// <summary>
    /// Manager responsible for orchestrating the application startup process
    /// </summary>
    public class ApplicationStartupManager : IApplicationStartupManager
    {
        private readonly IDatabaseInitializationManager _databaseManager;

        public ApplicationStartupManager(IDatabaseInitializationManager databaseManager)
        {
            _databaseManager = databaseManager;
        }

        public async Task InitializeApplicationAsync(IHost host)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ApplicationStartupManager: Starting application initialization...");

                // Step 1: Initialize database
                await _databaseManager.InitializeDatabaseAsync(host.Services);

                // Step 2: Initialize global hotkeys
                InitializeGlobalHotkeys(host.Services);

                // Step 3: Initialize clipboard listener
                InitializeClipboardListener(host.Services);

                // Step 4: Initialize theme management
                InitializeThemeManagement(host.Services);

                // Step 5: Initialize tray icon
                InitializeTrayIcon(host.Services);

                System.Diagnostics.Debug.WriteLine("ApplicationStartupManager: Application initialization completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplicationStartupManager: Error during application initialization: {ex.Message}");
                throw;
            }
        }

        private void InitializeGlobalHotkeys(IServiceProvider serviceProvider)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ApplicationStartupManager: Initializing global hotkeys...");
                serviceProvider.GetRequiredService<IGlobalHotkeyService>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplicationStartupManager: Error initializing global hotkeys: {ex.Message}");
                throw;
            }
        }

        private void InitializeClipboardListener(IServiceProvider serviceProvider)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ApplicationStartupManager: Initializing clipboard listener...");
                
                var clipboardService = serviceProvider.GetRequiredService<IClipboardService>();
                var mainWindowService = serviceProvider.GetRequiredService<IMainWindowService>();
                var mainWindow = mainWindowService.GetMainWindow();
                
                clipboardService.StartListener(new WindowInteropHelper(mainWindow).EnsureHandle());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplicationStartupManager: Error initializing clipboard listener: {ex.Message}");
                throw;
            }
        }

        private void InitializeThemeManagement(IServiceProvider serviceProvider)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ApplicationStartupManager: Initializing theme management...");
                
                var mainWindowService = serviceProvider.GetRequiredService<IMainWindowService>();
                var mainWindow = mainWindowService.GetMainWindow();
                
                SystemThemeWatcher.Watch(
                    mainWindow,
                    WindowBackdropType.Mica,
                    true
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplicationStartupManager: Error initializing theme management: {ex.Message}");
                // Theme management failure shouldn't crash the app
            }
        }

        private void InitializeTrayIcon(IServiceProvider serviceProvider)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("ApplicationStartupManager: Initializing tray icon...");
                
                var trayIcon = (Hardcodet.Wpf.TaskbarNotification.TaskbarIcon)Application.Current.FindResource("AppTrayIcon");
                if (trayIcon != null)
                {
                    var trayIconViewModel = serviceProvider.GetRequiredService<TrayIconViewModel>();
                    trayIcon.DataContext = trayIconViewModel;
                    System.Diagnostics.Debug.WriteLine("ApplicationStartupManager: Tray icon initialized successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ApplicationStartupManager: WARNING - Tray icon resource not found!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ApplicationStartupManager: Error initializing tray icon: {ex.Message}");
                // Tray icon failure shouldn't crash the app
            }
        }
    }
} 