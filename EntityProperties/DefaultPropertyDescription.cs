using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.EntityProperties
{
    public class DefaultPropertyDescription
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }

        public DefaultPropertyDescription() { }

        public DefaultPropertyDescription(string name, string description, string unit)
        {
            Name = name;
            Description = description;
            Unit = unit;
        }
    }
}
