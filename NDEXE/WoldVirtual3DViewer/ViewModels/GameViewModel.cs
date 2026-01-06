using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WoldVirtual3DViewer.Hosting;
using WoldVirtual3DViewer.Services;
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
        private Process? _process;
        private IntPtr _child = IntPtr.Zero;
        public GameViewModel(ExternalWindowHost host)
        {
            _host = host;
            StatusText = "Listo";
            _ = StartAsync();
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
