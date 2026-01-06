using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WoldVirtual3DViewer.Services
{
    public class GodotService
    {
        public string? GodotExePath { get; private set; }
        public string? ProjectPath { get; private set; }
        private readonly string _settingsPath;

        public GodotService()
        {
            ProjectPath = FindProjectRoot();
            GodotExePath = FindGodotExecutable();
            var la = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _settingsPath = Path.Combine(la, "WoldVirtual3D", "godot_path.txt");
        }

        public async Task<Process?> LaunchAsync(string sceneName, string? user = null, string? avatar = null)
        {
            if (string.IsNullOrEmpty(GodotExePath) || string.IsNullOrEmpty(ProjectPath)) return null;
            var scenePath = Path.Combine(ProjectPath, sceneName);
            var args = $"--path \"{ProjectPath}\" \"{scenePath}\" --windowed --borderless --rendering-driver opengl3 --single-window";
            if (!string.IsNullOrEmpty(user)) args += $" --user \"{user}\"";
            if (!string.IsNullOrEmpty(avatar)) args += $" --avatar \"{avatar}\"";
            var psi = new ProcessStartInfo
            {
                FileName = GodotExePath,
                Arguments = args,
                WorkingDirectory = ProjectPath,
                UseShellExecute = false,
                CreateNoWindow = false
            };
            var p = Process.Start(psi);
            if (p == null) return null;
            await Task.Run(async () =>
            {
                try { p.WaitForInputIdle(5000); } catch { }
                while (p.MainWindowHandle == IntPtr.Zero)
                {
                    await Task.Delay(100);
                    p.Refresh();
                    if (p.HasExited) break;
                }
            });
            return p;
        }

        public void SetGodotExecutablePath(string path)
        {
            if (File.Exists(path))
            {
                GodotExePath = path;
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
                    File.WriteAllText(_settingsPath, path);
                }
                catch { }
            }
        }

        private static string? FindProjectRoot()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            var current = new DirectoryInfo(dir);
            while (current != null)
            {
                var godotProj = Path.Combine(current.FullName, "project.godot");
                if (File.Exists(godotProj)) return current.FullName;
                current = current.Parent;
            }
            var fallback = Path.Combine(dir, "..", "..");
            var fi = new DirectoryInfo(fallback);
            if (fi.Exists && File.Exists(Path.Combine(fi.FullName, "project.godot"))) return fi.FullName;
            return null;
        }

        private static string? FindGodotExecutable()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string engineDir = Path.Combine(baseDir, "Engine");
            if (Directory.Exists(engineDir))
            {
                var exe = Path.Combine(engineDir, "Godot.exe");
                if (File.Exists(exe)) return exe;
                exe = Path.Combine(engineDir, "godot.exe");
                if (File.Exists(exe)) return exe;
            }
            var saved = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WoldVirtual3D", "godot_path.txt");
            if (File.Exists(saved))
            {
                var path = File.ReadAllText(saved).Trim();
                if (File.Exists(path)) return path;
            }
            string[] roots = [
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            ];
            foreach (var r in roots)
            {
                var d = Path.Combine(r, "Godot");
                if (Directory.Exists(d))
                {
                    var exe = Path.Combine(d, "Godot.exe");
                    if (File.Exists(exe)) return exe;
                }
            }
            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrEmpty(pathEnv))
            {
                foreach (var p in pathEnv.Split(';'))
                {
                    var exe = Path.Combine(p.Trim(), "godot.exe");
                    if (File.Exists(exe)) return exe;
                    exe = Path.Combine(p.Trim(), "Godot.exe");
                    if (File.Exists(exe)) return exe;
                }
            }
            return null;
        }
    }
}
