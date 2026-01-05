using System;
using System.Drawing;
using System.Windows.Forms;
using WoldVirtual3D.Viewer.Services;
using WoldVirtual3D.Viewer.Forms;

namespace WoldVirtual3D.Viewer
{
    /// <summary>
    /// Clase principal del visor 3D
    /// Responsabilidad: Coordinar todos los componentes del visor
    /// </summary>
    public class Viewer3D
    {
        private MainViewerForm? mainForm;
        private GodotService? godotService;
        private SceneManager? sceneManager;

        /// <summary>
        /// Inicializa el visor 3D
        /// </summary>
        public void Initialize()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                
                mainForm = new MainViewerForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al inicializar visor: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Ejecuta el visor
        /// </summary>
        public void Run()
        {
            if (mainForm == null)
            {
                Initialize();
            }

            Application.Run(mainForm);
        }

        /// <summary>
        /// Carga una escena específica
        /// </summary>
        public async System.Threading.Tasks.Task<bool> LoadSceneAsync(string scenePath)
        {
            if (mainForm == null || sceneManager == null || godotService == null)
                return false;

            return await sceneManager.LoadSceneAsync(godotService, scenePath);
        }

        /// <summary>
        /// Obtiene el formulario principal
        /// </summary>
        public MainViewerForm? GetMainForm()
        {
            return mainForm;
        }
    }
}

