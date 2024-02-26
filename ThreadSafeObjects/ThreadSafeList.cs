using IoTDBdotNET;
using Newtonsoft.Json;
using System.Collections;


namespace BrickSchema.Net.ThreadSafeObjects
{
    public class ThreadSafeList<T> : IList<T>
    {
		#region Private Variables
		private readonly List<T> _list = new List<T>();
        private readonly object _syncRoot = new object();
        private readonly bool _isTHasRefProperty = false;
		#endregion

		#region Constructors
		public ThreadSafeList()
        {
			// No need to lock here since _list is being initialized and not yet accessible to other threads.
			_list = new List<T>();
            _isTHasRefProperty = IsTHasRefProperty();

		}

		public ThreadSafeList(IEnumerable<T> collection)
        {
            
            _list = new List<T>(collection);
			_isTHasRefProperty = IsTHasRefProperty();
		}

		#endregion

		#region Array
		public T this[int index]
        {
            get
            {
                lock (_syncRoot)
                {
                    return _list[index];
                }
            }
            set
            {
                lock (_syncRoot)
                {
                    if (value == null)
                    {
                        Console.Out.WriteLineAsync($"Cannot add null item. Exit function.");

                    }
                    else
                    {
                        _list[index] = value;
                        AddThreadSafeListRef(value);
                        
                    }
				}
            }
        }
		#endregion

		#region A

		public void AddThreadSafeListRef(T item)
        {
			if (_isTHasRefProperty)
			{
				// Check if the item has a 'Parent' property of type ThreadSafeList<T>
				var parentProperty = item?.GetType().GetProperties()
					.FirstOrDefault(prop => Attribute.IsDefined(prop, typeof(ThreadSafeListRefAttribute)) && prop.PropertyType == typeof(ThreadSafeList<T>));

				if (parentProperty != null)
				{
					// If the 'Parent' property exists, set it to this instance
					parentProperty.SetValue(item, this);
				}
			}
		}

		public void Add(T item)
		{
            if (item == null)
            {
                Console.Out.WriteLineAsync($"Cannot add null item. Exit function.");
                return;
            }
			lock (_syncRoot)
			{
                // First, check if T is a class to avoid unnecessary reflection for value types
                AddThreadSafeListRef(item);
				_list.Add(item);
			}
		}

        public void AddRange(IEnumerable<T> items)
        {
            lock (_syncRoot)
            {
                foreach (var item in items)
                {

                    AddThreadSafeListRef(item);
                    _list.Add(item); // Add the item to the list after setting the parent property
                }

                _list.AddRange(items);
            }
        }
		#endregion

		#region C
		public int Count
        {
            get
            {
                lock (_syncRoot)
                {
                    return _list.Count;
                }
            }
        }

        

        public void Clear()
        {
            lock (_syncRoot)
            {
                _list.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (_syncRoot)
            {
                return _list.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_syncRoot)
            {
                _list.CopyTo(array, arrayIndex);
            }
        }

		#endregion

		#region Conversion

        public string ToJson()
        {
            string json = string.Empty;
            try
            {
                lock (_syncRoot)
                {
                    var settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, Formatting = Newtonsoft.Json.Formatting.Indented };
                    json = JsonConvert.SerializeObject(this, settings);
                }
            } catch { }
            return json;
		}

		#endregion

		#region F
		public T? Find(Predicate<T> match)
        {
            lock (_syncRoot)
            {
                return _list.Find(match);
            }
        }

        public T? FirstOrDefault(Func<T, bool> predicate)
        {
            lock (_syncRoot)
            {
                return _list.FirstOrDefault(predicate);
            }
        }
		#endregion

		#region G
		public IEnumerator<T> GetEnumerator()
        {
            lock (_syncRoot)
            {
                return new List<T>(_list).GetEnumerator();
            }
        }
		#endregion

		#region I
		public bool IsReadOnly => false;

		public int IndexOf(T item)
        {
            lock (_syncRoot)
            {
                return _list.IndexOf(item);
            }
        }

        public void Insert(int index, T item)
        {
            lock (_syncRoot)
            {
                _list.Insert(index, item);
                AddThreadSafeListRef(item);

			}
        }
		#endregion

		#region R
		public bool Remove(T item)
        {
            lock (_syncRoot)
            {
                return _list.Remove(item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (_syncRoot)
            {
                _list.RemoveAt(index);
            }
        }
        #endregion

        #region T
        public ThreadSafeList<T> ToThreadSafeList()
        {
            lock (_syncRoot)
            {
                // Directly use the locked list to create a new ThreadSafeList<T>.
                // This assumes you have a constructor that accepts IEnumerable<T>.
                return new ThreadSafeList<T>(_list);
            }
        }

        #endregion

        #region W
        public IEnumerable<T> Where(Func<T, bool> predicate)
        {
            lock (_syncRoot)
            {
                var items = _list.Where(predicate).ToArray();
                var newList = new ThreadSafeList<T>(items);
                return newList;
            }
        }
        #endregion

        #region Private Functions
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


		private bool IsTHasRefProperty()
		{
			if (!typeof(T).IsClass)
			{
				return false;
			}
			// Get the type object of T
			Type typeOfT = typeof(T);

			// Iterate over all public properties of T
			var refProperty = typeOfT.GetProperties()
				.FirstOrDefault(prop =>
					// Check if the property is marked with the ThreadSafeListRefAttribute
					Attribute.IsDefined(prop, typeof(ThreadSafeListRefAttribute)));
			if (refProperty == null)
			{
				return false;
			}

			if (!IsPropertyOfTypeThreadSafeListT(refProperty.PropertyType, typeOfT))
			{
				throw new InvalidCastException($"ThreadSafeListRef property has invalid data type. Cannot cast {refProperty.PropertyType} to {typeOfT}");
			}
			return true;
		}

		private bool IsPropertyOfTypeThreadSafeListT(Type propertyType, Type targetType)
		{
			// Check if the propertyType is a generic type and if it is a ThreadSafeList<>
			if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(ThreadSafeList<>))
			{
				// Get the generic argument of the property type
				Type genericArgument = propertyType.GetGenericArguments()[0];

				// Check if the generic argument matches T
				return genericArgument == targetType;
			}
			return false;
		}

		#endregion
	}
}
