using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.ThreadSafeObjects
{
	/* *********** HOW TO USE CONVERTER *********
	var settings = new JsonSerializerSettings
					{
						Converters = new List<JsonConverter> { new ThreadSafeListConverter<BrickEntity>()},
						TypeNameHandling = TypeNameHandling.All,
						Formatting = Newtonsoft.Json.Formatting.Indented
					};
	var b = JsonConvert.DeserializeObject<ThreadSafeList<BrickEntity>>(json, settings);
	*/
	public class ThreadSafeListConverter<T> : JsonConverter where T : class
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(ThreadSafeList<T>);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			// Make sure to ignore the converter for this operation to prevent a stack overflow
			serializer.Converters.Remove(this);

			var jsonObject = JObject.Load(reader);
			var targetType = Type.GetType((string)jsonObject["$type"]);
			Type typeOfT = typeof(T);

			// Check if targetType is assignable to ThreadSafeList<T>
			if (typeof(ThreadSafeList<T>).IsAssignableFrom(targetType))
			{
				var list = jsonObject.ToObject(targetType, serializer) as ThreadSafeList<T>;
				// Optionally, set the parent list if applicable
				if (list != null)
				{
					foreach (var itemCheck in list)
					{
						var parentPropertyCheck = itemCheck.GetType().GetProperties()
								.FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(ThreadSafeListRefAttribute)) && prop.PropertyType == typeof(ThreadSafeList<T>));
						if (parentPropertyCheck != null)
						{
							foreach (var item in list)
							{
								var parentProperty = item.GetType().GetProperties()
									.FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(ThreadSafeListRefAttribute)) && prop.PropertyType == typeof(ThreadSafeList<T>));
								if (parentProperty != null)
								{
									parentProperty?.SetValue(item, list);
								}
							}
						}
						break;
					}
				}
				return list ?? new ThreadSafeList<T>();
			}
			else if (targetType.IsArray && targetType.GetElementType().Equals(typeOfT))
			{
				
				var list = jsonObject.ToObject(targetType, serializer) as IList;
				
				var tsList = new ThreadSafeList<T>();
				if (list == null) return tsList;
				foreach (var item in list)
				{
					tsList.Add(item as T);
				}
				return tsList;
			}
			else
			{
				throw new JsonSerializationException($"Unexpected type: {targetType}. Expected type: {typeof(ThreadSafeList<>)}");
			}
		}


		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			// Serialize the list normally
			var list = (ThreadSafeList<T>)value;
			serializer.Serialize(writer, list);
		}
	}
}
