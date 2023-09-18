using BrickSchema.Net.Behaviors;
using Microsoft.Extensions.Logging;

namespace BrickSchema.Net
{
    public partial class BrickBehavior
    {
        protected void NotifyErrorAndLog(string errorCode, string message, List<BehaviorResult> results)
        {
            
            if (NotifyError(errorCode))
            {
                results.Add(new()
                {
                    BehaviorId = Id,
                    BehaviorType = EntityTypeName??string.Empty,
                    BehaviorName = Name,
                    EntityId = Parent?.Id??string.Empty,
                    Value = null,
                    Timestamp = DateTime.Now,
                    Text = message,
                    Status = BehaviorResultStatusTypes.Skipped
                });
                _logger?.LogWarning($"Behavior {Name} for {Parent?.GetProperty<string>(BrickSchema.Net.EntityProperties.PropertiesEnum.Name)}: {message}.");
            }
            

        }
    }
}
