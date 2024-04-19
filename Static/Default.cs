using BrickSchema.Net.Classes.Devices.Vendor.Broadsens.Devices;
using BrickSchema.Net.Classes.Devices.Vendor.Broadsens.MqttGateway;
using BrickSchema.Net.Classes.Locations;
using BrickSchema.Net.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Static
{
    public static class Default
    {

        public static List<DefaultAttributeDescription> GetDefaultAttributes(string typeName)
        {
            if (typeName.Equals(typeof(Broadsens_GU200S).Name, StringComparison.OrdinalIgnoreCase))
            {
                return BroadsensDefault.Broadsens_GU200S_DefaultAttributes();
            }
            else if (typeName.Equals(typeof(Broadsens_GU300).Name, StringComparison.OrdinalIgnoreCase))
            {
                return BroadsensDefault.Broadsens_GU300_DefaultAttributes();
            }
            else if (typeName.Equals(typeof(Broadsens_GU300S).Name, StringComparison.OrdinalIgnoreCase))
            {
                return BroadsensDefault.Broadsens_GU300S_DefaultAttributes();
            }
            else if (typeName.Equals(typeof(Broadsens_SVT200_V).Name, StringComparison.OrdinalIgnoreCase))
            {
                return BroadsensDefault.Broadsens_SVT200_V_DefaultAttributes();
            }
            return new();
        }
        
        public static string GetSvgIcon(string typeName)
        {
            if (typeof(Building).Name.Equals(typeName, StringComparison.OrdinalIgnoreCase))
            {

            }

            return string.Empty;
        }
    }
}
