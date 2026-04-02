using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LiquidDispenser.Core.Data
{
    public record JobRequestDto
    {
        [Key]
        public int JobId { get; set; } // Primary Key for EF Core
        public int SourceRowStart { get; set; }
        public int SourceColumnIndex { get; set; }
        public int DestRowStart { get; set; }
        public int DestColumn { get; set; }
        public double Volume { get; set; }
    }
}
