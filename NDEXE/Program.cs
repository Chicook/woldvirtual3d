using System;
using System.Threading;
using System.Windows.Forms;
using WoldVirtual3D.Viewer;

[assembly: System.Runtime.Versioning.SupportedOSPlatform("windows")]

namespace WoldVirtual3D.Viewer
{
    /// <summary>
    /// Punto de entrada principal del visor 3D WoldVirtual
    /// Responsabilidad: Inicializar aplicación Windows Forms y gestionar excepciones
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
                var viewer = new Viewer3D();
                viewer.Initialize();
                viewer.Run();
            }
            catch (Exception ex)
            {
                HandleStartupError(ex);
            }
        }

        /// <summary>
        /// Configura manejadores de excepciones globales
        /// </summary>
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
                    LogError("Error crítico", ex);
                }
            };
        }

        /// <summary>
        /// Configura la aplicación Windows Forms
        /// </summary>
        private static void ConfigureApplication()
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
        }

        /// <summary>
        /// Maneja errores durante el inicio de la aplicación
        /// </summary>
        private static void HandleStartupError(Exception ex)
        {
            try
            {
                MessageBox.Show(
                    $"Error al iniciar WoldVirtual3D:\n\n{ex.Message}\n\nLa aplicación se cerrará.",
                    "Error de Inicio",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            catch
            {
                // Si falla el MessageBox, al menos registrar en consola
                System.Diagnostics.Debug.WriteLine($"Error crítico: {ex}");
            }
        }

        /// <summary>
        /// Registra errores en el sistema de logging
        /// </summary>
        private static void LogError(string context, Exception ex)
        {
            try
            {
                var logMessage = $"[{context}] {ex.GetType().Name}: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(logMessage);
                
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"  Inner: {ex.InnerException.Message}");
                }
            }
            catch
            {
                // Si falla el logging, no hacer nada para evitar loops
            }
        }
    }
}

