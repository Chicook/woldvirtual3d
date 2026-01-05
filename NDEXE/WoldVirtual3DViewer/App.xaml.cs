using System.Windows;

namespace WoldVirtual3DViewer
{
    public partial class App : Application
    {
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
