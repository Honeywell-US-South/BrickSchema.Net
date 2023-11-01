using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BrickSchema.Net
{
    public static class BrickSchemaUtility
    {
        public static List<BrickEntity> ImportBrickSchema(string jsonLdFilePath)
        {

            List<BrickEntity> b = new();
            string json = string.Empty;
            if (File.Exists(jsonLdFilePath))
            {
                json = File.ReadAllText(jsonLdFilePath);
            }
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Newtonsoft.Json.Formatting.Indented };
                    b = JsonConvert.DeserializeObject<List<BrickEntity>>(json, settings) ?? new();
                } catch (Exception ex) { Console.WriteLine(ex.ToString()); }
            }
            return b;
        }

        public static void WriteBrickSchemaToFile(List<BrickEntity> entities, string jsonLdFilePath)
        {

            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All, Formatting = Newtonsoft.Json.Formatting.Indented };
            JsonConvert.SerializeObject(entities, settings);
            File.WriteAllText(jsonLdFilePath, JsonConvert.SerializeObject(entities, settings));
        }

        public static string ExportBrickSchemaToJson(List<BrickEntity> entities)
        {
            
            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, Formatting = Newtonsoft.Json.Formatting.Indented };
            //JsonConvert.SerializeObject(entities, settings);
           
            var json = JsonConvert.SerializeObject(entities, settings);
            return json;
        }

        public static string EntityToJson(BrickEntity entity)
        {

            var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, Formatting = Newtonsoft.Json.Formatting.Indented };
            //JsonConvert.SerializeObject(entities, settings);

            var json = JsonConvert.SerializeObject(entity, settings);
            return json;
        }

    }
}
