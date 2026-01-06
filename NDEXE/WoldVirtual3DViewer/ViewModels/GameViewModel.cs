using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
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

        // Acción para devolver el foco a la vista (se asignará desde la vista)
        public Action? RequestFocusToGame { get; set; }

        private string _statusText = "Cargando Metaverso...";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        private decimal _wcvCoinBalance = 0.000m;
        public decimal WCVcoinBalance
        {
            get => _wcvCoinBalance;
            set => SetProperty(ref _wcvCoinBalance, value);
        }

        private string _chatMessage = string.Empty;
        public string ChatMessage
        {
            get => _chatMessage;
            set => SetProperty(ref _chatMessage, value);
        }

        // Colección para mensajes temporales
        public ObservableCollection<ChatMessageItem> ChatHistory { get; } = [];

        private bool _isChatVisible = true;
        public bool IsChatVisible
        {
            get => _isChatVisible;
            set => SetProperty(ref _isChatVisible, value);
        }

        public ICommand GoBackCommand { get; }
        public ICommand SendChatCommand { get; }
        public ICommand ToggleChatCommand { get; }

        public GameViewModel(GodotService godotService, RegistrationContext registrationContext, INavigationService navigationService)
        {
            _godotService = godotService;
            _registrationContext = registrationContext;
            _navigationService = navigationService;

            GoBackCommand = new RelayCommand(GoBack);
            SendChatCommand = new RelayCommand(SendChat);
            ToggleChatCommand = new RelayCommand(ToggleChat);

            // Mensaje de bienvenida
            AddTimedMessage("Sistema", "Bienvenido a WoldVirtual3D.", "#AAAAAA");
            AddTimedMessage("[Global]", "Conectado al nodo principal.", "#AAAAAA");
        }

        public void FocusGame()
        {
            // 1. Foco a nivel de WPF/WinForms (View)
            RequestFocusToGame?.Invoke();
            
            // 2. Foco explícito a la ventana de Godot (Win32)
            if (_godotHandle != IntPtr.Zero)
            {
                GodotService.FocusWindow(_godotHandle);
            }
        }

        private void ToggleChat()
        {
            IsChatVisible = !IsChatVisible;
            FocusGame(); // Devolver foco al juego al cerrar/abrir
        }

        private static void RunOnUIThread(Action action)
        {
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(action);
            }
            else
            {
                action();
            }
        }

        private void SendChat()
        {
            if (string.IsNullOrWhiteSpace(ChatMessage)) return;
            
            // Añadir mensaje al historial
            var msg = new ChatMessageItem 
            { 
                Sender = "Yo", 
                Content = ChatMessage, 
                Color = "#FFFFFF" // Blanco para mis mensajes
            };
            
            RunOnUIThread(() => ChatHistory.Add(msg));

            ChatMessage = string.Empty;

            // Devolver foco al juego
            FocusGame();
        }

        private void AddTimedMessage(string sender, string content, string color)
        {
            var msg = new ChatMessageItem { Sender = sender, Content = content, Color = color };
            
            RunOnUIThread(() => ChatHistory.Add(msg));

            // Eliminar tras 35 segundos
            _ = RemoveMessageAfterDelay(msg, 35000);
        }

        private async Task RemoveMessageAfterDelay(ChatMessageItem msg, int delayMs)
        {
            await Task.Delay(delayMs);
            RunOnUIThread(() => ChatHistory.Remove(msg));
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
                        
                        if (!GodotService.IsNoEmbedDiagnosticEnabled())
                        {
                            GodotService.EmbedWindow(_godotHandle, _hostControl.Handle);
                            ResizeGame();
                            StatusText = "Listo.";
                        }
                        else
                        {
                            StatusText = "Diagnóstico: ejecutando Godot sin embebido";
                            Logger.Log("Diagnostic mode: skipping embed");
                        }
                        
                        // Suscribirse al evento de redimensionado del host
                        _hostControl.SizeChanged += (s, e) => ResizeGame();

                        _ = Task.Run(async () =>
                        {
                            var end = DateTime.UtcNow.AddSeconds(12);
                            while (DateTime.UtcNow < end)
                            {
                                if (_godotProcess == null || _godotProcess.HasExited) break;
                                _godotProcess.Refresh();
                                var h = _godotProcess.MainWindowHandle;
                                if (!GodotService.IsNoEmbedDiagnosticEnabled() && h != IntPtr.Zero && h != _godotHandle && _hostControl != null)
                                {
                                    _godotHandle = h;
                                    Logger.Log("Re-embedding after handle change");
                                    GodotService.EmbedWindow(_godotHandle, _hostControl.Handle);
                                    ResizeGame();
                                    break;
                                }
                                await Task.Delay(500);
                            }
                        });
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
                GodotService.ResizeEmbeddedWindow(_godotHandle, _hostControl.Width, _hostControl.Height);
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
