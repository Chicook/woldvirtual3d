using System.ComponentModel;

namespace WoldVirtual3DViewer.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;

        public LoginViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _mainViewModel.PropertyChanged += MainViewModel_PropertyChanged;
        }

        private void MainViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Propagar cambios de propiedades relevantes
            if (e.PropertyName == nameof(MainViewModel.LoginUsername))
                OnPropertyChanged(nameof(LoginUsername));
            else if (e.PropertyName == nameof(MainViewModel.LoginPassword))
                OnPropertyChanged(nameof(LoginPassword));
            else if (e.PropertyName == nameof(MainViewModel.LoginStatus))
                OnPropertyChanged(nameof(LoginStatus));
            else if (e.PropertyName == nameof(MainViewModel.IsLoggedIn))
                OnPropertyChanged(nameof(IsLoggedIn));
        }

        // Propiedades delegadas al MainViewModel
        public string LoginUsername
        {
            get => _mainViewModel.LoginUsername;
            set => _mainViewModel.LoginUsername = value;
        }

        public string LoginPassword
        {
            get => _mainViewModel.LoginPassword;
            set => _mainViewModel.LoginPassword = value;
        }

        public string LoginStatus => _mainViewModel.LoginStatus;
        public bool IsLoggedIn => _mainViewModel.IsLoggedIn;

        // Comandos delegados
        public System.Windows.Input.ICommand LoginCommand => _mainViewModel.LoginCommand;
    }
}
