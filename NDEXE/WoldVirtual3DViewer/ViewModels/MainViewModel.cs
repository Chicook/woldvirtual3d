using WoldVirtual3DViewer.Utils;
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
        public RelayCommand SelectEngineCommand { get; }
        public RelayCommand EnterCommand { get; }
        private readonly Services.GodotService _godotService = new Services.GodotService();
        public string Username { get; set; } = "Usuario";
        public string Avatar { get; set; } = "chica";
        public MainViewModel()
        {
            CurrentView = new WoldVirtual3DViewer.Views.LoginView { DataContext = this };
            SelectEngineCommand = new RelayCommand(SelectEngine);
            EnterCommand = new RelayCommand(EnterGame);
        }
        private void SelectEngine()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Godot (*.exe)|*.exe",
                Title = "Seleccionar Godot.exe"
            };
            if (dlg.ShowDialog() == true)
            {
                _godotService.SetGodotExecutablePath(dlg.FileName);
                StatusText = "Motor configurado";
            }
        }
        private void EnterGame()
        {
            var view = new WoldVirtual3DViewer.Views.GameView();
            view.DataContext = new GameViewModel(view.ExternalHost) { };
            CurrentView = view;
            StatusText = "Iniciando...";
        }
    }
}
