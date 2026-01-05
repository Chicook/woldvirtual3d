using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WoldVirtual3DViewer.Models;
using WoldVirtual3DViewer.Services;
using WoldVirtual3DViewer.Utils;

namespace WoldVirtual3DViewer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly HardwareService _hardwareService;
        private readonly DataService _dataService;
        private readonly GodotService _godotService;

        // Estados de navegación
        private object? _currentView;
        private bool _isPCRegistered;
        private bool _isUserRegistered;
        private bool _isLoggedIn;

        // Datos del PC
        private PCInfo? _pcInfo;
        private string _pcRegistrationStatus = string.Empty;

        // Datos del usuario
        private UserAccount? _userAccount;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private AvatarInfo? _selectedAvatar;
        private string _userRegistrationStatus = string.Empty;

        // Login
        private string _loginUsername = string.Empty;
        private string _loginPassword = string.Empty;
        private string _loginStatus = string.Empty;

        // Comandos
        public ICommand RegisterPCCommand { get; }
        public ICommand DownloadPCHashCommand { get; }
        public ICommand SelectAvatarCommand { get; }
        public ICommand RegisterUserCommand { get; }
        public ICommand DownloadUserHashCommand { get; }
        public ICommand LoginCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainViewModel()
        {
            try
            {
                // Inicializar servicios
                _hardwareService = new HardwareService();
                _dataService = new DataService();
                _godotService = new GodotService();

                // Inicializar comandos
                RegisterPCCommand = new RelayCommand(async () => await RegisterPCAsync());
                DownloadPCHashCommand = new RelayCommand(DownloadPCHash);
                SelectAvatarCommand = new RelayCommand<AvatarInfo?>(SelectAvatar);
                RegisterUserCommand = new RelayCommand(async () => await RegisterUserAsync());
                DownloadUserHashCommand = new RelayCommand(DownloadUserHash);
                LoginCommand = new RelayCommand(async () => await LoginAsync());
                LogoutCommand = new RelayCommand(Logout);

                // Inicializar colección de avatares
                AvailableAvatars = new ObservableCollection<AvatarInfo>(AvatarInfo.GetAvailableAvatars());

                // Seleccionar avatar por defecto
                if (AvailableAvatars.Count > 0)
                {
                    SelectedAvatar = AvailableAvatars[0];
                }

                // Verificar estado inicial y establecer vista
                CheckInitialState();
            }
            catch (Exception ex)
            {
                // Mostrar error en un MessageBox para debugging
                System.Windows.MessageBox.Show($"Error crítico al inicializar: {ex.Message}\n\nStackTrace: {ex.StackTrace}",
                    "Error Crítico", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                throw;
            }
        }

        #region Propiedades

        public object? CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AvatarInfo> AvailableAvatars { get; }

        public AvatarInfo? SelectedAvatar
        {
            get => _selectedAvatar;
            set
            {
                _selectedAvatar = value;
                OnPropertyChanged();
            }
        }

        // PC Registration
        public PCInfo? PCInfo
        {
            get => _pcInfo;
            set
            {
                _pcInfo = value;
                OnPropertyChanged();
            }
        }

        public string PCRegistrationStatus
        {
            get => _pcRegistrationStatus;
            set
            {
                _pcRegistrationStatus = value;
                OnPropertyChanged();
            }
        }

        public bool IsPCRegistered
        {
            get => _isPCRegistered;
            set
            {
                _isPCRegistered = value;
                OnPropertyChanged();
            }
        }

        // User Registration
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
            }
        }

        public string UserRegistrationStatus
        {
            get => _userRegistrationStatus;
            set
            {
                _userRegistrationStatus = value;
                OnPropertyChanged();
            }
        }

        public bool IsUserRegistered
        {
            get => _isUserRegistered;
            set
            {
                _isUserRegistered = value;
                OnPropertyChanged();
            }
        }

        // Login
        public string LoginUsername
        {
            get => _loginUsername;
            set
            {
                _loginUsername = value;
                OnPropertyChanged();
            }
        }

        public string LoginPassword
        {
            get => _loginPassword;
            set
            {
                _loginPassword = value;
                OnPropertyChanged();
            }
        }

        public string LoginStatus
        {
            get => _loginStatus;
            set
            {
                _loginStatus = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set
            {
                _isLoggedIn = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Métodos

        private void CheckInitialState()
        {
            try
            {
                // Verificar si el PC ya está registrado
                var savedPCInfo = _dataService.LoadPCInfo();
                if (savedPCInfo != null)
                {
                    PCInfo = savedPCInfo;
                    IsPCRegistered = true;

                    try
                    {
                        // Verificar si coincide con el PC actual
                        var currentPCInfo = _hardwareService.GetPCInfo();
                        if (savedPCInfo.UniqueHash != null && _hardwareService.ValidatePCHash(savedPCInfo.UniqueHash, currentPCInfo))
                        {
                            PCRegistrationStatus = "PC registrado y validado";
                        }
                        else
                        {
                            PCRegistrationStatus = "El hash del PC no coincide. Registre este PC nuevamente.";
                            IsPCRegistered = false;
                        }
                    }
                    catch
                    {
                        // Si hay error al verificar hardware, asumir que no está registrado
                        PCRegistrationStatus = "Error al verificar PC. Registre nuevamente.";
                        IsPCRegistered = false;
                    }
                }
                else
                {
                    PCRegistrationStatus = "PC no registrado";
                }

                // Verificar si el usuario ya está registrado
                var savedUserAccount = _dataService.LoadUserAccount();
                if (savedUserAccount != null && savedUserAccount.IsValidated)
                {
                    _userAccount = savedUserAccount;
                    IsUserRegistered = true;
                    UserRegistrationStatus = "Usuario registrado y validado";
                }
                else
                {
                    UserRegistrationStatus = "Usuario no registrado";
                }

                // Determinar vista inicial
                if (!IsPCRegistered)
                {
                    CurrentView = new PCRegistrationViewModel(this);
                }
                else if (!IsUserRegistered)
                {
                    CurrentView = new UserRegistrationViewModel(this);
                }
                else
                {
                    CurrentView = new LoginViewModel(this);
                }
            }
            catch (Exception ex)
            {
                // En caso de error, mostrar vista de registro de PC
                PCRegistrationStatus = $"Error: {ex.Message}";
                CurrentView = new PCRegistrationViewModel(this);
            }
        }

        private async Task RegisterPCAsync()
        {
            try
            {
                PCRegistrationStatus = "Registrando PC...";
                
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

                    MessageBox.Show("PC registrado exitosamente. Ahora puede descargar el hash único.", "Registro Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Cambiar a vista de selección de avatar
                    CurrentView = new AvatarSelectionViewModel(this);
                }
                else
                {
                    throw new Exception("No se pudo obtener la información del PC.");
                }
            }
            catch (Exception ex)
            {
                PCRegistrationStatus = $"Error: {ex.Message}";
                MessageBox.Show($"Error al registrar PC: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadPCHash()
        {
            if (PCInfo == null)
            {
                MessageBox.Show("Primero debe registrar el PC.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string zipFilePath = _dataService.CreatePCInfoZip(PCInfo);

                var saveFileDialog = new SaveFileDialog
                {
                    FileName = $"pc_hash_{PCInfo.UniqueHash}.zip",
                    DefaultExt = ".zip",
                    Filter = "Archivo ZIP (*.zip)|*.zip"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    System.IO.File.Copy(zipFilePath, saveFileDialog.FileName, true);
                    MessageBox.Show("Hash del PC descargado exitosamente. Guárdelo en un lugar seguro.", "Descarga Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al descargar hash del PC: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectAvatar(AvatarInfo? avatar)
        {
            if (avatar == null) return;
            SelectedAvatar = avatar;

            // Cambiar a vista de registro de usuario
            CurrentView = new UserRegistrationViewModel(this);
        }

        private async Task RegisterUserAsync()
        {
            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(Username))
                {
                    UserRegistrationStatus = "El nombre de usuario es obligatorio";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    UserRegistrationStatus = "La contraseña es obligatoria";
                    return;
                }

                if (Password != ConfirmPassword)
                {
                    UserRegistrationStatus = "Las contraseñas no coinciden";
                    return;
                }

                if (Password.Length < 6)
                {
                    UserRegistrationStatus = "La contraseña debe tener al menos 6 caracteres";
                    return;
                }

                if (PCInfo == null)
                {
                    UserRegistrationStatus = "Primero debe registrar el PC";
                    return;
                }

                UserRegistrationStatus = "Registrando usuario...";

                UserAccount? createdUserAccount = null;
                string currentAvatarType = SelectedAvatar?.Type ?? "chica";
                string currentPCHash = PCInfo?.UniqueHash ?? string.Empty;
                string currentUsername = Username;
                string currentPassword = Password;

                await Task.Run(() =>
                {
                    // Crear cuenta de usuario
                    var userAccount = new UserAccount
                    {
                        Username = currentUsername,
                        AvatarType = currentAvatarType,
                        PCUniqueHash = currentPCHash,
                        IsValidated = true
                    };

                    userAccount.SetPassword(currentPassword);
                    userAccount.GenerateAccountHash();

                    // Guardar cuenta
                    _dataService.SaveUserAccount(userAccount);
                    createdUserAccount = userAccount;
                });

                if (createdUserAccount != null)
                {
                    _userAccount = createdUserAccount;
                    IsUserRegistered = true;
                    UserRegistrationStatus = "Usuario registrado exitosamente";

                    MessageBox.Show("Usuario registrado exitosamente. Ahora puede descargar el hash de su cuenta.", "Registro Exitoso", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Cambiar a vista de login
                    CurrentView = new LoginViewModel(this);
                }
            }
            catch (Exception ex)
            {
                UserRegistrationStatus = $"Error: {ex.Message}";
                MessageBox.Show($"Error al registrar usuario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DownloadUserHash()
        {
            if (_userAccount == null)
            {
                MessageBox.Show("Primero debe registrar un usuario.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string zipFilePath = _dataService.CreateUserAccountZip(_userAccount);

                var saveFileDialog = new SaveFileDialog
                {
                    FileName = $"user_account_{_userAccount.AccountHash}.zip",
                    DefaultExt = ".zip",
                    Filter = "Archivo ZIP (*.zip)|*.zip"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    System.IO.File.Copy(zipFilePath, saveFileDialog.FileName, true);
                    MessageBox.Show("Hash de cuenta descargado exitosamente. Guárdelo en un lugar seguro.", "Descarga Exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al descargar hash de cuenta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoginAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(LoginUsername) || string.IsNullOrWhiteSpace(LoginPassword))
                {
                    LoginStatus = "Usuario y contraseña son obligatorios";
                    return;
                }

                LoginStatus = "Iniciando sesión...";

                bool isValid = false;
                
                await Task.Run(() => 
                {
                    // Intentar recargar usuario por si hubo cambios externos
                    var currentAccount = _dataService.LoadUserAccount();
                    
                    if (currentAccount == null)
                    {
                         // No hay usuario registrado
                         return;
                    }

                    isValid = _dataService.ValidateUserCredentials(LoginUsername, LoginPassword);
                    if (isValid)
                    {
                        _userAccount = _dataService.GetValidatedUserAccount(LoginUsername);
                    }
                });

                if (isValid && _userAccount != null)
                {
                    IsLoggedIn = true;
                    LoginStatus = "Inicio de sesión exitoso";

                    MessageBox.Show("Inicio de sesión exitoso. Iniciando WoldVirtual3D...", "Bienvenido", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Iniciar Godot
                    await LaunchGodotAsync();
                }
                else
                {
                    // Verificar por qué falló para dar mejor feedback
                    var checkAccount = _dataService.LoadUserAccount();
                    if (checkAccount == null)
                    {
                        LoginStatus = "No hay ningún usuario registrado en este equipo.";
                        MessageBox.Show("No se encontró ninguna cuenta de usuario registrada.\nPor favor, regístrese primero.", "Cuenta no encontrada", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else if (!string.Equals(checkAccount.Username, LoginUsername, StringComparison.OrdinalIgnoreCase))
                    {
                        LoginStatus = "El nombre de usuario no coincide con el registrado.";
                        MessageBox.Show($"El usuario '{LoginUsername}' no coincide con la cuenta registrada en este equipo.", "Usuario Incorrecto", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        LoginStatus = "Contraseña incorrecta.";
                        MessageBox.Show("La contraseña ingresada es incorrecta.", "Error de Autenticación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                LoginStatus = $"Error: {ex.Message}";
                MessageBox.Show($"Error al iniciar sesión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LaunchGodotAsync()
        {
            if (_userAccount == null)
            {
                MessageBox.Show("No hay información de usuario válida.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (!_godotService.IsGodotAvailable())
                {
                    MessageBox.Show("Godot no está disponible. Asegúrese de que esté instalado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!_godotService.IsProjectValid())
                {
                    MessageBox.Show("El proyecto de Godot no es válido o no se encuentra.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LoginStatus = "Iniciando Godot...";
                bool success = await _godotService.LaunchGodotSceneAsync(_userAccount);

                if (success)
                {
                    LoginStatus = "Godot iniciado exitosamente";
                    await Task.Delay(1000); // Pequeña pausa antes de cerrar
                    // Cerrar el visor ya que Godot se está ejecutando
                    Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar Godot: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Logout()
        {
            _userAccount = null!;
            IsLoggedIn = false;
            LoginUsername = string.Empty;
            LoginPassword = string.Empty;
            LoginStatus = string.Empty;

            // Volver a la vista de login
            CurrentView = new LoginViewModel(this);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
