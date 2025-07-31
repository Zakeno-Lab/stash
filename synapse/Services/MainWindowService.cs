using System;
using Microsoft.Extensions.DependencyInjection;
using synapse.Views;

namespace synapse.Services
{
    /// <summary>
    /// Service that manages the lifecycle of the main application window
    /// Ensures only one main window exists throughout the application lifetime
    /// </summary>
    public class MainWindowService : IMainWindowService
    {
        private readonly IServiceProvider _serviceProvider;
        private ClipboardHistoryWindow? _mainWindow;

        public MainWindowService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ClipboardHistoryWindow GetMainWindow()
        {
            if (_mainWindow == null)
            {
                System.Diagnostics.Debug.WriteLine("MainWindowService: Creating main window instance");
                _mainWindow = _serviceProvider.GetRequiredService<ClipboardHistoryWindow>();
            }
            
            return _mainWindow;
        }

        public bool IsMainWindowCreated => _mainWindow != null;
    }
}