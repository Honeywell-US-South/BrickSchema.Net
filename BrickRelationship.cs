namespace BrickSchema.Net
{
    public class BrickRelationship : BrickEntity
    {
        public string ParentId { get; set; } = string.Empty;

        public BrickRelationship()
        {
            // Automatically set the EntityTypeName to the name of the derived class
            EntityTypeName = GetType().Name;
        }

        public override BrickEntity Clone()
        {
            var clone = (BrickRelationship)this.MemberwiseClone(); // Assuming Clone is implemented correctly in BrickEntity
            clone.ParentId = ParentId;
            // Ensure the cloned object has the correct EntityTypeName
            clone.EntityTypeName = GetType().Name;
            return clone;
        }

    }
}