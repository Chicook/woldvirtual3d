using System;
using System.Data.SQLite;
using System.IO;
using WoldVirtual3D.Viewer.Models;

namespace WoldVirtual3D.Viewer.Services
{
    /// <summary>
    /// Base de datos SQLite para usuarios
    /// Responsabilidad: Operaciones CRUD con la base de datos de usuarios
    /// </summary>
    public class UserDatabase : IDisposable
    {
        private string dbPath;
        private SQLiteConnection? connection;

        public UserDatabase()
        {
            // Crear carpeta DTUSER si no existe
            var dtuserPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "DTUSER", "database");
            if (!Directory.Exists(dtuserPath))
            {
                Directory.CreateDirectory(dtuserPath);
            }

            dbPath = Path.Combine(dtuserPath, "userdata.db");
            InitializeDatabase();
        }

        /// <summary>
        /// Inicializa la base de datos y crea las tablas si no existen
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                connection = new SQLiteConnection($"Data Source={dbPath};Version=3;");
                connection.Open();

                // Crear tabla de usuarios si no existe
                string createTable = @"
                    CREATE TABLE IF NOT EXISTS users (
                        id INTEGER PRIMARY KEY AUTOINCREMENT,
                        username TEXT UNIQUE NOT NULL,
                        password_hash TEXT NOT NULL,
                        created_at TEXT NOT NULL,
                        last_login_at TEXT NOT NULL,
                        remember_username INTEGER DEFAULT 0,
                        remember_password INTEGER DEFAULT 0
                    )";

                using (var command = new SQLiteCommand(createTable, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al inicializar base de datos: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene un usuario por nombre de usuario
        /// </summary>
        public UserData? GetUserByUsername(string username)
        {
            if (connection == null)
                return null;

            try
            {
                string query = "SELECT * FROM users WHERE username = @username";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserData
                            {
                                Id = reader.GetInt32(0),
                                Username = reader.GetString(1),
                                PasswordHash = reader.GetString(2),
                                CreatedAt = DateTime.Parse(reader.GetString(3)),
                                LastLoginAt = DateTime.Parse(reader.GetString(4)),
                                RememberUsername = reader.GetInt32(5) == 1,
                                RememberPassword = reader.GetInt32(6) == 1
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al obtener usuario: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// Guarda un usuario (crea o actualiza)
        /// </summary>
        public bool SaveUser(UserData user)
        {
            if (connection == null)
                return false;

            try
            {
                // Verificar si el usuario ya existe
                var existing = GetUserByUsername(user.Username);
                
                if (existing != null)
                {
                    // Actualizar
                    string update = @"UPDATE users SET 
                        password_hash = @password_hash,
                        last_login_at = @last_login_at,
                        remember_username = @remember_username,
                        remember_password = @remember_password
                        WHERE username = @username";

                    using (var command = new SQLiteCommand(update, connection))
                    {
                        command.Parameters.AddWithValue("@username", user.Username);
                        command.Parameters.AddWithValue("@password_hash", user.PasswordHash);
                        command.Parameters.AddWithValue("@last_login_at", user.LastLoginAt.ToString("O"));
                        command.Parameters.AddWithValue("@remember_username", user.RememberUsername ? 1 : 0);
                        command.Parameters.AddWithValue("@remember_password", user.RememberPassword ? 1 : 0);
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    // Insertar nuevo
                    string insert = @"INSERT INTO users 
                        (username, password_hash, created_at, last_login_at, remember_username, remember_password)
                        VALUES (@username, @password_hash, @created_at, @last_login_at, @remember_username, @remember_password)";

                    using (var command = new SQLiteCommand(insert, connection))
                    {
                        command.Parameters.AddWithValue("@username", user.Username);
                        command.Parameters.AddWithValue("@password_hash", user.PasswordHash);
                        command.Parameters.AddWithValue("@created_at", user.CreatedAt.ToString("O"));
                        command.Parameters.AddWithValue("@last_login_at", user.LastLoginAt.ToString("O"));
                        command.Parameters.AddWithValue("@remember_username", user.RememberUsername ? 1 : 0);
                        command.Parameters.AddWithValue("@remember_password", user.RememberPassword ? 1 : 0);
                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al guardar usuario: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Actualiza el último login de un usuario
        /// </summary>
        public void UpdateLastLogin(string username)
        {
            if (connection == null)
                return;

            try
            {
                string update = "UPDATE users SET last_login_at = @last_login_at WHERE username = @username";
                using (var command = new SQLiteCommand(update, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@last_login_at", DateTime.UtcNow.ToString("O"));
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al actualizar último login: {ex.Message}");
            }
        }

        public void Dispose()
        {
            connection?.Close();
            connection?.Dispose();
        }
    }
}

