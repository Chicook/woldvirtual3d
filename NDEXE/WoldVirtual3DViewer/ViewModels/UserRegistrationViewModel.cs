using System.ComponentModel;
using System.Runtime.CompilerServices;
using WoldVirtual3DViewer.Models;

namespace WoldVirtual3DViewer.ViewModels
{
    public class UserRegistrationViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;

        public UserRegistrationViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        // Propiedades delegadas al MainViewModel
        public string Username
        {
            get => _mainViewModel.Username;
            set => _mainViewModel.Username = value;
        }

        public string Password
        {
            get => _mainViewModel.Password;
            set => _mainViewModel.Password = value;
        }

        public string ConfirmPassword
        {
            get => _mainViewModel.ConfirmPassword;
            set => _mainViewModel.ConfirmPassword = value;
        }

        public AvatarInfo SelectedAvatar => _mainViewModel.SelectedAvatar;
        public string UserRegistrationStatus => _mainViewModel.UserRegistrationStatus;
        public bool IsUserRegistered => _mainViewModel.IsUserRegistered;

        // Comandos delegados
        public System.Windows.Input.ICommand RegisterUserCommand => _mainViewModel.RegisterUserCommand;
        public System.Windows.Input.ICommand DownloadUserHashCommand => _mainViewModel.DownloadUserHashCommand;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Notificar cambios cuando cambie el estado en MainViewModel
        public void NotifyStateChanged()
        {
            OnPropertyChanged(nameof(Username));
            OnPropertyChanged(nameof(Password));
            OnPropertyChanged(nameof(ConfirmPassword));
            OnPropertyChanged(nameof(SelectedAvatar));
            OnPropertyChanged(nameof(UserRegistrationStatus));
            OnPropertyChanged(nameof(IsUserRegistered));
        }
    }
}
