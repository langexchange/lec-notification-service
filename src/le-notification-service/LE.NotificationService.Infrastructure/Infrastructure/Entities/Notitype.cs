using System;
using System.Collections.Generic;

#nullable disable

namespace LE.NotificationService.Infrastructure.Infrastructure.Entities
{
    public partial class Notitype
    {
        public Notitype()
        {
            Notifications = new HashSet<Notification>();
        }

        public Guid Typeid { get; set; }
        public string Typestring { get; set; }
        public string Sample { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
