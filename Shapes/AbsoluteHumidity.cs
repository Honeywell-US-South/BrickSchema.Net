namespace BrickSchema.Net.Shapes
{
    public class AbsoluteHumidity : BrickShape
    {
        public enum Types
        {
            SLUG_PER_FT3,
            GRAIN_PER_GAL,
            TON_LONG_PER_YD3,
            TON_US_PER_YD3,
            KiloGM_PER_M3,
            OZ_PER_GAL,
            TON_SHORT_PER_YD3,
            LB_PER_GAL_US,
            PlanckDensity,
            LB_PER_GAL_UK,
            MilliGM_PER_DeciL,
            LB_PER_IN3,
            LB_PER_FT3,
            OZ_PER_IN3,
            LB_PER_M3,
            LB_PER_YD3,
            TON_UK_PER_YD3,
            LB_PER_GAL
        }
        
        public AbsoluteHumidity() { }
        public AbsoluteHumidity(string name) { 
        
            Value = name;
        }
        public AbsoluteHumidity(Types type) {

            Value = type.ToString();
        }

        
    }


}