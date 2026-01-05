using System;
using System.Management;
using WoldVirtual3DViewer.Models;

namespace WoldVirtual3DViewer.Services
{
    public class HardwareService
    {
        public PCInfo GetPCInfo()
        {
            var pcInfo = new PCInfo();

            try
            {
                // Obtener información de la placa base
                pcInfo.Motherboard = GetMotherboardInfo();

                // Obtener información del procesador
                pcInfo.Processor = GetProcessorInfo();

                // Obtener información del sistema
                pcInfo.MachineName = Environment.MachineName;
                pcInfo.OSVersion = Environment.OSVersion.ToString();
                pcInfo.SystemType = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";

                // Generar hash único
                pcInfo.GenerateUniqueHash();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener información del hardware: {ex.Message}", ex);
            }

            return pcInfo;
        }

        private string GetMotherboardInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string manufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown";
                        string product = obj["Product"]?.ToString() ?? "Unknown";
                        string serialNumber = obj["SerialNumber"]?.ToString() ?? "Unknown";

                        return $"{manufacturer} {product} (SN: {serialNumber})";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo información de motherboard: {ex.Message}");
            }

            return "Unknown Motherboard";
        }

        private string GetProcessorInfo()
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"]?.ToString()?.Trim() ?? "Unknown";
                        string manufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown";
                        string maxClockSpeed = obj["MaxClockSpeed"]?.ToString() ?? "Unknown";

                        return $"{manufacturer} {name} ({maxClockSpeed} MHz)";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo información del procesador: {ex.Message}");
            }

            return "Unknown Processor";
        }

        public bool ValidatePCHash(string storedHash, PCInfo currentPC)
        {
            return string.Equals(storedHash, currentPC.UniqueHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
