using WoldVirtual3DViewer.Utils;
using System.Windows.Controls;
namespace WoldVirtual3DViewer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _statusText = "Listo";
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }
        private object? _currentView;
        public object? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }
        public MainViewModel()
        {
            CurrentView = new WoldVirtual3DViewer.Views.GameView();
        }
    }
}
