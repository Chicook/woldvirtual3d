namespace WoldVirtual3DViewer.Views
{
    public partial class LoginView : System.Windows.Controls.UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }
        public void InitializeComponent()
        {
            var uri = new System.Uri("/WoldVirtual3DViewer;component/Views/LoginView.xaml", System.UriKind.Relative);
            System.Windows.Application.LoadComponent(this, uri);
        }
    }
}
