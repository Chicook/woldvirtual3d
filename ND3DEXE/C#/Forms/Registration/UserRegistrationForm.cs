using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WoldVirtual3D.Viewer.Forms.Registration
{
    /// <summary>
    /// Formulario de registro de usuario (Pantalla 3)
    /// Responsabilidad: Registrar usuario y contraseña, generar hash unico de cuenta
    /// </summary>
    public partial class UserRegistrationForm : Form
    {
        private Panel _mainPanel = null!;
        private Label _titleLabel = null!;
        private Label _subtitleLabel = null!;
        private TextBox _usernameInput = null!;
        private TextBox _passwordInput = null!;
        private TextBox _confirmPasswordInput = null!;
        private Button _registerButton = null!;
        private Button _validateButton = null!;
        private Label _statusLabel = null!;
        private string _accountHash = "";

        public event EventHandler<(string Username, string Password, string AccountHash)>? RegistrationCompleted;
        public string AccountHash => _accountHash;

        public UserRegistrationForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "WoldVirtual3D - Registro de Usuario";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            _mainPanel = new Panel();
            _mainPanel.Dock = DockStyle.Fill;
            _mainPanel.BackColor = Color.FromArgb(30, 30, 30);
            this.Controls.Add(_mainPanel);

            _titleLabel = new Label();
            _titleLabel.Text = "WoldVirtual v0.0.01";
            _titleLabel.Font = new Font("Segoe UI", 28, FontStyle.Bold);
            _titleLabel.ForeColor = Color.White;
            _titleLabel.AutoSize = false;
            _titleLabel.Size = new Size(600, 60);
            _titleLabel.Location = new Point(100, 40);
            _titleLabel.TextAlign = ContentAlignment.MiddleCenter;
            _mainPanel.Controls.Add(_titleLabel);

            _subtitleLabel = new Label();
            _subtitleLabel.Text = "Crea tu Cuenta";
            _subtitleLabel.Font = new Font("Segoe UI", 14);
            _subtitleLabel.ForeColor = Color.LightGray;
            _subtitleLabel.AutoSize = false;
            _subtitleLabel.Size = new Size(600, 30);
            _subtitleLabel.Location = new Point(100, 100);
            _subtitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            _mainPanel.Controls.Add(_subtitleLabel);

            var contentPanel = new Panel();
            contentPanel.Size = new Size(450, 400);
            contentPanel.Location = new Point(175, 150);
            contentPanel.BackColor = Color.FromArgb(45, 45, 45);
            contentPanel.BorderStyle = BorderStyle.None;
            _mainPanel.Controls.Add(contentPanel);

            var usernameLabel = new Label();
            usernameLabel.Text = "Usuario:";
            usernameLabel.Font = new Font("Segoe UI", 11);
            usernameLabel.ForeColor = Color.White;
            usernameLabel.AutoSize = false;
            usernameLabel.Size = new Size(380, 25);
            usernameLabel.Location = new Point(35, 40);
            contentPanel.Controls.Add(usernameLabel);

            _usernameInput = new TextBox();
            _usernameInput.Size = new Size(380, 35);
            _usernameInput.Location = new Point(35, 65);
            _usernameInput.Font = new Font("Segoe UI", 11);
            _usernameInput.BackColor = Color.FromArgb(60, 60, 60);
            _usernameInput.ForeColor = Color.White;
            _usernameInput.BorderStyle = BorderStyle.FixedSingle;
            contentPanel.Controls.Add(_usernameInput);

            var passwordLabel = new Label();
            passwordLabel.Text = "Contraseña:";
            passwordLabel.Font = new Font("Segoe UI", 11);
            passwordLabel.ForeColor = Color.White;
            passwordLabel.AutoSize = false;
            passwordLabel.Size = new Size(380, 25);
            passwordLabel.Location = new Point(35, 115);
            contentPanel.Controls.Add(passwordLabel);

            _passwordInput = new TextBox();
            _passwordInput.Size = new Size(380, 35);
            _passwordInput.Location = new Point(35, 140);
            _passwordInput.Font = new Font("Segoe UI", 11);
            _passwordInput.BackColor = Color.FromArgb(60, 60, 60);
            _passwordInput.ForeColor = Color.White;
            _passwordInput.BorderStyle = BorderStyle.FixedSingle;
            _passwordInput.UseSystemPasswordChar = true;
            contentPanel.Controls.Add(_passwordInput);

            var confirmPasswordLabel = new Label();
            confirmPasswordLabel.Text = "Confirmar Contraseña:";
            confirmPasswordLabel.Font = new Font("Segoe UI", 11);
            confirmPasswordLabel.ForeColor = Color.White;
            confirmPasswordLabel.AutoSize = false;
            confirmPasswordLabel.Size = new Size(380, 25);
            confirmPasswordLabel.Location = new Point(35, 190);
            contentPanel.Controls.Add(confirmPasswordLabel);

            _confirmPasswordInput = new TextBox();
            _confirmPasswordInput.Size = new Size(380, 35);
            _confirmPasswordInput.Location = new Point(35, 215);
            _confirmPasswordInput.Font = new Font("Segoe UI", 11);
            _confirmPasswordInput.BackColor = Color.FromArgb(60, 60, 60);
            _confirmPasswordInput.ForeColor = Color.White;
            _confirmPasswordInput.BorderStyle = BorderStyle.FixedSingle;
            _confirmPasswordInput.UseSystemPasswordChar = true;
            contentPanel.Controls.Add(_confirmPasswordInput);

            _validateButton = new Button();
            _validateButton.Text = "Validar";
            _validateButton.Size = new Size(180, 45);
            _validateButton.Location = new Point(35, 270);
            _validateButton.BackColor = Color.FromArgb(0, 120, 215);
            _validateButton.ForeColor = Color.White;
            _validateButton.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            _validateButton.FlatStyle = FlatStyle.Flat;
            _validateButton.FlatAppearance.BorderSize = 0;
            _validateButton.Cursor = Cursors.Hand;
            _validateButton.Click += ValidateButton_Click;
            contentPanel.Controls.Add(_validateButton);

            _registerButton = new Button();
            _registerButton.Text = "Continuar →";
            _registerButton.Size = new Size(180, 45);
            _registerButton.Location = new Point(235, 270);
            _registerButton.BackColor = Color.FromArgb(50, 50, 50);
            _registerButton.ForeColor = Color.White;
            _registerButton.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            _registerButton.FlatStyle = FlatStyle.Flat;
            _registerButton.FlatAppearance.BorderSize = 0;
            _registerButton.Cursor = Cursors.Hand;
            _registerButton.Enabled = false;
            _registerButton.Click += RegisterButton_Click;
            contentPanel.Controls.Add(_registerButton);

            _statusLabel = new Label();
            _statusLabel.Text = "";
            _statusLabel.Font = new Font("Segoe UI", 10);
            _statusLabel.ForeColor = Color.Yellow;
            _statusLabel.AutoSize = false;
            _statusLabel.Size = new Size(600, 30);
            _statusLabel.Location = new Point(100, 550);
            _statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            _mainPanel.Controls.Add(_statusLabel);
        }

        private void ValidateButton_Click(object? sender, EventArgs e)
        {
            var username = _usernameInput.Text.Trim();
            var password = _passwordInput.Text;
            var confirmPassword = _confirmPasswordInput.Text;

            if (string.IsNullOrEmpty(username))
            {
                _statusLabel.Text = "Error: El usuario es requerido";
                _statusLabel.ForeColor = Color.Red;
                return;
            }

            if (password.Length < 6)
            {
                _statusLabel.Text = "Error: La contraseña debe tener al menos 6 caracteres";
                _statusLabel.ForeColor = Color.Red;
                return;
            }

            if (password != confirmPassword)
            {
                _statusLabel.Text = "Error: Las contraseñas no coinciden";
                _statusLabel.ForeColor = Color.Red;
                return;
            }

            _accountHash = GenerateAccountHash(username, password);
            _registerButton.Enabled = true;
            _registerButton.BackColor = Color.FromArgb(0, 120, 215);
            _statusLabel.Text = "✓ Validacion exitosa. Listo para continuar.";
            _statusLabel.ForeColor = Color.LightGreen;
        }

        private async void RegisterButton_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_accountHash))
            {
                _statusLabel.Text = "Error: Debes validar primero";
                _statusLabel.ForeColor = Color.Red;
                return;
            }

            try
            {
                _registerButton.Enabled = false;
                _statusLabel.Text = "Generando hash de cuenta...";
                _statusLabel.ForeColor = Color.Yellow;

                await DownloadAccountHashFileAsync();

                _statusLabel.Text = "¡Registro completado!";
                _statusLabel.ForeColor = Color.LightGreen;

                await Task.Delay(1000);

                RegistrationCompleted?.Invoke(this, (_usernameInput.Text, _passwordInput.Text, _accountHash));
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"Error: {ex.Message}";
                _statusLabel.ForeColor = Color.Red;
                _registerButton.Enabled = true;
            }
        }

        private string GenerateAccountHash(string username, string password)
        {
            var combined = $"{username}|{password}|{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(combined);
                var hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }

        private async Task DownloadAccountHashFileAsync()
        {
            await Task.Run(() =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    using (var saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
                        saveDialog.FileName = $"Account_Hash_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                        saveDialog.Title = "Guardar Hash de Cuenta";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            var content = $"WoldVirtual3D - Hash de Registro de Cuenta\n" +
                                        $"===========================================\n\n" +
                                        $"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                                        $"Usuario: {_usernameInput.Text}\n" +
                                        $"Hash de Cuenta: {_accountHash}\n\n" +
                                        $"IMPORTANTE: Guarda este archivo en un lugar seguro.\n" +
                                        $"Lo necesitaras para acceder a tu cuenta.";

                            File.WriteAllText(saveDialog.FileName, content);
                        }
                    }
                });
            });
        }
    }
}

