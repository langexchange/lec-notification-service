﻿using System;
using System.Collections.Generic;

#nullable disable

namespace LE.NotificationService.Infrastructure.Infrastructure.Entities
{
    public partial class Vocabulary
    {
        public Guid Packageid { get; set; }
        public string Imageurl { get; set; }
        public string Front { get; set; }
        public string Back { get; set; }
        public string Type { get; set; }
        public bool? IsRemoved { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLearned { get; set; }
        public DateTime? NextLearned { get; set; }
        public Guid Vocabid { get; set; }
        public int? Repetitions { get; set; }
        public decimal? Easiness { get; set; }
        public int? Interval { get; set; }

        public virtual Vocabpackage Package { get; set; }
    }
}
