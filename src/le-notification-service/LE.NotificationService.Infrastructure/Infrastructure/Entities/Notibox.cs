﻿using System;
using System.Collections.Generic;

#nullable disable

namespace LE.NotificationService.Infrastructure.Infrastructure.Entities
{
    public partial class Notibox
    {
        public Notibox()
        {
            Notifications = new HashSet<Notification>();
            Users = new HashSet<User>();
        }

        public Guid Boxid { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<User> Users { get; set; }
    }
}
