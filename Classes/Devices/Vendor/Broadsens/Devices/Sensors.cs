using BrickSchema.Net.EntityProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Classes.Devices.Vendor.Broadsens.Devices
{
    public class Broadsens_SVT200_T : Device { }
    public class Broadsens_SVT200_V : Device
    {
        public static List<DefaultAttributeDescription> DefaultAttributes()
        {
            List<DefaultAttributeDescription> defaultProperties = new();
            defaultProperties.Add(new DefaultAttributeDescription("sensor_type", "Sensor type", "N/A"));
            defaultProperties.Add(new DefaultAttributeDescription("sensor_id", "Sensor id", "N/A"));
            defaultProperties.Add(new DefaultAttributeDescription("group_no", "Group Number", "N/A"));
            defaultProperties.Add(new DefaultAttributeDescription("velox", "Vibration velocity x axis", "mm/s"));
            defaultProperties.Add(new DefaultAttributeDescription("veloy", " Vibration velocity y axis", "mm/s"));
            defaultProperties.Add(new DefaultAttributeDescription("veloz", "Vibration velocity z axis", "mm/s"));
            defaultProperties.Add(new DefaultAttributeDescription("grmsx", " Acceleration rms x axis", "g"));
            defaultProperties.Add(new DefaultAttributeDescription("grmsy", "Acceleration rms y axis", "g"));
            defaultProperties.Add(new DefaultAttributeDescription("grmsz", "Acceleration rms z axis", "g"));


            return defaultProperties;

        }
    }
    public class Broadsens_SVT300_V : Device
    {
        public static List<DefaultAttributeDescription> DefaultAttributes()
        {
            List<DefaultAttributeDescription> defaultProperties = new();
            defaultProperties.Add(new DefaultAttributeDescription("sensor_type", "Sensor type", "N/A"));
            defaultProperties.Add(new DefaultAttributeDescription("sensor_id", "Sensor id", "N/A"));
            defaultProperties.Add(new DefaultAttributeDescription("group_no", "Group Number", "N/A"));
            defaultProperties.Add(new DefaultAttributeDescription("velox", "Vibration velocity x axis", "mm/s"));
            defaultProperties.Add(new DefaultAttributeDescription("veloy", " Vibration velocity y axis", "mm/s"));
            defaultProperties.Add(new DefaultAttributeDescription("veloz", "Vibration velocity z axis", "mm/s"));
            defaultProperties.Add(new DefaultAttributeDescription("grmsx", " Acceleration rms x axis", "g"));
            defaultProperties.Add(new DefaultAttributeDescription("grmsy", "Acceleration rms y axis", "g"));
            defaultProperties.Add(new DefaultAttributeDescription("grmsz", "Acceleration rms z axis", "g"));


            return defaultProperties;

        }
    }
    public class Broadsens_SVT400_V : Device
    {
        public static List<DefaultAttributeDescription> DefaultAttributes()
        {
            List<DefaultAttributeDescription> defaultProperties = new();
            defaultProperties.Add(new DefaultAttributeDescription("sensor_type", "Sensor type", "N/A"));
            defaultProperties.Add(new DefaultAttributeDescription("sensor_id", "Sensor id", "N/A"));
            defaultProperties.Add(new DefaultAttributeDescription("group_no", "Group Number", "N/A"));
            defaultProperties.Add(new DefaultAttributeDescription("velox", "Vibration velocity x axis", "mm/s"));
            defaultProperties.Add(new DefaultAttributeDescription("veloy", " Vibration velocity y axis", "mm/s"));
            defaultProperties.Add(new DefaultAttributeDescription("veloz", "Vibration velocity z axis", "mm/s"));
            defaultProperties.Add(new DefaultAttributeDescription("grmsx", " Acceleration rms x axis", "g"));
            defaultProperties.Add(new DefaultAttributeDescription("grmsy", "Acceleration rms y axis", "g"));
            defaultProperties.Add(new DefaultAttributeDescription("grmsz", "Acceleration rms z axis", "g"));


            return defaultProperties;

        }
    }
    public class Broadsens_SVT200_A : Device { }
    public class Broadsens_SVT300_A : Device { }
    public class Broadsens_SVT400_A : Device { }
    public class Broadsens_SAG200 : Device { }
}