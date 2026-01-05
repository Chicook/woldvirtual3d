using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WoldVirtual3D.Viewer.RegistroPC.Models;

namespace WoldVirtual3D.Viewer.RegistroPC
{
    /// <summary>
    /// Servicio completo de registro y validacion de hardware
    /// Responsabilidad: Integrar con DTUSER para almacenar y validar hash de hardware
    /// </summary>
    public class HardwareRegistrationService
    {
        private readonly HardwareIdentifier _hardwareIdentifier;
        private readonly string _dtuserPath;
        private readonly string _hardwareConfigPath;

        public HardwareRegistrationService(string? projectRoot = null)
        {
            _hardwareIdentifier = new HardwareIdentifier();
            
            if (string.IsNullOrEmpty(projectRoot))
            {
                var currentDir = Directory.GetCurrentDirectory();
                var dtuserDir = Path.Combine(currentDir, "DTUSER");
                
                if (!Directory.Exists(dtuserDir))
                {
                    var parentDir = Directory.GetParent(currentDir)?.FullName;
                    if (parentDir != null)
                    {
                        dtuserDir = Path.Combine(parentDir, "DTUSER");
                    }
                }
                
                _dtuserPath = dtuserDir;
            }
            else
            {
                _dtuserPath = Path.Combine(projectRoot, "DTUSER");
            }

            var configDir = Path.Combine(_dtuserPath, "config");
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
            _hardwareConfigPath = Path.Combine(configDir, "hardware_hash.json");
        }

        /// <summary>
        /// Registra el hardware del PC al descargar el visor
        /// Genera hash unico y lo almacena en DTUSER
        /// </summary>
        public async Task<HardwareInfo> RegisterHardwareAsync()
        {
            try
            {
                var hardwareInfo = await _hardwareIdentifier.GetHardwareInfoAsync();
                await SaveHardwareInfoAsync(hardwareInfo);
                System.Diagnostics.Debug.WriteLine($"Hardware registrado exitosamente: {hardwareInfo}");
                return hardwareInfo;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registrando hardware: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Valida el hardware al iniciar sesion
        /// Compara hash actual con hash almacenado
        /// </summary>
        public async Task<HardwareValidationResult> ValidateHardwareAsync()
        {
            try
            {
                var storedInfo = await LoadHardwareInfoAsync();
                
                if (storedInfo == null)
                {
                    return new HardwareValidationResult
                    {
                        IsValid = false,
                        RequiresRegistration = true,
                        Message = "Hardware no registrado. Se requiere registro inicial."
                    };
                }

                var currentInfo = await _hardwareIdentifier.GetHardwareInfoAsync();
                
                if (currentInfo.HardwareHash == storedInfo.HardwareHash)
                {
                    storedInfo.LastValidatedAt = DateTime.UtcNow;
                    storedInfo.ValidationCount++;
                    storedInfo.IsValid = true;
                    await SaveHardwareInfoAsync(storedInfo);
                    
                    return new HardwareValidationResult
                    {
                        IsValid = true,
                        RequiresRegistration = false,
                        Message = "Hardware validado correctamente.",
                        HardwareInfo = storedInfo
                    };
                }
                else
                {
                    return new HardwareValidationResult
                    {
                        IsValid = false,
                        RequiresRegistration = true,
                        Message = "Hardware detectado ha cambiado. Se requiere actualizacion de hash.",
                        HardwareInfo = storedInfo,
                        CurrentHardwareInfo = currentInfo
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error validando hardware: {ex.Message}");
                return new HardwareValidationResult
                {
                    IsValid = false,
                    RequiresRegistration = false,
                    Message = $"Error validando hardware: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Obtiene el hash de hardware almacenado
        /// </summary>
        public async Task<string?> GetStoredHardwareHashAsync()
        {
            var hardwareInfo = await LoadHardwareInfoAsync();
            return hardwareInfo?.HardwareHash;
        }

        /// <summary>
        /// Guarda informacion de hardware en DTUSER
        /// </summary>
        private async Task SaveHardwareInfoAsync(HardwareInfo hardwareInfo)
        {
            try
            {
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                var json = JsonSerializer.Serialize(hardwareInfo, jsonOptions);
                await File.WriteAllTextAsync(_hardwareConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error guardando hardware info: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Carga informacion de hardware desde DTUSER
        /// </summary>
        private async Task<HardwareInfo?> LoadHardwareInfoAsync()
        {
            try
            {
                if (!File.Exists(_hardwareConfigPath))
                {
                    return null;
                }

                var json = await File.ReadAllTextAsync(_hardwareConfigPath);
                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }

                var hardwareInfo = JsonSerializer.Deserialize<HardwareInfo>(json);
                return hardwareInfo;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando hardware info: {ex.Message}");
                return null;
            }
        }
    }

    /// <summary>
    /// Resultado de validacion de hardware
    /// </summary>
    public class HardwareValidationResult
    {
        public bool IsValid { get; set; }
        public bool RequiresRegistration { get; set; }
        public string Message { get; set; } = string.Empty;
        public HardwareInfo? HardwareInfo { get; set; }
        public HardwareInfo? CurrentHardwareInfo { get; set; }
    }
}

