using BrickSchema.Net.Behaviors;
using BrickSchema.Net.Classes;
using BrickSchema.Net.EntityProperties;
using BrickSchema.Net.Relationships;
using BrickSchema.Net.Shapes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Keep namespace as BirckSchema.Net
namespace BrickSchema.Net
{
    /// <summary>
    /// This class is part of BrickEnity. Placing in this folder for organization purpose only.
    /// </summary>
    public partial class BrickEntity
    {
        public BrickBehavior AddBehavior(BrickBehavior behavior)
        {

            var behaviors = GetBehaviors(behavior.EntityTypeName??string.Empty);
            if (behaviors.Count >= 1)
            {
                for (int i = 0; i < behaviors.Count - 1; i++)
                {
                    RemoveBehavior(behaviors[i]);
                }
                var foundBehavior = behaviors[behaviors.Count - 1];
                if (foundBehavior.Parent == null) foundBehavior.Parent = this;
                return foundBehavior;

            }

            if (!string.IsNullOrEmpty(behavior.EntityTypeName))
            {
                if (RegisteredBehaviors.ContainsKey(behavior.EntityTypeName))
                {

                    behavior.Id = RegisteredBehaviors[behavior.EntityTypeName];
                }
                else
                {
                    RegisteredBehaviors.Add(behavior.EntityTypeName, behavior.Id);
                }
            }

            behavior.Parent = this; //must set this before start
            behavior.Start();
            Behaviors.Add(behavior);
            LastUpdate = DateTime.Now;
            return behavior;
        }

        public void RemoveBehavior(string type)
        {
            var behaviors = GetBehaviors(type);
            foreach (var behavior in behaviors??new())
            {
                behavior.Stop();
                Behaviors.Remove(behavior);
            }
            LastUpdate = DateTime.Now;
        }

        public void RemoveBehavior(BrickBehavior behavior)
        {
            Behaviors.Remove(behavior);
            LastUpdate = DateTime.Now;
        }

        public List<BrickBehavior> GetBehaviors(bool byReference = true)
        {
            if (byReference)
            {
                
                return Behaviors;
            }

            List<BrickBehavior> brickBehaviors = new();
            foreach(var b in Behaviors??new())
            {
                brickBehaviors.Add(b.Clone());
            }
            return brickBehaviors;
        }

        public List<BrickBehavior> GetBehaviors(string type, bool byReference = true)
        {
            var behaviors = Behaviors.Where(x => x.EntityTypeName == type).ToList();
            if (byReference) return behaviors;

            List <BrickBehavior> brickBehaviors = new();
            foreach (var behavior in Behaviors)
            {
                brickBehaviors.Add(behavior.Clone());
            }
            return brickBehaviors;
        }

        public BrickBehavior? GetBehavior(string type)
        {
            var behaviors = GetBehaviors(type);
            if (behaviors.Count >= 1)
            {
                for (int i = 0; i < behaviors.Count - 1; i++)
                {
                    RemoveBehavior(behaviors[i]);
                }
                var foundBehavior = behaviors[behaviors.Count - 1];
                return foundBehavior;

            }
            return null;
        }

        public List<BrickBehavior> GetBehaviorsByShapeType(BehaviorFunction.Types type)
        {
            var behaviors = Behaviors.Where(x=>x.Shapes.Any(y=>y.Value == type.ToString())).ToList();
            return behaviors;
        }

        private readonly object lockSetConformanceObj = new object();

        public double? GetConformance(bool average = false)
        {
            if (!IsProperty(PropertiesEnum.Conformance)) return null;
            lock (lockSetConformanceObj)
            {

                if (average)
                {
                    return GetProperty<double?>(PropertiesEnum.AverageConformance);
                }

                return GetProperty<double?>(PropertiesEnum.Conformance);

            }
        }

        public List<Tuple<DateTime, double>> GetConformanceHistory(bool average = false)
        {
            Dictionary<DateTime, double> results = new();
            lock (lockSetConformanceObj)
            {

                if (average)
                {

                    results = GetProperty<Dictionary<DateTime, double>>(PropertiesEnum.AverageConformanceHistory) ?? new();
                }
                else
                {
                    results = GetProperty<Dictionary<DateTime, double>>(PropertiesEnum.ConformanceHistory) ?? new();
                }
            }
            List<Tuple<DateTime, double>> tuples= new List<Tuple<DateTime, double>>();
            foreach (var r in results)
            {
                var tuple = (r.Key, r.Value).ToTuple<DateTime, double>();

                tuples.Add(tuple);
            }
            return tuples;
        }

        public BehaviorValue? SetConformance(double value)
        {
            lock (lockSetConformanceObj)  // Lock to ensure thread safety
            {
                if (value < 0) value = 0;
                else if (value > 100) value = 100;

               
                var conformanceHistory = GetProperty<Dictionary<DateTime, double>>(PropertiesEnum.ConformanceHistory);
                if (conformanceHistory == null)
                {
                    conformanceHistory = new();
                }
                conformanceHistory.Add(DateTime.UtcNow, value);
                // Filter items newer than 14 days and create a new dictionary
                conformanceHistory = conformanceHistory.Where(x => x.Key >= DateTime.UtcNow.AddDays(-14)).ToDictionary(x => x.Key, x => x.Value);

                SetProperty(PropertiesEnum.Conformance, value);
                SetProperty(PropertiesEnum.ConformanceHistory, conformanceHistory);
                var avgconformance = GetProperty<double>(PropertiesEnum.AverageConformance);
                var avg = (avgconformance + value) / 2;

                var avgConformanceHistory = GetProperty<Dictionary<DateTime, double>>(PropertiesEnum.AverageConformanceHistory);
                if (avgConformanceHistory == null)
                {
                    avgConformanceHistory = new();
                }
                avgConformanceHistory.Add(DateTime.UtcNow, avg);
                // Filter items newer than 14 days and create a new dictionary
                avgConformanceHistory = avgConformanceHistory.Where(x => x.Key >= DateTime.UtcNow.AddDays(-14)).ToDictionary(x => x.Key, x => x.Value);
                SetProperty(PropertiesEnum.AverageConformance, avg);
                SetProperty(PropertiesEnum.AverageConformanceHistory, avgConformanceHistory);

                if (this is BrickBehavior bb)
                {
                    var behaviors = bb.Parent?.GetBehaviors().Where(b => b.IsProperty(PropertiesEnum.Conformance)).ToList() ?? new();
                    bb.Parent?.SetConformance(behaviors.Average(x => x.GetProperty<double>(PropertiesEnum.Conformance)));

                    BehaviorValue bv = new(PropertiesEnum.Conformance, Id, EntityTypeName, GetShapeStringValue<BehaviorFunction>());
                    bv.SetValue(value);
                    return bv;
                }
                else if (this is Equipment)
                {
                    var relationships = Relationships.Where(x => x is LocationOf).ToList();
                    foreach (var relationship in relationships)
                    {
                        if (!string.IsNullOrEmpty(relationship.ParentId))
                        {
                            var parent = GetEntity(relationship.ParentId);
                            if (parent != null)
                            {
                                parent.SetConformance(parent.GetChildEntities().Where(p=>p.IsProperty(PropertiesEnum.Conformance)).Average(x => x.GetProperty<double>(PropertiesEnum.Conformance)));
                            }
                        }
                    }
                }
            }
            return null;
        }


    }
}
