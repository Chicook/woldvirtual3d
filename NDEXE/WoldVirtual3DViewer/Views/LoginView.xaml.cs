using System.Windows.Controls;
using WoldVirtual3DViewer.ViewModels;

namespace WoldVirtual3DViewer.Views
{
    public partial class LoginView : System.Windows.Controls.UserControl
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
        }
    }
}
