﻿using System;
using System.Collections.Generic;

#nullable disable

namespace LE.NotificationService.Infrastructure.Infrastructure.Entities
{
    public partial class Setting
    {
        public Guid Id { get; set; }
        public string ServiceName { get; set; }
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Locale { get; set; }
    }
}
