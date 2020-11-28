using System;

namespace ChatApp.Models
{
    public class Message
    {
        public int Id { get; set; }
        
        public string Text { get; set; }
        
        public string AuthorName { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        public int ChatId { get; set; }
        
        public Chat Chat { get; set; }

    }
}