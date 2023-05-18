using System;
using System.Collections.Generic;

#nullable disable

namespace LE.NotificationService.Infrastructure.Infrastructure.Entities
{
    public partial class Correctcmt
    {
        public Guid Correctcmtid { get; set; }
        public Guid Commentid { get; set; }
        public string CorrectText { get; set; }

        public virtual Comment Comment { get; set; }
    }
}
