using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WoldVirtual3DViewer.ViewModels
{
    public class PCRegistrationViewModel : INotifyPropertyChanged
    {
        private readonly MainViewModel _mainViewModel;

        public PCRegistrationViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }

        // Propiedades delegadas al MainViewModel
        public Models.PCInfo? PCInfo => _mainViewModel.PCInfo;
        public string PCRegistrationStatus => _mainViewModel.PCRegistrationStatus;
        public bool IsPCRegistered => _mainViewModel.IsPCRegistered;

        // Comandos delegados
        public System.Windows.Input.ICommand RegisterPCCommand => _mainViewModel.RegisterPCCommand;
        public System.Windows.Input.ICommand DownloadPCHashCommand => _mainViewModel.DownloadPCHashCommand;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
