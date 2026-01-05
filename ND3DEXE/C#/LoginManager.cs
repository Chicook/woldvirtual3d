using Godot;
using System;
using System.Threading.Tasks;

namespace WoldVirtual3D.Viewer
{
    /// <summary>
    /// Gestor de inicio de sesión y autenticación de usuarios
    /// Responsabilidad: Validar credenciales y gestionar sesiones
    /// </summary>
    public partial class LoginManager : Node
    {
        private UserDatabase _userDatabase;
        private bool _isAuthenticated = false;
        private string _currentUserId = "";
        private string _currentUsername = "";

        public event Action<string, string> OnLoginSuccess;
        public event Action<string> OnLoginFailed;
        public event Action OnLogout;

        public bool IsAuthenticated => _isAuthenticated;
        public string CurrentUserId => _currentUserId;
        public string CurrentUsername => _currentUsername;

        public override void _Ready()
        {
            base._Ready();
            _userDatabase = GetNode<UserDatabase>("/root/UserDatabase");
            
            if (_userDatabase == null)
            {
                GD.PrintErr("LoginManager: UserDatabase no encontrado en autoload");
            }

            GD.Print("LoginManager: Sistema de autenticación inicializado");
        }

        /// <summary>
        /// Intenta iniciar sesión con credenciales
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                OnLoginFailed?.Invoke("Usuario y contraseña requeridos");
                return false;
            }

            try
            {
                var user = await _userDatabase.AuthenticateUserAsync(username, password);
                
                if (user != null)
                {
                    _isAuthenticated = true;
                    _currentUserId = user.Id;
                    _currentUsername = user.Username;
                    
                    await _userDatabase.UpdateLastLoginAsync(_currentUserId);
                    
                    GD.Print($"LoginManager: Usuario autenticado: {_currentUsername} (ID: {_currentUserId})");
                    OnLoginSuccess?.Invoke(_currentUserId, _currentUsername);
                    
                    return true;
                }
                else
                {
                    OnLoginFailed?.Invoke("Credenciales inválidas");
                    return false;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"LoginManager: Error en autenticación: {ex.Message}");
                OnLoginFailed?.Invoke("Error del sistema. Intente más tarde");
                return false;
            }
        }

        /// <summary>
        /// Registra un nuevo usuario
        /// </summary>
        public async Task<bool> RegisterAsync(string username, string password, string email = "")
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                OnLoginFailed?.Invoke("Usuario y contraseña requeridos");
                return false;
            }

            if (password.Length < 6)
            {
                OnLoginFailed?.Invoke("La contraseña debe tener al menos 6 caracteres");
                return false;
            }

            try
            {
                var userId = await _userDatabase.CreateUserAsync(username, password, email);
                
                if (!string.IsNullOrEmpty(userId))
                {
                    GD.Print($"LoginManager: Usuario registrado: {username} (ID: {userId})");
                    
                    // Auto-login después del registro
                    return await LoginAsync(username, password);
                }
                else
                {
                    OnLoginFailed?.Invoke("El usuario ya existe");
                    return false;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"LoginManager: Error en registro: {ex.Message}");
                OnLoginFailed?.Invoke("Error al crear usuario");
                return false;
            }
        }

        /// <summary>
        /// Cierra la sesión actual
        /// </summary>
        public void Logout()
        {
            if (_isAuthenticated)
            {
                GD.Print($"LoginManager: Cerrando sesión de usuario: {_currentUsername}");
                
                _isAuthenticated = false;
                _currentUserId = "";
                _currentUsername = "";
                
                OnLogout?.Invoke();
            }
        }

        /// <summary>
        /// Verifica si hay una sesión activa guardada
        /// </summary>
        public async Task<bool> CheckSavedSessionAsync()
        {
            try
            {
                var savedSession = await _userDatabase.GetLastActiveSessionAsync();
                
                if (savedSession != null && !string.IsNullOrEmpty(savedSession.UserId))
                {
                    var timeSinceLogin = DateTime.UtcNow - savedSession.LastLogin;
                    
                    // Sesión válida si fue hace menos de 7 días
                    if (timeSinceLogin.TotalDays < 7)
                    {
                        _isAuthenticated = true;
                        _currentUserId = savedSession.UserId;
                        _currentUsername = savedSession.Username;
                        
                        GD.Print($"LoginManager: Sesión restaurada para: {_currentUsername}");
                        OnLoginSuccess?.Invoke(_currentUserId, _currentUsername);
                        
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"LoginManager: Error al verificar sesión: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Obtiene información del usuario actual
        /// </summary>
        public async Task<UserData> GetCurrentUserDataAsync()
        {
            if (!_isAuthenticated || string.IsNullOrEmpty(_currentUserId))
            {
                return null;
            }

            return await _userDatabase.GetUserByIdAsync(_currentUserId);
        }
    }
}

