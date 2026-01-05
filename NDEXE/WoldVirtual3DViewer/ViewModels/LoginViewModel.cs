using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WoldVirtual3DViewer.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;

        public LoginViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Notificar cambios cuando cambie el estado en MainViewModel
        public void NotifyStateChanged()
        {
            OnPropertyChanged(nameof(LoginUsername));
            OnPropertyChanged(nameof(LoginPassword));
            OnPropertyChanged(nameof(LoginStatus));
            OnPropertyChanged(nameof(IsLoggedIn));
        }
    }
}
