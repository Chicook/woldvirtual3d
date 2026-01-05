using System.Windows;

namespace WoldVirtual3DViewer
{
    public partial class App : Application
    {
        public App()
        {
            // Manejador global de excepciones para evitar cierres inesperados
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"Ocurrió un error inesperado: {e.Exception.Message}\n\nStackTrace: {e.Exception.StackTrace}", 
                "Error Inesperado", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true; // Evitar que la aplicación se cierre
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"Error crítico del sistema: {ex.Message}\n\nStackTrace: {ex.StackTrace}", 
                    "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // TODO: Configurar el tema oscuro de la aplicación
            // Comentado temporalmente para debugging
            /*
            try
            {
                var darkTheme = new ResourceDictionary
                {
                    Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
                };
                Resources.MergedDictionaries.Add(darkTheme);
            }
            catch (Exception ex)
            {
                // Si hay error con el tema, continuar sin él
                System.Diagnostics.Debug.WriteLine($"Error cargando tema: {ex.Message}");
            }
            */
        }
    }
}
