using System.Collections.ObjectModel;
using WoldVirtual3DViewer.Models;

namespace WoldVirtual3DViewer.ViewModels
{
    public class AvatarSelectionViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;

        public AvatarSelectionViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _mainViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.AvailableAvatars)) OnPropertyChanged(nameof(Avatars));
                else if (e.PropertyName == nameof(MainViewModel.SelectedAvatar)) OnPropertyChanged(nameof(SelectedAvatar));
            };
        }

        // Propiedades delegadas
        public ObservableCollection<AvatarInfo> Avatars => _mainViewModel.AvailableAvatars;

        public AvatarInfo? SelectedAvatar
        {
            get => _mainViewModel.SelectedAvatar;
            set => _mainViewModel.SelectedAvatar = value;
        }

        // Comando delegado
        public System.Windows.Input.ICommand SelectAvatarCommand => _mainViewModel.SelectAvatarCommand;
    }
}
