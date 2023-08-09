using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Helpers
{
    public static class EntityUntils
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

        public static string BehaviorsToJson(List<BrickBehavior> behaviors)
        {
            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto};
            List<string> behaviorsJson = new();
            foreach (var b in behaviors)
            {
                var bjson = JsonConvert.SerializeObject(b as BrickBehavior, settings);
                behaviorsJson.Add(bjson);
            }
            return JsonConvert.SerializeObject(behaviorsJson, settings);
        }

        public static List<BrickBehavior> JsonToBehaviors(string json)
        {
            if (string.IsNullOrEmpty(json)) return new();
            List<string>? behaviorsJson = JsonConvert.DeserializeObject<List<string>>(json);
            List<BrickBehavior> brickBehaviors= new List<BrickBehavior>();
            foreach (var b in behaviorsJson)
            {
                var bb = JsonConvert.DeserializeObject<BrickBehavior>(b);
                if (bb != null) brickBehaviors.Add(bb);
            }

            return brickBehaviors;
        }
    }
}
