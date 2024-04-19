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

    public class Basement : Location { }
    public class Floor : Location { 
    
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
    public class CommonSpace : Space { }
    public class Entrance : Space { }
    public class GateHouse : Space { }
    public class MediaHotDesk : Space { }
    public class Parking : Space { }
    public class Room : Space { }
    public class TicketingBooth : Space { }
    public class Tunnel : Space { }
    public class VerticalSpace : Space { }
    public class WaterTank : Space { }
    public class Wing : Location { }
    public class Zone : Location { }
    public class ChilledWaterPlant : Location { }
    public class HotWaterPlant : Location { }
}
