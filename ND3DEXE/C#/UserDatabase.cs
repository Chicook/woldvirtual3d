using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace WoldVirtual3D.Viewer
{
    /// <summary>
    /// Gestor de base de datos SQLite para usuarios (version Windows Forms)
    /// Responsabilidad: Operaciones CRUD de usuarios y sesiones
    /// </summary>
    public class UserDatabase : IDisposable
    {
        private string _dbPath = "";

        public UserDatabase()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            var userDataDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbDir = Path.Combine(userDataDir, "WoldVirtual3D", "DTUSER", "database");
            
            if (!Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
            }

            _dbPath = Path.Combine(dbDir, "userdata.db");
            CreateTablesIfNotExist();
        }

        private void CreateTablesIfNotExist()
        {
            try
            {
                using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                {
                    conn.Open();
                    
                    var createUsersTable = @"
                        CREATE TABLE IF NOT EXISTS users (
                            id TEXT PRIMARY KEY,
                            username TEXT UNIQUE NOT NULL,
                            password_hash TEXT NOT NULL,
                            email TEXT,
                            created_at TEXT NOT NULL,
                            last_login TEXT,
                            is_active INTEGER DEFAULT 1
                        )";

                    var createSessionsTable = @"
                        CREATE TABLE IF NOT EXISTS sessions (
                            id TEXT PRIMARY KEY,
                            user_id TEXT NOT NULL,
                            username TEXT NOT NULL,
                            last_login TEXT NOT NULL,
                            FOREIGN KEY (user_id) REFERENCES users(id)
                        )";

                    var createUserSettingsTable = @"
                        CREATE TABLE IF NOT EXISTS user_settings (
                            user_id TEXT PRIMARY KEY,
                            avatar_data TEXT,
                            preferences TEXT,
                            world_id TEXT,
                            FOREIGN KEY (user_id) REFERENCES users(id)
                        )";

                    using (var cmd = new SQLiteCommand(createUsersTable, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SQLiteCommand(createSessionsTable, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SQLiteCommand(createUserSettingsTable, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UserDatabase: Error al crear tablas: {ex.Message}");
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Crea un nuevo usuario en la base de datos
        /// </summary>
        public async Task<string> CreateUserAsync(string username, string password, string email = "")
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                    {
                        conn.Open();
                        
                        // Verificar si el usuario ya existe
                        var checkCmd = @"SELECT id FROM users WHERE username = @username";
                        using (var check = new SQLiteCommand(checkCmd, conn))
                        {
                            check.Parameters.AddWithValue("@username", username);
                            var existingId = check.ExecuteScalar();
                            if (existingId != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"[UserDatabase] Usuario ya existe: {username}");
                                return "";
                            }
                        }
                        
                        var userId = Guid.NewGuid().ToString();
                        var passwordHash = HashPassword(password);
                        var createdAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                        System.Diagnostics.Debug.WriteLine($"[UserDatabase] Creando usuario: {username}");
                        System.Diagnostics.Debug.WriteLine($"[UserDatabase] Password hash: {passwordHash}");

                        var insertCmd = @"
                            INSERT INTO users (id, username, password_hash, email, created_at, is_active)
                            VALUES (@id, @username, @password_hash, @email, @created_at, 1)";

                        using (var cmd = new SQLiteCommand(insertCmd, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", userId);
                            cmd.Parameters.AddWithValue("@username", username);
                            cmd.Parameters.AddWithValue("@password_hash", passwordHash);
                            cmd.Parameters.AddWithValue("@email", email);
                            cmd.Parameters.AddWithValue("@created_at", createdAt);

                            cmd.ExecuteNonQuery();
                        }

                        var insertSettingsCmd = @"
                            INSERT INTO user_settings (user_id, preferences, world_id)
                            VALUES (@user_id, '{}', '')";

                        using (var cmd = new SQLiteCommand(insertSettingsCmd, conn))
                        {
                            cmd.Parameters.AddWithValue("@user_id", userId);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show($"Usuario creado exitosamente:\n\nUsuario: {username}\nID: {userId}", "Debug Create Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return userId;
                    }
                }
                catch (SQLiteException ex) when (ex.Message.Contains("UNIQUE"))
                {
                    System.Diagnostics.Debug.WriteLine($"[UserDatabase] Error UNIQUE al crear usuario: {ex.Message}");
                    return "";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[UserDatabase] Error al crear usuario: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[UserDatabase] StackTrace: {ex.StackTrace}");
                    return "";
                }
            });
        }

        /// <summary>
        /// Autentica un usuario con credenciales
        /// </summary>
        public async Task<UserData?> AuthenticateUserAsync(string username, string password)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                    {
                        conn.Open();
                        
                        var passwordHash = HashPassword(password);
                        
                        // Debug: verificar si el usuario existe
                        var checkUserCmd = @"SELECT username, password_hash FROM users WHERE username = @username";
                        using (var checkCmd = new SQLiteCommand(checkUserCmd, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@username", username);
                            using (var checkReader = checkCmd.ExecuteReader())
                            {
                                if (checkReader.Read())
                                {
                                    var storedHash = checkReader.GetString(1);
                                    var hashMatch = storedHash == passwordHash;
                                    var msg = $"Usuario encontrado: {username}\n\nHash almacenado: {storedHash}\nHash calculado: {passwordHash}\n\nCoinciden: {hashMatch}";
                                    MessageBox.Show(msg, "Debug Auth", MessageBoxButtons.OK, hashMatch ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                                }
                                else
                                {
                                    MessageBox.Show($"Usuario NO encontrado en la base de datos: {username}", "Debug Auth", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                        }
                        
                        var selectCmd = @"
                            SELECT id, username, email, created_at, last_login
                            FROM users
                            WHERE username = @username 
                            AND password_hash = @password_hash 
                            AND is_active = 1";

                        using (var cmd = new SQLiteCommand(selectCmd, conn))
                        {
                            cmd.Parameters.AddWithValue("@username", username);
                            cmd.Parameters.AddWithValue("@password_hash", passwordHash);

                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    System.Diagnostics.Debug.WriteLine($"[UserDatabase] Autenticacion exitosa para: {username}");
                                    return new UserData
                                    {
                                        Id = reader.GetString(0),
                                        Username = reader.GetString(1),
                                        Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                        CreatedAt = reader.GetString(3),
                                        LastLogin = reader.IsDBNull(4) ? "" : reader.GetString(4)
                                    };
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine($"[UserDatabase] Autenticacion fallida para: {username}");
                                }
                            }
                        }

                        return null;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[UserDatabase] Error en autenticacion: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[UserDatabase] StackTrace: {ex.StackTrace}");
                    return null;
                }
            });
        }

        /// <summary>
        /// Actualiza la fecha de ultimo inicio de sesion
        /// </summary>
        public async Task UpdateLastLoginAsync(string userId)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                    {
                        conn.Open();
                        
                        var lastLogin = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                        var updateCmd = @"
                            UPDATE users 
                            SET last_login = @last_login
                            WHERE id = @user_id";

                        using (var cmd = new SQLiteCommand(updateCmd, conn))
                        {
                            cmd.Parameters.AddWithValue("@last_login", lastLogin);
                            cmd.Parameters.AddWithValue("@user_id", userId);
                            cmd.ExecuteNonQuery();
                        }

                        var sessionCmd = @"
                            INSERT OR REPLACE INTO sessions (id, user_id, username, last_login)
                            SELECT @session_id, u.id, u.username, @last_login
                            FROM users u
                            WHERE u.id = @user_id";

                        using (var cmd = new SQLiteCommand(sessionCmd, conn))
                        {
                            cmd.Parameters.AddWithValue("@session_id", Guid.NewGuid().ToString());
                            cmd.Parameters.AddWithValue("@user_id", userId);
                            cmd.Parameters.AddWithValue("@last_login", lastLogin);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"UserDatabase: Error al actualizar ultimo login: {ex.Message}");
                }
            });
        }

        public void Dispose()
        {
            // SQLite connections are disposed automatically
        }
    }
}
