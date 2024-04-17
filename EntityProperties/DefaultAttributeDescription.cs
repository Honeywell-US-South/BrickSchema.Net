using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.EntityProperties
{
    public class DefaultAttributeDescription
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }

        public DefaultAttributeDescription() { }

        public DefaultAttributeDescription(string name, string description, string unit)
        {
            Name = name;
            Description = description;
            Unit = unit;
        }
    }
}
