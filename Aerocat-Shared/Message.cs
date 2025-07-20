// In project: Aerocat.Shared
// File: Message.cs
using System;

namespace Aerocat.Shared
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderUsername { get; set; }
        public string ReceiverUsername { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
}