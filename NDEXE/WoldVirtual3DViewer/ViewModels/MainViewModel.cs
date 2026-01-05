using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WoldVirtual3DViewer.Models;
using WoldVirtual3DViewer.Services;
using WoldVirtual3DViewer.Utils;

namespace WoldVirtual3DViewer.ViewModels
{
    public class MainViewModel : ViewModelBase, INavigationService
    {
        private readonly HardwareService _hardwareService;
        private readonly DataService _dataService;
        private readonly GodotService _godotService;

        private object _currentView;

        // Shared State for Registration Flow
        // We keep these here to share state between the wizard steps (PC -> Avatar -> User)
        // ideally this should be in a separate RegistrationContext object, but this works for now.
        public PCInfo? PCInfo { get; set; }
        public AvatarInfo? SelectedAvatar { get; set; }
        
        // These properties were used by child view models via binding to MainViewModel
        // We will keep them for compatibility but they should eventually move to specific VMs
        private string _pcRegistrationStatus = string.Empty;
        public string PCRegistrationStatus { get => _pcRegistrationStatus; set => SetProperty(ref _pcRegistrationStatus, value); }
        
        private bool _isPCRegistered;
        public bool IsPCRegistered { get => _isPCRegistered; set => SetProperty(ref _isPCRegistered, value); }

        private string _userRegistrationStatus = string.Empty;
        public string UserRegistrationStatus { get => _userRegistrationStatus; set => SetProperty(ref _userRegistrationStatus, value); }

        private bool _isUserRegistered;
        public bool IsUserRegistered { get => _isUserRegistered; set => SetProperty(ref _isUserRegistered, value); }

        // Registration Fields (Bound from Views)
        private string _username = string.Empty;
        public string Username { get => _username; set => SetProperty(ref _username, value); }

        private string _password = string.Empty;
        public string Password { get => _password; set => SetProperty(ref _password, value); }

        private string _confirmPassword = string.Empty;
        public string ConfirmPassword { get => _confirmPassword; set => SetProperty(ref _confirmPassword, value); }

        // Login Fields
        private string _loginUsername = string.Empty;
        public string LoginUsername { get => _loginUsername; set => SetProperty(ref _loginUsername, value); }

        private string _loginPassword = string.Empty;
        public string LoginPassword { get => _loginPassword; set => SetProperty(ref _loginPassword, value); }

        private string _loginStatus = string.Empty;
        public string LoginStatus { get => _loginStatus; set => SetProperty(ref _loginStatus, value); }

        private bool _isLoggedIn;
        public bool IsLoggedIn { get => _isLoggedIn; set => SetProperty(ref _isLoggedIn, value); }


        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        // Commands
        public ICommand RegisterPCCommand { get; }
        public ICommand DownloadPCHashCommand { get; }
        public ICommand SelectAvatarCommand { get; }
        public ICommand RegisterUserCommand { get; }
        public ICommand DownloadUserHashCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand LogoutCommand { get; }

        public ObservableCollection<AvatarInfo> AvailableAvatars { get; }

