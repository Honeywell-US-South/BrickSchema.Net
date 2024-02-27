using System;
using System.Collections.Concurrent;
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
using BrickSchema.Net.Helpers;
using BrickSchema.Net.StaticNames;
using BrickSchema.Net.ThreadSafeObjects;
using IoTDBdotNET;
using Newtonsoft.Json;

namespace BrickSchema.Net
{
    public partial class BrickSchemaManager
    {
       
        private ThreadSafeList<BrickEntity> _entities;
        private readonly string? _brickPath;

        // Object used as lock for thread-safety
        private readonly object _lockObject = new object();
        IoTDatabase? _database = null;

        public BrickSchemaManager()
        {
            _entities = new ThreadSafeList<BrickEntity>();
        }

        public BrickSchemaManager(string brickFilePath)
        {
            _entities = new ThreadSafeList<BrickEntity>();
            _brickPath = brickFilePath;
            if (string.IsNullOrEmpty(_brickPath)) throw new ArgumentNullException("Empty path");
            var dbPath = Path.Combine(Path.GetDirectoryName(_brickPath)??"", "IoTDB");
            _database = new IoTDatabase("EmberAnalytics", dbPath, true);
            LoadSchemaFromFile(_brickPath);
            SaveSchema();
        }

        public void LoadSchemaFromJson(string json, bool clearList = false)
        {
            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Newtonsoft.Json.Formatting.Indented };
            var entities = JsonConvert.DeserializeObject<ThreadSafeList<BrickEntity>>(json, settings) ?? new();
            ImportEntities(entities, clearList);
        }

