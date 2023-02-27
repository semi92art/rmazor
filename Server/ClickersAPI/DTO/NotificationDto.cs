using System;

namespace ClickersAPI.DTO
{
    public class NotificationDto
    {
        public string    Title         { get; set; }
        public string    Body          { get; set; }
        public string    SmallIcon     { get; set; }
        public string    LargeIcon     { get; set; }
        public string    Token         { get; set; }
        public string    Topic         { get; set; }
        public string    Condition     { get; set; }
        public TimeSpan? TimeToLive    { get; set; }
        public TimeSpan? TimeStampSpan { get; set; }
    }
}