using BrickSchema.Net.Relationships;
using BrickSchema.Net.ThreadSafeObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Classes
{
    //location class

    public class Location : BrickClass
    {
        public Location() 
        {
            SetProperty(EntityProperties.PropertiesEnum.BrickClass, typeof(Location).Name);
        }
        
        
        public Location(BrickEntity entity) : base(entity) { }

        public override Location Clone()
        {
            Location clone = new(base.Clone());
            return clone;
        }
    }

    
}
