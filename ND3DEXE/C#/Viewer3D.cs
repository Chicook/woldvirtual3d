using Godot;
using System;
using System.Threading.Tasks;

namespace WoldVirtual3D.Viewer
{
    /// <summary>
    /// Visor 3D principal que gestiona la carga de escenas después del login
    /// Responsabilidad: Coordinar inicio de sesión y carga de bsprincipal.tscn
    /// </summary>
    public partial class Viewer3D : Node
    {
        private LoginManager _loginManager;
        private Control _loginUI;
        private Node3D _mainScene;
        private bool _sceneLoaded = false;

        private const string MAIN_SCENE_PATH = "res://bsprincipal.tscn";
        private const string LOGIN_UI_SCENE_PATH = "res://ND3DEXE/C#/UI/LoginUI.tscn";

        public override void _Ready()
        {
            base._Ready();
            InitializeViewer();
        }

        private async void InitializeViewer()
        {
            GD.Print("Viewer3D: Inicializando visor 3D...");

            // Obtener LoginManager del autoload
            _loginManager = GetNode<LoginManager>("/root/LoginManager");
            
            if (_loginManager == null)
            {
                GD.PrintErr("Viewer3D: LoginManager no encontrado en autoload");
                return;
            }

            // Suscribirse a eventos de login
            _loginManager.OnLoginSuccess += OnLoginSuccessful;
            _loginManager.OnLoginFailed += OnLoginFailed;
            _loginManager.OnLogout += OnLogout;

            // Mostrar UI de login
            await ShowLoginUI();

            // Intentar restaurar sesión guardada
            var hasSession = await _loginManager.CheckSavedSessionAsync();
            
            if (!hasSession)
            {
                GD.Print("Viewer3D: No hay sesión guardada, mostrando login");
            }
        }

        private async Task ShowLoginUI()
        {
            // Crear UI de login básica si no existe la escena
            if (!ResourceLoader.Exists(LOGIN_UI_SCENE_PATH))
            {
                CreateBasicLoginUI();
            }
            else
            {
                var loginScene = GD.Load<PackedScene>(LOGIN_UI_SCENE_PATH);
                _loginUI = loginScene.Instantiate<Control>();
                AddChild(_loginUI);
            }
        }

        private void CreateBasicLoginUI()
        {
            _loginUI = new Control();
            _loginUI.Name = "LoginUI";
            _loginUI.SetAnchorsAndOffsetsPreset(Control.PresetMode.FullRect);
            AddChild(_loginUI);

            var vbox = new VBoxContainer();
            vbox.SetAnchorsAndOffsetsPreset(Control.PresetMode.Center);
            vbox.Size = new Vector2(400, 300);
            _loginUI.AddChild(vbox);

            var title = new Label();
            title.Text = "WoldVirtual3D";
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeFontSizeOverride("font_size", 32);
            vbox.AddChild(title);

            var usernameLabel = new Label();
            usernameLabel.Text = "Usuario:";
            vbox.AddChild(usernameLabel);

            var usernameInput = new LineEdit();
            usernameInput.PlaceholderText = "Ingrese su usuario";
            usernameInput.Name = "UsernameInput";
            vbox.AddChild(usernameInput);

            var passwordLabel = new Label();
            passwordLabel.Text = "Contraseña:";
            vbox.AddChild(passwordLabel);

            var passwordInput = new LineEdit();
            passwordInput.PlaceholderText = "Ingrese su contraseña";
            passwordInput.Secret = true;
            passwordInput.Name = "PasswordInput";
            vbox.AddChild(passwordInput);

            var loginButton = new Button();
            loginButton.Text = "Iniciar Sesión";
            loginButton.Pressed += async () => await OnLoginButtonPressed(usernameInput, passwordInput);
            vbox.AddChild(loginButton);

            var registerButton = new Button();
            registerButton.Text = "Registrarse";
            registerButton.Pressed += async () => await OnRegisterButtonPressed(usernameInput, passwordInput);
            vbox.AddChild(registerButton);

            var statusLabel = new Label();
            statusLabel.Name = "StatusLabel";
            statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(statusLabel);
        }

