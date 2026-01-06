using System;

namespace WoldVirtual3DViewer.Models
{
    public class ChatMessageItem
    {
        public string Content { get; set; } = string.Empty;
        public string Sender { get; set; } = "System";
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}