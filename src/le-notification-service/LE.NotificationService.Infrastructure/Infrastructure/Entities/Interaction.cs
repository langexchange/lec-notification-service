﻿using System;
using System.Collections.Generic;

#nullable disable

namespace LE.NotificationService.Infrastructure.Infrastructure.Entities
{
    public partial class Interaction
    {
        public Interaction()
        {
            Cmtinteracts = new HashSet<Cmtinteract>();
            Userintposts = new HashSet<Userintpost>();
        }

        public Guid Interactid { get; set; }
        public string Stringcode { get; set; }

        public virtual ICollection<Cmtinteract> Cmtinteracts { get; set; }
        public virtual ICollection<Userintpost> Userintposts { get; set; }
    }
}
