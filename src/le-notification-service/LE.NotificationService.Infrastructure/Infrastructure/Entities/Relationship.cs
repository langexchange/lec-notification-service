﻿using System;
using System.Collections.Generic;

#nullable disable

namespace LE.NotificationService.Infrastructure.Infrastructure.Entities
{
    public partial class Relationship
    {
        public Guid User1 { get; set; }
        public Guid User2 { get; set; }
        public bool? Type { get; set; }
        public string Action { get; set; }

        public virtual User User1Navigation { get; set; }
        public virtual User User2Navigation { get; set; }
    }
}
