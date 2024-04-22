using BrickSchema.Net.Classes.Locations;
using BrickSchema.Net.Classes.Locations.Floors;
using BrickSchema.Net.Classes.Locations.Spaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net
{
    public partial class BrickSchemaManager
    {
        //location
        public Building AddLocationBuilding(string? id = null) => AddEntity<Building>(id);
        public Floor AddLocationFloor(string? id = null) => AddEntity<Floor>(id);
        public OutdoorArea AddLocationOutdoorArea(string? id = null) => AddEntity<OutdoorArea>(id);
        public Outside AddLocationOutside(string? id = null) => AddEntity<Outside>(id);
        public Region AddLocationRegion(string? id = null) => AddEntity<Region>(id);
        public Site AddLocationSite(string? id = null) => AddEntity<Site>(id);
        public Space AddLocationSpace(string? id = null) => AddEntity<Space>(id);
        public CommonSpace AddLocationCommonSpace(string? id = null) => AddEntity<CommonSpace>(id);
        public Entrance AddLocationEntrance(string? id = null) => AddEntity<Entrance>(id);
        public GateHouse AddLocationGateHouse(string? id = null) => AddEntity<GateHouse>(id);
        public MediaHotDesk AddLocationMediaHotDesk(string? id = null) => AddEntity<MediaHotDesk>(id);
        public Parking AddLocationParking(string? id = null) => AddEntity<Parking>(id);
        public Room AddLocationRoom(string? id = null) => AddEntity<Room>(id);
        public TicketingBooth AddLocationTicketingBooth(string? id = null) => AddEntity<TicketingBooth>(id);
        public Tunnel AddLocationTunnel(string? id = null) => AddEntity<Tunnel>(id);
        public VerticalSpace AddLocationVerticalSpace(string? id = null) => AddEntity<VerticalSpace>(id);
        public WaterTank AddLocationWaterTank(string? id = null) => AddEntity<WaterTank>(id);
        public Basement AddLocationBasement(string? id = null) => AddEntity<Basement>(id);
        public Wing AddLocationWing(string? id = null) => AddEntity<Wing>(id);
        public Zone AddLocationZone(string? id = null) => AddEntity<Zone>(id);
        public ChilledWaterPlant AddLocationChilledWaterPlant(string? id = null) => AddEntity<ChilledWaterPlant>(id);
        public HotWaterPlant AddLocationHotWaterPlant(string? id = null) => AddEntity<HotWaterPlant>(id);
    }
}
