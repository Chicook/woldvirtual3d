using System;
using System.Threading.Tasks;
using System.Windows;
using WoldVirtual3DViewer.Services;

namespace WoldVirtual3DViewer.ViewModels
{
    public class LoadingViewModel : ViewModelBase
    {
        private readonly DataService _dataService;
        private readonly INavigationService _navigationService;
        private readonly RegistrationContext _registrationContext;

        private string _statusMessage = "Inicializando...";
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        public LoadingViewModel(
            DataService dataService, 
            INavigationService navigationService,
            RegistrationContext registrationContext)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            _registrationContext = registrationContext;

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                await Task.Delay(1000); // UI breathing room

                await Task.Run(() =>
                {
                    // Check User Account
                    var userAccount = _dataService.LoadUserAccount();
                    if (userAccount != null && userAccount.IsValidated)
                    {
                        Application.Current.Dispatcher.Invoke(() => 
                            _navigationService.NavigateTo<LoginViewModel>());
                        return;
                    }

                    // Check PC Info
                    var pcInfo = _dataService.LoadPCInfo();
                    if (pcInfo != null)
                    {
                        _registrationContext.PCInfo = pcInfo;
                        Application.Current.Dispatcher.Invoke(() => 
                            _navigationService.NavigateTo<AvatarSelectionViewModel>());
                    }
                    else
                    {
                        Application.Current.Dispatcher.Invoke(() => 
                            _navigationService.NavigateTo<PCRegistrationViewModel>());
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error durante la inicialización: {ex.Message}");
                Application.Current.Dispatcher.Invoke(() => 
                    _navigationService.NavigateTo<PCRegistrationViewModel>());
            }
        }
    }
}
