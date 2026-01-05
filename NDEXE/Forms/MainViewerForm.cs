using System;
using System.Drawing;
using System.Windows.Forms;
using WoldVirtual3D.Viewer.Services;
using WoldVirtual3D.Viewer.Controls;
using WoldVirtual3D.Viewer.Models;

namespace WoldVirtual3D.Viewer.Forms
{
    /// <summary>
    /// Formulario principal del visor 3D
    /// Responsabilidad: Gestionar UI principal, login, panel de Godot y controles de usuario
    /// </summary>
    public partial class MainViewerForm : Form
    {
        private GodotService? godotService;
        private Panel? godotPanel;
        private SceneManager? sceneManager;
        private LoginPanel? loginPanel;
        private LoginManager? loginManager;
        private UserData? currentUser;
        private bool isClosing = false;
        private bool isLoggedIn = false;

        public MainViewerForm()
        {
            InitializeForm();
            InitializeComponents();
            InitializeLogin();
        }

        /// <summary>
        /// Inicializa la configuración básica del formulario
        /// </summary>
        private void InitializeForm()
        {
            this.Text = "WoldVirtual3D Viewer v0.6.0";
            this.Size = new Size(1280, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black;
            this.MinimumSize = new Size(1024, 600);
            this.KeyPreview = true;
            
            this.FormClosing += MainViewerForm_FormClosing;
            this.Resize += MainViewerForm_Resize;
            this.KeyDown += MainViewerForm_KeyDown;
        }

        /// <summary>
        /// Inicializa los componentes visuales
        /// </summary>
        private void InitializeComponents()
        {
            // Panel principal para la escena de Godot (oculto inicialmente)
            godotPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                Visible = false
            };
            this.Controls.Add(godotPanel);
        }

        /// <summary>
        /// Inicializa el sistema de login
        /// </summary>
        private void InitializeLogin()
        {
            loginManager = new LoginManager();

            // Crear y mostrar panel de login
            loginPanel = new LoginPanel
            {
                Dock = DockStyle.Fill
            };
            loginPanel.OnLoginClick += LoginPanel_OnLoginClick;
            this.Controls.Add(loginPanel);
            loginPanel.BringToFront();
        }

        /// <summary>
        /// Maneja el evento de click en iniciar sesión
        /// </summary>
        private async void LoginPanel_OnLoginClick(object? sender, LoginEventArgs e)
        {
            if (loginManager == null || loginPanel == null)
                return;

            try
            {
                // Autenticar usuario
                bool authenticated = loginManager.AuthenticateUser(
                    e.Usuario,
                    e.Contrasena,
                    out currentUser
                );

                if (authenticated && currentUser != null)
                {
                    // Login exitoso - ocultar panel de login
                    loginPanel.Visible = false;
                    loginPanel.Hide();
                    isLoggedIn = true;

                    // Inicializar servicios y cargar escena
                    await InitializeServicesAndLoadSceneAsync();
                }
                else
                {
                    loginPanel.ShowError("Usuario o contraseña incorrectos");
                }
            }
            catch (Exception ex)
            {
                if (loginPanel != null)
                {
                    loginPanel.ShowError($"Error al iniciar sesión: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Inicializa los servicios y carga la escena después del login
        /// </summary>
        private async System.Threading.Tasks.Task InitializeServicesAndLoadSceneAsync()
        {
            try
            {
                // Mostrar panel de Godot
                if (godotPanel != null)
                {
                    godotPanel.Visible = true;
                    godotPanel.BringToFront();
                }

                // Inicializar servicios
                sceneManager = new SceneManager();
                godotService = new GodotService();

                // Inicializar Godot
                if (godotPanel != null)
                {
                    var initialized = await godotService.InitializeAsync(godotPanel.Handle);

                    if (initialized)
                    {
                        // Cargar escena principal después del login
                        await LoadMainSceneAsync();
                    }
                    else
                    {
                        ShowGodotError();
                        // Volver a mostrar login si falla
                        if (loginPanel != null)
                        {
                            loginPanel.Visible = true;
                            loginPanel.BringToFront();
                            isLoggedIn = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al inicializar servicios: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


        /// <summary>
        /// Carga la escena principal del metaverso (después del login)
        /// </summary>
        private async System.Threading.Tasks.Task LoadMainSceneAsync()
        {
            if (godotService == null || sceneManager == null)
                return;

            try
            {
                // Cambiar título para indicar que estamos en el metaverso
                this.Text = $"WoldVirtual3D v0.6.0 - {currentUser?.Username ?? "Usuario"}";

                // Cargar escena principal
                var scenePath = "res://BSINIMTVRS/bsprincipal.tscn";
                var loaded = await sceneManager.LoadSceneAsync(godotService, scenePath);
                
                if (loaded)
                {
                    System.Diagnostics.Debug.WriteLine("[MainViewerForm] ✓ Escena cargada exitosamente después del login");
                }
                else
                {
                    MessageBox.Show(
                        "No se pudo cargar la escena principal. Verifique que el proyecto Godot esté configurado correctamente.",
                        "Advertencia",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al cargar escena: {ex.Message}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Muestra mensaje de error cuando Godot no se puede inicializar
        /// </summary>
        private void ShowGodotError()
        {
            var message = "Godot ejecutable no encontrado.\n\n" +
                         "Coloca Godot.exe en la carpeta Godot/ junto al ejecutable.\n\n" +
                         "Asegúrate de colocar Godot.exe en:\n" +
                         $"{Application.StartupPath}\\Godot\\Godot.exe";
            
            MessageBox.Show(message, "Error al cargar escena", 
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Maneja el cierre del formulario
        /// </summary>
        private void MainViewerForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (isClosing)
            {
                e.Cancel = false;
                return;
            }

            e.Cancel = true;
            isClosing = true;

            CleanupAsync();
        }

        /// <summary>
        /// Limpia recursos de forma asíncrona antes de cerrar
        /// </summary>
        private async void CleanupAsync()
        {
            try
            {
                if (godotService != null)
                {
                    await godotService.ShutdownAsync();
                }
                
                this.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en cleanup: {ex.Message}");
                this.Close();
            }
        }

        /// <summary>
        /// Maneja el redimensionamiento del formulario
        /// </summary>
        private void MainViewerForm_Resize(object? sender, EventArgs e)
        {
            if (godotService != null && godotPanel != null)
            {
                godotService.UpdateViewportSize(godotPanel.Width, godotPanel.Height);
            }
        }

        /// <summary>
        /// Maneja eventos de teclado
        /// </summary>
        private void MainViewerForm_KeyDown(object? sender, KeyEventArgs e)
        {
            // ESC para salir
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }
    }
}

