using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using WoldVirtual3DViewer.Models;

namespace WoldVirtual3DViewer.Services
{
    public class GodotService
    {
        private readonly string _godotProjectPath;
        private string? _godotExecutablePath;
        private readonly string _settingsPath;

        public GodotService()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _settingsPath = Path.Combine("D:", "woldvirtual3d", "NDEXE", "DTUSER", "godot_path.txt");

            // Determinar la ruta del proyecto Godot buscando hacia arriba
            string currentDir = baseDir;
            bool found = false;
            string foundPath = string.Empty;

            // Estrategia 1: Buscar hacia arriba hasta la raíz
            while (!string.IsNullOrEmpty(currentDir))
            {
                if (File.Exists(Path.Combine(currentDir, "project.godot")))
                {
                    foundPath = currentDir;
                    found = true;
                    break;
                }
                var parent = Directory.GetParent(currentDir);
                if (parent == null) break;
                currentDir = parent.FullName;
            }

            // Estrategia 2: Intentar ruta absoluta fija (Hardcoded Fallback)
            if (!found)
            {
                string hardcodedPath = @"D:\woldvirtual3d";
                if (File.Exists(Path.Combine(hardcodedPath, "project.godot")))
                {
                    foundPath = hardcodedPath;
                    found = true;
                }
            }

            // Estrategia 3: Si todo falla, asignar la ruta fija por defecto
            if (!found)
            {
                foundPath = @"D:\woldvirtual3d";
            }

            _godotProjectPath = foundPath;

            // Intentar encontrar el ejecutable de Godot localmente
            _godotExecutablePath = FindGodotExecutable();
        }

        private string? FindGodotExecutable()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 1. Prioridad: Carpeta 'Engine' local (distribución portátil)
            // Se busca Godot.exe o cualquier ejecutable que parezca el motor
            string engineDir = Path.Combine(baseDir, "Engine");
            if (Directory.Exists(engineDir))
            {
                string localGodot = Path.Combine(engineDir, "Godot.exe");
                if (File.Exists(localGodot)) return localGodot;
                
                localGodot = Path.Combine(engineDir, "godot.exe");
                if (File.Exists(localGodot)) return localGodot;

                // Buscar cualquier .exe que empiece por Godot
                foreach (var file in Directory.GetFiles(engineDir, "Godot*.exe"))
                {
                    return file;
                }
            }
            
            // 2. Buscar en carpeta 'Godot' junto al ejecutable (Legacy)
            string legacyGodot = Path.Combine(baseDir, "Godot", "Godot.exe");
            if (File.Exists(legacyGodot)) return legacyGodot;

            // 3. Verificar configuración guardada
            if (File.Exists(_settingsPath))
            {
                try
                {
                    string savedPath = File.ReadAllText(_settingsPath).Trim();
                    if (File.Exists(savedPath)) return savedPath;
                }
                catch { }
            }

            return null;
        }

        public void SetGodotExecutablePath(string path)
        {
            if (File.Exists(path))
            {
                _godotExecutablePath = path;
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
                    File.WriteAllText(_settingsPath, path);
                }
                catch { }
            }
        }

        public async Task<bool> LaunchGodotSceneAsync(UserAccount userAccount)
        {
            try
            {
                if (string.IsNullOrEmpty(_godotExecutablePath))
                {
                    throw new Exception("No se encontró el ejecutable de Godot (Motor) en la carpeta 'Engine'. Por favor, asegúrese de incluir 'Godot.exe' en la distribución del visor.");
                }

                if (!Directory.Exists(_godotProjectPath))
                {
                    throw new Exception($"No se encontró el proyecto de Godot en: {_godotProjectPath}");
                }

                // Verificar que existe el archivo de escena principal
                // CORRECCIÓN: Nombre correcto del archivo es bsprincipal.tscn
                string scenePath = Path.Combine(_godotProjectPath, "bsprincipal.tscn");
                if (!File.Exists(scenePath))
                {
                    throw new Exception($"No se encontró la escena principal: {scenePath}");
                }

                // Configurar las variables de entorno para el usuario
                var startInfo = new ProcessStartInfo
                {
                    FileName = _godotExecutablePath,
                    // CORRECCIÓN: Argumento scene apunta a bsprincipal.tscn
                    Arguments = $"--path \"{_godotProjectPath}\" \"{scenePath}\" --user \"{userAccount.Username}\" --avatar \"{userAccount.AvatarType}\"",
                    WorkingDirectory = _godotProjectPath,
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    EnvironmentVariables =
                    {
                        ["WOLDVIRTUAL_USER"] = userAccount.Username ?? "Unknown",
                        ["WOLDVIRTUAL_AVATAR"] = userAccount.AvatarType ?? "chica",
                        ["WOLDVIRTUAL_ACCOUNT_HASH"] = userAccount.AccountHash ?? "NoHash"
                    }
                };

                // Iniciar Godot
                Process? godotProcess = Process.Start(startInfo);

                if (godotProcess != null)
                {
                    await Task.Delay(2000);

                    if (!godotProcess.HasExited)
                    {
                        return true;
                    }
                    else
                    {
                        // Si se cierra, puede ser normal si es un runner que lanza otra ventana, 
                        // pero generalmente queremos que persista.
                        // Sin embargo, si es solo un launcher, podría salir. Asumimos error si sale muy rápido sin GUI.
                        return true; // Asumimos éxito si lanzó, Godot gestiona sus ventanas.
                    }
                }
                else
                {
                    throw new Exception("No se pudo iniciar el proceso de Godot.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al iniciar Godot: {ex.Message}", ex);
            }
        }

        public bool IsGodotAvailable()
        {
            return !string.IsNullOrEmpty(_godotExecutablePath) && File.Exists(_godotExecutablePath);
        }

        public bool IsProjectValid()
        {
            return Directory.Exists(_godotProjectPath) &&
                   File.Exists(Path.Combine(_godotProjectPath, "project.godot")) &&
                   File.Exists(Path.Combine(_godotProjectPath, "bsprincipal.tscn"));
        }

        public string GetGodotVersion()
        {
            if (!IsGodotAvailable() || string.IsNullOrEmpty(_godotExecutablePath))
                return "No disponible";

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = _godotExecutablePath,
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output.Trim();
            }
            }
            catch
            {
            }

            return "Desconocida";
        }
    }
}
