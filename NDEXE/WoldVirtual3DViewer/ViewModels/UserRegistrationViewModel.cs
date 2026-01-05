using System.Windows.Input;
using WoldVirtual3DViewer.Models;

namespace WoldVirtual3DViewer.ViewModels
{
    public class UserRegistrationViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;

        public UserRegistrationViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _mainViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.Username)) OnPropertyChanged(nameof(Username));
                else if (e.PropertyName == nameof(MainViewModel.Password)) OnPropertyChanged(nameof(Password));
                else if (e.PropertyName == nameof(MainViewModel.ConfirmPassword)) OnPropertyChanged(nameof(ConfirmPassword));
                else if (e.PropertyName == nameof(MainViewModel.SelectedAvatar)) OnPropertyChanged(nameof(SelectedAvatar));
                else if (e.PropertyName == nameof(MainViewModel.UserRegistrationStatus)) OnPropertyChanged(nameof(UserRegistrationStatus));
                else if (e.PropertyName == nameof(MainViewModel.IsUserRegistered)) OnPropertyChanged(nameof(IsUserRegistered));
            };
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

        public AvatarInfo? SelectedAvatar => _mainViewModel.SelectedAvatar;
        public string UserRegistrationStatus => _mainViewModel.UserRegistrationStatus;
        public bool IsUserRegistered => _mainViewModel.IsUserRegistered;

        // Comandos delegados
        public ICommand RegisterUserCommand => _mainViewModel.RegisterUserCommand;
        public ICommand DownloadUserHashCommand => _mainViewModel.DownloadUserHashCommand;
    }
}
