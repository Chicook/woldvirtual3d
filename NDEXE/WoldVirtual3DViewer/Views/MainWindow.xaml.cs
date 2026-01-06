using System.Windows;
namespace WoldVirtual3DViewer.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new WoldVirtual3DViewer.ViewModels.MainViewModel();
        }
    }
}
