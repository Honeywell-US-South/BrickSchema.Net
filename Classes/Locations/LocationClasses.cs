using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Classes.Locations
{
    public class Building : Location {
    
        public Building() { }
        public Building(BrickEntity entity) : base(entity) { }

        public override Building Clone()
        {
            Building clone = new(base.Clone());
            return clone;
        }

    }

    public class Floor : Location
    {

        public Floor() { }
        public Floor(BrickEntity entity) : base(entity) { }

        public override Floor Clone()
        {
            Floor clone = new(base.Clone());
            return clone;
        }
    }


    public class OutdoorArea : Location
    {
        public OutdoorArea() { }
        public OutdoorArea(BrickEntity entity) : base(entity) { }

        public override OutdoorArea Clone()
        {
            OutdoorArea clone = new(base.Clone());
            return clone;
        }
    }

    public class Outside : Location { 
    
    public Outside() { }
        public Outside(BrickEntity entity) : base(entity) { }

        public override Outside Clone()
        {
            Outside clone = new(base.Clone());
            return clone;
        }
    }
    public class Region : Location { }
    public class Site : Location { }
    public class Space : Location { }
    
    public class Wing : Location { }
    public class Zone : Location { }
    public class ChilledWaterPlant : Location { }
    public class HotWaterPlant : Location { }
}
