﻿using BrickSchema.Net.Classes;
using BrickSchema.Net.EntityProperties;
using BrickSchema.Net.Enums;
using BrickSchema.Net.Relationships;
using BrickSchema.Net.Shapes;
using Newtonsoft.Json;
using System.Drawing;

namespace BrickSchema.Net
{
    public partial class BrickEntity
    {
        [JsonIgnore]
        internal List<BrickEntity> OtherEntities { get; set; } = new List<BrickEntity>();

        public string Id { get; set; } = string.Empty;
        public string EntityTypeName { get; set; } = string.Empty;
        public DateTime LastUpdate { get; set; }

        public List<EntityProperty> Properties { get; set; } = new();
        public List<BrickRelationship> Relationships { get; set; } = new();

        public List<BrickShape> Shapes { get; set; } = new();

        public Dictionary<string, string> RegisteredBehaviors { get; set; } = new(); //type, guid

        [JsonIgnore]
        public List<BrickBehavior> Behaviors { get; set; } = new();

        public BrickEntity(BrickEntity entity)
        {
            OnPropertyValueChanged = delegate { };
            var e = entity.Clone();
            Id = e.Id;
            EntityTypeName = e.EntityTypeName;
            Properties = e.Properties;
            Relationships = e.Relationships;
            Shapes = e.Shapes;
            RegisteredBehaviors = e.RegisteredBehaviors;
            LastUpdate = e.LastUpdate;
        }

        public BrickEntity()
        {
            OnPropertyValueChanged = delegate { };
            OtherEntities = new List<BrickEntity>();
            Id = Guid.NewGuid().ToString();
            Properties = new List<EntityProperty>();
            Relationships = new List<BrickRelationship>();
            Shapes = new List<BrickShape>();
            RegisteredBehaviors = new Dictionary<string, string>();
            Behaviors = new List<BrickBehavior>();
            LastUpdate = DateTime.Now;
            EntityTypeName = string.Empty;
        }

        public virtual BrickEntity Clone()
        {
            var clone = new BrickEntity();
            clone.Id = Id;
            clone.EntityTypeName = EntityTypeName;
            foreach (var p in Properties ?? new())
            {
                clone.Properties.Add(p.Clone());
            }
            foreach (var r in Relationships ?? new())
            {
                clone.Relationships.Add(r.Clone());
            }
            clone.RegisteredBehaviors = new(RegisteredBehaviors);
            //do not clone behaviors
            foreach (var s in Shapes??new())
            {
                clone.Shapes.Add(s.Clone());
            }
            return clone;
        }

        public virtual void Copy(BrickEntity e)
        {
            if (e == null) return;
            if (e.Id != Id) return;
            if (e.EntityTypeName != EntityTypeName) return;
            Properties = e.Properties;
            Relationships = e.Relationships;
            RegisteredBehaviors = e.RegisteredBehaviors;
            Shapes = e.Shapes;
            LastUpdate = e.LastUpdate;
        }

        public BrickEntity? GetEntity(string Id)
        {
            var entity = OtherEntities.FirstOrDefault(x => x.Id == Id);
            return entity;
        }

        public List<BrickEntity> GetParentEntities()
        {
            var entities = OtherEntities
            .Where(entity => Relationships.Any(x => x.ParentId == entity.Id)).ToList();
            return entities;
        }

        public List<BrickEntity> GetParentEntities<T>()
        {
            var rs = Relationships.Where(relationship => relationship.EntityTypeName.Equals(typeof(T).Name));

            var entities = OtherEntities
            .Where(entity => rs.Any(x => x.ParentId == entity.Id)).ToList();
            return entities;
        }

        public List<U> GetParentEntities<T, U>()
        {
            var entities = GetParentEntities<T>();
            List<U> result = new List<U>();
            foreach(var entity in entities)
            {
                if (entity is  U u) result.Add(u);
            }

            return result;
        }

