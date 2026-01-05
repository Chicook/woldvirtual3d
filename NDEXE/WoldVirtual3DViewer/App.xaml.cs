using System.Windows;

namespace WoldVirtual3DViewer
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Configurar el tema oscuro de la aplicación
            var darkTheme = new ResourceDictionary
            {
                Source = new Uri("Themes/DarkTheme.xaml", UriKind.Relative)
            };
            Resources.MergedDictionaries.Add(darkTheme);
        }
    }
}
