using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace WoldVirtual3D.Viewer.Server
{
    public class GodotHeadlessServer : IDisposable
    {
        private Process? godotProcess;
        private bool isRunning = false;
        private string godotExecutablePath;
        private string projectPath;
        private string scenePath;
        private readonly object lockObject = new object();
        private string lastError = string.Empty;

        public bool IsRunning
        {
            get
            {
                lock (lockObject)
                {
                    return isRunning && godotProcess != null && !godotProcess.HasExited;
                }
            }
        }

        public event EventHandler<string>? OnOutputReceived;
        public event EventHandler<string>? OnErrorReceived;
        public event EventHandler? OnServerStarted;
        public event EventHandler? OnServerStopped;

        public GodotHeadlessServer(string godotExecutablePath, string projectPath, string scenePath)
        {
            this.godotExecutablePath = godotExecutablePath;
            this.projectPath = projectPath;
            this.scenePath = scenePath;
        }

        public async Task<bool> StartAsync()
        {
            if (IsRunning)
            {
                return true;
            }

            try
            {
                if (!File.Exists(godotExecutablePath))
                {
                    lastError = $"Godot ejecutable no encontrado en: {godotExecutablePath}";
                    OnErrorReceived?.Invoke(this, lastError);
                    return false;
                }

                if (!File.Exists(scenePath))
                {
                    lastError = $"Escena no encontrada en: {scenePath}";
                    OnErrorReceived?.Invoke(this, lastError);
                    return false;
                }

                string relativeScenePath;
                try
                {
                    relativeScenePath = Path.GetRelativePath(projectPath, scenePath);
                }
                catch
                {
                    string fullPath = Path.GetFullPath(scenePath);
                    string fullProjectPath = Path.GetFullPath(projectPath);
                    relativeScenePath = fullPath.Replace(fullProjectPath, "").TrimStart('\\', '/');
                }
                
                relativeScenePath = relativeScenePath.Replace('\\', '/');
                string sceneResourcePath = $"res://{relativeScenePath}";
                
                System.Diagnostics.Debug.WriteLine($"=== Calculando ruta de escena ===");
                System.Diagnostics.Debug.WriteLine($"  Proyecto: {projectPath}");
                System.Diagnostics.Debug.WriteLine($"  Escena física: {scenePath}");
                System.Diagnostics.Debug.WriteLine($"  Ruta relativa calculada: {relativeScenePath}");
                System.Diagnostics.Debug.WriteLine($"  Ruta resource final: {sceneResourcePath}");
                
                string godotExecutable = godotExecutablePath;
                string arguments = $"--path \"{projectPath}\" \"{sceneResourcePath}\" --verbose";
                
                System.Diagnostics.Debug.WriteLine($"[GodotHeadlessServer] Cargando escena: {sceneResourcePath}");
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = godotExecutable,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = projectPath,
                    ErrorDialog = false
                };
                
                System.Diagnostics.Debug.WriteLine($"Comando: {godotExecutable} {startInfo.Arguments}");

                lock (lockObject)
                {
                    godotProcess = new Process
                    {
                        StartInfo = startInfo,
                        EnableRaisingEvents = true
                    };

                    godotProcess.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            OnOutputReceived?.Invoke(this, e.Data);
                        }
                    };

                    godotProcess.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            OnErrorReceived?.Invoke(this, e.Data);
                        }
                    };

                    godotProcess.Exited += (sender, e) =>
                    {
                        lock (lockObject)
                        {
                            isRunning = false;
                            OnServerStopped?.Invoke(this, EventArgs.Empty);
                        }
                    };

                    bool started = godotProcess.Start();
                    if (!started)
                    {
                        OnErrorReceived?.Invoke(this, "No se pudo iniciar el proceso de Godot");
                        return false;
                    }

                    System.Diagnostics.Debug.WriteLine($"Proceso iniciado - PID: {godotProcess.Id}");
                    
                    godotProcess.BeginOutputReadLine();
                    godotProcess.BeginErrorReadLine();
                    isRunning = true;
                }

                await Task.Delay(500);
                
                lock (lockObject)
                {
                    if (godotProcess != null && !godotProcess.HasExited)
                    {
                        System.Diagnostics.Debug.WriteLine($"[GodotHeadlessServer] ✓ Proceso Godot corriendo - PID: {godotProcess.Id}");
                        OnServerStarted?.Invoke(this, EventArgs.Empty);
                        return true;
                    }
                    else
                    {
                        lastError = "El proceso de Godot se cerró inmediatamente después de iniciar.";
                        System.Diagnostics.Debug.WriteLine($"[GodotHeadlessServer] ✗ {lastError}");
                        OnErrorReceived?.Invoke(this, lastError);
                        isRunning = false;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                lastError = $"Error al iniciar servidor Godot: {ex.Message}";
                OnErrorReceived?.Invoke(this, lastError);
                return false;
            }
        }

        public Process? GetGodotProcess()
        {
            lock (lockObject)
            {
                return godotProcess;
            }
        }

        public string GetLastError()
        {
            return lastError;
        }

        public async Task<bool> StopAsync()
        {
            if (!IsRunning && (godotProcess == null || godotProcess.HasExited))
            {
                return true;
            }

            try
            {
                Process? processToKill = null;
                
                lock (lockObject)
                {
                    processToKill = godotProcess;
                    isRunning = false;
                }

                if (processToKill != null && !processToKill.HasExited)
                {
                    try
                    {
                        processToKill.Kill();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al matar proceso: {ex.Message}");
                    }
                    
                    try
                    {
                        if (!processToKill.WaitForExit(3000))
                        {
                            try
                            {
                                processToKill.Kill();
                            }
                            catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error esperando cierre: {ex.Message}");
                    }
                }

                await Task.Delay(200);
                
                lock (lockObject)
                {
                    try
                    {
                        godotProcess?.Dispose();
                        godotProcess = null;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error al liberar proceso: {ex.Message}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al detener servidor Godot: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            try
            {
                var stopTask = StopAsync();
                if (!stopTask.Wait(3000))
                {
                    System.Diagnostics.Debug.WriteLine("Timeout al detener en Dispose");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en Dispose: {ex.Message}");
            }
            finally
            {
                try
                {
                    godotProcess?.Dispose();
                    godotProcess = null;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error al liberar proceso en Dispose: {ex.Message}");
                }
            }
        }
    }
}

