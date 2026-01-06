using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms; // Necesario para la integración con Panel si se usa, o IntPtr
using WoldVirtual3DViewer.Models;
using WoldVirtual3DViewer.Utils;

namespace WoldVirtual3DViewer.Services
{
    public partial class GodotService
    {
        // P/Invoke
        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, [MarshalAs(UnmanagedType.Bool)] bool bRepaint);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [LibraryImport("user32.dll", EntryPoint = "SendMessageW", SetLastError = true)]
        private static partial IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [LibraryImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool UpdateWindow(IntPtr hWnd);

        [LibraryImport("user32.dll", EntryPoint = "SetWindowLongA")]
        private static partial int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [LibraryImport("user32.dll", EntryPoint = "GetWindowLongA")]
        private static partial int GetWindowLong(IntPtr hWnd, int nIndex);

        [LibraryImport("user32.dll", SetLastError = true)]
        private static partial IntPtr SetFocus(IntPtr hWnd);

        private const int GWL_STYLE = -16;
        private const int WS_CHILD = 0x40000000;

        private readonly string _godotProjectPath;
        private string? _godotExecutablePath;
        private readonly string _settingsPath;

        public GodotService()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            // Guardar configuración en AppData local en lugar de una ruta fija
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _settingsPath = Path.Combine(appData, "WoldVirtual3D", "godot_path.txt");

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

            // Estrategia 2: Si no se encuentra, asumir el directorio base (para portabilidad)
            if (!found)
            {
                foundPath = baseDir;
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

            // 4. Buscar en ubicaciones estándar del sistema
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string pf86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            string la = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string[] candidates = new[]
            {
                Path.Combine(pf, "Godot"),
                Path.Combine(pf86, "Godot"),
                Path.Combine(la, "Godot")
            };
            foreach (var dir in candidates)
            {
                if (Directory.Exists(dir))
                {
                    var exe = Path.Combine(dir, "Godot.exe");
                    if (File.Exists(exe)) return exe;
                    foreach (var file in Directory.GetFiles(dir, "Godot*.exe"))
                    {
                        return file;
                    }
                    foreach (var file in Directory.GetFiles(dir, "godot*.exe"))
                    {
                        return file;
                    }
                }
            }

            // 5. Buscar en directorios del PATH
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                foreach (var p in pathEnv.Split(';'))
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(p)) continue;
                        var exe = Path.Combine(p.Trim(), "godot.exe");
                        if (File.Exists(exe)) return exe;
                        exe = Path.Combine(p.Trim(), "Godot.exe");
                        if (File.Exists(exe)) return exe;
                    }
                    catch { }
                }
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
                 Arguments = $"--path \"{_godotProjectPath}\" \"{scenePath}\" --user \"{userAccount.Username}\" --avatar \"{userAccount.AvatarType}\" --windowed --borderless --rendering-driver opengl3 --single-window --verbose",
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
                    try { process.WaitForInputIdle(5000); } catch { }
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
        private const uint SWP_FRAMECHANGED = 0x0020;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_SHOWWINDOW = 0x0040;
        private const int SW_SHOW = 5;
        private const uint WM_SIZE = 0x0005;

        public static void EmbedWindow(IntPtr childHandle, IntPtr parentHandle)
        {
            if (childHandle == IntPtr.Zero || parentHandle == IntPtr.Zero) return;

            if (!IsNoStyleChangeEnabled())
            {
                int style = GetWindowLong(childHandle, GWL_STYLE);
                style = style & ~WS_CAPTION & ~WS_THICKFRAME;
                style |= WS_CHILD;
                
                int result = SetWindowLong(childHandle, GWL_STYLE, style);
                if (result == 0)
                {
                }
            }

            Logger.Log("Embedding window");
            SetParent(childHandle, parentHandle);

            SetWindowPos(childHandle, IntPtr.Zero, 0, 0, 0, 0, SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW);
            ShowWindow(childHandle, SW_SHOW);
            Logger.Log("Embedded and shown");
        }

        public static bool IsNoEmbedDiagnosticEnabled()
        {
            var v = Environment.GetEnvironmentVariable("WOLDVIRTUAL_NO_EMBED");
            if (string.IsNullOrEmpty(v)) return false;
            v = v.ToLowerInvariant();
            return v == "1" || v == "true" || v == "yes";
        }

        public static bool IsNoStyleChangeEnabled()
        {
            var v = Environment.GetEnvironmentVariable("WOLDVIRTUAL_NO_STYLE");
            if (string.IsNullOrEmpty(v)) return false;
            v = v.ToLowerInvariant();
            return v == "1" || v == "true" || v == "yes";
        }

        public static void ResizeEmbeddedWindow(IntPtr childHandle, int width, int height)
        {
            MoveWindow(childHandle, 0, 0, width, height, true);
            IntPtr lParam = (IntPtr)(((height & 0xFFFF) << 16) | (width & 0xFFFF));
            SendMessage(childHandle, WM_SIZE, IntPtr.Zero, lParam);
            UpdateWindow(childHandle);
        }

        public static void FocusWindow(IntPtr hWnd)
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
