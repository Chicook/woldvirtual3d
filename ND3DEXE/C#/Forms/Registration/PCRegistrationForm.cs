using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using WoldVirtual3D.Viewer.RegistroPC;
using WoldVirtual3D.Viewer.RegistroPC.Models;

namespace WoldVirtual3D.Viewer.Forms.Registration
{
    /// <summary>
    /// Formulario de registro de PC (Pantalla 1)
    /// Responsabilidad: Registrar placa base y procesador, generar hash unico
    /// </summary>
    public partial class PCRegistrationForm : Form
    {
        private readonly HardwareRegistrationService _registrationService;
        private HardwareInfo? _hardwareInfo;
        private Panel _mainPanel = null!;
        private Label _titleLabel = null!;
        private Label _subtitleLabel = null!;
        private PictureBox _pcIcon = null!;
        private Label _infoLabel = null!;
        private Button _registerButton = null!;
        private Label _statusLabel = null!;

        public event EventHandler<HardwareInfo>? RegistrationCompleted;
        public string? HardwareHash => _hardwareInfo?.HardwareHash;

        public PCRegistrationForm()
        {
            _registrationService = new HardwareRegistrationService();
            InitializeComponent();
            _ = LoadHardwareInfoAsync();
        }

        private void InitializeComponent()
        {
            this.Text = "WoldVirtual3D - Registro de PC";
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
            _subtitleLabel.Text = "Metaverso Descentralizado";
            _subtitleLabel.Font = new Font("Segoe UI", 12);
            _subtitleLabel.ForeColor = Color.LightGray;
            _subtitleLabel.AutoSize = false;
            _subtitleLabel.Size = new Size(600, 30);
            _subtitleLabel.Location = new Point(100, 100);
            _subtitleLabel.TextAlign = ContentAlignment.MiddleCenter;
            _mainPanel.Controls.Add(_subtitleLabel);

            var contentPanel = new Panel();
            contentPanel.Size = new Size(500, 300);
            contentPanel.Location = new Point(150, 160);
            contentPanel.BackColor = Color.FromArgb(45, 45, 45);
            contentPanel.BorderStyle = BorderStyle.None;
            _mainPanel.Controls.Add(contentPanel);

            _pcIcon = new PictureBox();
            _pcIcon.Size = new Size(80, 80);
            _pcIcon.Location = new Point(210, 50);
            _pcIcon.BackColor = Color.Transparent;
            _pcIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            contentPanel.Controls.Add(_pcIcon);
            DrawPCIcon();

            _infoLabel = new Label();
            _infoLabel.Text = "Registra tu PC para continuar";
            _infoLabel.Font = new Font("Segoe UI", 14);
            _infoLabel.ForeColor = Color.FromArgb(100, 181, 246);
            _infoLabel.AutoSize = false;
            _infoLabel.Size = new Size(400, 40);
            _infoLabel.Location = new Point(50, 150);
            _infoLabel.TextAlign = ContentAlignment.MiddleCenter;
            contentPanel.Controls.Add(_infoLabel);

            _registerButton = new Button();
            _registerButton.Text = "Registrarse (PC KYC)";
            _registerButton.Size = new Size(400, 50);
            _registerButton.Location = new Point(50, 220);
            _registerButton.BackColor = Color.FromArgb(0, 120, 215);
            _registerButton.ForeColor = Color.White;
            _registerButton.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            _registerButton.FlatStyle = FlatStyle.Flat;
            _registerButton.FlatAppearance.BorderSize = 0;
            _registerButton.Cursor = Cursors.Hand;
            _registerButton.Click += RegisterButton_Click;
            contentPanel.Controls.Add(_registerButton);

            _statusLabel = new Label();
            _statusLabel.Text = "";
            _statusLabel.Font = new Font("Segoe UI", 10);
            _statusLabel.ForeColor = Color.Yellow;
            _statusLabel.AutoSize = false;
            _statusLabel.Size = new Size(600, 30);
            _statusLabel.Location = new Point(100, 500);
            _statusLabel.TextAlign = ContentAlignment.MiddleCenter;
            _mainPanel.Controls.Add(_statusLabel);
        }

        private void DrawPCIcon()
        {
            var bmp = new Bitmap(80, 80);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                var brush = new SolidBrush(Color.FromArgb(100, 181, 246));
                var pen = new Pen(brush, 3);
                
                var rect = new Rectangle(15, 20, 50, 35);
                g.FillRectangle(brush, rect);
                g.DrawRectangle(pen, rect);
                
                var baseRect = new Rectangle(25, 55, 30, 5);
                g.FillRectangle(brush, baseRect);
            }
            _pcIcon.Image = bmp;
        }

        private async Task LoadHardwareInfoAsync()
        {
            try
            {
                var validation = await _registrationService.ValidateHardwareAsync();
                if (validation.IsValid && validation.HardwareInfo != null)
                {
                    _hardwareInfo = validation.HardwareInfo;
                    _registerButton.Text = "Continuar →";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando hardware: {ex.Message}");
            }
        }

        private async void RegisterButton_Click(object? sender, EventArgs e)
        {
            try
            {
                _registerButton.Enabled = false;
                _statusLabel.Text = "Registrando hardware...";
                _statusLabel.ForeColor = Color.Yellow;

                _hardwareInfo = await _registrationService.RegisterHardwareAsync();
                
                if (_hardwareInfo != null)
                {
                    _statusLabel.Text = "Hardware registrado. Descargando hash...";
                    await DownloadHashFileAsync();
                    
                    _statusLabel.Text = "¡Registro completado!";
                    _statusLabel.ForeColor = Color.LightGreen;
                    
                    await Task.Delay(1000);
                    
                    RegistrationCompleted?.Invoke(this, _hardwareInfo);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"Error: {ex.Message}";
                _statusLabel.ForeColor = Color.Red;
                _registerButton.Enabled = true;
            }
        }

        private async Task DownloadHashFileAsync()
        {
            await Task.Run(() =>
            {
                this.Invoke((MethodInvoker)delegate
                {
                    using (var saveDialog = new SaveFileDialog())
                    {
                        saveDialog.Filter = "Archivos de texto (*.txt)|*.txt|Todos los archivos (*.*)|*.*";
                        saveDialog.FileName = $"PC_Hash_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                        saveDialog.Title = "Guardar Hash del PC";

                        if (saveDialog.ShowDialog() == DialogResult.OK)
                        {
                            var content = $"WoldVirtual3D - Hash de Registro de PC\n" +
                                        $"========================================\n\n" +
                                        $"Fecha: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                                        $"Hash: {_hardwareInfo?.HardwareHash}\n" +
                                        $"Placa Base: {_hardwareInfo?.MotherboardSerial}\n" +
                                        $"Procesador: {_hardwareInfo?.ProcessorId}\n\n" +
                                        $"IMPORTANTE: Guarda este archivo en un lugar seguro.\n" +
                                        $"Lo necesitaras para continuar con tu registro.";

                            File.WriteAllText(saveDialog.FileName, content);
                        }
                    }
                });
            });
        }
    }
}

