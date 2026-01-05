using WoldVirtual3DViewer.Models;

namespace WoldVirtual3DViewer.Services
{
    public class RegistrationContext
    {
        public PCInfo? PCInfo { get; set; }
        public AvatarInfo? SelectedAvatar { get; set; }
        public string Username { get; set; } = string.Empty;
        
        public void Clear()
        {
            PCInfo = null;
            SelectedAvatar = null;
            Username = string.Empty;
        }
    }
}
