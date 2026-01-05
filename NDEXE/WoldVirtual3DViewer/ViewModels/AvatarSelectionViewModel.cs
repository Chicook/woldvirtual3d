using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WoldVirtual3DViewer.Models;

namespace WoldVirtual3DViewer.ViewModels
{
    public class AvatarSelectionViewModel(MainViewModel mainViewModel) : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel = mainViewModel;

        // Propiedades delegadas
        public ObservableCollection<AvatarInfo> Avatars => _mainViewModel.AvailableAvatars;

        public AvatarInfo? SelectedAvatar
        {
            get => _mainViewModel.SelectedAvatar;
            set => _mainViewModel.SelectedAvatar = value;
        }

        // Comando delegado
        public System.Windows.Input.ICommand SelectAvatarCommand => _mainViewModel.SelectAvatarCommand;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
