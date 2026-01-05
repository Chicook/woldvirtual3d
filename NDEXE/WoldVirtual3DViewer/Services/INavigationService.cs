using System;

namespace WoldVirtual3DViewer.Services
{
    public interface INavigationService
    {
        void NavigateTo<TViewModel>() where TViewModel : class;
        void NavigateTo(object viewModel);
    }
}
