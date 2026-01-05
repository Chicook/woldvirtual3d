using System.Windows;
using WoldVirtual3DViewer.ViewModels;

namespace WoldVirtual3DViewer.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Aquí se puede agregar lógica para guardar estado antes de cerrar
            var viewModel = DataContext as MainViewModel;
            if (viewModel != null)
            {
                // Guardar cualquier estado necesario
            }
        }
    }
}
