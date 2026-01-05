using System;

namespace WoldVirtual3D.Viewer.Models
{
    /// <summary>
    /// Modelo de datos de usuario
    /// </summary>
    public class UserData
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastLoginAt { get; set; }
        public bool RememberUsername { get; set; }
        public bool RememberPassword { get; set; }
    }
}

