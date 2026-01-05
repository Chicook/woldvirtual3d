using System.Windows.Input;
using WoldVirtual3DViewer.Models;

namespace WoldVirtual3DViewer.ViewModels
{
    public class PCRegistrationViewModel : ViewModelBase
    {
        private readonly MainViewModel _mainViewModel;

        public PCRegistrationViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            _mainViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.PCRegistrationStatus)) OnPropertyChanged(nameof(RegistrationStatus));
                else if (e.PropertyName == nameof(MainViewModel.PCInfo)) OnPropertyChanged(nameof(PCInfo));
                else if (e.PropertyName == nameof(MainViewModel.IsPCRegistered)) OnPropertyChanged(nameof(IsRegistered));
            };
        }

        // Propiedades delegadas al MainViewModel
        public string RegistrationStatus => _mainViewModel.PCRegistrationStatus;
        public PCInfo? PCInfo => _mainViewModel.PCInfo;
        public bool IsRegistered => _mainViewModel.IsPCRegistered;

        // Comandos delegados
        public ICommand RegisterPCCommand => _mainViewModel.RegisterPCCommand;
        public ICommand DownloadHashCommand => _mainViewModel.DownloadPCHashCommand;
    }
}
