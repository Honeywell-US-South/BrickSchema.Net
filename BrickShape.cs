using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net
{
    public class BrickShape : BrickEntity
    {
        [JsonIgnore]
        public string Value
        {
            get
            {
                return GetProperty<string>(EntityProperties.PropertiesEnum.Value)??string.Empty;
            }
            set
            {
                AddOrUpdateProperty(EntityProperties.PropertiesEnum.Value, value);
            }
        }

        public override BrickShape Clone()
        {
            var clone = (BrickShape)base.Clone();
            return clone;
        }

    }
}
