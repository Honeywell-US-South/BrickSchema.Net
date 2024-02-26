using BrickSchema.Net.ThreadSafeObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Classes
{
    public class Tag : BrickClass
    {

        public Tag()
        {
            SetProperty(EntityProperties.PropertiesEnum.BrickClass, typeof(Tag).Name);
        }

        internal Tag(BrickEntity entity) : base(entity) //for internal cloning
        {
            SetProperty(EntityProperties.PropertiesEnum.BrickClass, typeof(Tag).Name);
        }

        public ThreadSafeList<BrickEntity> GetEntities()
        {
            var entities = OtherEntities
                .Where(entity => entity.Relationships.Any(relationship => relationship.ParentId == Id))
                .ToThreadSafeList();
            return entities;
        }

        public override Tag Clone()
        {
            var clone = new Tag(base.Clone());
            return clone;
        }

    }

}
