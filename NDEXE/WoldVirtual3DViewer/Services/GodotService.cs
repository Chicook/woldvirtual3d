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
        private readonly string _godotExecutablePath;

        public GodotService()
        {
            // Ruta al proyecto de Godot
            _godotProjectPath = Path.Combine("D:", "woldvirtual3d");

            // Intentar encontrar el ejecutable de Godot
            _godotExecutablePath = FindGodotExecutable();
        }

        private string FindGodotExecutable()
        {
            // Posibles ubicaciones del ejecutable de Godot
            string[] possiblePaths = {
                Path.Combine(_godotProjectPath, "Godot.exe"),
                Path.Combine(_godotProjectPath, "Godot_v4.2.1.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Godot", "Godot.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Godot", "Godot.exe"),
                "godot.exe" // En PATH
            };

            foreach (string path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    return path;
                }
            }

            // Si no se encuentra, buscar en el directorio del visor
            string viewerDir = AppDomain.CurrentDomain.BaseDirectory;
            string embeddedGodot = Path.Combine(viewerDir, "Godot", "Godot.exe");
            if (File.Exists(embeddedGodot))
            {
                return embeddedGodot;
            }

            return null;
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
                        ["WOLDVIRTUAL_USER"] = userAccount.Username,
                        ["WOLDVIRTUAL_AVATAR"] = userAccount.AvatarType ?? "chica",
                        ["WOLDVIRTUAL_ACCOUNT_HASH"] = userAccount.AccountHash
                    }
                };

                // Iniciar Godot
                Process godotProcess = Process.Start(startInfo);

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
