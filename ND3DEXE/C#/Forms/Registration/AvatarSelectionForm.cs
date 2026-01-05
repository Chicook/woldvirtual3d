using System;
using System.Drawing;
using System.Windows.Forms;
using WoldVirtual3D.Viewer.RegistroPC.Models;

namespace WoldVirtual3D.Viewer.Forms.Registration
{
    /// <summary>
    /// Formulario de seleccion de avatar (Pantalla 2)
    /// Responsabilidad: Permitir al usuario seleccionar su avatar (por ahora solo mujer)
    /// </summary>
    public partial class AvatarSelectionForm : Form
    {
        private Panel _mainPanel = null!;
        private Label _titleLabel = null!;
        private Label _subtitleLabel = null!;
        private Button _avatarMujerButton = null!;
        private Button _continueButton = null!;
        private Label _statusLabel = null!;

        public event EventHandler<string>? AvatarSelected;
        public string SelectedAvatar { get; private set; } = "";

        public AvatarSelectionForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "WoldVirtual3D - Seleccion de Avatar";
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
            _subtitleLabel.Text = "Selecciona tu Avatar";
            _subtitleLabel.Font = new Font("Segoe UI", 14);
            _subtitleLabel.ForeColor = Color.LightGray;
            _subtitleLabel.AutoSize = false;
            _subtitleLabel.Size = new Size(600, 30);
            _subtitleLabel.Location = new Point(100, 120);
            _subtitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            _mainPanel.Controls.Add(_subtitleLabel);

            var contentPanel = new Panel();
            contentPanel.Size = new Size(400, 350);
            contentPanel.Location = new Point(200, 180);
            contentPanel.BackColor = Color.FromArgb(45, 45, 45);
            contentPanel.BorderStyle = BorderStyle.None;
            _mainPanel.Controls.Add(contentPanel);

            var avatarLabel = new Label();
            avatarLabel.Text = "Avatar Disponible:";
            avatarLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            avatarLabel.ForeColor = Color.White;
            avatarLabel.AutoSize = false;
            avatarLabel.Size = new Size(350, 30);
            avatarLabel.Location = new Point(25, 30);
            avatarLabel.TextAlign = ContentAlignment.MiddleCenter;
            contentPanel.Controls.Add(avatarLabel);

            _avatarMujerButton = new Button();
            _avatarMujerButton.Text = "👩 Avatar Mujer";
            _avatarMujerButton.Size = new Size(300, 120);
            _avatarMujerButton.Location = new Point(50, 80);
            _avatarMujerButton.BackColor = Color.FromArgb(0, 120, 215);
            _avatarMujerButton.ForeColor = Color.White;
            _avatarMujerButton.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            _avatarMujerButton.FlatStyle = FlatStyle.Flat;
            _avatarMujerButton.FlatAppearance.BorderSize = 0;
            _avatarMujerButton.Cursor = Cursors.Hand;
            _avatarMujerButton.Click += AvatarMujerButton_Click;
            contentPanel.Controls.Add(_avatarMujerButton);

            var infoLabel = new Label();
            infoLabel.Text = "(Por el momento solo esta disponible\nel avatar mujer)";
            infoLabel.Font = new Font("Segoe UI", 10);
            infoLabel.ForeColor = Color.LightGray;
            infoLabel.AutoSize = false;
            infoLabel.Size = new Size(350, 50);
            infoLabel.Location = new Point(25, 210);
            infoLabel.TextAlign = ContentAlignment.MiddleCenter;
            contentPanel.Controls.Add(infoLabel);

            _continueButton = new Button();
            _continueButton.Text = "Continuar →";
            _continueButton.Size = new Size(300, 45);
            _continueButton.Location = new Point(50, 270);
            _continueButton.BackColor = Color.FromArgb(50, 50, 50);
            _continueButton.ForeColor = Color.White;
            _continueButton.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            _continueButton.FlatStyle = FlatStyle.Flat;
            _continueButton.FlatAppearance.BorderSize = 0;
            _continueButton.Cursor = Cursors.Hand;
            _continueButton.Enabled = false;
            _continueButton.Click += ContinueButton_Click;
            contentPanel.Controls.Add(_continueButton);

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

        private void AvatarMujerButton_Click(object? sender, EventArgs e)
        {
            SelectedAvatar = "mujer";
            _avatarMujerButton.BackColor = Color.FromArgb(0, 150, 0);
            _continueButton.Enabled = true;
            _continueButton.BackColor = Color.FromArgb(0, 120, 215);
            _statusLabel.Text = "Avatar seleccionado: Mujer";
            _statusLabel.ForeColor = Color.LightGreen;
        }

        private void ContinueButton_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(SelectedAvatar))
            {
                AvatarSelected?.Invoke(this, SelectedAvatar);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}

