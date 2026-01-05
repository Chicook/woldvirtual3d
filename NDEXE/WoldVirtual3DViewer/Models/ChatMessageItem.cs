namespace WoldVirtual3DViewer.Models
{
    public class ChatMessageItem
    {
        public string Sender { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string FullText => $"{Sender}: {Content}";
        public string Color { get; set; } = "White"; // Para binding
    }
}
