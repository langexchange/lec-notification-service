﻿using System;
using System.Collections.Generic;

#nullable disable

namespace LE.NotificationService.Infrastructure.Infrastructure.Entities
{
    public partial class Punishment
    {
        public Punishment()
        {
            Cmtpunishes = new HashSet<Cmtpunish>();
            Grouppunishes = new HashSet<Grouppunish>();
            Postpunishes = new HashSet<Postpunish>();
            Restrictpunishes = new HashSet<Restrictpunish>();
            Userpunishes = new HashSet<Userpunish>();
        }

        public Guid Punishid { get; set; }
        public int? Relapse { get; set; }
        public string Type { get; set; }
        public bool? IsRestrict { get; set; }

        public virtual ICollection<Cmtpunish> Cmtpunishes { get; set; }
        public virtual ICollection<Grouppunish> Grouppunishes { get; set; }
        public virtual ICollection<Postpunish> Postpunishes { get; set; }
        public virtual ICollection<Restrictpunish> Restrictpunishes { get; set; }
        public virtual ICollection<Userpunish> Userpunishes { get; set; }
    }
}
