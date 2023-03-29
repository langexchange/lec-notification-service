﻿using System;
using System.Collections.Generic;
using System.Collections;

#nullable disable

namespace LE.NotificationService.Infrastructure.Infrastructure.Entities
{
    public partial class Groupmember
    {
        public Guid Userid { get; set; }
        public Guid Groupid { get; set; }
        public Guid Roleid { get; set; }
        public BitArray RestrictLevel { get; set; }
        public bool? IsRemoved { get; set; }

        public virtual Group Group { get; set; }
        public virtual Role Role { get; set; }
        public virtual User User { get; set; }
    }
}
