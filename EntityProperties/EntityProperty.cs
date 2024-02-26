using Newtonsoft.Json;


namespace BrickSchema.Net.EntityProperties
{
    public class EntityProperty
    {
        public string Id { get; set; }
        public string DataTypeName { get; set; } = string.Empty;
        public string Name { get; set; }
        public string Value { get; set; } = string.Empty;

        public EntityProperty Clone()
        {
            var clone = new EntityProperty();
            clone.Id = Id;
            clone.DataTypeName = DataTypeName;
            clone.Name = Name;
            clone.Value = Value;
            return clone;
        }
        public EntityProperty()
        {
            Id = Guid.NewGuid().ToString();
            DataTypeName = string.Empty;
            Name = string.Empty;
            
        }

        [JsonConstructor]
        public EntityProperty(string id, string dataTypeName, string name, string value)
        {
            Id = id;
            DataTypeName = dataTypeName;
            Name = name;
            Value = value;

        }

        public void SetValue<T> (string name, T value)
        {
            if (value == null) { Console.WriteLine($"Property Set Value [{name}:null]"); return; }
            try
            {
                this.DataTypeName = GetTypeName<T>();
                this.Name = name;
                this.Value = JsonConvert.SerializeObject(value);
            } catch (Exception ex) { Console.WriteLine($"Property Set Value [{name}:{ex.Message}]"); }
        }

        public T? GetValue<T>()
        {
            if (DataTypeName == null) return default(T);

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
                    // Check if the target type is an integer type
                    else if (typeof(T) == typeof(int) || typeof(T) == typeof(long) || typeof(T) == typeof(short))
                    {
                        // First try converting to a double
                        var intermediateValue = Convert.ToDouble(this.Value);
                        return (T)Convert.ChangeType(intermediateValue, typeof(T));
                    }
                    else
                    {
                        // For other types, use Convert.ChangeType directly
                        var convertedValue = Convert.ChangeType(this.Value, typeof(T));
                        return (T)convertedValue;
                    }
                }
                catch (Exception innerEx)
                {
					// Log or handle the inner exception if needed
					Console.Out.WriteLineAsync($"Error converting the value for type {tName}: {innerEx.Message}");
                }
                // Logging or handle the exception as needed
                Console.Out.WriteLineAsync($"Error deserializing the value for type {tName}: {ex.Message}");
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
                Type underlyingType = Nullable.GetUnderlyingType(nullableType)??nullableType;
                tName = underlyingType?.Name??string.Empty;
            }
            return tName;
        }

    }
}
