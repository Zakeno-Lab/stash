using System;
using System.Windows;
using synapse.Views;

namespace synapse.Services
{
    /// <summary>
    /// Implementation of window management operations for MVVM compliance
    /// </summary>
    public class WindowManager : IWindowManager
    {
        private readonly IClipboardService _clipboardService;
        private readonly IMainWindowService _mainWindowService;

        public WindowManager(IClipboardService clipboardService, IMainWindowService mainWindowService)
        {
            _clipboardService = clipboardService;
            _mainWindowService = mainWindowService;
        }

        public void ShowMainWindow()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("WindowManager: ShowMainWindow called");
                
                var mainWindow = _mainWindowService.GetMainWindow();
                mainWindow.Show();
                mainWindow.Activate();
                
                if (mainWindow.WindowState == WindowState.Minimized)
                {
                    mainWindow.WindowState = WindowState.Normal;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing main window: {ex.Message}");
            }
        }

        public void HideMainWindow()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("WindowManager: HideMainWindow called");
                
                if (_mainWindowService.IsMainWindowCreated)
                {
                    var mainWindow = _mainWindowService.GetMainWindow();
                    mainWindow.Hide();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error hiding main window: {ex.Message}");
            }
        }

        public bool IsMainWindowVisible
        {
            get
            {
                try
                {
                    if (_mainWindowService.IsMainWindowCreated)
                    {
                        var mainWindow = _mainWindowService.GetMainWindow();
                        return mainWindow.IsVisible;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error checking main window visibility: {ex.Message}");
                    return false;
                }
            }
        }

        public void ActivateMainWindow()
        {
            try
            {
                var mainWindow = _mainWindowService.GetMainWindow();
                mainWindow.Activate();
                
                if (mainWindow.WindowState == WindowState.Minimized)
                {
                    mainWindow.WindowState = WindowState.Normal;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error activating main window: {ex.Message}");
            }
        }

        public void InitializeClipboardListener(nint windowHandle)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("WindowManager: Initializing clipboard listener");
                _clipboardService.StartListener(windowHandle);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing clipboard listener: {ex.Message}");
            }
        }
    }
}