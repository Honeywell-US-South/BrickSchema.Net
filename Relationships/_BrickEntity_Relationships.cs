using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace BrickSchema.Net
{
    /// <summary>
    /// This class is part of BrickEnity. Placing in this folder for organization purpose only.
    /// </summary>
    public partial class BrickEntity
    {
        public double GetRelationshipDependancySore(string rootId = "", double factor = 1)
        {
            double score = 0;
            if (this.Id == rootId) return score; //exit if circular
            if (string.IsNullOrEmpty(rootId)) rootId = this.Id; //this is the true root
            

            foreach (var entity in OtherEntities.ToArray())
            {
                if (entity.Relationships.Any(x => x.ParentId == this.Id))
                {
                    score += (entity.Relationships.Count(x => x.ParentId == this.Id) * factor);
                    score += entity.GetRelationshipDependancySore(rootId, factor / 2);
                }
                
            }

            return score;
        }

       
    }
}
