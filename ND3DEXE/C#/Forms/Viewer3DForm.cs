using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using WoldVirtual3D.Viewer;
using WoldVirtual3D.Viewer.Forms.Registration;
using WoldVirtual3D.Viewer.RegistroPC;
using WoldVirtual3D.Viewer.RegistroPC.Models;
using WoldVirtual3D.Viewer.Services;

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
        private HardwareRegistrationService? _hardwareRegistrationService;
        private UserDataStorage? _userDataStorage;
        private string _currentHardwareHash = "";
        private string _selectedAvatar = "";

        public Viewer3DForm()
        {
            InitializeComponent();
            InitializeServices();
            _ = CheckHardwareRegistrationAsync();
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
                _hardwareRegistrationService = new HardwareRegistrationService();
                _userDatabase = new UserDatabase();
                _userDataStorage = new UserDataStorage();
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

        private async Task CheckHardwareRegistrationAsync()
        {
            try
            {
                if (_hardwareRegistrationService == null) return;

                var validation = await _hardwareRegistrationService.ValidateHardwareAsync();
                
                if (!validation.IsValid || validation.RequiresRegistration)
                {
                    ShowPCRegistration();
                }
                else
                {
                    ShowLoginPanel();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error verificando registro de hardware: {ex.Message}");
                ShowPCRegistration();
            }
        }

        private void ShowPCRegistration()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { ShowPCRegistration(); });
                return;
            }

            using (var pcRegistrationForm = new PCRegistrationForm())
            {
                var result = pcRegistrationForm.ShowDialog(this);
                
                if (result == DialogResult.OK)
                {
                    _currentHardwareHash = pcRegistrationForm.HardwareHash ?? "";
                    ShowAvatarSelection();
                }
                else
                {
                    this.Close();
                }
            }
        }

        private void ShowAvatarSelection()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { ShowAvatarSelection(); });
                return;
            }

            using (var avatarForm = new AvatarSelectionForm())
            {
                var result = avatarForm.ShowDialog(this);
                
                if (result == DialogResult.OK)
                {
                    _selectedAvatar = avatarForm.SelectedAvatar;
                    ShowUserRegistration();
                }
                else
                {
                    this.Close();
                }
            }
        }

        private void ShowUserRegistration()
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate { ShowUserRegistration(); });
                return;
            }

            using (var userRegForm = new UserRegistrationForm())
            {
                userRegForm.RegistrationCompleted += async (s, data) =>
                {
                    System.Diagnostics.Debug.WriteLine($"[Viewer3DForm] RegistrationCompleted evento recibido");
                    System.Console.WriteLine($"[Viewer3DForm] RegistrationCompleted evento recibido");
                    
                    if (_loginManager != null && _userDatabase != null && _userDataStorage != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Viewer3DForm] Servicios disponibles, registrando usuario...");
                        var success = await _loginManager.RegisterAsync(data.Username, data.Password);
                        
                        System.Diagnostics.Debug.WriteLine($"[Viewer3DForm] Registro de usuario resultado: {success}");
                        
                        if (success)
                        {
                            var userData = new UserRegistrationData
                            {
                                Username = data.Username,
                                PasswordHash = data.Password,
                                AccountHash = data.AccountHash,
                                HardwareHash = _currentHardwareHash,
                                SelectedAvatar = _selectedAvatar,
                                RegistrationDate = DateTime.UtcNow
                            };

                            System.Diagnostics.Debug.WriteLine($"[Viewer3DForm] Guardando datos de usuario en JSON...");
                            System.Console.WriteLine($"[Viewer3DForm] Guardando datos de usuario en JSON...");
                            
                            try
                            {
                                await _userDataStorage.SaveUserDataMainAsync(userData);
                                System.Diagnostics.Debug.WriteLine($"[Viewer3DForm] Usuario registrado: {data.Username}, Hash: {data.AccountHash}");
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine($"[Viewer3DForm] ERROR al guardar JSON: {ex.Message}");
                                System.Console.WriteLine($"[Viewer3DForm] ERROR al guardar JSON: {ex.Message}");
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[Viewer3DForm] ERROR: Servicios no disponibles");
                        System.Console.WriteLine($"[Viewer3DForm] ERROR: Servicios no disponibles");
                    }
                };

                var result = userRegForm.ShowDialog(this);
                
                if (result == DialogResult.OK)
                {
                    ShowLoginPanel();
                }
                else
                {
                    this.Close();
                }
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
            
            // Cargar la escena bsprincipal.tscn después de inicializar
            _viewer3D.LoadGodotScene("res://bsprincipal.tscn");
        }

        private void Viewer3DForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _viewer3D?.Dispose();
            _loginManager?.Dispose();
            _userDatabase?.Dispose();
        }
    }
}

