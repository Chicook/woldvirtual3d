using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WoldVirtual3D.Viewer.Models;

namespace WoldVirtual3D.Viewer.Controls
{
    /// <summary>
    /// Panel de login para autenticación de usuarios
    /// Responsabilidad: Mostrar UI de login y capturar credenciales
    /// </summary>
    public class LoginPanel : Panel
    {
        private TextBox txtUsuario = null!;
        private TextBox txtContrasena = null!;
        private CheckBox chkRecordarNombre = null!;
        private CheckBox chkRecordarContrasena = null!;
        private Button btnIniciarSesion = null!;
        private Label lblError = null!;

        public event EventHandler<LoginEventArgs>? OnLoginClick;

        public LoginPanel()
        {
            InitializeComponent();
            SetupControls();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(600, 300);
            this.BackColor = Color.FromArgb(30, 30, 35);
            this.Paint += LoginPanel_Paint;
        }

        private void SetupControls()
        {
            int centerX = this.Width / 2;
            int startY = 50;
            int spacing = 40;
            int controlWidth = 250;
            int controlHeight = 30;

            // Título
            var lblTitulo = new Label
            {
                Text = "WoldVirtual3D - Iniciar Sesión",
                Location = new Point(centerX - 150, 20),
                Size = new Size(300, 30),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Usuario
            var lblUsuario = new Label
            {
                Text = "Usuario:",
                Location = new Point(centerX - controlWidth / 2, startY),
                Size = new Size(controlWidth, 20),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            txtUsuario = new TextBox
            {
                Location = new Point(centerX - controlWidth / 2, startY + 25),
                Size = new Size(controlWidth, controlHeight),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            chkRecordarNombre = new CheckBox
            {
                Text = "Recordar nombre de usuario",
                Location = new Point(centerX - controlWidth / 2, startY + controlHeight + 30),
                Size = new Size(controlWidth, 20),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Checked = true
            };

            // Contraseña
            int passwordY = startY + spacing * 3;
            var lblContrasena = new Label
            {
                Text = "Contraseña:",
                Location = new Point(centerX - controlWidth / 2, passwordY),
                Size = new Size(controlWidth, 20),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            txtContrasena = new TextBox
            {
                Location = new Point(centerX - controlWidth / 2, passwordY + 25),
                Size = new Size(controlWidth, controlHeight),
                PasswordChar = '•',
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtContrasena.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    BtnIniciarSesion_Click(null, null);
                }
            };

            chkRecordarContrasena = new CheckBox
            {
                Text = "Recordar contraseña",
                Location = new Point(centerX - controlWidth / 2, passwordY + controlHeight + 30),
                Size = new Size(controlWidth, 20),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };

            // Botón iniciar sesión
            btnIniciarSesion = new Button
            {
                Text = "Iniciar Sesión",
                Location = new Point(centerX - 100, passwordY + controlHeight + 70),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnIniciarSesion.FlatAppearance.BorderSize = 0;
            btnIniciarSesion.Click += BtnIniciarSesion_Click;

            // Label de error
            lblError = new Label
            {
                Text = "",
                Location = new Point(centerX - controlWidth / 2, passwordY + controlHeight + 120),
                Size = new Size(controlWidth, 30),
                ForeColor = Color.Red,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleCenter,
                Visible = false
            };

            this.Controls.AddRange(new Control[] {
                lblTitulo,
                lblUsuario, txtUsuario, chkRecordarNombre,
                lblContrasena, txtContrasena, chkRecordarContrasena,
                btnIniciarSesion,
                lblError
            });
        }

        private void LoginPanel_Paint(object? sender, PaintEventArgs e)
        {
            if (e == null) return;
            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                Color.FromArgb(25, 25, 30),
                Color.FromArgb(35, 35, 40),
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void BtnIniciarSesion_Click(object? sender, EventArgs? e)
        {
            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                ShowError("Por favor ingrese un nombre de usuario");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtContrasena.Text))
            {
                ShowError("Por favor ingrese una contraseña");
                return;
            }

            HideError();

            var args = new LoginEventArgs
            {
                Usuario = txtUsuario.Text.Trim(),
                Contrasena = txtContrasena.Text,
                RecordarNombre = chkRecordarNombre.Checked,
                RecordarContrasena = chkRecordarContrasena.Checked
            };

            OnLoginClick?.Invoke(this, args);
        }

        public void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
        }

        public void HideError()
        {
            lblError.Visible = false;
            lblError.Text = "";
        }

        public void ResetForm()
        {
            txtUsuario.Clear();
            txtContrasena.Clear();
            HideError();
        }
    }

    public class LoginEventArgs : EventArgs
    {
        public string Usuario { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public bool RecordarNombre { get; set; }
        public bool RecordarContrasena { get; set; }
    }
}

