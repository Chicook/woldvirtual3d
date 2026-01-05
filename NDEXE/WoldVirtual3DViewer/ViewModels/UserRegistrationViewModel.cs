using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WoldVirtual3DViewer.Models;
using WoldVirtual3DViewer.Services;
using WoldVirtual3DViewer.Utils;

namespace WoldVirtual3DViewer.ViewModels
{
    public class UserRegistrationViewModel : ViewModelBase
    {
        private readonly DataService _dataService;
        private readonly INavigationService _navigationService;
        private readonly RegistrationContext _registrationContext;

        private string _username = string.Empty;
        public string Username { get => _username; set => SetProperty(ref _username, value); }

        private string _password = string.Empty;
        public string Password { get => _password; set => SetProperty(ref _password, value); }

        private string _confirmPassword = string.Empty;
        public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }

        private string _status = string.Empty;
        public string RegistrationStatus { get => _status; set => SetProperty(ref _status, value); }

        private bool _isRegistered;
        public bool IsRegistered { get => _isRegistered; set => SetProperty(ref _isRegistered, value); }

        public AvatarInfo? SelectedAvatar => _registrationContext.SelectedAvatar;

        public ICommand RegisterUserCommand { get; }
        public ICommand DownloadHashCommand { get; }

        public UserRegistrationViewModel(
            DataService dataService,
            INavigationService navigationService,
            RegistrationContext registrationContext)
        {
            _dataService = dataService;
            _navigationService = navigationService;
            _registrationContext = registrationContext;

            RegisterUserCommand = new RelayCommand(async () => await RegisterUserAsync());
            DownloadHashCommand = new RelayCommand(DownloadUserHash);
        }

        private async Task RegisterUserAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                System.Windows.MessageBox.Show("Complete todos los campos.");
                return;
            }
            if (Password != ConfirmPassword)
            {
                System.Windows.MessageBox.Show("Las contraseñas no coinciden.");
                return;
            }
            if (_registrationContext.PCInfo == null)
            {
                System.Windows.MessageBox.Show("Error: Falta información del PC. Reiniciando proceso.");
                _navigationService.NavigateTo<PCRegistrationViewModel>();
                return;
            }

            try
            {
                RegistrationStatus = "Registrando usuario...";
                
                // Capture values for thread safety
                string u = Username;
                string p = Password;
                string a = SelectedAvatar?.Type ?? "chica";
                string h = _registrationContext.PCInfo.UniqueHash ?? "unknown";

                await Task.Run(() =>
                {
                    var account = new UserAccount
                    {
                        Username = u,
                        AvatarType = a,
                        PCUniqueHash = h,
                        IsValidated = true
                    };
                    account.SetPassword(p);
                    account.GenerateAccountHash();
                    _dataService.SaveUserAccount(account);
                });

                IsRegistered = true;
                RegistrationStatus = "Usuario registrado.";
                System.Windows.MessageBox.Show("Usuario registrado correctamente. Iniciando sesión...");
                
                _navigationService.NavigateTo<LoginViewModel>();
            }
            catch (Exception ex)
            {
                RegistrationStatus = "Error en registro";
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void DownloadUserHash()
        {
            var userAccount = _dataService.LoadUserAccount();
            if (userAccount == null)
            {
                System.Windows.MessageBox.Show("No se encontró información de usuario para descargar.");
                return;
            }

            try
            {
                string path = _dataService.CreateUserAccountZip(userAccount);
                var dialog = new Microsoft.Win32.SaveFileDialog { FileName = $"user_{userAccount.AccountHash}.zip" };
                if (dialog.ShowDialog() == true)
                {
                    System.IO.File.Copy(path, dialog.FileName, true);
                    System.Windows.MessageBox.Show("Archivo de usuario guardado exitosamente.");
                }
            }
            catch (Exception ex) 
            { 
                System.Windows.MessageBox.Show($"Error al guardar archivo: {ex.Message}"); 
            }
        }
    }
}
