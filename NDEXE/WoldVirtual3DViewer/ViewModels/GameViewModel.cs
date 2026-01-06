using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using WoldVirtual3DViewer.Hosting;
using WoldVirtual3DViewer.Services;
using WoldVirtual3DViewer.Models;
using WoldVirtual3DViewer.Utils;

namespace WoldVirtual3DViewer.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private readonly ExternalWindowHost _host;
        private string _statusText = "Cargando...";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public ObservableCollection<ChatMessageItem> ChatMessages { get; } =
        [
            new ChatMessageItem { Content = "Bienvenido a WoldVirtual3D!" }
        ];
        
        private string _chatInput = "";
        public string ChatInput
        {
            get => _chatInput;
            set => SetProperty(ref _chatInput, value);
        }

        public RelayCommand SendChatCommand { get; }

        private Process? _process;
        private IntPtr _child = IntPtr.Zero;

        public GameViewModel(ExternalWindowHost host)
        {
            _host = host;
            StatusText = "Listo";
            SendChatCommand = new RelayCommand(SendChat);
            _ = StartAsync();
        }

        private void SendChat()
        {
            if (string.IsNullOrWhiteSpace(ChatInput)) return;
            ChatMessages.Add(new ChatMessageItem { Content = $"Yo: {ChatInput}" });
            ChatInput = "";
            // TODO: Send to Godot via pipe or socket
        }

        private async Task StartAsync()
        {
            var service = new GodotService();
            _process = await service.LaunchAsync("bsprincipal.tscn");
            if (_process != null && !_process.HasExited)
            {
                _child = _process.MainWindowHandle;
                if (_child != IntPtr.Zero)
                {
                    _host.AttachExternal(_child);
                    StatusText = "Motor incrustado";
                }
                else
                {
                    StatusText = "Handle no disponible";
                }
            }
            else
            {
                StatusText = "No se pudo iniciar Godot";
            }
        }
    }
}
