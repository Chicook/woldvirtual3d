using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WoldVirtual3D.Viewer.Server
{
    public class InternalServer
    {
        private GodotSceneManager? sceneManager;
        private string projectRoot;
        private bool isInitialized = false;
        private string lastError = string.Empty;
        private Control? parentControl;

        public bool IsInitialized => isInitialized;
        public bool IsRunning => sceneManager?.IsSceneRunning() ?? false;
        public string LastError => lastError;

        public void SetParentControl(Control control)
        {
            this.parentControl = control;
        }

        public InternalServer()
        {
            projectRoot = FindProjectRoot();
        }

        public InternalServer(string projectRoot)
        {
            this.projectRoot = projectRoot;
        }

        public async Task<bool> InitializeAsync()
        {
            if (isInitialized)
            {
                return true;
            }

            try
            {
                if (string.IsNullOrEmpty(projectRoot))
                {
                    lastError = "No se pudo encontrar la raíz del proyecto (project.godot)";
                    System.Diagnostics.Debug.WriteLine($"[InternalServer] {lastError}");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine("[InternalServer] Inicializando servidor con escena bsprincipal.tscn...");
                
                sceneManager = new GodotSceneManager(projectRoot, "bsprincipal.tscn", parentControl);
                
                sceneManager.OnGodotQuit += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("[InternalServer] Godot se cerró");
                    OnGodotQuit?.Invoke(this, EventArgs.Empty);
                };
                
                bool result = await sceneManager.InitializeSceneAsync();
                
                if (result)
                {
                    isInitialized = true;
                    lastError = string.Empty;
                    System.Diagnostics.Debug.WriteLine("[InternalServer] ✓ Servidor inicializado - Escena bsprincipal.tscn cargada");
                }
                else
                {
                    lastError = sceneManager.GetLastError();
                    System.Diagnostics.Debug.WriteLine($"[InternalServer] ✗ Error al inicializar: {lastError}");
                }

                return result;
            }
            catch (Exception ex)
            {
                lastError = $"Excepción: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[InternalServer] Error: {ex.Message}");
                return false;
            }
        }

        public string GetLastError()
        {
            if (!string.IsNullOrEmpty(lastError))
            {
                return lastError;
            }
            
            if (sceneManager != null)
            {
                return sceneManager.GetLastError();
            }
            
            return "Error desconocido";
        }

        public void ResizeGodotWindow(int width, int height)
        {
            sceneManager?.ResizeWindow(width, height);
        }

        public async Task<bool> StopAsync()
        {
            if (sceneManager != null)
            {
                bool result = await sceneManager.StopSceneAsync();
                isInitialized = false;
                return result;
            }
            return true;
        }

        private string FindProjectRoot()
        {
            string currentDir = Directory.GetCurrentDirectory();
            System.Diagnostics.Debug.WriteLine($"Directorio actual: {currentDir}");
            
            string searchDir = currentDir;
            int maxDepth = 10;
            int depth = 0;

            while (!string.IsNullOrEmpty(searchDir) && depth < maxDepth)
            {
                string projectFile = Path.Combine(searchDir, "project.godot");
                if (File.Exists(projectFile))
                {
                    System.Diagnostics.Debug.WriteLine($"Proyecto encontrado en: {searchDir}");
                    return searchDir;
                }

                string? parentDir = Directory.GetParent(searchDir)?.FullName;
                if (string.IsNullOrEmpty(parentDir) || parentDir == searchDir)
                {
                    break;
                }
                searchDir = parentDir;
                depth++;
            }

            // Fallback: buscar desde el directorio del ejecutable
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string? possibleRoot = Directory.GetParent(Directory.GetParent(exeDir)?.FullName ?? "")?.FullName;
            if (!string.IsNullOrEmpty(possibleRoot))
            {
                string projectFile = Path.Combine(possibleRoot, "project.godot");
                if (File.Exists(projectFile))
                {
                    System.Diagnostics.Debug.WriteLine($"Proyecto encontrado (desde ejecutable): {possibleRoot}");
                    return possibleRoot;
                }
            }

            // Último fallback: usar D:\woldvirtual3d directamente
            string defaultRoot = @"D:\woldvirtual3d";
            if (Directory.Exists(defaultRoot) && File.Exists(Path.Combine(defaultRoot, "project.godot")))
            {
                System.Diagnostics.Debug.WriteLine($"Usando ruta por defecto: {defaultRoot}");
                return defaultRoot;
            }

            System.Diagnostics.Debug.WriteLine($"ADVERTENCIA: No se encontró project.godot, usando: {currentDir}");
            return currentDir;
        }

        public string GetProjectRoot()
        {
            return projectRoot;
        }

        public event EventHandler? OnGodotQuit;

        public void Dispose()
        {
            sceneManager?.Dispose();
        }
    }
}

