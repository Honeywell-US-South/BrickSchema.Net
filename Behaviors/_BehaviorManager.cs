using BrickSchema.Net.Shapes;
using BrickSchema.Net.ThreadSafeObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net
{
    public partial class BrickSchemaManager
    {
        
        public ThreadSafeList<BrickBehavior> GetBehaviors(List<string> behaviorIds, bool byReference = true)
        {
            ThreadSafeList<BrickBehavior> brickBehaviors = new ThreadSafeList<BrickBehavior>();
            foreach (var entity in _entities)
            {
                var e = entity as BrickEntity;
                brickBehaviors.AddRange(e.GetBehaviors().Where(x => behaviorIds.Contains(x.Id) || behaviorIds.Count == 0));// e?.Behaviors.Where(x => behaviorIds.Contains(x.Id) || behaviorIds.Count == 0) ?? new List<BrickBehavior>());
            }

            return brickBehaviors;
        }

        public ThreadSafeList<BrickBehavior> GetBehaviorsByShapeType(List<string> behaviorIds, BehaviorFunction.Types type, bool byReference = true)
        {
            ThreadSafeList<BrickBehavior> brickBehaviors = new ThreadSafeList<BrickBehavior>();
            foreach (var entity in _entities)
            {
                var e = entity as BrickEntity;
                brickBehaviors.AddRange(e.GetBehaviorsByShapeType(type).Where(x => behaviorIds.Contains(x.Id) || behaviorIds.Count == 0) ?? new ThreadSafeList<BrickBehavior>());
            }

            return brickBehaviors;
        }

        public ThreadSafeList<BrickBehavior> GetEquipmentBehaviors(string equipmentId, bool byReference = true)
        {
            var equipments = GetEquipments(new() { equipmentId }, byReference);

            ThreadSafeList<BrickBehavior> brickBehaviors = new();
            foreach (var entity in equipments)
            {
                var e = entity as BrickEntity;
                brickBehaviors.AddRange(e.GetBehaviors());

            }

            return brickBehaviors;
        }

        public Dictionary<string, string> GetRegisteredEquipmentBehaviors(string equipmentId, bool byReference = true)
        {
            var equipments = GetEquipments(new() { equipmentId });

            Dictionary<string, string> registeredBrickBehaviors = new();
            foreach (var entity in equipments)
            {
                var e = entity as BrickEntity;
                registeredBrickBehaviors = (e?.RegisteredBehaviors ?? new Dictionary<string, string>());
                break;
            }

            return byReference? registeredBrickBehaviors : new(registeredBrickBehaviors);
        }

    }
}
