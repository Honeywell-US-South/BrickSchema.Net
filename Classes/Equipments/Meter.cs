using BrickSchema.Net.Relationships;
using BrickSchema.Net.ThreadSafeObjects;

namespace BrickSchema.Net.Classes.Equipments
{
    public class Meter : Equipment {

        
        public ThreadSafeList<BrickEntity> HasSubMeter()
        {
            var entities = OtherEntities
            .Where(entity => entity.Relationships.Any(relationship => relationship.EntityTypeName?.Equals(typeof(SubmeterOf).Name) ?? false && relationship.ParentId == this.Id))
            .ToThreadSafeList();
            return entities;
        }
    }
}
