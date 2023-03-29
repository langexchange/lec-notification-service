using System;

namespace LE.NotificationService.Models
{
    public class NotificationResponse
    {
        public Guid Notiid { get; set; }
        public Guid Boxid { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
    }
}
