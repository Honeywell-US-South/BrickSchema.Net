using Newtonsoft.Json;


namespace BrickSchema.Net.EntityProperties
{
    public class EntityProperty
    {
        public string Id { get; set; }
        public string? Type { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public EntityProperty Clone()
        {
            var clone = new EntityProperty();
            clone.Id = Id;
            clone.Type = Type;
            clone.Name = Name;
            clone.Value = Value;
            return clone;
        }
        public EntityProperty()
        {
            Id = Guid.NewGuid().ToString();
            Type = null;
            Name = string.Empty;
            
        }

        [JsonConstructor]
        public EntityProperty(string id, string type, string name, string value)
        {
            Id = id;
            Type = type;
            Name = name;
            Value = value;

        }

        public void SetValue<T> (string name, T value)
        {
            if (value == null) { Console.WriteLine($"Property Set Value [{name}:null]"); return; }
            try
            {
                this.Type = GetTypeName<T>();
                this.Name = name;
                this.Value = JsonConvert.SerializeObject(value);
            } catch (Exception ex) { Console.WriteLine($"Property Set Value [{name}:{ex.Message}]"); }
        }

        public T? GetValue<T>()
        {
            if (Type == null) return default(T);

            string tName = GetTypeName<T>();

            // Configure the JsonSerializerSettings with TypeNameHandling.Auto
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };

            try
            {
                T? deserializedObject = JsonConvert.DeserializeObject<T>(this.Value, settings);
                return deserializedObject;
            }
            catch (Exception ex)
            {
                try
                {
                    if (typeof(T) == typeof(bool))
                    {
                        return (T)(object)DeserializeToBool(Value);
                    }
                } catch { }
                // Logging or handle the exception as needed
                Console.WriteLine($"Error deserializing the value for type {tName}: {ex.Message}");
            }

            return default(T?);
        }

        bool DeserializeToBool(string value)
        {
            if (double.TryParse(value, out double result))
            {
                return result != 0;
            }

            return false;
        }

        private string GetTypeName<T>()
        {
            string tName = typeof(T).Name;
            if (tName.StartsWith("Nullable"))
            {
                Type nullableType = typeof(T?);
                Type underlyingType = Nullable.GetUnderlyingType(nullableType);
                tName = underlyingType.Name;
            }
            return tName;
        }

    }
}
