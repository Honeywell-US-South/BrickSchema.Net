using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace BrickSchema.Net.Classes
{
    public class BrickClass : BrickEntity {

        

        public BrickClass():base() { 
        
            
        }

        internal BrickClass(BrickEntity entity) : base(entity) { } //for internal cloning

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