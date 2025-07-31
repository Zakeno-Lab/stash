using NHotkey;
using NHotkey.Wpf;
using System.Windows.Input;

namespace synapse.Services
{
    public class GlobalHotkeyService : IGlobalHotkeyService
    {
        private readonly IApplicationService _applicationService;

        public GlobalHotkeyService(IApplicationService applicationService)
        {
            _applicationService = applicationService;
            try
            {
                HotkeyManager.Current.AddOrReplace("ShowStash", Key.C, ModifierKeys.Control | ModifierKeys.Shift, OnShowStash);
            }
            catch (HotkeyAlreadyRegisteredException)
            {
                // Handle exception if needed
            }
        }

        private void OnShowStash(object? sender, HotkeyEventArgs e)
        {
            _applicationService.ShowMainWindow();
        }
    }
} 