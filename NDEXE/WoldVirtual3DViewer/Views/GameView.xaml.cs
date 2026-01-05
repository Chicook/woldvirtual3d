using System.Windows.Controls;
using WoldVirtual3DViewer.ViewModels;

namespace WoldVirtual3DViewer.Views
{
    public partial class GameView : System.Windows.Controls.UserControl
    {
        public GameView()
        {
            InitializeComponent();
            this.Loaded += GameView_Loaded;
        }

        private void GameView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is GameViewModel vm)
            {
                // Pasamos el control de Windows Forms (Panel) al ViewModel para que incruste Godot allí
                vm.InitializeGame(GamePanel);
            }
        }
    }
}
