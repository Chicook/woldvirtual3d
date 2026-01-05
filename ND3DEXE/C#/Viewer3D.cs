using System;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using WoldVirtual3D.Viewer.Server;

namespace WoldVirtual3D.Viewer
{
    /// <summary>
    /// Visor 3D principal (version Windows Forms)
    /// Responsabilidad: Gestionar el renderizado 3D embebiendo Godot dentro del Panel
    /// </summary>
    public class Viewer3D : IDisposable
    {
        private Panel? _renderPanel;
        private bool _disposed = false;
        private Label? _statusLabel;
        private InternalServer? _internalServer;

        public void Initialize(Panel renderPanel)
        {
            _renderPanel = renderPanel;
            _renderPanel.BackColor = System.Drawing.Color.Black;
            
            ShowLoadingMessage("Inicializando visor 3D...");
        }

        /// <summary>
        /// Carga una escena de Godot (bsprincipal.tscn) embebida en el Panel
        /// </summary>
        public async void LoadGodotScene(string scenePath = "res://bsprincipal.tscn")
        {
            if (_renderPanel == null || _disposed) return;

            try
            {
                ShowLoadingMessage("Cargando escena 3D...");

                _internalServer = new InternalServer();
                _internalServer.SetParentControl(_renderPanel);
                
                _internalServer.OnGodotQuit += (s, e) =>
                {
                    if (_renderPanel != null && !_disposed)
                    {
                        _renderPanel.Invoke((MethodInvoker)delegate
                        {
                            ShowLoadingMessage("Escena cerrada.");
                        });
                    }
                };

                bool success = await _internalServer.InitializeAsync();
                
                if (!success)
                {
                    string error = _internalServer.GetLastError();
                    var exeDir = AppDomain.CurrentDomain.BaseDirectory;
                    var godotLocalPath = Path.Combine(exeDir, "Godot", "Godot.exe");
                    var msg = $"Error al cargar escena:\n{error}\n\n" +
                              $"Asegúrate de colocar Godot.exe en:\n{godotLocalPath}";
                    ShowLoadingMessage(msg);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando escena Godot: {ex.Message}");
                ShowLoadingMessage($"Error al cargar escena: {ex.Message}");
            }
        }

        private void ShowLoadingMessage(string message)
        {
            if (_renderPanel == null || _disposed) return;

            _renderPanel.Controls.Clear();

            _statusLabel = new Label
            {
                Text = message,
                ForeColor = Color.White,
                Font = new Font("Arial", 14, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                AutoSize = false
            };

            _renderPanel.Controls.Add(_statusLabel);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    _internalServer?.Dispose();
                }
                catch { }

                _renderPanel = null;
                _statusLabel = null;
                _disposed = true;
            }
        }
    }
}