        public List<BrickEntity> GetChildEntities(string entityTypeName = "", OperationTypes comparisonOperation = OperationTypes.Equals, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            List<BrickEntity> entities = new();
            
         
                entities = OtherEntities
                    .Where(oe => oe.Relationships.Any(r => r.ParentId == Id)).ToList();
           
                switch (comparisonOperation)
                {
                    case OperationTypes.Equals:
                        entities = entities
                            .Where(oe => oe.EntityTypeName.Equals(entityTypeName, comparisonType)).ToList();
                        break;
                    case OperationTypes.Contains:
                        entities = entities
                            .Where(oe => oe.EntityTypeName.Equals(entityTypeName, comparisonType)).ToList();
                    break;
                    case OperationTypes.EndsWith:
                        entities = entities
                            .Where(oe => oe.EntityTypeName.Equals(entityTypeName, comparisonType)).ToList();
                    break;
                    case OperationTypes.StartsWith:
                        entities = entities
                            .Where(oe => oe.EntityTypeName.Equals(entityTypeName, comparisonType)).ToList();
                    break;
                }
            
            return entities;
        }

        public List<BrickEntity> GetChildEntities<T>()
        {
            var entities = OtherEntities
                            .Where(oe => oe.Relationships.Any(r => r.EntityTypeName.Equals(typeof(T).Name) && r.ParentId == Id)).ToList();

            return entities;
        }

        public List<U> GetChildEntities<T, U>()
        {
            var entities = GetChildEntities<T>().ToList();

            List<U> result = new List<U>();
            foreach (var entity in entities)
            {
                if (entity is  U u) result.Add(u);
            }

            return result;
        }


        public List<Classes.Point> GetPointEntities(List<string> tags)
        {
            var entities = GetChildEntities<PointOf, Classes.Point>();

            List<Classes.Point> points = new List<Classes.Point>();
            foreach (var entity in entities)
            {
                if (entity is Classes.Point p)
                {
                    var foundTags = p.GetTags().Any(x => tags.Contains(x.Name));
                    if (foundTags)
                    {
                        points.Add(p);
                    }
                }
                
            }

            return points;
        }

        public Classes.Point? GetPointEntity(string tagName)
        {
            var entities = OtherEntities
            .Where(entity => entity.Relationships.Any(relationship => (relationship.EntityTypeName?.Equals(typeof(PointOf).Name) ?? false) && relationship.ParentId == this.Id))
            .ToList();

            foreach (var entity in entities)
            {
                var foundTags = entity.GetTags();
                foreach (var tag in foundTags)
                {
                    if (tag.Name.Equals(tagName))
                    {
                        if (entity is Classes.Point)
                        {
                            return entity as Classes.Point;
                        }
                    }
                }
            }

            return null;
        }

        public List<Tag> GetTags()
        {
            var tags = OtherEntities
            .Where(entity => entity.Relationships.Any(relationship => (relationship.EntityTypeName?.Equals(typeof(TagOf).Name) ?? false) && relationship.ParentId == this.Id))
            .ToList();
            List<Tag> foundTags = new List<Tag>();
            foreach (var tag in tags)
            {
                
                    var t = tag as Tag;
                if (t != null)
                {
                    foundTags.Add(t);
                }
                
            }
            return foundTags;
        }

        public List<BrickEntity> GetEquipmentEntities()
        {
            var entities = OtherEntities
                .Where(e => e is Equipment && e.Relationships.Any(x => x.ParentId == this.Id)).ToList();
            return entities;
        }


        public List<BrickEntity> GetEquipmentEntities(params string[] entityTypeNames)
        {
            var entities = OtherEntities
                .Where(e => e is Equipment && entityTypeNames.Contains(e.EntityTypeName) && e.Relationships.Any(x => x.ParentId == this.Id)).ToList();
            return entities;
        }

        public string ToJson()
        {
            var behaviorsJson = Helpers.EntityUntils.BehaviorsToJson(Behaviors);

            SetProperty(EntityProperties.PropertiesEnum.Behaviors, behaviorsJson);
            CleanUpDuplicatedProperties();
            return BrickSchemaUtility.EntityToJson(this);
        }

    }
}