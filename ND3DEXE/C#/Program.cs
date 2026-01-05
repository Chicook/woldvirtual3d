using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using WoldVirtual3D.Viewer.Forms;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace WoldVirtual3D.Viewer
{
    /// <summary>
    /// Punto de entrada principal del visor 3D
    /// Responsabilidad: Inicializar y ejecutar la aplicacion Windows Forms
    /// </summary>
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ConfigureExceptionHandling();
            ConfigureApplication();
            
            try
            {
                var mainForm = new Viewer3DForm();
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                HandleStartupError(ex);
            }
        }

        private static void ConfigureExceptionHandling()
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            
            Application.ThreadException += (s, e) =>
            {
                LogError("Error UI", e.Exception);
            };
            
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    LogError("Error critico", ex);
                }
            };
        }

        private static void ConfigureApplication()
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }


        private static void LogError(string category, Exception ex)
        {
            try
            {
                string errorMsg = $"{category}: {ex.Message}\n\nStack trace:\n{ex.StackTrace}";
                System.Diagnostics.Debug.WriteLine(errorMsg);
                
                if (Application.MessageLoop)
                {
                    MessageBox.Show(errorMsg, category, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch { }
        }

        private static void HandleStartupError(Exception ex)
        {
            try
            {
                string errorMsg = $"Error al iniciar: {ex.Message}\n\nStack trace:\n{ex.StackTrace}";
                System.Diagnostics.Debug.WriteLine(errorMsg);
                MessageBox.Show(errorMsg, "Error al Iniciar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
        }
    }
}

