using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Classes
{
    public class Tenant : BrickClass
    {
        public Tenant() 
        {
            SetProperty(EntityProperties.PropertiesEnum.BrickClass, typeof(Tenant).Name);
        }

        public Tenant(BrickEntity entity) : base(entity) { }

        public override Tenant Clone()
        {
            Tenant clone = new Tenant(base.Clone());
            return clone;
        }
    }
}
