using System;
using System.Windows.Forms;

namespace WoldVirtual3D.Viewer
{
    /// <summary>
    /// Visor 3D principal (version Windows Forms)
    /// Responsabilidad: Gestionar el renderizado 3D usando OpenTK
    /// </summary>
    public class Viewer3D : IDisposable
    {
        private Panel? _renderPanel;
        private bool _disposed = false;

        public void Initialize(Panel renderPanel)
        {
            _renderPanel = renderPanel;
            _renderPanel.BackColor = System.Drawing.Color.Black;
            
            // TODO: Inicializar OpenTK y renderizado 3D
            // Por ahora, solo mostramos un panel negro como placeholder
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _renderPanel = null;
                _disposed = true;
            }
        }
    }
}
