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

        private string? FindGodotExecutable()
        {
            // PRIMERO: Buscar en carpetas locales (incluido con la aplicación)
            var localPaths = new List<string>
            {
                // Carpeta Godot junto al ejecutable
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Godot", "Godot.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Godot", "Godot_v4.5-stable_win64.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Godot", "Godot_v4.3-stable_win64.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Godot", "Godot_v4.2-stable_win64.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Godot", "Godot_v4.1-stable_win64.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Godot", "Godot_v4.0-stable_win64.exe"),
                
                // En el directorio del ejecutable directamente
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Godot.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Godot_v4.5-stable_win64.exe"),
                
                // En el directorio del proyecto (para desarrollo)
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Godot", "Godot.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Godot", "Godot.exe"),
            };

            foreach (var path in localPaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    System.Diagnostics.Debug.WriteLine($"[Viewer3D] Godot encontrado localmente en: {fullPath}");
                    return fullPath;
                }
            }

            // SEGUNDO: Buscar en ubicaciones del sistema (solo como fallback)
            var systemPaths = new List<string>
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                    "Programs", "Godot", "Godot_v4.5-stable_win64.exe"),
                @"C:\Program Files\Godot\Godot_v4.5-stable_win64.exe",
                @"C:\Program Files (x86)\Godot\Godot_v4.5-stable_win64.exe",
                @"C:\Godot\Godot_v4.5-stable_win64.exe",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                    "Programs", "Godot", "Godot.exe"),
                @"C:\Program Files\Godot\Godot.exe",
                @"C:\Program Files (x86)\Godot\Godot.exe",
                @"C:\Godot\Godot.exe",
            };

            foreach (var path in systemPaths)
            {
                if (File.Exists(path))
                {
                    System.Diagnostics.Debug.WriteLine($"[Viewer3D] Godot encontrado en sistema en: {path}");
                    return path;
                }
            }

            // Buscar usando el comando 'where' de Windows (busca en PATH)
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "where.exe",
                    Arguments = "godot",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        var output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        
                        if (process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output))
                        {
                            var foundPath = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                                                   .FirstOrDefault(p => p.EndsWith(".exe", StringComparison.OrdinalIgnoreCase));
                            
                            if (!string.IsNullOrEmpty(foundPath) && File.Exists(foundPath))
                            {
                                System.Diagnostics.Debug.WriteLine($"[Viewer3D] Godot encontrado en PATH: {foundPath}");
                                return foundPath.Trim();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Viewer3D] Error buscando Godot en PATH: {ex.Message}");
            }

            // Buscar recursivamente en directorios comunes
            var searchDirs = new[]
            {
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"C:\Program Files",
                @"C:\Program Files (x86)",
                @"C:\"
            };

            foreach (var searchDir in searchDirs)
            {
                if (Directory.Exists(searchDir))
                {
                    try
                    {
                        var godotExe = Directory.GetFiles(searchDir, "Godot*.exe", SearchOption.TopDirectoryOnly)
                                                .FirstOrDefault();
                        if (!string.IsNullOrEmpty(godotExe) && File.Exists(godotExe))
                        {
                            System.Diagnostics.Debug.WriteLine($"[Viewer3D] Godot encontrado en búsqueda: {godotExe}");
                            return godotExe;
                        }
                    }
                    catch
                    {
                        // Ignorar errores de acceso
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("[Viewer3D] Godot no encontrado en ninguna ubicación");
            return null;
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
