using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WoldVirtual3DViewer.Models;
using WoldVirtual3DViewer.Services;
using WoldVirtual3DViewer.Utils;

namespace WoldVirtual3DViewer.ViewModels
{
    public class PCRegistrationViewModel : ViewModelBase
    {
        private readonly HardwareService _hardwareService;
        private readonly DataService _dataService;
        private readonly INavigationService _navigationService;
        private readonly RegistrationContext _registrationContext;

        private string _registrationStatus = string.Empty;
        public string RegistrationStatus { get => _registrationStatus; set => SetProperty(ref _registrationStatus, value); }

        private PCInfo? _pcInfo;
        public PCInfo? PCInfo { get => _pcInfo; set => SetProperty(ref _pcInfo, value); }

        private bool _isRegistered;
        public bool IsRegistered { get => _isRegistered; set => SetProperty(ref _isRegistered, value); }

        public ICommand RegisterPCCommand { get; }
        public ICommand DownloadHashCommand { get; }

        public PCRegistrationViewModel(
            HardwareService hardwareService, 
            DataService dataService, 
            INavigationService navigationService,
            RegistrationContext registrationContext)
        {
            _hardwareService = hardwareService;
            _dataService = dataService;
            _navigationService = navigationService;
            _registrationContext = registrationContext;

            RegisterPCCommand = new RelayCommand(async () => await RegisterPCAsync());
            DownloadHashCommand = new RelayCommand(DownloadPCHash);
        }

        private async Task RegisterPCAsync()
        {
            try
            {
                RegistrationStatus = "Obteniendo información de hardware...";
                
                PCInfo? localPCInfo = null;
                await Task.Run(() => 
                {
                    localPCInfo = _hardwareService.GetPCInfo();
                    _dataService.SavePCInfo(localPCInfo);
                });

                if (localPCInfo != null)
                {
                    PCInfo = localPCInfo;
                    IsRegistered = true;
                    RegistrationStatus = "PC registrado exitosamente";
                    
                    // Update shared context
                    _registrationContext.PCInfo = localPCInfo;

                    System.Windows.MessageBox.Show("PC registrado. Ahora seleccione su avatar.", "Éxito");
                    _navigationService.NavigateTo<AvatarSelectionViewModel>();
                }
            }
            catch (Exception ex)
            {
                RegistrationStatus = $"Error: {ex.Message}";
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void DownloadPCHash()
        {
            if (PCInfo == null) return;
            try
            {
                string path = _dataService.CreatePCInfoZip(PCInfo);
                var dialog = new Microsoft.Win32.SaveFileDialog { FileName = $"pc_{PCInfo.UniqueHash}.zip" };
                if (dialog.ShowDialog() == true)
                {
                    System.IO.File.Copy(path, dialog.FileName, true);
                    System.Windows.MessageBox.Show("Archivo guardado.");
                }
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }
        }
    }
}
