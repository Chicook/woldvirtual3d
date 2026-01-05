using System.Collections.ObjectModel;
using System.Windows.Input;
using WoldVirtual3DViewer.Models;
using WoldVirtual3DViewer.Services;
using WoldVirtual3DViewer.Utils;

namespace WoldVirtual3DViewer.ViewModels
{
    public class AvatarSelectionViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly RegistrationContext _registrationContext;

        public ObservableCollection<AvatarInfo> Avatars { get; }

        private AvatarInfo? _selectedAvatar;
        public AvatarInfo? SelectedAvatar
        {
            get => _selectedAvatar;
            set => SetProperty(ref _selectedAvatar, value);
        }

        public ICommand SelectAvatarCommand { get; }

        public AvatarSelectionViewModel(
            INavigationService navigationService,
            RegistrationContext registrationContext)
        {
            _navigationService = navigationService;
            _registrationContext = registrationContext;

            Avatars = new ObservableCollection<AvatarInfo>(AvatarInfo.GetAvailableAvatars());
            if (Avatars.Count > 0) SelectedAvatar = Avatars[0];

            SelectAvatarCommand = new RelayCommand<AvatarInfo?>(SelectAvatar);
        }

        private void SelectAvatar(AvatarInfo? avatar)
        {
            if (avatar != null)
            {
                SelectedAvatar = avatar;
                _registrationContext.SelectedAvatar = avatar;
                _navigationService.NavigateTo<UserRegistrationViewModel>();
            }
        }
    }
}
