using System;
using System.Security.Cryptography;
using System.Text;
using WoldVirtual3D.Viewer.Models;

namespace WoldVirtual3D.Viewer.Services
{
    /// <summary>
    /// Gestor de autenticación de usuarios
    /// Responsabilidad: Validar credenciales y gestionar sesiones
    /// </summary>
    public class LoginManager
    {
        private UserDatabase? userDatabase;

        public LoginManager()
        {
            userDatabase = new UserDatabase();
        }

        /// <summary>
        /// Autentica un usuario con nombre y contraseña
        /// </summary>
        public bool AuthenticateUser(string username, string password, out UserData? user)
        {
            user = null;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (userDatabase == null)
            {
                return false;
            }

            // Buscar usuario
            user = userDatabase.GetUserByUsername(username);
            if (user == null)
            {
                // Si no existe, crear nuevo usuario
                user = new UserData
                {
                    Username = username,
                    PasswordHash = HashPassword(password),
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                if (userDatabase.SaveUser(user))
                {
                    return true;
                }
                return false;
            }

            // Verificar contraseña
            string passwordHash = HashPassword(password);
            if (user.PasswordHash == passwordHash)
            {
                // Actualizar último login
                user.LastLoginAt = DateTime.UtcNow;
                userDatabase.UpdateLastLogin(username);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Hashea una contraseña usando SHA256
        /// </summary>
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Verifica si un usuario existe
        /// </summary>
        public bool UserExists(string username)
        {
            if (userDatabase == null)
                return false;

            var user = userDatabase.GetUserByUsername(username);
            return user != null;
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        public bool CreateUser(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (UserExists(username))
            {
                return false; // Usuario ya existe
            }

            if (userDatabase == null)
            {
                return false;
            }

            var user = new UserData
            {
                Username = username,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            return userDatabase.SaveUser(user);
        }
    }
}

