using BrickSchema.Net.Classes.Devices.Vendor.Broadsens.MqttGateway;
using BrickSchema.Net.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Static
{
    public static partial class Default
    {

        public static List<DefaultAttributeDescription> GetDefaultAttributes(string typeName)
        {
            if (typeName.Equals(typeof(Broadsens_GU200S).Name, StringComparison.OrdinalIgnoreCase))
            {
                return Broadsens_GU200S_DefaultAttributes();
            }
            else if (typeName.Equals(typeof(Broadsens_GU300).Name, StringComparison.OrdinalIgnoreCase))
            {
                return Broadsens_GU300_DefaultAttributes();
            }
            else if (typeName.Equals(typeof(Broadsens_GU300S).Name, StringComparison.OrdinalIgnoreCase))
            {
                return Broadsens_GU300S_DefaultAttributes();
            }
            return new();
        }
        
    }
}
