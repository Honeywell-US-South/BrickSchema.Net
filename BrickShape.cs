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
                SetProperty(EntityProperties.PropertiesEnum.Value, value);
            }
        }

        public override BrickShape Clone()
        {
            var clone = new BrickShape();
            clone.Id = Id;
            clone.EntityTypeName = EntityTypeName;
            foreach (var p in Properties ?? new())
            {
                clone.Properties.Add(p.Clone());
            }
            foreach (var r in Relationships ?? new())
            {
                clone.Relationships.Add(r.Clone());
            }
            clone.RegisteredBehaviors = new(RegisteredBehaviors);
            //do not clone behaviors
            foreach (var s in Shapes ?? new())
            {
                clone.Shapes.Add(s.Clone());
            }
            return clone;
        }

    }
}
