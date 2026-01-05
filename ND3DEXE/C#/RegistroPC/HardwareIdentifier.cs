using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WoldVirtual3D.Viewer.RegistroPC.Models;

namespace WoldVirtual3D.Viewer.RegistroPC
{
    /// <summary>
    /// Identificador de hardware del PC
    /// Responsabilidad: Obtener informacion de componentes vitales (placa base y procesador)
    /// y generar un hash unico SHA256 para identificacion del PC
    /// </summary>
    public class HardwareIdentifier
    {
        private const string WMI_NAMESPACE = "root\\CIMV2";
        private const string MOTHERBOARD_QUERY = "SELECT SerialNumber FROM Win32_BaseBoard";
        private const string PROCESSOR_QUERY = "SELECT ProcessorId FROM Win32_Processor";

        /// <summary>
        /// Obtiene el numero de serie de la placa base usando WMI
        /// </summary>
        public async Task<string> GetMotherboardSerialAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher(WMI_NAMESPACE, MOTHERBOARD_QUERY))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var serial = obj["SerialNumber"]?.ToString();
                            if (!string.IsNullOrEmpty(serial) && serial != "To be filled by O.E.M.")
                            {
                                return serial.Trim();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error obteniendo serial de placa base: {ex.Message}");
                }

                return GetFallbackMotherboardId();
            });
        }

        /// <summary>
        /// Obtiene el ID del procesador usando WMI
        /// </summary>
        public async Task<string> GetProcessorIdAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher(WMI_NAMESPACE, PROCESSOR_QUERY))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var processorId = obj["ProcessorId"]?.ToString();
                            if (!string.IsNullOrEmpty(processorId))
                            {
                                return processorId.Trim();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error obteniendo ID de procesador: {ex.Message}");
                }

                return GetFallbackProcessorId();
            });
        }

        /// <summary>
        /// Genera un hash SHA256 unico basado en placa base + procesador
        /// </summary>
        public async Task<string> GenerateHardwareHashAsync()
        {
            var motherboardSerial = await GetMotherboardSerialAsync();
            var processorId = await GetProcessorIdAsync();

            var combinedInfo = $"{motherboardSerial}|{processorId}";

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(combinedInfo);
                var hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }

        /// <summary>
        /// Obtiene informacion completa del hardware
        /// </summary>
        public async Task<HardwareInfo> GetHardwareInfoAsync()
        {
            var motherboardSerial = await GetMotherboardSerialAsync();
            var processorId = await GetProcessorIdAsync();
            var hardwareHash = await GenerateHardwareHashAsync();

            var hardwareInfo = new HardwareInfo
            {
                MotherboardSerial = motherboardSerial,
                ProcessorId = processorId,
                HardwareHash = hardwareHash,
                RegisteredAt = DateTime.UtcNow,
                LastValidatedAt = DateTime.UtcNow,
                Version = 1
            };

            await AddAdditionalInfoAsync(hardwareInfo);

            return hardwareInfo;
        }

        /// <summary>
        /// Valida que el hardware actual coincida con el hash almacenado
        /// </summary>
        public async Task<bool> ValidateHardwareHashAsync(string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            var currentHash = await GenerateHardwareHashAsync();
            return currentHash == storedHash;
        }

        /// <summary>
        /// Obtiene informacion adicional del hardware (opcional)
        /// </summary>
        private async Task AddAdditionalInfoAsync(HardwareInfo hardwareInfo)
        {
            await Task.Run(() =>
            {
                try
                {
                    using (var searcher = new ManagementObjectSearcher(WMI_NAMESPACE, "SELECT Manufacturer, Product FROM Win32_BaseBoard"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var manufacturer = obj["Manufacturer"]?.ToString();
                            var product = obj["Product"]?.ToString();
                            if (!string.IsNullOrEmpty(manufacturer))
                            {
                                hardwareInfo.AdditionalInfo["MotherboardManufacturer"] = manufacturer;
                            }
                            if (!string.IsNullOrEmpty(product))
                            {
                                hardwareInfo.AdditionalInfo["MotherboardProduct"] = product;
                            }
                        }
                    }

                    using (var searcher = new ManagementObjectSearcher(WMI_NAMESPACE, "SELECT Name, Manufacturer FROM Win32_Processor"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            var name = obj["Name"]?.ToString();
                            var manufacturer = obj["Manufacturer"]?.ToString();
                            if (!string.IsNullOrEmpty(name))
                            {
                                hardwareInfo.AdditionalInfo["ProcessorName"] = name;
                            }
                            if (!string.IsNullOrEmpty(manufacturer))
                            {
                                hardwareInfo.AdditionalInfo["ProcessorManufacturer"] = manufacturer;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error obteniendo informacion adicional: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// Obtiene un identificador alternativo para placa base si WMI falla
        /// </summary>
        private string GetFallbackMotherboardId()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(WMI_NAMESPACE, "SELECT SerialNumber FROM Win32_BIOS"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var serial = obj["SerialNumber"]?.ToString();
                        if (!string.IsNullOrEmpty(serial))
                        {
                            return $"BIOS_{serial}";
                        }
                    }
                }
            }
            catch
            {
                // Ignorar errores en fallback
            }

            return $"FALLBACK_MB_{Environment.MachineName}_{Environment.UserName}";
        }

        /// <summary>
        /// Obtiene un identificador alternativo para procesador si WMI falla
        /// </summary>
        private string GetFallbackProcessorId()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(WMI_NAMESPACE, "SELECT Name FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var name = obj["Name"]?.ToString();
                        if (!string.IsNullOrEmpty(name))
                        {
                            return $"CPU_{name.GetHashCode()}";
                        }
                    }
                }
            }
            catch
            {
                // Ignorar errores en fallback
            }

            return $"FALLBACK_CPU_{Environment.ProcessorCount}_{Environment.OSVersion}";
        }
    }
}

