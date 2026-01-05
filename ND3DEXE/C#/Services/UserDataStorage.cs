using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WoldVirtual3D.Viewer.RegistroPC.Models;

namespace WoldVirtual3D.Viewer.Services
{
    /// <summary>
    /// Servicio de almacenamiento de datos de usuario
    /// Responsabilidad: Guardar datos del usuario en formato JSON en userVS
    /// </summary>
    public class UserDataStorage
    {
        private readonly string _userVSPath;

        public UserDataStorage(string? projectRoot = null)
        {
            _userVSPath = @"D:\woldvirtual3d\ND3DEXE\DTUSER\userVS";

            try
            {
                var parentDir = Path.GetDirectoryName(_userVSPath);
                if (parentDir != null && !Directory.Exists(parentDir))
                {
                    Directory.CreateDirectory(parentDir);
                }

                if (!Directory.Exists(_userVSPath))
                {
                    Directory.CreateDirectory(_userVSPath);
                }

                System.Diagnostics.Debug.WriteLine($"[UserDataStorage] Ruta configurada: {_userVSPath}");
                System.Console.WriteLine($"[UserDataStorage] Ruta configurada: {_userVSPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UserDataStorage] Error configurando ruta: {ex.Message}");
                System.Console.WriteLine($"[UserDataStorage] Error configurando ruta: {ex.Message}");
            }
        }

        /// <summary>
        /// Guarda los datos completos del usuario en formato JSON
        /// </summary>
        public async Task SaveUserDataAsync(UserRegistrationData userData)
        {
            try
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(userData, jsonOptions);
                var fileName = $"user_data_{userData.Username}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                var filePath = Path.GetFullPath(Path.Combine(_userVSPath, fileName));
                
                await File.WriteAllTextAsync(filePath, json);
                
                System.Diagnostics.Debug.WriteLine($"[UserDataStorage] Datos guardados en: {filePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UserDataStorage] Error guardando datos: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Guarda datos de usuario en archivo principal (sobrescribe)
        /// </summary>
        public async Task SaveUserDataMainAsync(UserRegistrationData userData)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[UserDataStorage] SaveUserDataMainAsync llamado");
                System.Console.WriteLine($"[UserDataStorage] SaveUserDataMainAsync llamado");

                // Asegurar que el directorio existe
                if (!Directory.Exists(_userVSPath))
                {
                    Directory.CreateDirectory(_userVSPath);
                    System.Diagnostics.Debug.WriteLine($"[UserDataStorage] Directorio creado: {_userVSPath}");
                    System.Console.WriteLine($"[UserDataStorage] Directorio creado: {_userVSPath}");
                }

                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(userData, jsonOptions);
                
                // Usar ruta absoluta directa sin Path.GetFullPath para evitar resolución incorrecta
                var filePath = Path.Combine(_userVSPath, "user_data.json");
                
                System.Diagnostics.Debug.WriteLine($"[UserDataStorage] Ruta base: {_userVSPath}");
                System.Diagnostics.Debug.WriteLine($"[UserDataStorage] Ruta completa del archivo: {filePath}");
                System.Diagnostics.Debug.WriteLine($"[UserDataStorage] Directorio existe: {Directory.Exists(_userVSPath)}");
                System.Console.WriteLine($"[UserDataStorage] Ruta base: {_userVSPath}");
                System.Console.WriteLine($"[UserDataStorage] Ruta completa del archivo: {filePath}");
                System.Console.WriteLine($"[UserDataStorage] Directorio existe: {Directory.Exists(_userVSPath)}");
                
                await File.WriteAllTextAsync(filePath, json);
                
                // Verificar que el archivo se guardó correctamente
                var fileExists = File.Exists(filePath);
                if (fileExists)
                {
                    var fileInfo = new FileInfo(filePath);
                    System.Diagnostics.Debug.WriteLine($"[UserDataStorage] Archivo guardado exitosamente. Tamaño: {fileInfo.Length} bytes");
                    System.Console.WriteLine($"[UserDataStorage] JSON guardado exitosamente en: {filePath}");
                    System.Console.WriteLine($"[UserDataStorage] Tamaño del archivo: {fileInfo.Length} bytes");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[UserDataStorage] ERROR: El archivo no existe después de guardarlo");
                    System.Console.WriteLine($"[UserDataStorage] ERROR: El archivo no existe después de guardarlo");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[UserDataStorage] ERROR guardando: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[UserDataStorage] StackTrace: {ex.StackTrace}");
                System.Console.WriteLine($"[UserDataStorage] ERROR guardando: {ex.Message}");
                System.Console.WriteLine($"[UserDataStorage] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Carga los datos del usuario desde el archivo principal
        /// </summary>
        public async Task<UserRegistrationData?> LoadUserDataAsync()
        {
            try
            {
                var filePath = Path.GetFullPath(Path.Combine(_userVSPath, "user_data.json"));
                
                if (!File.Exists(filePath))
                {
                    return null;
                }

                var json = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }

                var userData = JsonSerializer.Deserialize<UserRegistrationData>(json);
                return userData;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando datos de usuario: {ex.Message}");
                return null;
            }
        }
    }

    /// <summary>
    /// Datos completos de registro del usuario
    /// </summary>
    public class UserRegistrationData
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string AccountHash { get; set; } = string.Empty;
        public string HardwareHash { get; set; } = string.Empty;
        public string SelectedAvatar { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
        public HardwareInfo? HardwareInfo { get; set; }
    }
}
