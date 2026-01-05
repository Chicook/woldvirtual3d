using System;
using WoldVirtual3DViewer.ViewModels;

namespace WoldVirtual3DViewer.Services
{
    public interface INavigationService
    {
        ViewModelBase? CurrentView { get; }
        event Action? CurrentViewChanged;
        void NavigateTo<T>() where T : ViewModelBase;
        void NavigateTo(ViewModelBase viewModel);
    }

    public class NavigationService(Func<Type, ViewModelBase> viewModelFactory) : INavigationService
    {
        private readonly Func<Type, ViewModelBase> _viewModelFactory = viewModelFactory;
        private ViewModelBase? _currentView;

        public event Action? CurrentViewChanged;

        public ViewModelBase? CurrentView
        {
            get => _currentView;
            private set
            {
                _currentView = value;
                CurrentViewChanged?.Invoke();
            }
        }

        public void NavigateTo<T>() where T : ViewModelBase
        {
            ViewModelBase viewModel = _viewModelFactory(typeof(T));
            CurrentView = viewModel;
        }

        public void NavigateTo(ViewModelBase viewModel)
        {
            CurrentView = viewModel;
        }
    }
}
