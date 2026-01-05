using System;
using System.Windows;
using System.Windows.Input;
using WoldVirtual3DViewer.ViewModels;

namespace WoldVirtual3DViewer.Views
{
    public partial class GameOverlayWindow : Window
    {
        public GameOverlayWindow()
        {
            InitializeComponent();
            this.PreviewKeyDown += GameOverlayWindow_PreviewKeyDown;
            // No necesitamos PreviewMouseDown porque el foco no se debe perder del juego
            // a menos que se haga clic explícitamente en el TextBox.
        }

        private void GameOverlayWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Si el foco está en el TextBox del chat, permitimos escribir.
            // Si el usuario presiona Escape, salimos del chat.
            if (ChatInputBox.IsKeyboardFocused)
            {
                if (e.Key == Key.Escape)
                {
                    // Quitar foco del TextBox
                    Keyboard.ClearFocus();
                    ReturnFocusToGame();
                    e.Handled = true;
                }
                return;
            }

            // Si NO estamos escribiendo en el chat, interceptamos teclas de movimiento
            // y devolvemos el foco al juego inmediatamente.
            // Esto cubre Flechas, Espacio, y WASD si fuera necesario.
            if (e.Key == Key.Up || e.Key == Key.Down || 
                e.Key == Key.Left || e.Key == Key.Right || 
                e.Key == Key.Space || 
                e.Key == Key.W || e.Key == Key.A || e.Key == Key.S || e.Key == Key.D)
            {
                ReturnFocusToGame();
                // No marcamos e.Handled = true para que el evento no muera completamente,
                // aunque al cambiar el foco, probablemente se pierda para esta ventana.
                // Lo importante es que el foco vuelva a Godot para la SIGUIENTE tecla.
            }
        }

        private void ReturnFocusToGame()
        {
            if (DataContext is GameViewModel vm)
            {
                // Invocar el método robusto de foco
                vm.FocusGame();
            }
        }

        public void UpdatePosition(Window parentWindow)
        {
            if (parentWindow == null) return;
            // Sincroniza tamaño y posición con el padre si es necesario
        }

        private void OnBackgroundClick(object sender, MouseButtonEventArgs e)
        {
            // Si el usuario hace clic en el fondo transparente (no en un control),
            // devolvemos el foco al juego inmediatamente.
            ReturnFocusToGame();
        }
    }
}