using System;

namespace WoldVirtual3D.Viewer
{
    /// <summary>
    /// Modelo de datos de usuario
    /// </summary>
    public class UserData
    {
        public string Id { get; set; } = "";
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string CreatedAt { get; set; } = "";
        public string LastLogin { get; set; } = "";
    }

    /// <summary>
    /// Modelo de datos de sesión
    /// </summary>
    public class SessionData
    {
        public string UserId { get; set; } = "";
        public string Username { get; set; } = "";
        public DateTime LastLogin { get; set; }
    }

    /// <summary>
    /// Modelo de datos de configuración de usuario
    /// </summary>
    public class UserSettings
    {
        public string UserId { get; set; } = "";
        public string AvatarData { get; set; } = "";
        public string Preferences { get; set; } = "{}";
        public string WorldId { get; set; } = "";
    }

    /// <summary>
    /// Modelo de datos de nodo P2P
    /// </summary>
    public class P2PNode
    {
        public string NodeId { get; set; } = "";
        public string UserId { get; set; } = "";
        public string Username { get; set; } = "";
        public string IpAddress { get; set; } = "";
        public int Port { get; set; } = 0;
        public DateTime LastSeen { get; set; }
        public bool IsConnected { get; set; } = false;
    }

    /// <summary>
    /// Modelo de mensaje P2P
    /// </summary>
    public class P2PMessage
    {
        public string MessageId { get; set; } = "";
        public string FromNodeId { get; set; } = "";
        public string ToNodeId { get; set; } = "";
        public string Type { get; set; } = "";
        public string Data { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}

