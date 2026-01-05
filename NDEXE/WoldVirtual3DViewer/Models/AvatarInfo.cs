using System.Collections.Generic;

namespace WoldVirtual3DViewer.Models
{
    public class AvatarInfo
    {
        public string? Type { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? PreviewImage { get; set; }
        public Dictionary<string, string>? Properties { get; set; }

        public AvatarInfo()
        {
            Properties = new Dictionary<string, string>();
        }

        public static List<AvatarInfo> GetAvailableAvatars()
        {
            return new List<AvatarInfo>
            {
                new AvatarInfo
                {
                    Type = "chica",
                    Name = "Avatar Chica",
                    Description = "Avatar femenino por defecto para desarrollo",
                    PreviewImage = "Resources/Avatars/chica_preview.png",
                    Properties = new Dictionary<string, string>
                    {
                        { "gender", "female" },
                        { "style", "default" },
                        { "status", "development" }
                    }
                },
                new AvatarInfo
                {
                    Type = "chico",
                    Name = "Avatar Chico",
                    Description = "Avatar masculino",
                    PreviewImage = "Resources/Avatars/chico_preview.png",
                    Properties = new Dictionary<string, string>
                    {
                        { "gender", "male" },
                        { "style", "default" },
                        { "status", "available" }
                    }
                },
                new AvatarInfo
                {
                    Type = "custom",
                    Name = "Avatar Personalizado",
                    Description = "Avatar personalizado del usuario",
                    PreviewImage = "Resources/Avatars/custom_preview.png",
                    Properties = new Dictionary<string, string>
                    {
                        { "gender", "custom" },
                        { "style", "custom" },
                        { "status", "coming_soon" }
                    }
                }
            };
        }
    }
}
