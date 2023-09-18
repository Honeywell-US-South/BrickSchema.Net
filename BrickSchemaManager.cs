using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrickSchema.Net.Classes;
using BrickSchema.Net.Classes.Collection;
using BrickSchema.Net.Classes.Collection.Loop;
using BrickSchema.Net.Classes.Devices;
using BrickSchema.Net.Classes.Equipments;
using BrickSchema.Net.Classes.Equipments.HVACType;
using BrickSchema.Net.Classes.Equipments.HVACType.TerminalUnits;
using BrickSchema.Net.Classes.Locations;
using BrickSchema.Net.Classes.Measureable;
using BrickSchema.Net.Classes.Points;
using Newtonsoft.Json;

namespace BrickSchema.Net
{
    public partial class BrickSchemaManager
    {
        private List<BrickEntity> _entities;
        private readonly string? _brickPath;

        // Object used as lock for thread-safety
        private readonly object _lockObject = new object();


        public BrickSchemaManager()
        {
            _entities = new List<BrickEntity>();
        }

        public BrickSchemaManager(string brickFilePath)
        {
            _entities = new List<BrickEntity>();
            _brickPath = brickFilePath;
            LoadSchemaFromFile(_brickPath);
        }

        public void LoadSchemaFromJson(string json, bool appendOrUpdate = false)
        {
            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Newtonsoft.Json.Formatting.Indented };
            var entities = JsonConvert.DeserializeObject<List<BrickEntity>>(json, settings) ?? new();
            ImportEntities(entities, appendOrUpdate);
        }

