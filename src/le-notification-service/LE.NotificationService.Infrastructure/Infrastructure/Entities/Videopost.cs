﻿using System;
using System.Collections.Generic;

#nullable disable

namespace LE.NotificationService.Infrastructure.Infrastructure.Entities
{
    public partial class Videopost
    {
        public Guid Videopostid { get; set; }
        public Guid Postid { get; set; }
        public string Url { get; set; }

        public virtual Post Post { get; set; }
    }
}
