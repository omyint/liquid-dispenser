using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LiquidDispenser.Core.Data
{
    public record TelemetryPayload
    {
        [Key]
        public int Id { get; set; }

        public string? Status { get; set; }

        public DateTime Timestamp { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }
    }
}
