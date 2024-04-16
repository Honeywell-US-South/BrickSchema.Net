using BrickSchema.Net.Classes.Equipments.HVACType;
using BrickSchema.Net.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Classes.Devices.Vendor.Broadsens.MqttGateway
{
    public class Broadsens_GU300 : MqttDevice
    {
        public static List<DefaultPropertyDescription> DefautPropertyNames()
        {
            List<DefaultPropertyDescription> defaultProperties = new();
            defaultProperties.Add(new DefaultPropertyDescription("overall", "Gateway CPU overall usage", "%"));
            defaultProperties.Add(new DefaultPropertyDescription("drv_tot", "Total drive space", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("drv-usd", "Drive space used", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("uptime", "Gateway running time", "Day, hour, minutes, seconds"));
            defaultProperties.Add(new DefaultPropertyDescription("free", "Gateway free memory", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("buffers", "Gateway memory buffered", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("cached", "Gateway memory cached", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("active", "Gateway memory active", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("temperature", "Gateway CPU temperature", "0C"));
            defaultProperties.Add(new DefaultPropertyDescription("name", "Gateway name", "N/A"));
            defaultProperties.Add(new DefaultPropertyDescription("latitude", "Gateway location latitude", "Degree"));
            defaultProperties.Add(new DefaultPropertyDescription("longitude", "Gateway location longitude", "Degree"));

            return defaultProperties;

        }
        
    }
    public class Broadsens_GU300S : MqttDevice
    {
        public static List<DefaultPropertyDescription> DefautPropertyNames()
        {
            List<DefaultPropertyDescription> defaultProperties = new();
            defaultProperties.Add(new DefaultPropertyDescription("overall", "Gateway CPU overall usage", "%"));
            defaultProperties.Add(new DefaultPropertyDescription("drv_tot", "Total drive space", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("drv-usd", "Drive space used", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("uptime", "Gateway running time", "Day, hour, minutes, seconds"));
            defaultProperties.Add(new DefaultPropertyDescription("free", "Gateway free memory", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("buffers", "Gateway memory buffered", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("cached", "Gateway memory cached", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("active", "Gateway memory active", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("temperature", "Gateway CPU temperature", "0C"));
            defaultProperties.Add(new DefaultPropertyDescription("name", "Gateway name", "N/A"));
            defaultProperties.Add(new DefaultPropertyDescription("latitude", "Gateway location latitude", "Degree"));
            defaultProperties.Add(new DefaultPropertyDescription("longitude", "Gateway location longitude", "Degree"));

            return defaultProperties;

        }

    }
    public class Broadsens_GU200S : MqttDevice
    {
        public static List<DefaultPropertyDescription> DefautPropertyNames()
        {
            List<DefaultPropertyDescription> defaultProperties = new();
            defaultProperties.Add(new DefaultPropertyDescription("overall", "Gateway CPU overall usage", "%"));
            defaultProperties.Add(new DefaultPropertyDescription("drv_tot", "Total drive space", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("drv-usd", "Drive space used", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("uptime", "Gateway running time", "Day, hour, minutes, seconds"));
            defaultProperties.Add(new DefaultPropertyDescription("free", "Gateway free memory", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("buffers", "Gateway memory buffered", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("cached", "Gateway memory cached", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("active", "Gateway memory active", "Mega bytes"));
            defaultProperties.Add(new DefaultPropertyDescription("temperature", "Gateway CPU temperature", "0C"));
            defaultProperties.Add(new DefaultPropertyDescription("name", "Gateway name", "N/A"));
            defaultProperties.Add(new DefaultPropertyDescription("latitude", "Gateway location latitude", "Degree"));
            defaultProperties.Add(new DefaultPropertyDescription("longitude", "Gateway location longitude", "Degree"));

            return defaultProperties;

        }

    }
}
