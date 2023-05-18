﻿using System;
using System.Collections.Generic;

#nullable disable

namespace LE.NotificationService.Infrastructure.Infrastructure.Entities
{
    public partial class Usrreportcmt
    {
        public Guid Cmtreportid { get; set; }
        public Guid Commentid { get; set; }
        public Guid Userid { get; set; }
        public string Statement { get; set; }

        public virtual Cmtreport Cmtreport { get; set; }
        public virtual Comment Comment { get; set; }
        public virtual User User { get; set; }
    }
}
