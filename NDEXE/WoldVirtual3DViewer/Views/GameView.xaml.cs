namespace WoldVirtual3DViewer.Views
{
    public partial class GameView : System.Windows.Controls.UserControl
    {
        public WoldVirtual3DViewer.Hosting.ExternalWindowHost ExternalHost => Host;
        public GameView()
        {
            InitializeComponent();
            // DataContext se asigna desde MainViewModel
        }
    }
}
