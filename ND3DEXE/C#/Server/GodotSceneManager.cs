using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WoldVirtual3D.Viewer.Server
{
    public class GodotSceneManager
    {
        private GodotHeadlessServer? server;
        private WindowCapture? windowCapture;
        private string projectRoot;
        private string sceneName;
        private string lastError = string.Empty;
        private Control? parentControl;

        public event EventHandler? OnGodotQuit;

        public GodotSceneManager(string projectRoot, string sceneName = "bsprincipal.tscn", Control? parentControl = null)
        {
            this.projectRoot = projectRoot;
            this.sceneName = sceneName;
            this.parentControl = parentControl;
        }

        public async Task<bool> InitializeSceneAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Inicializando escena: {sceneName}");
                System.Diagnostics.Debug.WriteLine($"Raíz del proyecto: {projectRoot}");
                
                string projectFile = Path.Combine(projectRoot, "project.godot");
                if (!File.Exists(projectFile))
                {
                    System.Diagnostics.Debug.WriteLine($"ERROR: project.godot no encontrado en: {projectFile}");
                    lastError = $"project.godot no encontrado en: {projectFile}";
                    return false;
                }
                System.Diagnostics.Debug.WriteLine($"project.godot encontrado: {projectFile}");

                string godotPath = FindGodotExecutable();
                if (string.IsNullOrEmpty(godotPath))
                {
                    string? savedPath = GodotPathHelper.GetSavedGodotPath(projectRoot);
                    if (!string.IsNullOrEmpty(savedPath))
                    {
                        godotPath = savedPath;
                        System.Diagnostics.Debug.WriteLine($"Usando ruta guardada: {godotPath}");
                    }
                    else
                    {
                        lastError = "Godot ejecutable no encontrado. Coloca Godot.exe en la carpeta Godot/ junto al ejecutable.";
                        System.Diagnostics.Debug.WriteLine($"ERROR: {lastError}");
                        return false;
                    }
                }
                System.Diagnostics.Debug.WriteLine($"Godot ejecutable encontrado: {godotPath}");

                // Buscar bsprincipal.tscn en la raíz del proyecto
                string scenePath = Path.Combine(projectRoot, sceneName);
                
                System.Diagnostics.Debug.WriteLine($"[GodotSceneManager] Buscando escena: {sceneName}");
                System.Diagnostics.Debug.WriteLine($"[GodotSceneManager] Ruta esperada: {scenePath}");
                
                if (!File.Exists(scenePath))
                {
                    lastError = $"Escena no encontrada: {scenePath}";
                    System.Diagnostics.Debug.WriteLine($"ERROR: {lastError}");
                    return false;
                }
                
                System.Diagnostics.Debug.WriteLine($"✓ Escena encontrada: {scenePath}");

                server = new GodotHeadlessServer(godotPath, projectRoot, scenePath);
                
                server.OnOutputReceived += (sender, msg) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[Godot Output] {msg}");
                };

                server.OnErrorReceived += (sender, msg) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[Godot Error] {msg}");
                    lastError = msg;
                };

                server.OnServerStarted += (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("[GodotSceneManager] Servidor Godot iniciado correctamente");
                    
                    if (parentControl != null)
                    {
                        Process? godotProcess = server.GetGodotProcess();
                        if (godotProcess != null && !godotProcess.HasExited)
                        {
                            try
                            {
                                windowCapture = new WindowCapture();
                                
                                _ = Task.Run(async () =>
                                {
                                    await Task.Delay(200);
                                    
                                    System.Diagnostics.Debug.WriteLine("[GodotSceneManager] Iniciando captura de ventana...");
                                    
                                    bool attached = await windowCapture.AttachGodotWindow(godotProcess, parentControl);
                                    
                                    if (attached)
                                    {
                                        System.Diagnostics.Debug.WriteLine("[GodotSceneManager] ✓ Ventana de Godot adjuntada al visor");
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine("[GodotSceneManager] ⚠ No se pudo adjuntar en primer intento - reintentando...");
                                        
                                        for (int retry = 0; retry < 3; retry++)
                                        {
                                            await Task.Delay(500);
                                            attached = await windowCapture.AttachGodotWindow(godotProcess, parentControl);
                                            if (attached)
                                            {
                                                System.Diagnostics.Debug.WriteLine($"[GodotSceneManager] ✓ Ventana adjuntada en intento {retry + 2}");
                                                break;
                                            }
                                        }
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[GodotSceneManager] Error al adjuntar ventana: {ex.Message}");
                            }
                        }
                    }
                };

                server.OnServerStopped += (sender, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("[GodotSceneManager] Servidor Godot detenido");
                    OnGodotQuit?.Invoke(this, EventArgs.Empty);
                };

                return await server.StartAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al inicializar escena: {ex.Message}");
                lastError = ex.Message;
                return false;
            }
        }

        public async Task<bool> StopSceneAsync()
        {
            if (server != null)
            {
                return await server.StopAsync();
            }
            return true;
        }

        public bool IsSceneRunning()
        {
            return server?.IsRunning ?? false;
        }

        public string GetLastError()
        {
            if (!string.IsNullOrEmpty(lastError))
            {
                return lastError;
            }
            
            if (server != null)
            {
                return server.GetLastError();
            }
            
            return "Error desconocido";
        }

        public void ResizeWindow(int width, int height)
        {
            windowCapture?.ResizeWindow(width, height);
        }

        public void SetFocusToGodot()
        {
            windowCapture?.SetFocusToGodotWindow();
        }

        private string FindGodotExecutable()
        {
            // 1. Buscar en archivo de configuración
            string configFile = Path.Combine(projectRoot, "godot_path.txt");
            if (File.Exists(configFile))
            {
                string customPath = File.ReadAllText(configFile).Trim();
                if (!string.IsNullOrEmpty(customPath) && File.Exists(customPath))
                {
                    System.Diagnostics.Debug.WriteLine($"✓ Godot encontrado en ruta personalizada: {customPath}");
                    return customPath;
                }
            }

            // 2. Buscar en carpeta local (junto al ejecutable)
            var localPaths = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Godot", "Godot.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Godot", "Godot_v4.5-stable_win64.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Godot", "Godot_v4.3-stable_win64.exe"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Godot.exe"),
            };

            foreach (var path in localPaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    System.Diagnostics.Debug.WriteLine($"✓ Godot encontrado localmente: {fullPath}");
                    return fullPath;
                }
            }

            // 3. Buscar en ubicaciones comunes del sistema
            var systemPaths = new[]
            {
                Path.Combine(projectRoot, "godot.exe"),
                Path.Combine(projectRoot, "Godot", "godot.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Godot", "godot.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Godot", "Godot.exe"),
            };

            foreach (string path in systemPaths)
            {
                if (File.Exists(path))
                {
                    System.Diagnostics.Debug.WriteLine($"✓ Godot encontrado: {path}");
                    return path;
                }
            }

            System.Diagnostics.Debug.WriteLine("✗ Godot no encontrado");
            return string.Empty;
        }

        public void Dispose()
        {
            windowCapture?.Dispose();
            server?.Dispose();
        }
    }
}

