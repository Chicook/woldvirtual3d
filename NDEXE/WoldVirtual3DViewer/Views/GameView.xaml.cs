using System.Windows;
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
            this.Unloaded += GameView_Unloaded;
        }

        private GameOverlayWindow? _overlayWindow;

        private void GameView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is GameViewModel vm)
            {
                vm.InitializeGame(GamePanel);
                // Suscribirse para devolver foco
                vm.RequestFocusToGame = () => 
                {
                    // Intentar dar foco al host de WindowsForms
                    GamePanel.Focus();
                    // También podemos intentar activar la ventana principal para asegurar
                    var w = Window.GetWindow(this);
                    if (w != null) w.Activate();
                };
            }

            // Iniciar Overlay
            var parentWindow = System.Windows.Window.GetWindow(this);
            if (parentWindow != null)
            {
                if (_overlayWindow == null)
                {
                    _overlayWindow = new GameOverlayWindow();
                    _overlayWindow.DataContext = this.DataContext; // Compartir ViewModel
                    _overlayWindow.Owner = parentWindow;
                    
                    // Sincronizar tamaño y posición inicial
                    SyncOverlayPosition(parentWindow);

                    // Eventos para seguir a la ventana principal
                    parentWindow.LocationChanged += ParentWindow_LocationChanged;
                    parentWindow.SizeChanged += ParentWindow_SizeChanged;
                    parentWindow.StateChanged += ParentWindow_StateChanged;
                    parentWindow.Closed += ParentWindow_Closed;
                    
                    // Mostrar sin activar para no robar foco
                    _overlayWindow.Show();
                }
            }
        }

        private void SyncOverlayPosition(Window parent)
        {
            if (_overlayWindow == null) return;

            // Obtener coordenadas del GameView dentro de la ventana
            try 
            {
                var location = this.PointToScreen(new System.Windows.Point(0, 0));
                _overlayWindow.Left = location.X;
                _overlayWindow.Top = location.Y;
                _overlayWindow.Width = this.ActualWidth;
                _overlayWindow.Height = this.ActualHeight;
            }
            catch { /* Ignorar si no está visible aún */ }
        }

        private void ParentWindow_LocationChanged(object? sender, System.EventArgs e)
        {
            if (sender is Window w) SyncOverlayPosition(w);
        }

        private void ParentWindow_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (sender is Window w) SyncOverlayPosition(w);
        }

        private void ParentWindow_StateChanged(object? sender, System.EventArgs e)
        {
            if (sender is Window w && _overlayWindow != null)
            {
                _overlayWindow.WindowState = w.WindowState == System.Windows.WindowState.Minimized 
                    ? System.Windows.WindowState.Minimized 
                    : System.Windows.WindowState.Normal;
            }
        }

        private void ParentWindow_Closed(object? sender, System.EventArgs e)
        {
            _overlayWindow?.Close();
            _overlayWindow = null;
        }

        // Limpieza al descargar el control (por si se navega fuera)
        private void GameView_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_overlayWindow != null)
            {
                _overlayWindow.Close();
                _overlayWindow = null;
            }
            
            var parentWindow = System.Windows.Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.LocationChanged -= ParentWindow_LocationChanged;
                parentWindow.SizeChanged -= ParentWindow_SizeChanged;
                parentWindow.StateChanged -= ParentWindow_StateChanged;
                parentWindow.Closed -= ParentWindow_Closed;
            }
        }
    }
}
