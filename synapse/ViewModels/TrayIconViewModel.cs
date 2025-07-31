using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using synapse.Services;

namespace synapse.ViewModels
{
    public partial class TrayIconViewModel : ObservableObject
    {
        private readonly IApplicationService _applicationService;

        public TrayIconViewModel(IApplicationService applicationService)
        {
            _applicationService = applicationService;
            System.Diagnostics.Debug.WriteLine("TrayIconViewModel: Created successfully");
        }

        [RelayCommand]
        private void ExitApplication()
        {
            System.Diagnostics.Debug.WriteLine("TrayIconViewModel: ExitApplication command called");
            _applicationService.Shutdown();
        }
    }
} 