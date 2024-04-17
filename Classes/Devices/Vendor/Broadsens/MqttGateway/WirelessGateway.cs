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
        public static List<DefaultAttributeDescription> DefaultAttributes()
        {
            List<DefaultAttributeDescription> defaultProperties = new();
            defaultProperties.Add(new DefaultAttributeDescription("overall", "Gateway CPU overall usage", "%"));
            defaultProperties.Add(new DefaultAttributeDescription("drv_tot", "Total drive space", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("drv-usd", "Drive space used", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("uptime", "Gateway running time", "Day, hour, minutes, seconds"));
            defaultProperties.Add(new DefaultAttributeDescription("free", "Gateway free memory", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("buffers", "Gateway memory buffered", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("cached", "Gateway memory cached", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("active", "Gateway memory active", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("temperature", "Gateway CPU temperature", "0C"));
            defaultProperties.Add(new DefaultAttributeDescription("name", "Gateway name", "N/A"));
            defaultProperties.Add(new DefaultAttributeDescription("latitude", "Gateway location latitude", "Degree"));
            defaultProperties.Add(new DefaultAttributeDescription("longitude", "Gateway location longitude", "Degree"));

            return defaultProperties;

        }
        
    }
    public class Broadsens_GU300S : MqttDevice
    {
        public static List<DefaultAttributeDescription> DefaultAttributes()
        {
            List<DefaultAttributeDescription> defaultProperties = new();
            defaultProperties.Add(new DefaultAttributeDescription("overall", "Gateway CPU overall usage", "%"));
            defaultProperties.Add(new DefaultAttributeDescription("drv_tot", "Total drive space", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("drv-usd", "Drive space used", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("uptime", "Gateway running time", "Day, hour, minutes, seconds"));
            defaultProperties.Add(new DefaultAttributeDescription("free", "Gateway free memory", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("buffers", "Gateway memory buffered", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("cached", "Gateway memory cached", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("active", "Gateway memory active", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("temperature", "Gateway CPU temperature", "0C"));
            defaultProperties.Add(new DefaultAttributeDescription("name", "Gateway name", "N/A"));
            defaultProperties.Add(new DefaultAttributeDescription("latitude", "Gateway location latitude", "Degree"));
            defaultProperties.Add(new DefaultAttributeDescription("longitude", "Gateway location longitude", "Degree"));

            return defaultProperties;

        }

    }
    public class Broadsens_GU200S : MqttDevice
    {
        public static List<DefaultAttributeDescription> DefaultAttributes()
        {
            List<DefaultAttributeDescription> defaultProperties = new();
            defaultProperties.Add(new DefaultAttributeDescription("overall", "Gateway CPU overall usage", "%"));
            defaultProperties.Add(new DefaultAttributeDescription("drv_tot", "Total drive space", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("drv-usd", "Drive space used", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("uptime", "Gateway running time", "Day, hour, minutes, seconds"));
            defaultProperties.Add(new DefaultAttributeDescription("free", "Gateway free memory", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("buffers", "Gateway memory buffered", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("cached", "Gateway memory cached", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("active", "Gateway memory active", "Mega bytes"));
            defaultProperties.Add(new DefaultAttributeDescription("temperature", "Gateway CPU temperature", "0C"));
            defaultProperties.Add(new DefaultAttributeDescription("name", "Gateway name", "N/A"));
            defaultProperties.Add(new DefaultAttributeDescription("latitude", "Gateway location latitude", "Degree"));
            defaultProperties.Add(new DefaultAttributeDescription("longitude", "Gateway location longitude", "Degree"));

            return defaultProperties;

        }

    }
}
