using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WoldVirtual3DViewer.Models;
using WoldVirtual3DViewer.Services;
using WoldVirtual3DViewer.Utils;

namespace WoldVirtual3DViewer.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly DataService _dataService;
        private readonly GodotService _godotService;
        private readonly INavigationService _navigationService;

        private string _username = string.Empty;
        public string LoginUsername { get => _username; set => SetProperty(ref _username, value); }

        private string _password = string.Empty;
        public string LoginPassword { get => _password; set => SetProperty(ref _password, value); }

        private string _status = string.Empty;
        public string LoginStatus { get => _status; set => SetProperty(ref _status, value); }

        private bool _isLoggedIn;
        public bool IsLoggedIn { get => _isLoggedIn; set => SetProperty(ref _isLoggedIn, value); }

        public ICommand LoginCommand { get; }

        public LoginViewModel(
            DataService dataService, 
            GodotService godotService,
            INavigationService navigationService)
        {
            _dataService = dataService;
            _godotService = godotService;
            _navigationService = navigationService;

            LoginCommand = new RelayCommand(async () => await LoginAsync());
            
            // Pre-fill if possible (optional, maybe passed via context later)
        }

        private async Task LoginAsync()
        {
            LoginStatus = "Verificando...";
            try
            {
                bool success = false;
                UserAccount? account = null;
                
                string u = LoginUsername;
                string p = LoginPassword;

                await Task.Run(() =>
                {
                    account = _dataService.LoadUserAccount();
                    if (account != null && account.IsValidated && 
                        account.Username == u && account.VerifyPassword(p))
                    {
                        success = true;
                    }
                });

                if (success && account != null)
                {
                    IsLoggedIn = true;
                    LoginStatus = "Conectado. Cargando vista de juego...";
                    
                    // En lugar de lanzar Godot aquí, navegamos a la vista del juego
                    _navigationService.NavigateTo<GameViewModel>();
                }
                else
                {
                    LoginStatus = "Credenciales incorrectas.";
                    MessageBox.Show("Usuario o contraseña incorrectos.");
                }
            }
            catch (Exception ex)
            {
                LoginStatus = "Error";
                Logger.LogError("Login error", ex);
                MessageBox.Show(ex.Message);
            }
        }

        private async Task LaunchGodotAsync(UserAccount account)
        {
            try 
            {
                if (!_godotService.IsGodotAvailable())
                {
                    // Manual search logic
                    if (MessageBox.Show("No se encuentra Godot.exe. ¿Buscar manualmente?", "Godot", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Godot.exe|*.exe" };
                        if (dialog.ShowDialog() == true)
                        {
                            _godotService.SetGodotExecutablePath(dialog.FileName);
                        }
                        else return;
                    }
                    else return;
                }

                if (!_godotService.IsProjectValid())
                {
                    MessageBox.Show("No se encuentra el proyecto (project.godot).");
                    return;
                }

                bool launched = await _godotService.LaunchGodotSceneAsync(account);
                if (launched)
                {
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al lanzar Godot: {ex.Message}");
            }
        }
    }
}