        private async Task OnLoginButtonPressed(LineEdit usernameInput, LineEdit passwordInput)
        {
            var username = usernameInput.Text;
            var password = passwordInput.Text;

            var statusLabel = _loginUI.GetNode<Label>("StatusLabel");
            if (statusLabel != null)
            {
                statusLabel.Text = "Autenticando...";
            }

            var success = await _loginManager.LoginAsync(username, password);

            if (!success && statusLabel != null)
            {
                statusLabel.Text = "Error: Credenciales inválidas";
            }
        }

        private async Task OnRegisterButtonPressed(LineEdit usernameInput, LineEdit passwordInput)
        {
            var username = usernameInput.Text;
            var password = passwordInput.Text;

            var statusLabel = _loginUI.GetNode<Label>("StatusLabel");
            if (statusLabel != null)
            {
                statusLabel.Text = "Registrando...";
            }

            var success = await _loginManager.RegisterAsync(username, password);

            if (!success && statusLabel != null)
            {
                statusLabel.Text = "Error: No se pudo registrar";
            }
        }

        private async void OnLoginSuccessful(string userId, string username)
        {
            GD.Print($"Viewer3D: Login exitoso para {username}, cargando escena principal...");

            // Ocultar UI de login
            if (_loginUI != null)
            {
                _loginUI.Visible = false;
            }

            // Cargar escena principal
            await LoadMainScene();
        }

        private async Task LoadMainScene()
        {
            if (_sceneLoaded)
            {
                GD.Print("Viewer3D: Escena principal ya cargada");
                return;
            }

            try
            {
                if (!ResourceLoader.Exists(MAIN_SCENE_PATH))
                {
                    GD.PrintErr($"Viewer3D: No se encuentra la escena: {MAIN_SCENE_PATH}");
                    return;
                }

                var mainScene = GD.Load<PackedScene>(MAIN_SCENE_PATH);
                
                if (mainScene == null)
                {
                    GD.PrintErr("Viewer3D: Error al cargar la escena principal");
                    return;
                }

                _mainScene = mainScene.Instantiate<Node3D>();
                AddChild(_mainScene);

                _sceneLoaded = true;
                GD.Print($"Viewer3D: Escena principal cargada: {MAIN_SCENE_PATH}");

                // Inicializar red P2P después de cargar la escena
                await InitializeP2PNetwork();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Viewer3D: Error al cargar escena principal: {ex.Message}");
            }
        }

        private async Task InitializeP2PNetwork()
        {
            GD.Print("Viewer3D: Inicializando red P2P...");

            var p2pNetwork = GetNode<P2PNetwork>("/root/P2PNetwork");
            
            if (p2pNetwork != null)
            {
                await p2pNetwork.InitializeAsync(_loginManager.CurrentUserId);
            }
        }

        private void OnLoginFailed(string errorMessage)
        {
            GD.PrintErr($"Viewer3D: Error de login: {errorMessage}");
            
            var statusLabel = _loginUI?.GetNode<Label>("StatusLabel");
            if (statusLabel != null)
            {
                statusLabel.Text = $"Error: {errorMessage}";
            }
        }

        private void OnLogout()
        {
            GD.Print("Viewer3D: Usuario cerró sesión");

            // Eliminar escena principal
            if (_mainScene != null)
            {
                _mainScene.QueueFree();
                _mainScene = null;
                _sceneLoaded = false;
            }

            // Mostrar UI de login nuevamente
            if (_loginUI != null)
            {
                _loginUI.Visible = true;
            }

            // Desconectar red P2P
            var p2pNetwork = GetNode<P2PNetwork>("/root/P2PNetwork");
            p2pNetwork?.Disconnect();
        }

        public override void _ExitTree()
        {
            if (_loginManager != null)
            {
                _loginManager.OnLoginSuccess -= OnLoginSuccessful;
                _loginManager.OnLoginFailed -= OnLoginFailed;
                _loginManager.OnLogout -= OnLogout;
            }

            base._ExitTree();
        }
    }
}

