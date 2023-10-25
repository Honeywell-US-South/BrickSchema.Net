using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Alerts
{
    public class AlertActivity
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Activity { get; set; }
        public string Description { get; set; }
    }
}
