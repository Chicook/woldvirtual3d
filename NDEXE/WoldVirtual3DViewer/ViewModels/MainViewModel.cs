using System;
using System.Windows.Input;
using WoldVirtual3DViewer.Services;
using WoldVirtual3DViewer.Utils;

namespace WoldVirtual3DViewer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        
        public ViewModelBase? CurrentView => _navigationService.CurrentView;

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            _navigationService.CurrentViewChanged += OnCurrentViewChanged;
        }

        private void OnCurrentViewChanged()
        {
            OnPropertyChanged(nameof(CurrentView));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _navigationService.CurrentViewChanged -= OnCurrentViewChanged;
            }
            base.Dispose(disposing);
        }
    }
}
