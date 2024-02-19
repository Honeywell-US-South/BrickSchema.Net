using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BrickSchema.Net.Alerts;
using BrickSchema.Net.Behaviors;
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
using BrickSchema.Net.DB;
using BrickSchema.Net.StaticNames;
using IoTDBdotNET;
using Newtonsoft.Json;

namespace BrickSchema.Net
{
    public partial class BrickSchemaManager
    {
        private List<BrickEntity> _entities;
        private readonly string? _brickPath;

        // Object used as lock for thread-safety
        private readonly object _lockObject = new object();
        IoTDatabase _database;

        public BrickSchemaManager()
        {
            _entities = new List<BrickEntity>();
        }

        public BrickSchemaManager(string brickFilePath)
        {
            _entities = new List<BrickEntity>();
            _brickPath = brickFilePath;
            if (string.IsNullOrEmpty(_brickPath)) throw new ArgumentNullException("Empty path");
            var dbPath = Path.Combine(Path.GetDirectoryName(_brickPath)??"", "IoTDB");
            _database = new IoTDatabase("EmberAnalytics", dbPath, true);
            LoadSchemaFromFile(_brickPath);
            SaveSchema();
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
            if (entity != null)
            {

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
                            var blist = e.GetProperty<List<string>>(EntityProperties.PropertiesEnum.Behaviors) ?? new();

                            _e.Behaviors = entities
                                .Where(x => blist.Contains(x.Id) && x is BrickBehavior) // Find all entities that match the criteria.
                                .Select(y => y as BrickBehavior??new()) // Safely cast them to BrickBehavior.
                                .ToList(); // Convert the result to a list.
                            _entities.Add(_e);
                        }
                        else //update
                        {
                            if (e?.GetProperty<string>(EntityProperties.PropertyName.Name)?.Equals("SIM_FCU_1") ?? false)
                            {
                                bool debug = true;
                            }
                            _e.Clone(e);
                            var blist = e.GetProperty<List<string>>(EntityProperties.PropertiesEnum.Behaviors) ?? new();

                            _e.Behaviors = entities
                                .Where(x => blist.Contains(x.Id) && x is BrickBehavior) // Find all entities that match the criteria.
                                .Select(y => y as BrickBehavior ?? new()) // Safely cast them to BrickBehavior.
                                .ToList(); // Convert the result to a list.
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
                        var blist = e.GetProperty<List<string>>(EntityProperties.PropertiesEnum.Behaviors) ?? new();

                        e.Behaviors = entities
                                .Where(x => blist.Contains(x.Id) && x is BrickBehavior) // Find all entities that match the criteria.
                                .Select(y => y as BrickBehavior ?? new()) // Safely cast them to BrickBehavior.
                                .ToList(); // Convert the result to a list.
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
                //var properties = _database.Tables<PropertyTable>("Properties");
                foreach (var existingEntity in _entities)
                {
                    foreach (var _e in _entities)
                    {
                        foreach (var property in _e.Properties)
                        {
                            
                            if (property.Name.Equals(PropertyName.ConformanceHistory) || property.Name.Equals(PropertyName.AverageConformanceHistory))
                            {
                                var histories = property.GetValue<Dictionary<DateTime, double>>();
                                List<DateTime> deleteList = new();
                                foreach (var history in histories??new())
                                {
                                    if (history.Key.ToLocalTime().AddDays(-1) < DateTime.Now) //archive if older than 1 day.
                                    {
                                        deleteList.Add(history.Key);
                                        _database.TimeSeries.Insert(property.Id, history.Value, timestamp: history.Key);
                                    }
                                }
                                if (histories?.Count > 0)
                                {
                                    foreach (var d in deleteList)
                                    {
                                        histories.Remove(d);
                                    }
                                }
                                property.SetValue(property.Name, histories);
                            }
                            else if (property.Name.Equals(PropertyName.BehaviorValues))
                            {
                                var bvalues = property.GetValue<List<BehaviorValue>>();
                                foreach (var bv in bvalues??new())
                                {
                                    List<BehaviorValue> keepList = new();
                                    foreach (var h in bv.Histories)
                                    {
                                        if (h.Timestamp.ToLocalTime().AddDays(-1) < DateTime.Now) //archive if older than 1 day.
                                        {
                                            _database.TimeSeries.Insert(bv.BehaviorId, h.GetValue<double>(), h.Timestamp);
                                        } else
                                        {
                                            keepList.Add(h);
                                        }
                                        
                                    }
                                    bv.Histories.Clear();
                                    bv.Histories.AddRange(keepList);
                                }
                                property.SetValue(PropertyName.BehaviorValues, bvalues);
                            }
                            else if (property.Name.Equals("AlertValue"))
                            {
                                property.Value = "";
                            }
                            else if (property.Name.Equals(PropertyName.Behaviors))
                            {
                                property.Value = "";
                            }
                        }

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
                SaveSchema(_brickPath ?? string.Empty);

            }
        }

        public void SaveSchema(string jsonLdFilePath)
        {
            if (string.IsNullOrEmpty(jsonLdFilePath)) return;
            lock (_lockObject) // Locking here
            {
                var dir = Path.GetDirectoryName(jsonLdFilePath);
                if (!string.IsNullOrEmpty(dir))
                {
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                }
                BrickSchemaUtility.WriteBrickSchemaToFile(_entities, jsonLdFilePath);
            }
        }

        public void ArchiveEntityProperties(string entityId, int olderThanDays = 30)
        {
            lock (_lockObject) // Locking here
            {
                BrickEntity? entity = _entities.FirstOrDefault(e => e.Id == entityId);
                if (entity != null)
                {
                    var dir = Path.GetDirectoryName(_brickPath);
                    if (string.IsNullOrEmpty(dir)) return;
                    var archiveFolder = Path.Combine(dir, "Archive");
                    if (!Directory.Exists(archiveFolder)) Directory.CreateDirectory(archiveFolder);
                    if (!Directory.Exists(archiveFolder)) return;

                    var archivedData = entity.ArchiveBehaiorValue(olderThanDays);
                    if (archivedData.Count > 0)
                    {
                        var archiveEquipmentFolder = Path.Combine(archiveFolder, entity.Id, StaticNames.PropertyName.BehaviorValues);
                        if (!Directory.Exists(archiveEquipmentFolder)) Directory.CreateDirectory(archiveEquipmentFolder);
                        var archiveFile = Path.Combine(archiveEquipmentFolder, $"{DateTime.Now.ToString("yyyy-MM-dd")}.json");
                        var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented };
                        var jsonEntities = JsonConvert.SerializeObject(archivedData, settings);
                        BrickSchemaUtility.AppendBrickSchemaToFile(jsonEntities, archiveFile);

                    }

                    var archivedConformanceData = entity.ArchiveConformanceHistory(olderThanDays);
                    if (archivedConformanceData.Count > 0)
                    {
                        var archiveEquipmentFolder = Path.Combine(archiveFolder, entity.Id, StaticNames.PropertyName.ConformanceHistory);
                        if (!Directory.Exists(archiveEquipmentFolder)) Directory.CreateDirectory(archiveEquipmentFolder);
                        var archiveFile = Path.Combine(archiveEquipmentFolder, $"{DateTime.Now.ToString("yyyy-MM-dd")}.json");
                        var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented };
                        var jsonEntities = JsonConvert.SerializeObject(archivedConformanceData, settings);
                        BrickSchemaUtility.AppendBrickSchemaToFile(jsonEntities, archiveFile);

                    }

                    if (Directory.Exists(archiveFolder))
                    {
                        var archivedAvgConformanceData = entity.ArchiveAvgConformanceHistory(olderThanDays);
                        if (archivedAvgConformanceData.Count > 0)
                        {
                            var archiveEquipmentFolder = Path.Combine(archiveFolder, entity.Id, StaticNames.PropertyName.AverageConformanceHistory);
                            if (!Directory.Exists(archiveEquipmentFolder)) Directory.CreateDirectory(archiveEquipmentFolder);
                            var archiveFile = Path.Combine(archiveEquipmentFolder, $"{DateTime.Now.ToString("yyyy-MM-dd")}.json");
                            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Formatting.Indented };
                            var jsonEntities = JsonConvert.SerializeObject(archivedAvgConformanceData, settings);
                            BrickSchemaUtility.AppendBrickSchemaToFile(jsonEntities, archiveFile);

                        }
                    }
                }

            }
        }

