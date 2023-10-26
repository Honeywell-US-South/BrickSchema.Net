﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Alerts
{
    public enum AlertStatuses
    {
        None,
        Active,
        AckActive,
        WorkAssigned,
        ReturnToNormal,
        RtnWorkAssigned,
        Resolved,
        Cleared
    }
}
