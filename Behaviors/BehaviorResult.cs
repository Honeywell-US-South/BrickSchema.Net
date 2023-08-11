using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BrickSchema.Net.Behaviors
{
    public class BehaviorResult
    {
        public string EntityId { get; set; } = string.Empty;
        public string BehaviorType { get; set; } = string.Empty;
        public string BehaviorId { get; set; } = string.Empty;
        public string BehaviorName { get; set; } = string.Empty;
        public double? Value { get; set; } = null;
        public string Text { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public BehaviorResultStatusTypes Status { get; set; } = BehaviorResultStatusTypes.Skipped;
        public Dictionary<string, List<BehaviorResultDataItem>> AnalyticsData { get; set; } = new Dictionary<string, List<BehaviorResultDataItem>>();
    }
}