        public MainViewModel()
        {
            // Services
            _hardwareService = new HardwareService();
            _dataService = new DataService();
            _godotService = new GodotService();

            AvailableAvatars = new ObservableCollection<AvatarInfo>(AvatarInfo.GetAvailableAvatars());
            if (AvailableAvatars.Count > 0) SelectedAvatar = AvailableAvatars[0];

            // Commands
            RegisterPCCommand = new RelayCommand(async () => await RegisterPCAsync());
            DownloadPCHashCommand = new RelayCommand(DownloadPCHash);
            SelectAvatarCommand = new RelayCommand<AvatarInfo?>(SelectAvatarFunc);
            RegisterUserCommand = new RelayCommand(async () => await RegisterUserAsync());
            DownloadUserHashCommand = new RelayCommand(DownloadUserHash);
            LoginCommand = new RelayCommand(async () => await LoginAsync());
            LogoutCommand = new RelayCommand(Logout);

            // Start with Loading
            _currentView = new LoadingViewModel();
            
            // Trigger initialization
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                if (CurrentView is LoadingViewModel loadingVM)
                {
                    loadingVM.StatusMessage = "Cargando datos...";
                }

                await Task.Delay(1000); // Simulate/Ensure UI has time to render loading state

                await Task.Run(() =>
                {
                    // Check User Account
                    var userAccount = _dataService.LoadUserAccount();
                    if (userAccount != null && userAccount.IsValidated)
                    {
                        // User is registered -> Go to Login
                        IsUserRegistered = true;
                        UserRegistrationStatus = "Usuario registrado y validado";
                        
                        // Pre-fill username if desired
                        LoginUsername = userAccount.Username ?? "";

                        // Also load PC info just in case we need it
                        PCInfo = _dataService.LoadPCInfo();
                        if (PCInfo != null) IsPCRegistered = true;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            NavigateTo(new LoginViewModel(this));
                        });
                        return;
                    }

                    // If no user, check PC
                    PCInfo = _dataService.LoadPCInfo();
                    if (PCInfo != null)
                    {
                        // Validate PC Hash logic if needed...
                        IsPCRegistered = true;
                        PCRegistrationStatus = "PC registrado previamente";
                        
                        // PC registered but User not -> Go to Avatar/User Registration
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // If we have PC but no user, skip to Avatar or User registration?
                            // Let's go to Avatar Selection first as it leads to User Registration
                             NavigateTo(new AvatarSelectionViewModel(this));
                        });
                    }
                    else
                    {
                        // Nothing registered -> Go to PC Registration
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            NavigateTo(new PCRegistrationViewModel(this));
                        });
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error durante la inicialización: {ex.Message}");
                // Fallback
                NavigateTo(new PCRegistrationViewModel(this));
            }
        }

        public void NavigateTo<TViewModel>() where TViewModel : class
        {
            // Implementation if using DI container, but here we instantiate manually mostly
            throw new NotImplementedException();
        }

        public void NavigateTo(object viewModel)
        {
            CurrentView = viewModel;
        }

        // --- Logic Methods ---

        private async Task RegisterPCAsync()
        {
            try
            {
                PCRegistrationStatus = "Obteniendo información de hardware...";
                
                PCInfo? localPCInfo = null;
                await Task.Run(() => 
                {
                    localPCInfo = _hardwareService.GetPCInfo();
                    _dataService.SavePCInfo(localPCInfo);
                });

                if (localPCInfo != null)
                {
                    PCInfo = localPCInfo;
                    IsPCRegistered = true;
                    PCRegistrationStatus = "PC registrado exitosamente";
                    
                    MessageBox.Show("PC registrado. Ahora seleccione su avatar.", "Éxito");
                    NavigateTo(new AvatarSelectionViewModel(this));
                }
            }
            catch (Exception ex)
            {
                PCRegistrationStatus = $"Error: {ex.Message}";
                MessageBox.Show(ex.Message);
            }
        }

        private void DownloadPCHash()
        {
            if (PCInfo == null) return;
            try
            {
                string path = _dataService.CreatePCInfoZip(PCInfo);
                var dialog = new Microsoft.Win32.SaveFileDialog { FileName = $"pc_{PCInfo.UniqueHash}.zip" };
                if (dialog.ShowDialog() == true)
                {
                    System.IO.File.Copy(path, dialog.FileName, true);
                    MessageBox.Show("Archivo guardado.");
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void SelectAvatarFunc(AvatarInfo? avatar)
        {
            if (avatar != null)
            {
                SelectedAvatar = avatar;
                NavigateTo(new UserRegistrationViewModel(this));
            }
        }

        private async Task RegisterUserAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Complete todos los campos.");
                return;
            }
            if (Password != ConfirmPassword)
            {
                MessageBox.Show("Las contraseñas no coinciden.");
                return;
            }
            if (PCInfo == null)
            {
                MessageBox.Show("Error: Falta información del PC.");
                NavigateTo(new PCRegistrationViewModel(this));
                return;
            }

            try
            {
                UserRegistrationStatus = "Registrando usuario...";
                
                // Capture values for thread safety
                string u = Username;
                string p = Password;
                string a = SelectedAvatar?.Type ?? "chica";
                string h = PCInfo.UniqueHash ?? "unknown";

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

                IsUserRegistered = true;
                UserRegistrationStatus = "Usuario registrado.";
                MessageBox.Show("Usuario registrado correctamente. Iniciando sesión...");
                
                NavigateTo(new LoginViewModel(this));
            }
            catch (Exception ex)
            {
                UserRegistrationStatus = "Error en registro";
                MessageBox.Show(ex.Message);
            }
        }

        private void DownloadUserHash()
        {
            var userAccount = _dataService.LoadUserAccount();
            if (userAccount == null)
            {
                MessageBox.Show("No se encontró información de usuario para descargar.");
                return;
            }

            try
            {
                string path = _dataService.CreateUserAccountZip(userAccount);
                var dialog = new Microsoft.Win32.SaveFileDialog { FileName = $"user_{userAccount.AccountHash}.zip" };
                if (dialog.ShowDialog() == true)
                {
                    System.IO.File.Copy(path, dialog.FileName, true);
                    MessageBox.Show("Archivo de usuario guardado exitosamente.");
                }
            }
            catch (Exception ex) 
            { 
                MessageBox.Show($"Error al guardar archivo: {ex.Message}"); 
            }
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
                    LoginStatus = "Conectado. Iniciando Godot...";
                    await LaunchGodotAsync(account);
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

        private void Logout()
        {
            IsLoggedIn = false;
            NavigateTo(new LoginViewModel(this));
        }
    }
}