        public void AppendOrUpdateEntityFromJson(string json)
        {
            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Newtonsoft.Json.Formatting.Indented };
            var entity = JsonConvert.DeserializeObject<BrickEntity>(json, settings);
            if (entity != null)
            {

                ThreadSafeList<BrickEntity> entities = new();
                entities.Add(entity);
                ImportEntities(entities, false);
            }
        }



        public void ImportEntities(ThreadSafeList<BrickEntity> newEntities, bool clearList = false)
        {
            lock (_lockObject) // Locking here 
            {
                if (clearList) _entities.Clear();
                if (_entities.Count == 0)
                {
                    int i = 0;
                }
                foreach (var e in newEntities)
                {
                    if (e.EntityTypeName.Contains("AHU"))
                    {
                        int i = 0;
                    }
                    var _e = _entities.FirstOrDefault(x => x.Id == e.Id);
                    if (_e == null) //add new
                    {
                        _e = e;
                        //var blist = e.GetProperty<ThreadSafeList<string====>>(EntityProperties.PropertiesEnum.Behaviors);

                        //if (blist == null)
                        //{
                            var bslist = e.GetProperty<string>(EntityProperties.PropertiesEnum.Behaviors) ?? "";
                           EntityUtils.JsonToBehaviors(_e, bslist);
      //                  }
      //                  else
      //                  {
						//	_e.Behaviors = newEntities
	     //                       .Where(x => blist.Contains(x.Id) && x is BrickBehavior) // Find all entities that match the criteria.
	     //                       .Select(x => x as BrickBehavior) // Safely cast them to BrickBehavior.
	     //                       .Where(y => y != null) // Ensure y is not null.
	     //                       .Select(y => { y.Parent = _e; return y; }) // Assign _e as the Parent for each BrickBehavior.
	     //                       .ToThreadSafeList(); // Convert the result to a thread-safe list.
						//}
                        _entities.Add(_e);
                    }
                    else //update
                    {
                        //if (e?.GetProperty<string>(EntityProperties.PropertyName.Name)?.Equals("SIM_FCU_1") ?? false)
                        //{
                        //    bool debug = true;
                        //}
                        _e.Clone(e);
                        //var blist = e.GetProperty<List<string>>(EntityProperties.PropertiesEnum.Behaviors);
                        //if (blist == null)
                        //{
                            var bslist = e.GetProperty<string>(EntityProperties.PropertiesEnum.Behaviors) ?? "";
                            EntityUtils.JsonToBehaviors(_e, bslist);
      //                  }
      //                  else
      //                  {
						//	_e.Behaviors = newEntities
						//		.Where(x => blist.Contains(x.Id) && x is BrickBehavior) // Find all entities that match the criteria.
						//		.Select(x => x as BrickBehavior) // Safely cast them to BrickBehavior.
						//		.Where(y => y != null) // Ensure y is not null.
						//		.Select(y => { y.Parent = _e; return y; }) // Assign _e as the Parent for each BrickBehavior.
						//		.ToThreadSafeList(); // Convert the result to a thread-safe list.
						//}
                    }

                }

            }
        }

        public void LoadSchemaFromFile(string jsonLdFilePath)
        {
            lock (_lockObject) // Locking here
            {
                if (jsonLdFilePath.Equals(_brickPath))
                {
                    var dir = Path.GetDirectoryName(jsonLdFilePath)?? "";
                    var name = Path.GetFileNameWithoutExtension(jsonLdFilePath);
                    var backup = Path.Combine(dir, $"{name}.~json");
                    try
                    {
                        File.Copy(jsonLdFilePath, backup, true);
                    }
                    catch { }
                }
               
                BrickSchemaUtility.CreateBrickSchemaFromJsonFile(_entities, jsonLdFilePath);
                    
                
            }
            
        }

		private void PushEntitiesDataToDatabase(bool clearEntityPropertyBehaviors = false)
		{
			lock (_lockObject)
			{
				int timeout = 1000 * 60; // Timeout in milliseconds (60 seconds)
				var cts = new CancellationTokenSource();
				cts.CancelAfter(timeout); // Set your timeout
				var token = cts.Token;
				var inProgressOperations = new ConcurrentDictionary<int, bool>();
				List<Exception> exceptions = new List<Exception>(); // To capture exceptions

				try
				{
					Parallel.For(0, _entities.Count, new ParallelOptions { CancellationToken = token }, (i, state) =>
					{
						inProgressOperations.TryAdd(i, true); // Mark as in progress
						try
						{
							ProcessEntity(i, clearEntityPropertyBehaviors);
						}
						catch (Exception ex)
						{
							lock (exceptions)
							{
								exceptions.Add(ex);
							}
						}
						finally
						{
							bool removed;
							inProgressOperations.TryRemove(i, out removed); // Mark as completed
						}
					});
				}
				catch (OperationCanceledException)
				{
					// Log timed out operations
					foreach (var inProgressOperation in inProgressOperations.Keys)
					{
						Console.Out.WriteLineAsync($"Operation {inProgressOperation} timed out.");
					}
				}
				finally
				{
					// Log exceptions
					if (exceptions.Any())
					{
						foreach (var ex in exceptions)
						{
							Console.Out.WriteLineAsync(ex.ToString());
						}
					}
				}
			}
		}

		private void ProcessEntity(int i, bool clearEntityPropertyBehaviors)
		{
			var existingEntity = _entities[i];
			Console.Out.WriteLineAsync($"Processing entity {i} of {_entities.Count}");

			foreach (var property in existingEntity.Properties)
			{

				if (property.Name.Equals(PropertyName.ConformanceHistory) || property.Name.Equals(PropertyName.AverageConformanceHistory))
				{

					var histories = property.GetValue<Dictionary<DateTime, double>>();
					if (histories != null && histories.Count > 0)
					{

						var archiveList = histories.Where(x => x.Key.ToLocalTime().AddDays(7) < DateTime.Now).ToList();

						foreach (var history in archiveList ?? new())
						{
							if (history.Key.ToLocalTime().AddDays(7) < DateTime.Now) //archive if older than 1 day.
							{

								try
								{
									_database?.TimeSeries.Insert(property.Id, history.Value, timestamp: history.Key);
								}
								catch (Exception ex) { Console.Out.WriteLineAsync(ex.ToString()); }
							}
						}

						var keepList = histories.Where(x => x.Key.ToLocalTime().AddDays(7) >= DateTime.Now);
						histories.Clear();
						histories = keepList.ToDictionary(x => x.Key, x => x.Value);


						property.SetValue(property.Name, histories);
					}
				}
				else if (property.Name.Equals(PropertyName.BehaviorValues))
				{
					var bvalues = property.GetValue<List<BehaviorValue>>();
					if (bvalues != null)
					{
						foreach (var bv in bvalues)
						{

							if (bv.Histories.Count > 0)
							{
								var archiveList = bv.Histories.Where(x => x.Timestamp.ToLocalTime().AddDays(7) < DateTime.Now).ToList();
								foreach (var h in archiveList ?? new())
								{

									try
									{
										if (h.DataTypeName.Equals("Boolean"))
										{
											_database?.TimeSeries.Insert(bv.BehaviorId, (h.GetValue<Boolean>() ? 1 : 0), h.Timestamp);
										}
										else
										{
											_database?.TimeSeries.Insert(bv.BehaviorId, h.GetValue<double>(), h.Timestamp);
										}
									}
									catch (Exception ex) { Console.Out.WriteLineAsync(ex.ToString()); }


								}
								var keepList = bv.Histories.Where(x => x.Timestamp.ToLocalTime().AddDays(7) >= DateTime.Now).ToList();
								bv.Histories.Clear();
								if (keepList?.Count > 0)
								{
									bv.Histories.AddRange(keepList);
								}
							}
						}
						property.SetValue(PropertyName.BehaviorValues, bvalues);
					}
				}
				else if (property.Name.Equals("AlertValue"))
				{
					property.Value = "";
				}
				else if (property.Name.Equals(PropertyName.Behaviors))
				{
					if (clearEntityPropertyBehaviors)
					{
						property.Value = "";
					}
				}
			}
		}

        public void SaveSchema()
        {
            SaveSchemaAsync().Wait();
        }

		public async Task SaveSchemaAsync()
		{
			await Task.Run(() =>
			{
				lock (_lockObject) // Locking here as before
				{
					PushEntitiesDataToDatabase(false);
					try
					{
						UpdateBehaviorsProperty();
						SaveSchema(_brickPath ?? string.Empty);
					}
					catch
					{
						// Consider logging the exception or handling it appropriately
					}
				}
			});
		}


		public void SaveSchema(string jsonLdFilePath)
        {
            if (string.IsNullOrEmpty(jsonLdFilePath)) return;

            lock (_lockObject) // Locking here
            {
                


                var dir = Path.GetDirectoryName(jsonLdFilePath)??"";
                var name = Path.GetFileNameWithoutExtension(jsonLdFilePath);
                var backup = Path.Combine(dir, $"{name}.+json");
                var backup2 = Path.Combine(dir, $"{name}.-json");
                if (!string.IsNullOrEmpty(dir))
                {
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                }
                if (File.Exists(backup))
                {
                    try
                    {
                        if (File.Exists(backup2))
                        {
                            if (IsFileNewerThan(backup, backup2, TimeSpan.FromMinutes(60)))
                            {
                                File.Move(backup, backup2, true);
                            }
                        } else
                        {
                            File.Move(backup, backup2, true);
                        }
                        
                    }
                    catch { }
                }
                
                if (File.Exists(jsonLdFilePath))
                {
                    File.Move(jsonLdFilePath, backup, true);
                }

                BrickSchemaUtility.WriteBrickSchemaToFile(_entities, jsonLdFilePath);
            }
        }

        private bool IsFileNewerThan(string filePath1, string filePath2, TimeSpan timeSpan)
        {
            FileInfo file1Info = new FileInfo(filePath1);
            FileInfo file2Info = new FileInfo(filePath2);

            // Ensure both files exist
            if (!file1Info.Exists || !file2Info.Exists)
            {
                throw new FileNotFoundException("One or both of the files do not exist.");
            }

            // Compare last write times
            DateTime lastWriteTime1 = file1Info.LastWriteTime;
            DateTime lastWriteTime2 = file2Info.LastWriteTime;

            // Check if file1 is at least 'timeSpan' newer than file2
            return lastWriteTime1 - lastWriteTime2 >= timeSpan;
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


        public ThreadSafeList<dynamic> SearchEntities(Func<dynamic, bool> predicate)
        {
            return _entities.Where<dynamic>(predicate).ToThreadSafeList();
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

                //foreach (var _e in _entities)
                //{
                //    //entity.OtherEntities.Add(_e);
                //    var e = _e as BrickEntity;
                //    e.OtherEntities.Add(entity);
                //}
                //entity.OtherEntities = new ThreadSafeList<BrickEntity>(_entities);
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

                _entities.Add(entity);
            }
            return entity;
        }

        public BrickEntity? GetEntity(string id, bool byReference = true)
        {
            lock (_lockObject) // Locking here
            {
                var entity = _entities.FirstOrDefault(x => x.Id.Equals(id));
                //var behaviors = entity?.GetBehaviors();
                var e = byReference ? entity : entity?.Clone();

                return e;
            }

        }

        public void UpdateBehaviorsProperty()
        {
            lock (_lockObject)
            {
                foreach (var entity in _entities)
                {
                    var behaviorsJson = Helpers.EntityUtils.BehaviorsToJson(entity.GetBehaviors());

                    entity.SetProperty(EntityProperties.PropertiesEnum.Behaviors, behaviorsJson);
                    entity.CleanUpDuplicatedProperties();
                }
            }
		}

        public ThreadSafeList<BrickEntity> GetEntities()
        {
			//lock (_lockObject) // Locking here
			//{

   //             UpdateBehaviorsProperty();
                
   //         }
			return new(_entities);

		}

		public void GetEntities(ThreadSafeList<BrickEntity> entities)
        {


            lock (_lockObject) // Locking here
            {
				//UpdateBehaviorsProperty();
				entities = _entities;

                
            }
          

        }


        public ThreadSafeList<BrickEntity> GetEntities<T>()
        {
			lock (_lockObject) // Locking here
			{
				//UpdateBehaviorsProperty();
				var type = Helpers.EntityUtils.GetTypeName<T>();
				if (string.IsNullOrEmpty(type) || type.Equals("null"))
				{ //no type so get all
                    return new(_entities);
				}
				else
				{ //get specified type
                    ThreadSafeList<BrickEntity> entities = new();
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
                            
							entities.Add(entity);

						}

					}
                    return entities;

				}
			}
           
		}
		public void GetEntities<T>(ThreadSafeList<BrickEntity> entities)
        {
            lock (_lockObject) // Locking here
            {
                var type = Helpers.EntityUtils.GetTypeName<T>();
                if (string.IsNullOrEmpty(type) || type.Equals("null"))
                { //no type so get all
                    GetEntities(entities);
                }
                else
                { //get specified type
                   
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
                            //var behaviors = entity.GetBehaviors();

                           
                            var behaviorsJson = Helpers.EntityUtils.BehaviorsToJson(entity.GetBehaviors());

							entity.SetProperty(EntityProperties.PropertiesEnum.Behaviors, behaviorsJson);
							entity.CleanUpDuplicatedProperties();
                            entities.Add(entity);

                        }

                    }
  
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

        public ThreadSafeList<BrickEntity> GetEquipments(List<string> equipmentIds, bool byReference = true)
        {
            ThreadSafeList<BrickEntity> entities = new ThreadSafeList<BrickEntity>();
            var matchedEntities = _entities.Where(x => equipmentIds.Contains(x.Id)).ToList();

            foreach (var entity in matchedEntities)
            {
                if (entity is Equipment)
                {
                    //var behaviors = entity.GetBehaviors();

                    var e = byReference ? entity : new(entity);
                    var behaviorsJson = Helpers.EntityUtils.BehaviorsToJson(entity.GetBehaviors());

                    e.SetProperty(EntityProperties.PropertiesEnum.Behaviors, behaviorsJson);
                    e.CleanUpDuplicatedProperties();
                    entities.Add(e);
                }
            }

            return entities;
        }

    }
}
