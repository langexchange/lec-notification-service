using System;

namespace LE.NotificationService.Dtos
{
    public class NotificationDto
    {
        public Guid Notiid { get; set; }
        public Guid Boxid { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
        public Guid Type { get; set; }

    }
}
