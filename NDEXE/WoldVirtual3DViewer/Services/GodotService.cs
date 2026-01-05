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

            // Determinar la ruta del proyecto Godot de forma relativa
            // 1. Intentar estructura de publicación (App/ -> woldvirtual3d/)
            string potentialProjectRoot = Path.GetFullPath(Path.Combine(baseDir, "../../"));
            
            if (!File.Exists(Path.Combine(potentialProjectRoot, "project.godot")))
            {
                // 2. Intentar estructura de desarrollo (bin/Debug/net8.0-windows/ -> woldvirtual3d/)
                potentialProjectRoot = Path.GetFullPath(Path.Combine(baseDir, "../../../../"));
            }

            // Si aún no se encuentra, usar ruta absoluta por defecto (fallback)
            if (!File.Exists(Path.Combine(potentialProjectRoot, "project.godot")))
            {
                 potentialProjectRoot = Path.Combine("D:", "woldvirtual3d");
            }

            _godotProjectPath = potentialProjectRoot;

            // Intentar encontrar el ejecutable de Godot localmente
            _godotExecutablePath = FindGodotExecutable();
        }

        private string? FindGodotExecutable()
        {
            // 0. Verificar configuración guardada
            if (File.Exists(_settingsPath))
            {
                try
                {
                    string savedPath = File.ReadAllText(_settingsPath).Trim();
                    if (File.Exists(savedPath)) return savedPath;
                }
                catch { }
            }

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // 1. Buscar en carpeta 'Godot' junto al ejecutable del visor (Portátil)
            string localGodot = Path.Combine(baseDir, "Godot", "Godot.exe");
            if (File.Exists(localGodot)) return localGodot;

            // 2. Buscar en carpeta 'Godot' un nivel arriba (NDEXE/Godot)
            string upperGodot = Path.Combine(baseDir, "..", "Godot", "Godot.exe");
            if (File.Exists(upperGodot)) return Path.GetFullPath(upperGodot);

            return null;
        }

        public void SetGodotExecutablePath(string path)
        {
            if (File.Exists(path))
            {
                _godotExecutablePath = path;
                try
                {
                    // Asegurar que el directorio existe
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
                    throw new Exception("No se encontró el ejecutable de Godot. Asegúrese de que Godot esté instalado.");
                }

                if (!Directory.Exists(_godotProjectPath))
                {
                    throw new Exception($"No se encontró el proyecto de Godot en: {_godotProjectPath}");
                }

                // Verificar que existe el archivo de escena principal
                string scenePath = Path.Combine(_godotProjectPath, "bspeincipal.tscn");
                if (!File.Exists(scenePath))
                {
                    throw new Exception($"No se encontró la escena principal: {scenePath}");
                }

                // Configurar las variables de entorno para el usuario
                var startInfo = new ProcessStartInfo
                {
                    FileName = _godotExecutablePath,
                    Arguments = $"--path \"{_godotProjectPath}\" --scene \"bspeincipal.tscn\" --user \"{userAccount.Username}\" --avatar \"{userAccount.AvatarType}\"",
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
                    // Esperar un poco para verificar que se inició correctamente
                    await Task.Delay(2000);

                    // Verificar si el proceso sigue ejecutándose
                    if (!godotProcess.HasExited)
                    {
                        return true;
                    }
                    else
                    {
                        throw new Exception("Godot se cerró inmediatamente. Verifique que el proyecto sea válido.");
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
                   File.Exists(Path.Combine(_godotProjectPath, "bspeincipal.tscn"));
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

                using (var process = Process.Start(startInfo))
                {
                    if (process != null)
                    {
                        string output = process.StandardOutput.ReadToEnd();
                        process.WaitForExit();
                        return output.Trim();
                    }
                }
            }
            catch
            {
                // Ignorar errores al obtener versión
            }

            return "Desconocida";
        }
    }
}
