using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WoldVirtual3DViewer.ViewModels
{
    public class PCRegistrationViewModel(MainViewModel mainViewModel) : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel = mainViewModel;

        // Propiedades delegadas al MainViewModel
        public string RegistrationStatus => _mainViewModel.PCRegistrationStatus;
        public PCInfo? PCInfo => _mainViewModel.PCInfo;
        public bool IsRegistered => _mainViewModel.IsPCRegistered;

        // Comandos delegados
        public ICommand RegisterPCCommand => _mainViewModel.RegisterPCCommand;
        public ICommand DownloadHashCommand => _mainViewModel.DownloadPCHashCommand;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

        // Notificar cambios cuando cambie el estado en MainViewModel
        public void NotifyStateChanged()
        {
            OnPropertyChanged(nameof(PCInfo));
            OnPropertyChanged(nameof(PCRegistrationStatus));
            OnPropertyChanged(nameof(IsPCRegistered));
        }
    }
}
