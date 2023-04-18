﻿using System;
using System.Collections.Generic;

#nullable disable

namespace LE.NotificationService.Infrastructure.Infrastructure.Entities
{
    public partial class Commentnotification
    {
        public Guid Notiid { get; set; }
        public Guid Boxid { get; set; }
        public string NotifiKey { get; set; }
        public string NotifyData { get; set; }
        public Guid? Commentid { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public virtual Notibox Box { get; set; }
    }
}
