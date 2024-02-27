using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml;
using BrickSchema.Net.ThreadSafeObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BrickSchema.Net
{
    public static class BrickSchemaUtility
    {
        public static ThreadSafeList<BrickEntity> ImportBrickSchema(string jsonLdFilePath)
        {

            ThreadSafeList<BrickEntity> b = new();
            string json = string.Empty;
            if (File.Exists(jsonLdFilePath))
            {
                json = File.ReadAllText(jsonLdFilePath);
            }
            //var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Newtonsoft.Json.Formatting.Indented };
            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new ThreadSafeListConverter<BrickEntity>() },
                TypeNameHandling = TypeNameHandling.All,
                Formatting = Newtonsoft.Json.Formatting.Indented
            };
            if (!string.IsNullOrEmpty(json))
            {
                try
                {

					json = json.Replace("System.Collections.Generic.List`1[[BrickSchema.Net.BrickEntity, BrickSchema.Net]], System.Private.CoreLib", "BrickSchema.Net.ThreadSafeObjects.ThreadSafeList`1[[BrickSchema.Net.BrickEntity, BrickSchema.Net]], BrickSchema.Net");
					json = json.Replace("System.Collections.Generic.List`1[[BrickSchema.Net.EntityProperties.EntityProperty, BrickSchema.Net]], System.Private.CoreLib", "BrickSchema.Net.ThreadSafeObjects.ThreadSafeList`1[[BrickSchema.Net.EntityProperties.EntityProperty, BrickSchema.Net]], BrickSchema.Net");
					json = json.Replace("System.Collections.Generic.List`1[[BrickSchema.Net.BrickRelationship, BrickSchema.Net]], System.Private.CoreLib", "BrickSchema.Net.ThreadSafeObjects.ThreadSafeList`1[[BrickSchema.Net.BrickRelationship, BrickSchema.Net]], BrickSchema.Net");
					json = json.Replace("System.Collections.Generic.List`1[[BrickSchema.Net.BrickShape, BrickSchema.Net]], System.Private.CoreLib", "BrickSchema.Net.ThreadSafeObjects.ThreadSafeList`1[[BrickSchema.Net.BrickShape, BrickSchema.Net]], BrickSchema.Net");
					b = JsonConvert.DeserializeObject<ThreadSafeList<BrickEntity>>(json, settings) ?? new();
				} catch (Exception ex) { 
                        Console.Out.WriteLineAsync(ex.ToString());
                }
            }
            return b;
        }

        public static void WriteBrickSchemaToFile(ThreadSafeList<BrickEntity> entities, string jsonLdFilePath)
        {

            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Newtonsoft.Json.Formatting.Indented };
            var jsonEntities = JsonConvert.SerializeObject(entities, settings);
            WriteBrickSchemaToFile(jsonEntities, jsonLdFilePath);
        }

        public static void WriteBrickSchemaToFile(string jsonEntities, string jsonLdFilePath)
        {

            File.WriteAllText(jsonLdFilePath, jsonEntities);
        }

        public static void AppendBrickSchemaToFile(string jsonEntities, string jsonLdFilePath)
        {
            if (!File.Exists(jsonLdFilePath))
            {
                File.WriteAllText(jsonLdFilePath, jsonEntities);
            } else
            {
                File.AppendAllText(jsonLdFilePath, "\n" + jsonEntities);
            }
        }

        public static string ExportBrickSchemaToJson(ThreadSafeList<BrickEntity> entities)
        {
            
            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, Formatting = Newtonsoft.Json.Formatting.Indented };
            var json = JsonConvert.SerializeObject(entities, settings);
            return json;
        }

        public static string EntityToJson(BrickEntity entity)
        {

            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, Formatting = Newtonsoft.Json.Formatting.Indented };
            
            var json = JsonConvert.SerializeObject(entity, settings);
            return json;
        }

        

        public static Dictionary<string, Tuple<double, double, double>> SpreadEntitiesInStarConfiguration(ThreadSafeList<BrickEntity> entities, double centerX, double centerY, int desiredSpacing, params Type[] relationshipTypes)
        {
            Dictionary<string, Tuple<double, double, double>> coordinates = new();

            var parentEntities = FindRootEntities(entities, relationshipTypes);
            double parentRadius = CalculateOptimumRadius(50, desiredSpacing, parentEntities.Count);
            SpreadNodesInStarConfiguration(parentEntities, centerX, centerY, parentRadius, 0, coordinates); // Parent layer z-coordinate set to 0

            foreach (var parent in parentEntities)
            {
                var childEntitiesWithZ = FindChildEntities(parent, entities, relationshipTypes);
                foreach (var (childEntity, zCoordinate) in childEntitiesWithZ)
                {
                    // Use zCoordinate as layer
                    var childEntities = childEntitiesWithZ.Select(ce => ce.Item1).ToThreadSafeList(); // Extract just the entities for spreading
                    double childRadius = CalculateOptimumRadius(30, 5, childEntities.Count);
                    var (newCenterX, newCenterY) = CalculateNewCenter(parent, coordinates);
                    SpreadNodesInStarConfiguration(childEntities, newCenterX, newCenterY, childRadius, zCoordinate, coordinates);
                }
            }

            return coordinates;
        }



        private static void SpreadNodesInStarConfiguration(ThreadSafeList<BrickEntity> entities, double centerX, double centerY, double radius, int zCoordinate, Dictionary<string, Tuple<double, double, double>> coordinates)
        {
            int totalNodes = entities.Count;
            double angleIncrement = 360.0 / totalNodes;

            for (int i = 0; i < totalNodes; i++)
            {
                double angleInRadians = (Math.PI / 180) * (angleIncrement * i);
                var x = centerX + radius * Math.Cos(angleInRadians);
                var y = centerY + radius * Math.Sin(angleInRadians);
                coordinates[entities[i].Id] = Tuple.Create(x, y, (double)zCoordinate);
            }
        }



        private static (double, double) CalculateNewCenter(BrickEntity parent, Dictionary<string, Tuple<double, double, double>> coordinates)
        {
            if (coordinates.TryGetValue(parent.Id, out var parentCoords))
            {
                
                return (parentCoords.Item1, parentCoords.Item2);
            }
            return (0, 0); // Fallback if parent's coordinates not found
        }

        public static ThreadSafeList<BrickEntity> FindRootEntities(ThreadSafeList<BrickEntity> entities, params Type[] relationshipTypes)
        {
            
            var relationshipTypeNamesSet = new HashSet<string>(relationshipTypes.Select(t => t.Name));

            var childEntityIds = new HashSet<string>();
            foreach (var entity in entities)
            {
                foreach (var relationship in entity.Relationships)
                {
                   
                    if (relationshipTypeNamesSet.Contains(relationship.EntityTypeName))
                    {
                        childEntityIds.Add(relationship.Id);
                    }
                }
            }

           
            var result = entities.Where(entity => !childEntityIds.Contains(entity.Id)).ToThreadSafeList();

            return result;
        }

        

        private static ThreadSafeList<(BrickEntity, int)> FindChildEntities(BrickEntity parent, ThreadSafeList<BrickEntity> entities, params Type[] relationshipTypes)
        {
            var relationshipTypeNamesSet = new Dictionary<string, int>();
            for (int i = 0; i < relationshipTypes.Length; i++)
            {
                relationshipTypeNamesSet[relationshipTypes[i].Name] = i;
            }

            var childEntities = new ThreadSafeList<(BrickEntity, int)>();

            foreach (var entity in entities)
            {
                foreach (var relationship in entity.Relationships)
                {
                    if (relationshipTypeNamesSet.TryGetValue(relationship.EntityTypeName, out int zCoordinate) && relationship.ParentId == parent.Id)
                    {
                        childEntities.Add((entity, zCoordinate));
                        break;
                    }
                }
            }

            return childEntities;
        }


        public static double CalculateOptimumRadius(int nodeDiameter, int desiredSpacing, int numberOfNodes)
        {
            // Calculate the space required for one node plus the desired spacing
            int spacePerNode = nodeDiameter + desiredSpacing;

            // Calculate the total circumference needed to place all nodes side by side without overlapping
            double totalCircumference = spacePerNode * numberOfNodes;

            // Calculate and return the optimum radius
            double optimumRadius = totalCircumference / (2 * Math.PI);
            return optimumRadius;
        }


    }
}
