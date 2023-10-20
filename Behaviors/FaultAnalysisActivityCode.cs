using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Behaviors
{
    public enum FaultAnalysisActivityCode
    {
        Pass, // Pass is no fault
        Fail, // Fail is fault exists 
        FailHumanInterventionRequired,
        UnableToProcess,
        Unknown = 10000
    }
}
