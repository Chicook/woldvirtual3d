using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WoldVirtual3D.Viewer.Services
{
    /// <summary>
    /// Servicio para gestionar la ejecución de Godot Engine
    /// Responsabilidad: Inicializar, ejecutar y comunicarse con el proceso de Godot
    /// </summary>
    public class GodotService : IDisposable
    {
        private Process? godotProcess;
        private string? godotPath;
        private string? projectPath;
        private IntPtr windowHandle;
        private bool isInitialized = false;

        /// <summary>
        /// Inicializa el servicio Godot de forma asíncrona
        /// </summary>
        public async Task<bool> InitializeAsync(IntPtr parentHandle)
        {
            windowHandle = parentHandle;
            
            if (!FindGodotExecutable())
            {
                return false;
            }

            if (!FindProjectPath())
            {
                return false;
            }

            return await StartGodotProcessAsync();
        }

        /// <summary>
        /// Busca el ejecutable de Godot en las ubicaciones estándar
        /// </summary>
        private bool FindGodotExecutable()
        {
            var possiblePaths = new[]
            {
                Path.Combine(Application.StartupPath, "Godot", "Godot.exe"),
                Path.Combine(Application.StartupPath, "..", "Godot", "Godot.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Godot", "Godot.exe"),
                "Godot.exe" // En PATH
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    godotPath = Path.GetFullPath(path);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Busca el archivo project.godot para determinar la ruta del proyecto
        /// </summary>
        private bool FindProjectPath()
        {
            var currentDir = Application.StartupPath;
            var searchDepth = 5;

            for (int i = 0; i < searchDepth; i++)
            {
                var projectFile = Path.Combine(currentDir, "project.godot");
                if (File.Exists(projectFile))
                {
                    projectPath = currentDir;
                    return true;
                }

                var parent = Directory.GetParent(currentDir);
                if (parent == null)
                    break;

                currentDir = parent.FullName;
            }

            return false;
        }

        /// <summary>
        /// Inicia el proceso de Godot
        /// </summary>
        private async Task<bool> StartGodotProcessAsync()
        {
            if (godotPath == null || projectPath == null)
                return false;

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = godotPath,
                    WorkingDirectory = projectPath,
                    Arguments = $"--path \"{projectPath}\" --headless",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                godotProcess = new Process
                {
                    StartInfo = startInfo,
                    EnableRaisingEvents = true
                };

                godotProcess.Exited += GodotProcess_Exited;
                godotProcess.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        System.Diagnostics.Debug.WriteLine($"[Godot Error] {e.Data}");
                };

                godotProcess.OutputDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                        System.Diagnostics.Debug.WriteLine($"[Godot] {e.Data}");
                };

                var started = godotProcess.Start();
                
                if (started)
                {
                    godotProcess.BeginOutputReadLine();
                    godotProcess.BeginErrorReadLine();
                    
                    // Esperar un momento para que Godot se inicialice
                    await Task.Delay(1000);
                    
                    isInitialized = true;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al iniciar Godot: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Actualiza el tamaño del viewport de Godot
        /// </summary>
        public void UpdateViewportSize(int width, int height)
        {
            if (!isInitialized || godotProcess == null || godotProcess.HasExited)
                return;

            // Enviar comando para redimensionar viewport
            // Esto requeriría comunicación IPC con Godot
            System.Diagnostics.Debug.WriteLine($"Viewport size: {width}x{height}");
        }

        /// <summary>
        /// Cierra el proceso de Godot de forma asíncrona
        /// </summary>
        public async Task ShutdownAsync()
        {
            if (godotProcess == null || godotProcess.HasExited)
                return;

            try
            {
                if (!godotProcess.HasExited)
                {
                    godotProcess.Kill();
                    await godotProcess.WaitForExitAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cerrar Godot: {ex.Message}");
            }
            finally
            {
                godotProcess?.Dispose();
                godotProcess = null;
                isInitialized = false;
            }
        }

        /// <summary>
        /// Maneja el evento de salida del proceso Godot
        /// </summary>
        private void GodotProcess_Exited(object? sender, EventArgs e)
        {
            isInitialized = false;
            System.Diagnostics.Debug.WriteLine("[Godot] Proceso terminado");
        }

        public void Dispose()
        {
            if (godotProcess != null && !godotProcess.HasExited)
            {
                godotProcess.Kill();
                godotProcess.Dispose();
            }
        }
    }
}

