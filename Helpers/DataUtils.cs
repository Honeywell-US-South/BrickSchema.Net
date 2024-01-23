using BrickSchema.Net;
using System.Collections;

namespace EmberAnalyticsService.GraphBehaviors.Utils
{
    public static class DataUtils
    {
        public static double? WeightedConformanceAverage(this IEnumerable<BrickBehavior>? behaviors)
        {
            if(behaviors == null) return null;
            double totalWeight = 0;
            double totalWeightedSum = 0;

            foreach(var b in behaviors)
            {
                double? conformance = b.GetConformance();
                if(conformance.HasValue)
                {
                    totalWeight += b.Weight;
                    totalWeightedSum += (b.Weight * conformance.Value);
                }
            }
            if (totalWeight == 0) return null;
            return totalWeightedSum / totalWeight;
        }

    }
}
