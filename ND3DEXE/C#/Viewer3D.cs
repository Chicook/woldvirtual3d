using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Drawing;

namespace WoldVirtual3D.Viewer
{
    /// <summary>
    /// Visor 3D principal (version Windows Forms)
    /// Responsabilidad: Gestionar el renderizado 3D usando OpenTK y cargar escenas de Godot
    /// </summary>
    public class Viewer3D : IDisposable
    {
        private Panel? _renderPanel;
        private bool _disposed = false;
        private Label? _statusLabel;
        private Process? _godotProcess;

        public void Initialize(Panel renderPanel)
        {
            _renderPanel = renderPanel;
            _renderPanel.BackColor = System.Drawing.Color.Black;
            
            // Mostrar mensaje de carga inicial
            ShowLoadingMessage("Inicializando visor 3D...");
        }

        /// <summary>
        /// Carga una escena de Godot (bsprincipal.tscn)
        /// </summary>
        public void LoadGodotScene(string scenePath = "res://bsprincipal.tscn")
        {
            if (_renderPanel == null || _disposed) return;

            try
            {
                ShowLoadingMessage("Cargando escena 3D...");

                // Buscar ejecutable de Godot
                var godotPath = FindGodotExecutable();
                if (godotPath != null && File.Exists(godotPath))
                {
                    // Obtener el directorio del proyecto Godot (dos niveles arriba desde ND3DEXE/C#)
                    var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
                    
                    // Iniciar Godot con la escena
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = godotPath,
                        Arguments = $"--path \"{projectRoot}\" --scene \"{scenePath}\"",
                        UseShellExecute = false,
                        CreateNoWindow = false
                    };

                    _godotProcess = Process.Start(startInfo);
                    if (_godotProcess != null)
                    {
                        _godotProcess.Exited += (s, e) =>
                        {
                            if (_renderPanel != null && !_disposed)
                            {
                                _renderPanel.Invoke((MethodInvoker)delegate
                                {
                                    ShowLoadingMessage("Escena cerrada. Presiona cualquier tecla para continuar...");
                                });
                            }
                        };
                    }
                }
                else
                {
                    // Si no se encuentra Godot, mostrar mensaje informativo
                    ShowLoadingMessage("Godot no encontrado. La escena se cargaría aquí.\n\nRuta de escena: " + scenePath);
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
            var possiblePaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                    "Programs", "Godot", "Godot_v4.5-stable_win64.exe"),
                @"C:\Program Files\Godot\Godot_v4.5-stable_win64.exe",
                @"C:\Godot\Godot_v4.5-stable_win64.exe",
                // También buscar versiones sin el sufijo específico
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                    "Programs", "Godot", "Godot.exe"),
                @"C:\Program Files\Godot\Godot.exe",
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

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
                    if (_godotProcess != null && !_godotProcess.HasExited)
                    {
                        _godotProcess.Kill();
                        _godotProcess.Dispose();
                    }
                }
                catch { }

                _renderPanel = null;
                _statusLabel = null;
                _disposed = true;
            }
        }
    }
}
