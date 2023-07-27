using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BrickSchema.Net.Shapes
{
    public class Classification : BrickShape
    {
        public enum Types
        {
            Critial,
            HVAC,
            Process,
            OperationalTechnolgoy,
            Utilities,
            Alarms
        }

        public Classification() { }
        public Classification(Types type) {
            Value = type.ToString();
        }
        public Classification(string name) { 
            Value = name;
        }
    }
}
