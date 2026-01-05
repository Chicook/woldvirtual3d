using System;
using System.Threading.Tasks;

namespace WoldVirtual3D.Viewer
{
    /// <summary>
    /// Gestor de inicio de sesion y autenticacion de usuarios (version Windows Forms)
    /// Responsabilidad: Validar credenciales y gestionar sesiones
    /// </summary>
    public class LoginManager : IDisposable
    {
        private UserDatabase? _userDatabase;
        private bool _isAuthenticated = false;
        private string _currentUserId = "";
        private string _currentUsername = "";

        public event Action<string, string>? OnLoginSuccess;
        public event Action<string>? OnLoginFailed;
        public event Action? OnLogout;

        public bool IsAuthenticated => _isAuthenticated;
        public string CurrentUserId => _currentUserId;
        public string CurrentUsername => _currentUsername;

        public LoginManager(UserDatabase? userDatabase)
        {
            _userDatabase = userDatabase;
        }

        /// <summary>
        /// Intenta iniciar sesion con credenciales
        /// </summary>
        public async Task<bool> LoginAsync(string username, string password)
        {
            System.Diagnostics.Debug.WriteLine($"[LoginManager] LoginAsync llamado para usuario: {username}");
            
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                System.Diagnostics.Debug.WriteLine($"[LoginManager] Usuario o contraseña vacíos");
                OnLoginFailed?.Invoke("Usuario y contrasena requeridos");
                return false;
            }

            try
            {
                if (_userDatabase == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[LoginManager] Base de datos no inicializada");
                    OnLoginFailed?.Invoke("Base de datos no inicializada");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"[LoginManager] Llamando a AuthenticateUserAsync...");
                var user = await _userDatabase.AuthenticateUserAsync(username, password);
                
                if (user != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[LoginManager] Autenticación exitosa para: {username}");
                    _isAuthenticated = true;
                    _currentUserId = user.Id;
                    _currentUsername = user.Username;
                    
                    await _userDatabase.UpdateLastLoginAsync(_currentUserId);
                    
                    OnLoginSuccess?.Invoke(_currentUserId, _currentUsername);
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[LoginManager] Autenticación fallida - credenciales inválidas");
                    OnLoginFailed?.Invoke("Credenciales invalidas");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LoginManager] Excepción durante login: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[LoginManager] StackTrace: {ex.StackTrace}");
                OnLoginFailed?.Invoke($"Error del sistema: {ex.Message}");
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
                OnLoginFailed?.Invoke("Usuario y contrasena requeridos");
                return false;
            }

            if (password.Length < 6)
            {
                OnLoginFailed?.Invoke("La contrasena debe tener al menos 6 caracteres");
                return false;
            }

            try
            {
                if (_userDatabase == null)
                {
                    OnLoginFailed?.Invoke("Base de datos no inicializada");
                    return false;
                }

                var userId = await _userDatabase.CreateUserAsync(username, password, email);
                
                if (!string.IsNullOrEmpty(userId))
                {
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
                OnLoginFailed?.Invoke($"Error al crear usuario: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Cierra la sesion actual
        /// </summary>
        public void Logout()
        {
            if (_isAuthenticated)
            {
                _isAuthenticated = false;
                _currentUserId = "";
                _currentUsername = "";
                OnLogout?.Invoke();
            }
        }

        public void Dispose()
        {
            _userDatabase = null;
        }
    }
}
