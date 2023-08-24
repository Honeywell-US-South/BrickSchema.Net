using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Shapes
{
    public class BehaviorFunction : BrickShape
    {
        public enum Types
        {
            Core,
            Background,
            DataAccess,
            Analytics,
            Fault

        }
    }
}
