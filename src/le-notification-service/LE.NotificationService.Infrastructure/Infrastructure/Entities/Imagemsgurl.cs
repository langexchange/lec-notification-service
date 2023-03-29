using System;
using System.Collections.Generic;

#nullable disable

namespace LE.NotificationService.Infrastructure.Infrastructure.Entities
{
    public partial class Imagemsgurl
    {
        public Guid Urlid { get; set; }
        public Guid? Messid { get; set; }
        public string Url { get; set; }

        public virtual Imagemsg Mess { get; set; }
    }
}