        public void AppendOrUpdateEntityFromJson(string json)
        {
            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Newtonsoft.Json.Formatting.Indented };
            var entity = JsonConvert.DeserializeObject<BrickEntity>(json, settings);
            if (entity != null) {
                
                List<BrickEntity> entities = new();
                entities.Add(entity);
                ImportEntities(entities, true);
            }
        }



        public void ImportEntities(List<BrickEntity> entities, bool appendOrUpdate = false)
        {
            lock (_lockObject) // Locking here 
            {
                if (appendOrUpdate)
                {
                    foreach (var e in entities)
                    {
                        var _e = _entities.FirstOrDefault(x => x.Id == e.Id);
                        if (_e == null) //add new
                        {
                            _e = e;
                            var json = e.GetProperty<string>(EntityProperties.PropertiesEnum.Behaviors)??string.Empty;

                            _e.Behaviors = Helpers.EntityUntils.JsonToBehaviors(json);
                            _entities.Add(_e);
                        }
                        else //update
                        {
                            _e.Copy(e);
                            var json = e.GetProperty<string>(EntityProperties.PropertiesEnum.Behaviors)??string.Empty;

                            _e.Behaviors = Helpers.EntityUntils.JsonToBehaviors(json);
                        }
                        foreach (var _b in _e.Behaviors)
                        {
                            _b.Parent = _e;
                        }

                    }
                    foreach (var _e in _entities)
                    {
                        _e.OtherEntities = _entities;
                    }
                }
                else
                {
                    
                    foreach (var e in entities)
                    {
                        var json = e.GetProperty<string>(EntityProperties.PropertiesEnum.Behaviors)??string.Empty;

                        e.Behaviors = Helpers.EntityUntils.JsonToBehaviors(json);
                        foreach (var b in e.Behaviors)
                        {
                            b.Parent = e;
                        }
                    }
                    foreach (var e in entities)
                    {
                        e.OtherEntities = entities;
                    }
                    _entities = entities;
                }
                
            }
        }

        public void LoadSchemaFromFile(string jsonLdFilePath)
        {
            lock (_lockObject) // Locking here
            {
                _entities = BrickSchemaUtility.ImportBrickSchema(jsonLdFilePath);
                // Update the OtherEntities property of all entities
                foreach (var existingEntity in _entities)
                {
                    foreach (var _e in _entities)
                    {
                        _e.CleanUpDuplicatedProperties();
                        existingEntity.OtherEntities.Add(_e);
                    }

                }
            }
        }

        public void SaveSchema()
        {
            lock (_lockObject) // Locking here
            {
                if (!string.IsNullOrEmpty(_brickPath)) { SaveSchema(_brickPath ?? string.Empty); }
                else throw new NullReferenceException("Brick file path is null or empty.");
            }
        }

        public void SaveSchema(string jsonLdFilePath)
        {
            lock (_lockObject) // Locking here
            {
                BrickSchemaUtility.WriteBrickSchemaToFile(_entities, jsonLdFilePath);
            }
        }

        public string ToJson()
        {
            string json = "";
            lock (_lockObject) // Locking here
            {
                
                foreach (var _e in _entities)
                {
                    
                    //JsonConvert.SerializeObject(entities, settings);
                    var behaviorsJson = Helpers.EntityUntils.BehaviorsToJson(_e.Behaviors);
                    
                    _e.SetProperty(EntityProperties.PropertiesEnum.Behaviors, behaviorsJson);
                    _e.CleanUpDuplicatedProperties();

                }
                json = BrickSchemaUtility.ExportBrickSchemaToJson(_entities);
            }
            return json;
        }


        public List<dynamic> SearchEntities(Func<dynamic, bool> predicate)
        {
            return _entities.Where(predicate).ToList();
        }

        public bool UpdateEntity(dynamic updatedEntity)
        {
            lock (_lockObject) // Locking here
            {
                BrickEntity? entityToUpdate = _entities.FirstOrDefault(e => e.Id == updatedEntity.Id);
                if (entityToUpdate == null)
                {
                    return false;
                }

                entityToUpdate.EntityTypeName = updatedEntity.Type;
                entityToUpdate.Properties = updatedEntity.Properties;
                entityToUpdate.Relationships = updatedEntity.Relationships;
            }
            return true;
        }

        public bool IsEntity(string id)
        {
            return _entities.Any(x=>x.Id.Equals(id));
        }

        public bool IsTag(string name)
        {
            var tags = _entities.Where(x => (x.EntityTypeName?.Equals(typeof(Tag).Name) ?? false)).ToList();
            foreach (var tag in tags)
            {
                var t = tag as Tag;
                if (t?.Name.Equals(name)??false) return true;
            }
            return false;
        }

        public T AddEntity<T>(string id, string name) where T : BrickEntity, new()
        {
            T entity = new T();
            lock (_lockObject) // Locking here
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var existingEntity = _entities.FirstOrDefault(x => x.Id.Equals(id));
                    if (existingEntity != null) return (T)existingEntity;
                }
                entity = new T
                {
                    Id = id ?? Guid.NewGuid().ToString(),
                    EntityTypeName = typeof(T).Name

                };
                entity.SetProperty(EntityProperties.PropertiesEnum.Name, name);

                foreach (var _e in _entities)
                {
                    //entity.OtherEntities.Add(_e);
                    var e = _e as BrickEntity;
                    e.OtherEntities.Add(entity);
                }
                entity.OtherEntities = new List<BrickEntity>(_entities);
                _entities.Add(entity);
            }
            return entity;
        }

        public T AddEntity<T>(string? id) where T : BrickEntity, new()
        {
            T entity;
            lock (_lockObject) // Locking here
            {
                if (id == null)
                {
                    entity = AddEntity<T>();
                }
                else
                {
                    entity = AddEntity<T>(id, "");
                }
            }
        
            return entity;
        }

        public T AddEntity<T>() where T : BrickEntity, new()
        {

            T entity = new T();
            lock (_lockObject) // Locking here
            {
                entity = new T
                {
                    Id = Guid.NewGuid().ToString(),
                    EntityTypeName = typeof(T).Name
                };

                foreach (var _e in _entities)
                {
                    entity.OtherEntities.Add(_e);
                }

                _entities.Add(entity);
            }
            return entity;
        }

        public BrickEntity? GetEntity(string id, bool byReference = true)
        {
            lock (_lockObject) // Locking here
            {
                var entity = _entities.FirstOrDefault(x => x.Id.Equals(id));
                var behaviors = entity?.GetBehaviors(false);
                var e = byReference ? entity : entity?.Clone();

                if (e != null)
                {
                    //JsonConvert.SerializeObject(entities, settings);
                    var behaviorsJson = Helpers.EntityUntils.BehaviorsToJson(e.Behaviors);

                    e.SetProperty(EntityProperties.PropertiesEnum.Behaviors, behaviorsJson);
                    e.CleanUpDuplicatedProperties();
                }
                return e;
            }

        }
        public List<BrickEntity> GetEntities(bool byReference = true)
        {
            

            List<BrickEntity> entities = new();
            lock (_lockObject) // Locking here
            {
                foreach (var entity in _entities)
                {
                    var behaviors = entity.GetBehaviors(false);
                    var e = byReference ? entity : entity.Clone();
                    var behaviorsJson = Helpers.EntityUntils.BehaviorsToJson(e.Behaviors);

                    e.SetProperty(EntityProperties.PropertiesEnum.Behaviors, behaviorsJson);
                    e.CleanUpDuplicatedProperties();
                    entities.Add(e);

                }
            }
            return entities;

        }
        public List<BrickEntity> GetEntities<T>(bool byReference = true)
        {
            lock (_lockObject) // Locking here
            {
                var type = Helpers.EntityUntils.GetTypeName<T>();
                if (string.IsNullOrEmpty(type) || type.Equals("null"))
                { //no type so get all
                    return GetEntities(byReference);
                }
                else
                { //get specified type
                    List<BrickEntity> entities = new();
                    var isBrickClass = typeof(T).IsSubclassOf(typeof(BrickClass));
                    foreach (var entity in _entities)
                    {
                        bool add = entity.GetType() == typeof(T);
                        if (!add && isBrickClass)
                        {
                            var brickClassName = entity.GetProperty<string>(EntityProperties.PropertiesEnum.BrickClass);
                            if (brickClassName?.Equals(type) ?? false)
                            {
                                add = true;
                            }
                        }

                        if (add)
                        {
                            var behaviors = entity.GetBehaviors(false);

                            var e = byReference ? entity : entity.Clone();
                            var behaviorsJson = Helpers.EntityUntils.BehaviorsToJson(e.Behaviors);

                            e.SetProperty(EntityProperties.PropertiesEnum.Behaviors, behaviorsJson);
                            e.CleanUpDuplicatedProperties();
                            entities.Add(e);

                        }

                    }
                    return entities;
                }
            }
            
        }

        public Tag? GetTag(string name, bool byReference = true)
        {
            var tags = _entities.Where(x => (x.EntityTypeName?.Equals(typeof(Tag).Name) ?? false)).ToList();
            if (tags == null) return null;
            if (tags.Count == 0) return null;
            foreach (var tag in tags)
            {
                var t = tag as Tag;
                if (t?.Name.Equals(name)??false) return byReference?t:t.Clone(); 
            }
            return null;
        }

        public List<BrickEntity> GetEquipments(List<string> equipmentIds, bool byReference = true)
        {
            List<BrickEntity> entities = new List<BrickEntity>();
            var matchedEntities = _entities.Where(x => equipmentIds.Contains(x.Id)).ToList();
            
            foreach (var entity in matchedEntities)
            {
                if (entity is Equipment)
                {
                    var behaviors = entity.GetBehaviors(false);
          
                    var e = byReference ? entity : entity.Clone();
                    var behaviorsJson = Helpers.EntityUntils.BehaviorsToJson(e.Behaviors);

                    e.SetProperty(EntityProperties.PropertiesEnum.Behaviors, behaviorsJson);
                    e.CleanUpDuplicatedProperties();
                    entities.Add(e);
                }
            }

            return entities;
        }

    }
}
