namespace WoldVirtual3DViewer.Views
{
    public partial class GameView : System.Windows.Controls.UserControl
    {
        public GameView()
        {
            InitializeComponent();
            DataContext = new WoldVirtual3DViewer.ViewModels.GameViewModel(Host);
        }
    }
}
