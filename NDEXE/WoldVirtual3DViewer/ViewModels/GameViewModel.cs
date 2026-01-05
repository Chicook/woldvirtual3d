using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Input;
using WoldVirtual3DViewer.Models;
using WoldVirtual3DViewer.Services;
using WoldVirtual3DViewer.Utils;

namespace WoldVirtual3DViewer.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private readonly GodotService _godotService;
        private readonly RegistrationContext _registrationContext;
        private readonly INavigationService _navigationService;
        
        private Process? _godotProcess;
        private IntPtr _godotHandle = IntPtr.Zero;
        private Control? _hostControl; // El control de WinForms que aloja a Godot

        private string _statusText = "Cargando Metaverso...";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public ICommand GoBackCommand { get; }

        public GameViewModel(GodotService godotService, RegistrationContext registrationContext, INavigationService navigationService)
        {
            _godotService = godotService;
            _registrationContext = registrationContext;
            _navigationService = navigationService;

            GoBackCommand = new RelayCommand(GoBack);
        }

        public void InitializeGame(Control hostControl)
        {
            _hostControl = hostControl;
            _ = StartGodotAsync();
        }

        private async Task StartGodotAsync()
        {
            try
            {
                StatusText = "Iniciando motor gráfico...";
                
                // Crear objeto de cuenta temporal si no tenemos uno completo (usando el contexto)
                var account = new UserAccount 
                { 
                    Username = _registrationContext.Username,
                    // Si tuviéramos más datos reales del login, los usaríamos aquí.
                    // Por ahora asumimos que el RegistrationContext o un UserSession tiene los datos.
                    // OJO: El RegistrationContext es para registro. Deberíamos tener un UserSessionService.
                    // Usaremos valores por defecto si faltan, o lo que haya en RegistrationContext.
                    AvatarType = _registrationContext.SelectedAvatar?.Type ?? "chica"
                };

                _godotProcess = await _godotService.LaunchGodotForEmbeddingAsync(account);

                if (_godotProcess != null && !_godotProcess.HasExited)
                {
                    _godotHandle = _godotProcess.MainWindowHandle;
                    
                    if (_hostControl != null && _hostControl.Handle != IntPtr.Zero)
                    {
                        StatusText = "Incrustando ventana...";
                        // Pequeño delay para asegurar que Godot creó su ventana completamente
                        await Task.Delay(500); 
                        
                        _godotService.EmbedWindow(_godotHandle, _hostControl.Handle);
                        
                        // Ajustar tamaño inicial
                        ResizeGame();
                        
                        StatusText = "Listo.";
                        
                        // Suscribirse al evento de redimensionado del host
                        _hostControl.SizeChanged += (s, e) => ResizeGame();
                    }
                }
                else
                {
                    StatusText = "Error: Godot se cerró inesperadamente.";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Error crítico: {ex.Message}";
                Logger.LogError("Game start error", ex);
            }
        }

        private void ResizeGame()
        {
            if (_godotHandle != IntPtr.Zero && _hostControl != null)
            {
                _godotService.ResizeEmbeddedWindow(_godotHandle, _hostControl.Width, _hostControl.Height);
            }
        }

        private void GoBack()
        {
            // Cerrar Godot limpiamente
            if (_godotProcess != null && !_godotProcess.HasExited)
            {
                _godotProcess.CloseMainWindow();
                _godotProcess.Close();
            }
            
            // Navegar al Login o menú principal
            _navigationService.NavigateTo<LoginViewModel>();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_godotProcess != null && !_godotProcess.HasExited)
                {
                    try { _godotProcess.Kill(); } catch { }
                    _godotProcess.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}
