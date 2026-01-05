using Godot;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace WoldVirtual3D.Viewer
{
    /// <summary>
    /// Gestor de base de datos SQLite para usuarios
    /// Responsabilidad: Operaciones CRUD de usuarios y sesiones
    /// </summary>
    public partial class UserDatabase : Node
    {
        private string _dbPath;
        private SQLiteConnection _connection;

        public override void _Ready()
        {
            base._Ready();
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            var userDataDir = OS.GetUserDataDir();
            var dbDir = Path.Combine(userDataDir, "DTUSER", "database");
            
            if (!Directory.Exists(dbDir))
            {
                Directory.CreateDirectory(dbDir);
            }

            _dbPath = Path.Combine(dbDir, "userdata.db");
            GD.Print($"UserDatabase: Ruta de BD: {_dbPath}");

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

                    GD.Print("UserDatabase: Tablas creadas/verificadas correctamente");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"UserDatabase: Error al crear tablas: {ex.Message}");
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
                        
                        var userId = Guid.NewGuid().ToString();
                        var passwordHash = HashPassword(password);
                        var createdAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

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

                        // Crear registro de settings por defecto
                        var insertSettingsCmd = @"
                            INSERT INTO user_settings (user_id, preferences, world_id)
                            VALUES (@user_id, '{}', '')";

                        using (var cmd = new SQLiteCommand(insertSettingsCmd, conn))
                        {
                            cmd.Parameters.AddWithValue("@user_id", userId);
                            cmd.ExecuteNonQuery();
                        }

                        GD.Print($"UserDatabase: Usuario creado: {username} (ID: {userId})");
                        return userId;
                    }
                }
                catch (SQLiteException ex) when (ex.Message.Contains("UNIQUE"))
                {
                    GD.PrintErr($"UserDatabase: Usuario ya existe: {username}");
                    return null;
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"UserDatabase: Error al crear usuario: {ex.Message}");
                    return null;
                }
            });
        }

        /// <summary>
        /// Autentica un usuario con credenciales
        /// </summary>
        public async Task<UserData> AuthenticateUserAsync(string username, string password)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                    {
                        conn.Open();
                        
                        var passwordHash = HashPassword(password);
                        
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
                                    return new UserData
                                    {
                                        Id = reader.GetString(0),
                                        Username = reader.GetString(1),
                                        Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                        CreatedAt = reader.GetString(3),
                                        LastLogin = reader.IsDBNull(4) ? "" : reader.GetString(4)
                                    };
                                }
                            }
                        }

                        return null;
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"UserDatabase: Error en autenticación: {ex.Message}");
                    return null;
                }
            });
        }

        /// <summary>
        /// Actualiza la fecha de último inicio de sesión
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

                        // Actualizar o crear sesión
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
                    GD.PrintErr($"UserDatabase: Error al actualizar último login: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Obtiene la última sesión activa
        /// </summary>
        public async Task<SessionData> GetLastActiveSessionAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                    {
                        conn.Open();
                        
                        var selectCmd = @"
                            SELECT user_id, username, last_login
                            FROM sessions
                            ORDER BY last_login DESC
                            LIMIT 1";

                        using (var cmd = new SQLiteCommand(selectCmd, conn))
                        {
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    return new SessionData
                                    {
                                        UserId = reader.GetString(0),
                                        Username = reader.GetString(1),
                                        LastLogin = DateTime.Parse(reader.GetString(2))
                                    };
                                }
                            }
                        }

                        return null;
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"UserDatabase: Error al obtener sesión: {ex.Message}");
                    return null;
                }
            });
        }

        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
        public async Task<UserData> GetUserByIdAsync(string userId)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var conn = new SQLiteConnection($"Data Source={_dbPath};Version=3;"))
                    {
                        conn.Open();
                        
                        var selectCmd = @"
                            SELECT id, username, email, created_at, last_login
                            FROM users
                            WHERE id = @user_id AND is_active = 1";

                        using (var cmd = new SQLiteCommand(selectCmd, conn))
                        {
                            cmd.Parameters.AddWithValue("@user_id", userId);

                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    return new UserData
                                    {
                                        Id = reader.GetString(0),
                                        Username = reader.GetString(1),
                                        Email = reader.IsDBNull(2) ? "" : reader.GetString(2),
                                        CreatedAt = reader.GetString(3),
                                        LastLogin = reader.IsDBNull(4) ? "" : reader.GetString(4)
                                    };
                                }
                            }
                        }

                        return null;
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"UserDatabase: Error al obtener usuario: {ex.Message}");
                    return null;
                }
            });
        }
    }
}

