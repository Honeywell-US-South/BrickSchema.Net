using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BrickSchema.Net.Classes.Locations.Floors
{
    public class Basement : Floor {

        public override Dictionary<Type, string> DefaultShapes()
        {

            return new()
            {
                {typeof(BrickSchema.Net.Shapes.Positions.Order), "0" },
                {typeof(BrickSchema.Net.Shapes.Area), "0" },
                {typeof(BrickSchema.Net.Shapes.Positions.Elevation), "0" }
            };
        }
    }


}
