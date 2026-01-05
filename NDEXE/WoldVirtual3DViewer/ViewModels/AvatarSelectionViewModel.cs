using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WoldVirtual3DViewer.Models;

namespace WoldVirtual3DViewer.ViewModels
{
    public class AvatarSelectionViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;

        public AvatarSelectionViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        // Propiedades delegadas al MainViewModel
        public ObservableCollection<AvatarInfo> AvailableAvatars => _mainViewModel.AvailableAvatars;
        public AvatarInfo SelectedAvatar => _mainViewModel.SelectedAvatar;

        // Comandos delegados
        public System.Windows.Input.ICommand SelectAvatarCommand => _mainViewModel.SelectAvatarCommand;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Notificar cambios cuando cambie el estado en MainViewModel
        public void NotifyStateChanged()
        {
            OnPropertyChanged(nameof(SelectedAvatar));
        }
    }
}
