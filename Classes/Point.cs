using BrickSchema.Net.Classes.Points;
using BrickSchema.Net.EntityProperties;

namespace BrickSchema.Net.Classes
{
    //Point class

    public class Point : BrickClass
    {
        
        public Point() 
        {
            SetProperty(EntityProperties.PropertiesEnum.BrickClass, typeof(Point).Name);
        }
        internal Point(BrickEntity entity) : base(entity) //for internal cloning
        {
            SetProperty(EntityProperties.PropertiesEnum.BrickClass, typeof(Point).Name);
        }

        public double? Value
        {
            get
            {
                return GetProperty<double>(EntityProperties.PropertiesEnum.Value);
            }
            private set
            {
                if (Value != value)
                {
                    SetProperty(EntityProperties.PropertiesEnum.Value, value);
                    NotifyPropertyValueChange(PropertiesEnum.Value);
                }
            }
        }
        public DateTime Timestamp
        {
            get
            {
                return GetProperty<DateTime>(EntityProperties.PropertiesEnum.Timestamp);
            }
            private set
            {
                SetProperty(EntityProperties.PropertiesEnum.Timestamp, value);
            }
        }
        public PointValueQuality Quality {
            get
            {
                return GetProperty<PointValueQuality>(EntityProperties.PropertiesEnum.ValueQuality);
            }
            private set
            {
                SetProperty(EntityProperties.PropertiesEnum.ValueQuality, value);
            }
        } 


        public void UpdateValue(double? value, DateTime timestamp, PointValueQuality valueQuality = PointValueQuality.Good)
        {
            Value = value;
            Timestamp = timestamp;
            Quality = valueQuality;
            
        }


        public override Point Clone()
        {
            var clone = new Point(base.Clone());
            return clone;
        }
    }

    
}
