using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using WoldVirtual3DViewer.Services;
using WoldVirtual3DViewer.ViewModels;
using WoldVirtual3DViewer.Views;

namespace WoldVirtual3DViewer
{
    public partial class App : System.Windows.Application
    {
        public IServiceProvider Services { get; }

        public App()
        {
            Services = ConfigureServices();
        }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            // Services
            services.AddSingleton<HardwareService>();
            services.AddSingleton<DataService>();
            services.AddSingleton<GodotService>();
            services.AddSingleton<RegistrationContext>();

            // Navigation Service with Factory
            services.AddSingleton<INavigationService>(provider =>
                new NavigationService(type => (ViewModelBase)provider.GetRequiredService(type)));

            // ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<LoadingViewModel>();
            services.AddTransient<PCRegistrationViewModel>();
            services.AddTransient<AvatarSelectionViewModel>();
            services.AddTransient<UserRegistrationViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<GameViewModel>(); // Registro del nuevo VM

            // Views
            services.AddTransient<MainWindow>();

            return services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = Services.GetRequiredService<MainWindow>();
            
            // Start navigation
            var navigationService = Services.GetRequiredService<INavigationService>();
            navigationService.NavigateTo<LoadingViewModel>();

            mainWindow.Show();
        }
    }
}
