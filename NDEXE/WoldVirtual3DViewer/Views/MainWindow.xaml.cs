using System.Windows;
using WoldVirtual3DViewer.ViewModels;

namespace WoldVirtual3DViewer.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
