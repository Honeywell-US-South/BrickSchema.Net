using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Classes.Collections
{
    //Collection Class
    public class Collection : BrickClass
    {
        public Collection()
        {
            SetProperty(EntityProperties.PropertiesEnum.BrickClass, typeof(Collection).Name);
        }

    }

    public class PVArray : Collection { }
    public class PhotovoltaicArray : Collection { }
    public class Portfolio : Collection { }
    public class System : Collection { }
    public class Group : Collection { }
}
