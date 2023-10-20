using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Behaviors
{

    public class FaultAnalysis
    {
        public FaultAnalysisActivityTypes Activity { get; set; } = FaultAnalysisActivityTypes.Unknown;
        public string ActivityName { get; set; } = string.Empty;
        public List<string> PointTags { get; set; } = new List<string>();
        public string ActivityDescription { get; set; } = string.Empty;
        public string ResolutionOnFail { get; set; } = string.Empty;
        public virtual FaultAnalysisActivityCode RunActivity(BrickEntity entity)
        {
            return FaultAnalysisActivityCode.Pass;
        }

        public FaultAnalysis(FaultAnalysisActivityTypes activity, string activityName,
                     string activityDescription, string resolutionOnFail, List<string>? pointTags = null)
        {
            Activity = activity;
            ActivityName = activityName;
            ActivityDescription = activityDescription;
            ResolutionOnFail = resolutionOnFail;
            PointTags = pointTags ?? new();
        }

    }

}
