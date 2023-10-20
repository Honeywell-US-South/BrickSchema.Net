using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Behaviors
{
    public enum FaultAnalysisActivityTypes
    {
        SensorCheck,
        BehaviorCheck,
        ActivityCheck,
        RelationshipCheck, // => Check Boilers or Chillers for Supply Water Availability
        HumanCheck,
        Unknown
    }
}
