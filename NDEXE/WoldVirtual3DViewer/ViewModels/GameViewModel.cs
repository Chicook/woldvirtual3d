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
    public class ChatMessageItem
    {
        public string Sender { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string FullText => $"{Sender}: {Content}";
        public string Color { get; set; } = "White"; // Para binding
    }

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
        public ObservableCollection<ChatMessageItem> ChatHistory { get; } = new ObservableCollection<ChatMessageItem>();

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

        private void ToggleChat()
        {
            IsChatVisible = !IsChatVisible;
            RequestFocusToGame?.Invoke(); // Devolver foco al juego al cerrar/abrir
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
            
            // Usar Application.Current.Dispatcher para asegurar hilo UI si es necesario
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => ChatHistory.Add(msg));
            }
            else
            {
                ChatHistory.Add(msg);
            }

            ChatMessage = string.Empty;

            // Devolver foco al juego
            RequestFocusToGame?.Invoke();
        }

        private void AddTimedMessage(string sender, string content, string color)
        {
            var msg = new ChatMessageItem { Sender = sender, Content = content, Color = color };
            
            // Ejecutar en UI Thread si es necesario (ObservableCollection)
            // Asumimos que estamos en UI Thread o usamos BindingOperations.EnableCollectionSynchronization
            // Pero como esto es ViewModel puro, vamos a confiar en el dispatcher de WPF o hacerlo simple.
            // Para seguridad, usamos App.Current.Dispatcher si existe.
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => ChatHistory.Add(msg));
            }
            else
            {
                ChatHistory.Add(msg);
            }

            // Eliminar tras 35 segundos
            _ = RemoveMessageAfterDelay(msg, 35000);
        }

        private async Task RemoveMessageAfterDelay(ChatMessageItem msg, int delayMs)
        {
            await Task.Delay(delayMs);
            if (System.Windows.Application.Current != null)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => ChatHistory.Remove(msg));
            }
            else
            {
                ChatHistory.Remove(msg);
            }
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
