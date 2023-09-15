using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Behaviors
{
    public class BehaviorExecutedEventArgs : EventArgs
    {
        public string ParentId { get; set; }
        public List<BehaviorValue> Values { get; set; } = new();
        public BehaviorTaskReturnCodes TaskReturnCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

    }
}
