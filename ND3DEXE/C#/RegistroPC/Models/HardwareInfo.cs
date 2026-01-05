using System;
using System.Collections.Generic;

namespace WoldVirtual3D.Viewer.RegistroPC.Models
{
    /// <summary>
    /// Modelo de datos para informacion de hardware del PC
    /// Responsabilidad: Almacenar informacion de componentes vitales (placa base y procesador)
    /// </summary>
    public class HardwareInfo
    {
        /// <summary>
        /// Numero de serie de la placa base (Motherboard)
        /// </summary>
        public string MotherboardSerial { get; set; } = string.Empty;

        /// <summary>
        /// ID unico del procesador
        /// </summary>
        public string ProcessorId { get; set; } = string.Empty;

        /// <summary>
        /// Hash SHA256 unico generado a partir de placa base + procesador
        /// Este hash se usa para identificar de forma unica el PC del usuario
        /// </summary>
        public string HardwareHash { get; set; } = string.Empty;

        /// <summary>
        /// Fecha y hora en que se registro el hardware por primera vez
        /// </summary>
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha y hora de la ultima validacion del hardware
        /// </summary>
        public DateTime LastValidatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Numero de veces que se ha validado el hardware
        /// </summary>
        public int ValidationCount { get; set; } = 0;

        /// <summary>
        /// Indica si el hardware ha sido validado correctamente
        /// </summary>
        public bool IsValid { get; set; } = false;

        /// <summary>
        /// Version del formato de hardware info
        /// Permite migracion futura si cambia la estructura
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Informacion adicional del hardware (opcional)
        /// Puede incluir modelo de placa base, modelo de procesador, etc.
        /// </summary>
        public Dictionary<string, string> AdditionalInfo { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Valida que el hardware info tenga todos los campos requeridos
        /// </summary>
        public bool IsValidData()
        {
            return !string.IsNullOrEmpty(MotherboardSerial) &&
                   !string.IsNullOrEmpty(ProcessorId) &&
                   !string.IsNullOrEmpty(HardwareHash) &&
                   RegisteredAt != default(DateTime);
        }

        /// <summary>
        /// Crea una copia del HardwareInfo
        /// </summary>
        public HardwareInfo Clone()
        {
            return new HardwareInfo
            {
                MotherboardSerial = this.MotherboardSerial,
                ProcessorId = this.ProcessorId,
                HardwareHash = this.HardwareHash,
                RegisteredAt = this.RegisteredAt,
                LastValidatedAt = this.LastValidatedAt,
                ValidationCount = this.ValidationCount,
                IsValid = this.IsValid,
                Version = this.Version,
                AdditionalInfo = new Dictionary<string, string>(this.AdditionalInfo)
            };
        }

        /// <summary>
        /// Compara dos HardwareInfo para verificar si son del mismo hardware
        /// </summary>
        public bool IsSameHardware(HardwareInfo? other)
        {
            if (other == null) return false;
            return this.MotherboardSerial == other.MotherboardSerial &&
                   this.ProcessorId == other.ProcessorId;
        }

        /// <summary>
        /// Obtiene una representacion en string del hardware info
        /// </summary>
        public override string ToString()
        {
            var hashPreview = HardwareHash.Length > 16 ? HardwareHash.Substring(0, 16) + "..." : HardwareHash;
            var mbPreview = MotherboardSerial.Length > 8 ? MotherboardSerial.Substring(0, 8) + "..." : MotherboardSerial;
            var cpuPreview = ProcessorId.Length > 8 ? ProcessorId.Substring(0, 8) + "..." : ProcessorId;
            
            return $"HardwareHash: {hashPreview}, " +
                   $"MB: {mbPreview}, " +
                   $"CPU: {cpuPreview}, " +
                   $"Valid: {IsValid}, Validations: {ValidationCount}";
        }
    }
}

