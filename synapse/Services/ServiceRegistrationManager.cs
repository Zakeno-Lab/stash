using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using synapse.Data;
using synapse.ViewModels;
using synapse.Views;

namespace synapse.Services
{
    /// <summary>
    /// Manager responsible for registering all application services in the DI container
    /// </summary>
    public class ServiceRegistrationManager : IServiceRegistrationManager
    {
        public void RegisterServices(IServiceCollection services)
        {
            System.Diagnostics.Debug.WriteLine("ServiceRegistrationManager: Registering services...");

            // Register core services
            RegisterCoreServices(services);

            // Register application services
            RegisterApplicationServices(services);

            // Register ViewModels
            RegisterViewModels(services);

            // Register Views
            RegisterViews(services);

            // Register data services
            RegisterDataServices(services);

            System.Diagnostics.Debug.WriteLine("ServiceRegistrationManager: All services registered successfully");
        }

        private void RegisterCoreServices(IServiceCollection services)
        {
            // UI Framework services
            services.AddSingleton<Wpf.Ui.IThemeService, Wpf.Ui.ThemeService>();
            services.AddSingleton<Wpf.Ui.ISnackbarService, Wpf.Ui.SnackbarService>();
        }

        private void RegisterApplicationServices(IServiceCollection services)
        {
            // Application lifecycle services
            services.AddSingleton<IApplicationService, ApplicationService>();
            
            // Startup managers
            services.AddSingleton<IServiceRegistrationManager, ServiceRegistrationManager>();
            services.AddSingleton<IDatabaseInitializationManager, DatabaseInitializationManager>();
            services.AddSingleton<IApplicationStartupManager, ApplicationStartupManager>();
            
            // Communication services
            services.AddSingleton<IEventBus, EventBus>();
            
            // Feature services
            services.AddSingleton<IGlobalHotkeyService, GlobalHotkeyService>();
            services.AddSingleton<ITrayIconService, TrayIconService>();
            services.AddSingleton<IDataService, DataService>();
            services.AddSingleton<IClipboardService, ClipboardService>();
            services.AddSingleton<IWindowManager, WindowManager>();
            services.AddSingleton<ISearchService, SearchService>();
            services.AddSingleton<IProgressiveSearchService, ProgressiveSearchService>();
            services.AddSingleton<IApplicationIconService, ApplicationIconService>();
            
            // Window lifecycle management
            services.AddSingleton<IMainWindowService, MainWindowService>();
        }

        private void RegisterViewModels(IServiceCollection services)
        {
            // ViewModels should be transient to avoid state corruption and memory leaks
            // Each time a ViewModel is requested, a new instance is created with fresh state
            services.AddTransient<ClipboardHistoryViewModel>();
            services.AddTransient<TrayIconViewModel>();
        }

        private void RegisterViews(IServiceCollection services)
        {
            // Views should be transient to support multiple windows and avoid memory leaks
            // Each time a View is requested, a new window instance is created
            services.AddTransient<ClipboardHistoryWindow>();
        }

        private void RegisterDataServices(IServiceCollection services)
        {
            services.AddDbContextFactory<AppDbContext>();
        }
    }
} 