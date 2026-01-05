using System.Windows.Controls;
using WoldVirtual3DViewer.ViewModels;

namespace WoldVirtual3DViewer.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void LoginPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.LoginPassword = LoginPasswordBox.Password;
            }
            else if (DataContext is MainViewModel mainViewModel)
            {
                mainViewModel.LoginPassword = LoginPasswordBox.Password;
            }
        }
    }
}
