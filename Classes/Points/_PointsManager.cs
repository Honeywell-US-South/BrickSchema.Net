using BrickSchema.Net.Classes.Points;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net
{
    public partial class BrickSchemaManager
    {
        //point

        public AlarmPoint AddPointAlarm(string? id = null) => AddEntity<AlarmPoint>(id);
        public CommandPoint AddPointCommand(string? id = null) => AddEntity<CommandPoint>(id);
        public ParameterPoint AddPointParameter(string? id = null) => AddEntity<ParameterPoint>(id);
        public SensorPoint AddPointSensor(string? id = null) => AddEntity<SensorPoint>(id);
        public SetpointPoint AddPointSetpoint(string? id = null) => AddEntity<SetpointPoint>(id);
        public StatusPoint AddPointStatus(string? id = null) => AddEntity<StatusPoint>(id);
    }
}
