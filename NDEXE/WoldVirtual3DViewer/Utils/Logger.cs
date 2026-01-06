using System;
using System.IO;
namespace WoldVirtual3DViewer.Utils
{
    public static class Logger
    {
        private static readonly string LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "viewer_debug.log");
        public static void Log(string message)
        {
            try
            {
                var s = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}\n";
                File.AppendAllText(LogPath, s);
            }
            catch { }
        }
        public static void LogError(string message, Exception ex)
        {
            Log($"{message} {ex.Message}");
        }
    }
}
