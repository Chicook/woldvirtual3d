using System;
using System.Security.Cryptography;
using System.Text;

namespace WoldVirtual3DViewer.Models
{
    public class UserAccount
    {
        public string? Username { get; set; }
        public string? PasswordHash { get; set; }
        public string? AccountHash { get; set; }
        public DateTime CreationDate { get; set; }
        public string? AvatarType { get; set; }
        public bool IsValidated { get; set; }
        public string? PCUniqueHash { get; set; }

        public UserAccount()
        {
            CreationDate = DateTime.Now;
            IsValidated = false;
            AvatarType = "chica"; // Default avatar for development
        }

        public void SetPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                PasswordHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrEmpty(PasswordHash))
                return false;

            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                string inputHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return string.Equals(PasswordHash, inputHash, StringComparison.OrdinalIgnoreCase);
            }
        }

        public void GenerateAccountHash()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(PasswordHash) || string.IsNullOrEmpty(PCUniqueHash))
            {
                throw new InvalidOperationException("Username, PasswordHash, and PCUniqueHash must be set before generating account hash.");
            }

            string combinedInfo = $"{Username}|{PasswordHash}|{PCUniqueHash}|{CreationDate:yyyyMMddHHmmss}";
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combinedInfo));
                AccountHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}
