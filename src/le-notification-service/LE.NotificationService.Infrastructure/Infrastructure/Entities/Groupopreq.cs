﻿using System;
using System.Collections.Generic;

#nullable disable

namespace LE.NotificationService.Infrastructure.Infrastructure.Entities
{
    public partial class Groupopreq
    {
        public Guid Requestid { get; set; }
        public Guid Ownerid { get; set; }
        public string Text { get; set; }
        public bool? IsQualified { get; set; }

        public virtual User Owner { get; set; }
    }
}
