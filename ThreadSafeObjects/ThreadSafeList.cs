using BrickSchema.Net.Classes.Equipments.HVACType;
using IoTDBdotNET;
using Newtonsoft.Json;
using System.Collections;
using System.Reflection;

namespace BrickSchema.Net.ThreadSafeObjects
{
    public class ThreadSafeList<T> : IList<T>
    {
		#region Private Variables
		private readonly List<T> _list = new List<T>();
        private readonly object _syncRoot = new object();
        private PropertyInfo? _refPropertyInfo = null;
        private PropertyInfo? _idPropertyInfo = null;

        #endregion

        #region Constructors
        public ThreadSafeList()
        {
			// No need to lock here since _list is being initialized and not yet accessible to other threads.
			_list = new List<T>();
            InitTRefProperty();
            InitTIdProperty();

		}

        public ThreadSafeList(IEnumerable<T> collection)
        {
            
            _list = new();
            InitTRefProperty();
            InitTIdProperty();

            foreach (var col in collection)
            {
                Add(Clone(col));
            }

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
                        AddThreadSafeListRef(value);
                        
                            _list[index] = value;
                            
                        
                        
                    }
				}
            }
        }

        public T? this[long id]
        {
            get
            {
                lock (_syncRoot)
                {
                    if (_idPropertyInfo == null) throw new NullReferenceException("ThreadSafeListId reference is null");
                    if (_idPropertyInfo.PropertyType != typeof(long))
                        throw new InvalidCastException("Invalid Id data type. ThreadSafeListId property valid data types: long, string");

                    return FindItemById(id);
                }
            }
            set
            {
                lock (_syncRoot)
                {
                    if (value == null)
                    {
                        Console.Out.WriteLineAsync("Cannot add null item. Exit function.");
                        return;
                    }

                    AddThreadSafeListRef(value);
                    var index = FindIndexById(id);
                    if (index >= 0) _list[index] = value;
                }
            }
        }

        public T? this[string id]
        {
            get
            {
                lock (_syncRoot)
                {
                    if (_idPropertyInfo == null) throw new NullReferenceException("ThreadSafeListId reference is null");
                    if (_idPropertyInfo.PropertyType != typeof(string))
                        throw new InvalidCastException("Invalid Id data type. ThreadSafeListId property valid data types: long, string");

                    return FindItemById(id);
                }
            }
            set
            {
                lock (_syncRoot)
                {
                    if (value == null)
                    {
                        Console.Out.WriteLineAsync("Cannot add null item. Exit function.");
                        return;
                    }

                    AddThreadSafeListRef(value);
                    var index = FindIndexById(id);
                    if (index >= 0) _list[index] = value;
                }
            }
        }
        #endregion

        #region A

        public void AddThreadSafeListRef(T item)
        {
            if (item == null) return;
            if (_refPropertyInfo!= null)
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

        public void AddNew(T item)
        {
            Add(Clone(item));
        }
		public void AddRaw(T item)
		{
			if (item == null)
			{
				Console.Out.WriteLineAsync($"Cannot add null item. Exit function.");
				return;
			}
			lock (_syncRoot)
			{
                //Do not AddThreadSafe list ref

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
                }

                _list.AddRange(items.Where(i=>i != null));
            }
        }

        public void AddRangeNew(IEnumerable<T> items)
        {
            AddRange(Clone(items)); 
        }

		public void AddRangeRaw(IEnumerable<T> items)
		{
			lock (_syncRoot)
			{
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

        public IEnumerable<T> Clone (IEnumerable<T> items)
        {
            List<T> list = new List<T>();
            foreach (var item in items) { list.Add(Clone(item)); }
            return list;
        }
        public T Clone(T item)
        {
            // Check if T is a class
            if (typeof(T).IsClass)
            {
                // Try to find a "Clone" method on T
                var cloneMethod = typeof(T).GetMethod("Clone", Type.EmptyTypes);

                if (cloneMethod != null && cloneMethod.ReturnType == typeof(T))
                {
                    // If a "Clone" method exists and returns the correct type, invoke it
                    var newItem = cloneMethod.Invoke(item, null);
                    if (newItem == null) return item;
                    return (T)newItem;
                }
            }

            // If T is not a class, does not have a "Clone" method, or the "Clone" method does not return the correct type,
            // return the original item
            return item;
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
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array), "Destination array cannot be null.");
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Array index must be non-negative.");
            }

            if (array.Length - arrayIndex < Count)
            {
                throw new ArgumentException("The number of elements in the ThreadSafeList is greater than the available space from arrayIndex to the end of the destination array.");
            }

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
                var item = _list.Find(match);
                if (item == null) return default(T?);
                return Clone(item);
            }
        }

        public U? Find<U>(Predicate<T> match) where U : class, T
        {
            lock (_syncRoot)
            {
                var item = _list.Find(match);
                if (item == null) return default(U?);
                return Clone(item) as U;
            }
        }

        public U? Find<U>() where U : class, T
        {
            lock (_syncRoot)
            {
                var item = Find<U>(x=>x is U);
                return item;
            }
        }

        public T? FirstOrDefault(Func<T, bool> predicate)
        {
            lock (_syncRoot)
            {
                var item = _list.FirstOrDefault(predicate);
                if (item == null) return default(T?);
                return Clone(item);
            }
        }
        public U? FirstOrDefault<U>(Func<T, bool> predicate) where U : class, T
        {
            lock (_syncRoot)
            {
                var item = _list.FirstOrDefault(predicate);
                if (item == null) return default(U?);
                return Clone(item) as U;
            }
        }

        public U? FirstOrDefault<U>() where U : class, T
        {
            lock (_syncRoot)
            {
                var item = FirstOrDefault<U>(x=>x is U);
                return item;
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

        private void InitTIdProperty()
        {
            _idPropertyInfo = null;
            if (!typeof(T).IsClass)
            {
                return;
            }

            // Get the type object of T
            Type typeOfT = typeof(T);

            // Iterate over all public properties of T
            var idProperty = typeOfT.GetProperties()
                .FirstOrDefault(prop =>
                    // Check if the property is marked with the ThreadSafeListIdAttribute
                    Attribute.IsDefined(prop, typeof(ThreadSafeListIdAttribute)));

            if (idProperty == null)
            {
                return;
            }


            // If not string, or long
            if (idProperty.PropertyType != typeof(string) && idProperty.PropertyType != typeof(long))
            {
                //Have id but wrong data type.
                throw new InvalidCastException($"ThreadSafeListId property has invalid data type. Valid data types: string or long");
            }
            _idPropertyInfo = idProperty;
        }


        private void InitTRefProperty()
		{
            _refPropertyInfo = null;

            if (!typeof(T).IsClass)
			{
				return;
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
				return;
			}

			if (!IsPropertyOfTypeThreadSafeListT(refProperty.PropertyType, typeOfT))
			{
				throw new InvalidCastException($"ThreadSafeListRef property has invalid data type. Cannot cast {refProperty.PropertyType} to {typeOfT}");
			}
            _refPropertyInfo = refProperty;

        }

		private bool IsPropertyOfTypeThreadSafeListT(Type propertyType, Type targetType)
		{
			// Check if the propertyType is a generic type and if it is a ThreadSafeList<>
			if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(ThreadSafeList<>))
			{
				// Get the generic argument of the property type
				Type genericArgument = propertyType.GetGenericArguments()[0];
				if (genericArgument.IsAssignableFrom(targetType))
				{
					return true;
				}
				// Check if the generic argument matches T
				return genericArgument == targetType;
			}
			return false;
		}

        private int FindIndexById(object id)
        {
            return _list.FindIndex(x =>
            {
                var propertyValue = _idPropertyInfo?.GetValue(x);
                return Equals(propertyValue, id);
            });
        }

        private T? FindItemById(object id)
        {
            return _list.FirstOrDefault(x =>
            {
                var propertyValue = _idPropertyInfo?.GetValue(x);
                return Equals(propertyValue, id);
            });
        }
        #endregion
    }
}
