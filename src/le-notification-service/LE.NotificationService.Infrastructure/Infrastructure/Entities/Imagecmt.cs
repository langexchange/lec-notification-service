﻿using System;
using System.Collections.Generic;

#nullable disable

namespace LE.NotificationService.Infrastructure.Infrastructure.Entities
{
    public partial class Imagecmt
    {
        public Guid Imagecmtid { get; set; }
        public Guid Commentid { get; set; }
        public string Url { get; set; }

        public virtual Comment Comment { get; set; }
    }
}
