using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms; // Necesario para la integración con Panel si se usa, o IntPtr
using WoldVirtual3DViewer.Models;

namespace WoldVirtual3DViewer.Services
{
    public class GodotService
    {
        // P/Invoke
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int GWL_STYLE = -16;
        private const int WS_VISIBLE = 0x10000000;
        private const int WS_CHILD = 0x40000000;

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

        public async Task<Process?> LaunchGodotForEmbeddingAsync(UserAccount userAccount)
        {
             if (string.IsNullOrEmpty(_godotExecutablePath))
                throw new Exception("No se encontró el ejecutable de Godot.");

             string scenePath = Path.Combine(_godotProjectPath, "bsprincipal.tscn");
             
             // Argumentos clave: --windowed --borderless --resolution ... (aunque resolution se ajustará al redimensionar)
             var startInfo = new ProcessStartInfo
             {
                 FileName = _godotExecutablePath,
                 Arguments = $"--path \"{_godotProjectPath}\" \"{scenePath}\" --user \"{userAccount.Username}\" --avatar \"{userAccount.AvatarType}\" --windowed --borderless",
                 WorkingDirectory = _godotProjectPath,
                 UseShellExecute = false,
                 CreateNoWindow = false
             };

             startInfo.EnvironmentVariables["WOLDVIRTUAL_USER"] = userAccount.Username ?? "Unknown";
             startInfo.EnvironmentVariables["WOLDVIRTUAL_AVATAR"] = userAccount.AvatarType ?? "chica";
             startInfo.EnvironmentVariables["WOLDVIRTUAL_ACCOUNT_HASH"] = userAccount.AccountHash ?? "NoHash";

             Process? process = Process.Start(startInfo);
             if (process != null)
             {
                 // Esperar a que la ventana tenga un Handle
                 await Task.Run(async () => 
                 {
                     while (process.MainWindowHandle == IntPtr.Zero)
                     {
                         await Task.Delay(100);
                         process.Refresh();
                         if (process.HasExited) return;
                     }
                 });
             }
             return process;
        }

        private const int WS_CAPTION = 0x00C00000;
        private const int WS_THICKFRAME = 0x00040000;
        private const int WS_POPUP = -2147483648; // 0x80000000
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;

        public void EmbedWindow(IntPtr childHandle, IntPtr parentHandle)
        {
            if (childHandle == IntPtr.Zero || parentHandle == IntPtr.Zero) return;

            // Obtener estilo actual
            int style = GetWindowLong(childHandle, GWL_STYLE);
            
            // Eliminar bordes, barra de título y convertir a child
            style = style & ~WS_CAPTION & ~WS_THICKFRAME; // Quitar título y bordes redimensionables
            style |= WS_CHILD; // Añadir estilo hijo
            
            SetWindowLong(childHandle, GWL_STYLE, style);

            // Establecer padre
            SetParent(childHandle, parentHandle);

            // Forzar actualización del estilo (Frame Changed)
            SetWindowPos(childHandle, IntPtr.Zero, 0, 0, 0, 0, SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER);
        }

        public void ResizeEmbeddedWindow(IntPtr childHandle, int width, int height)
        {
            MoveWindow(childHandle, 0, 0, width, height, true);
        }

        public void FocusWindow(IntPtr hWnd)
        {
            if (hWnd != IntPtr.Zero)
            {
                // Intentar dar foco explícito a la ventana de Win32
                SetFocus(hWnd);
                // Opcional: Traer al frente si es necesario, aunque al estar embebida
                // SetFocus suele ser suficiente si el padre tiene foco.
            }
        }
        
        // ... (Resto de métodos: IsGodotAvailable, IsProjectValid, GetGodotVersion) ...


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
