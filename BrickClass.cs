using BrickSchema.Net.ThreadSafeObjects;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace BrickSchema.Net.Classes
{
    public class BrickClass : BrickEntity {
        public BrickClass():base() { 
        
            
        }

        internal BrickClass(BrickEntity entity) : base(entity) { } //for internal cloning

		public ThreadSafeList<BrickEntity> GetBranch(params Type[] types)
		{
			ThreadSafeList<BrickEntity> branches = new ThreadSafeList<BrickEntity>();
			branches.AddClone(this);
			// Filter OtherEntities based on types and a condition on Relationships
			var children = OtherEntities
				.Where(o => types.Any(t => o.GetType().IsInstanceOfType(t)) && o.Relationships.Any(r => r.ParentId.Equals(Id)))
				.ToThreadSafeList();
			foreach (var child in children)
			{
				branches.AddClone(child);
				if (child is BrickClass c)
				{
					var grandChildren = c.GetBranch(types);
					branches.AddRangeClone(grandChildren);
				}
			}
			return branches;
		}

		[JsonIgnore]
        public string Name
        {
            get { return GetProperty<string>(EntityProperties.PropertiesEnum.Name) ?? string.Empty; }

            set { SetProperty(EntityProperties.PropertiesEnum.Name, value); }
        }

        [JsonIgnore]
        public string Description
        {
            get { return GetProperty<string>(EntityProperties.PropertiesEnum.Description) ?? string.Empty; }

            set { SetProperty(EntityProperties.PropertiesEnum.Description, value); }
        }

        
        public override BrickClass Clone()
        {
            var clone = new BrickClass(base.Clone());
            return clone;
        }

        
    }

}