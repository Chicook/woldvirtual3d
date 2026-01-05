using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using WoldVirtual3D.Viewer;

namespace WoldVirtual3D.Viewer.Forms
{
    /// <summary>
    /// Formulario principal del visor 3D
    /// Responsabilidad: Interfaz principal y gestion del visor 3D
    /// </summary>
    public partial class Viewer3DForm : Form
    {
        private Panel? _viewerPanel;
        private Panel? _loginPanel;
        private Viewer3D? _viewer3D;
        private LoginManager? _loginManager;
        private UserDatabase? _userDatabase;

        public Viewer3DForm()
        {
            InitializeComponent();
            InitializeServices();
            ShowLoginPanel();
        }

        private void InitializeComponent()
        {
            this.Text = "WoldVirtual3D Viewer";
            this.Size = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black;
            this.MinimumSize = new Size(800, 600);
            this.WindowState = FormWindowState.Normal;
            this.FormClosing += Viewer3DForm_FormClosing;
        }

        private void InitializeServices()
        {
            try
            {
                _userDatabase = new UserDatabase();
                _loginManager = new LoginManager(_userDatabase);
                _loginManager.OnLoginSuccess += OnLoginSuccess;
                _loginManager.OnLoginFailed += OnLoginFailed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al inicializar servicios: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowLoginPanel()
        {
            if (_loginPanel != null)
            {
                _loginPanel.Dispose();
            }

            _loginPanel = new Panel();
            _loginPanel.Dock = DockStyle.Fill;
            _loginPanel.BackColor = Color.FromArgb(30, 30, 30);
            this.Controls.Add(_loginPanel);

            CreateLoginUI();
        }

        private void CreateLoginUI()
        {
            if (_loginPanel == null) return;

            var container = new Panel();
            container.Size = new Size(400, 350);
            container.Location = new Point(
                (_loginPanel.Width - container.Width) / 2,
                (_loginPanel.Height - container.Height) / 2
            );
            container.Anchor = AnchorStyles.None;
            _loginPanel.Controls.Add(container);

            var titleLabel = new Label();
            titleLabel.Text = "WoldVirtual3D";
            titleLabel.Font = new Font("Arial", 24, FontStyle.Bold);
            titleLabel.ForeColor = Color.White;
            titleLabel.AutoSize = false;
            titleLabel.Size = new Size(400, 50);
            titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            titleLabel.Location = new Point(0, 20);
            container.Controls.Add(titleLabel);

            var usernameLabel = new Label();
            usernameLabel.Text = "Usuario:";
            usernameLabel.ForeColor = Color.White;
            usernameLabel.Location = new Point(50, 100);
            usernameLabel.Size = new Size(300, 20);
            container.Controls.Add(usernameLabel);

            var usernameInput = new TextBox();
            usernameInput.Location = new Point(50, 125);
            usernameInput.Size = new Size(300, 30);
            usernameInput.Font = new Font("Arial", 12);
            container.Controls.Add(usernameInput);

            var passwordLabel = new Label();
            passwordLabel.Text = "Contrasena:";
            passwordLabel.ForeColor = Color.White;
            passwordLabel.Location = new Point(50, 165);
            passwordLabel.Size = new Size(300, 20);
            container.Controls.Add(passwordLabel);

            var passwordInput = new TextBox();
            passwordInput.Location = new Point(50, 190);
            passwordInput.Size = new Size(300, 30);
            passwordInput.Font = new Font("Arial", 12);
            passwordInput.UseSystemPasswordChar = true;
            container.Controls.Add(passwordInput);

            var loginButton = new Button();
            loginButton.Text = "Iniciar Sesion";
            loginButton.Location = new Point(50, 240);
            loginButton.Size = new Size(140, 40);
            loginButton.BackColor = Color.FromArgb(0, 120, 215);
            loginButton.ForeColor = Color.White;
            loginButton.FlatStyle = FlatStyle.Flat;
            loginButton.Click += async (s, e) => await OnLoginClick(usernameInput.Text, passwordInput.Text);
            container.Controls.Add(loginButton);

            var registerButton = new Button();
            registerButton.Text = "Registrarse";
            registerButton.Location = new Point(210, 240);
            registerButton.Size = new Size(140, 40);
            registerButton.BackColor = Color.FromArgb(50, 50, 50);
            registerButton.ForeColor = Color.White;
            registerButton.FlatStyle = FlatStyle.Flat;
            registerButton.Click += async (s, e) => await OnRegisterClick(usernameInput.Text, passwordInput.Text);
            container.Controls.Add(registerButton);

            var statusLabel = new Label();
            statusLabel.Name = "StatusLabel";
            statusLabel.ForeColor = Color.Yellow;
            statusLabel.Location = new Point(50, 290);
            statusLabel.Size = new Size(300, 30);
            statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            container.Controls.Add(statusLabel);

            _loginPanel.Resize += (s, e) =>
            {
                container.Location = new Point(
                    (_loginPanel.Width - container.Width) / 2,
                    (_loginPanel.Height - container.Height) / 2
                );
            };
        }

        private async Task OnLoginClick(string username, string password)
        {
            if (_loginManager == null) return;

            var statusLabel = _loginPanel?.Controls.Find("StatusLabel", true);
            if (statusLabel != null && statusLabel.Length > 0)
            {
                ((Label)statusLabel[0]).Text = "Autenticando...";
            }

            var success = await _loginManager.LoginAsync(username, password);
            
            if (!success && statusLabel != null && statusLabel.Length > 0)
            {
                ((Label)statusLabel[0]).Text = "Error: Credenciales invalidas";
            }
        }

        private async Task OnRegisterClick(string username, string password)
        {
            if (_loginManager == null) return;

            var statusLabel = _loginPanel?.Controls.Find("StatusLabel", true);
            if (statusLabel != null && statusLabel.Length > 0)
            {
                ((Label)statusLabel[0]).Text = "Registrando...";
            }

            var success = await _loginManager.RegisterAsync(username, password);
            
            if (!success && statusLabel != null && statusLabel.Length > 0)
            {
                ((Label)statusLabel[0]).Text = "Error: No se pudo registrar";
            }
        }

        private void OnLoginSuccess(string userId, string username)
        {
            this.Invoke((MethodInvoker)delegate
            {
                ShowViewer3D();
            });
        }

        private void OnLoginFailed(string errorMessage)
        {
            var statusLabel = _loginPanel?.Controls.Find("StatusLabel", true);
            if (statusLabel != null && statusLabel.Length > 0)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    ((Label)statusLabel[0]).Text = $"Error: {errorMessage}";
                });
            }
        }

        private void ShowViewer3D()
        {
            if (_loginPanel != null)
            {
                _loginPanel.Visible = false;
                _loginPanel.Dispose();
                _loginPanel = null;
            }

            _viewerPanel = new Panel();
            _viewerPanel.Dock = DockStyle.Fill;
            _viewerPanel.BackColor = Color.Black;
            this.Controls.Add(_viewerPanel);

            _viewer3D = new Viewer3D();
            _viewer3D.Initialize(_viewerPanel);
        }

        private void Viewer3DForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _viewer3D?.Dispose();
            _loginManager?.Dispose();
            _userDatabase?.Dispose();
        }
    }
}

