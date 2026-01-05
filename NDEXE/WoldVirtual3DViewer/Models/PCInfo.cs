using System;

namespace WoldVirtual3DViewer.Models
{
    public class PCInfo
    {
        public string? Motherboard { get; set; }
        public string? Processor { get; set; }
        public string? UniqueHash { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string? MachineName { get; set; }
        public string? OSVersion { get; set; }
        public string? SystemType { get; set; }

        public PCInfo()
        {
            RegistrationDate = DateTime.Now;
        }

        public void GenerateUniqueHash()
        {
            if (string.IsNullOrEmpty(Motherboard) || string.IsNullOrEmpty(Processor))
            {
                throw new InvalidOperationException("Motherboard and Processor information must be set before generating hash.");
            }

            string combinedInfo = $"{Motherboard}|{Processor}|{MachineName}|{RegistrationDate:yyyyMMddHHmmss}";
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combinedInfo));
                UniqueHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