        public string ToJson()
        {
            string json = "";
            lock (_lockObject) // Locking here
            {

                foreach (var _e in _entities)
                {

                    //var behaviorsJson = Helpers.EntityUtils.BehaviorsToJson(_e.Behaviors);

                    //_e.SetProperty(EntityProperties.PropertiesEnum.Behaviors, behaviorsJson);
                    List<string> bList = new();
                    foreach (var b in _e.Behaviors)
                    {
                        bList.Add(b.Id);
                    }
                    _e.SetProperty(EntityProperties.PropertiesEnum.Behaviors, bList);

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
            return _entities.Any(x => x.Id.Equals(id));
        }

        public bool IsTag(string name)
        {
            var tags = _entities.Where(x => (x.EntityTypeName?.Equals(typeof(Tag).Name) ?? false)).ToList();
            foreach (var tag in tags)
            {
                var t = tag as Tag;
                if (t?.Name.Equals(name) ?? false) return true;
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
                    var behaviorsJson = Helpers.EntityUtils.BehaviorsToJson(e.Behaviors);

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
                    var e = byReference ? entity : new(entity);
                    var behaviorsJson = Helpers.EntityUtils.BehaviorsToJson(e.Behaviors);

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
                var type = Helpers.EntityUtils.GetTypeName<T>();
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

                            var e = byReference ? entity : new(entity);
                            var behaviorsJson = Helpers.EntityUtils.BehaviorsToJson(e.Behaviors);

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
                if (t?.Name.Equals(name) ?? false) return byReference ? t : t.Clone();
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

                    var e = byReference ? entity : new(entity);
                    var behaviorsJson = Helpers.EntityUtils.BehaviorsToJson(entity.Behaviors);

                    e.SetProperty(EntityProperties.PropertiesEnum.Behaviors, behaviorsJson);
                    e.CleanUpDuplicatedProperties();
                    entities.Add(e);
                }
            }

            return entities;
        }

    }
}
