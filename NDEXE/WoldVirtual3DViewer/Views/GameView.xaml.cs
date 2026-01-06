using System;
using System.Windows;
using System.Windows.Controls;

namespace WoldVirtual3DViewer.Views
{
    public partial class GameView : System.Windows.Controls.UserControl
    {
        public WoldVirtual3DViewer.Hosting.ExternalWindowHost ExternalHost => Host;
        private GameOverlayWindow? _overlay;

        public GameView()
        {
            InitializeComponent();
            Loaded += GameView_Loaded;
            Unloaded += GameView_Unloaded;
        }

        private void GameView_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                if (_overlay == null)
                {
                    _overlay = new GameOverlayWindow
                    {
                        Owner = window,
                        DataContext = this.DataContext // GameViewModel
                    };
                    
                    window.LocationChanged += UpdateOverlayPosition;
                    window.SizeChanged += UpdateOverlayPosition;
                    this.SizeChanged += UpdateOverlayPosition;
                }
                
                _overlay.Show();
                UpdateOverlayPosition(null, null);
            }
        }

        private void UpdateOverlayPosition(object? sender, EventArgs? e)
        {
            if (_overlay == null || !IsVisible) return;
            
            try
            {
                var p = this.PointToScreen(new System.Windows.Point(0, 0));
                // Convert from pixels to DPI-aware units if necessary, but typically PointToScreen returns pixels
                // and Window.Left/Top expects DI units. 
                // For simplicity assuming 96 DPI or system handling for now.
                // Actually, PointToScreen returns screen coordinates (pixels).
                // Window.Left/Top are in logical units.
                // We might need a converter source.
                
                var source = PresentationSource.FromVisual(this);
                if (source != null && source.CompositionTarget != null)
                {
                    var m = source.CompositionTarget.TransformFromDevice;
                    var logicalPos = m.Transform(p);
                    _overlay.Left = logicalPos.X;
                    _overlay.Top = logicalPos.Y;
                    _overlay.Width = this.ActualWidth;
                    _overlay.Height = this.ActualHeight;
                }
            }
            catch { }
        }

        private void GameView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_overlay != null)
            {
                _overlay.Close();
                _overlay = null;
            }
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.LocationChanged -= UpdateOverlayPosition;
                window.SizeChanged -= UpdateOverlayPosition;
            }
        }
    }
}
