using BrickSchema.Net.ThreadSafeObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Helpers
{
    public static class EntityUtils
    {
        public static string GetTypeName<T>()
        {
            string tName = typeof(T).Name;
            if (tName.StartsWith("Nullable"))
            {
                Type nullableType = typeof(T?);
                Type? underlyingType = Nullable.GetUnderlyingType(nullableType);
                tName = underlyingType?.Name ?? "null";

            }
            return tName;
        }

        public static string BehaviorsToJson(ThreadSafeList<BrickBehavior> behaviors)
        {
            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
 
            // Create a complete copy of the list
            var behaviorsCopy = new ThreadSafeList<BrickBehavior>(behaviors);

            ThreadSafeList<string> behaviorsJson = new();
            foreach (var b in behaviorsCopy)
            {
                var bjson = JsonConvert.SerializeObject(b, settings);
                behaviorsJson.Add(bjson);
            }
            return JsonConvert.SerializeObject(behaviorsJson, settings);
        }

        public static string BehaviorsToIdList(ThreadSafeList<BrickBehavior> behaviors)
        {
            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto };
            
            // Create a complete copy of the list
            var behaviorsCopy = new ThreadSafeList<BrickBehavior>(behaviors);

            List<string> behaviorsJson = new();
            foreach (var b in behaviorsCopy)
            {
                //var bjson = JsonConvert.SerializeObject(b, settings);
                behaviorsJson.Add(b.Id);
            }
            return JsonConvert.SerializeObject(behaviorsJson, settings);
        }

        public static void JsonToBehaviors(ThreadSafeList<BrickBehavior> brickBehaviors, string json, BrickEntity? parent = null)
        {
            if (string.IsNullOrEmpty(json)) return;
            List<string> behaviorsJson = JsonConvert.DeserializeObject<List<string>>(json)??new();
            brickBehaviors.Clear();

			foreach (var b in behaviorsJson)
            {
                var bb = JsonConvert.DeserializeObject<BrickBehavior>(b);
                if (bb != null)
                {
                    if (parent != null) bb.Parent = parent;
                    brickBehaviors.Add(bb);
                }
            }

        }

		public static void JsonToBehaviors (BrickEntity entity, string json)
        {
            JsonToBehaviors(entity.Behaviors, json, entity);
        }


		public static T? GetTypeFromString<T>(string typeName) where T : class
        {
            var type = Type.GetType(typeName);
            if (type == null)
                return null;
            var instance = Activator.CreateInstance(type);
            if (instance is T result)
                return result;
            return null;
        }
    }
}
