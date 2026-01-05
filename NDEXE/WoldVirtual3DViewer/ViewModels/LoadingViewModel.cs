using System.Threading.Tasks;

namespace WoldVirtual3DViewer.ViewModels
{
    public class LoadingViewModel : ViewModelBase
    {
        private string _statusMessage = "Inicializando...";

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
    }
}
