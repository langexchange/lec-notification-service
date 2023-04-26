using System;
using System.Collections.Generic;

namespace LE.NotificationService.Dtos
{
    public class NotificationDto
    {
        public NotificationDto()
        {
            SubNotification = new List<string>();
        }

        public Guid Notiid { get; set; }
        public string NotifyMessage { get; set; }
        public string NotifiKey { get; set; }
        public string NotifyData { get; set; }
        public List<string> SubNotification { get; set; }
        public string Type { get; set; }
        public Guid? Postid { get; set; }
        public Guid? Commentid { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
