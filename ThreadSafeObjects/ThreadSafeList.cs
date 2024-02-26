using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BrickSchema.Net.ThreadSafeObjects
{
    public class ThreadSafeList<T> : IList<T>
    {
        private readonly List<T> _list = new List<T>();
        private readonly object _syncRoot = new object();

        public ThreadSafeList()
        {
            _list = new List<T>();
        }

        public ThreadSafeList(IEnumerable<T> collection)
        {
            // No need to lock here since _list is being initialized and not yet accessible to other threads.
            _list = new List<T>(collection);
        }

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
                    _list[index] = value;
                }
            }
        }

        public bool IsReadOnly => false;

        #region A
        public void Add(T item)
        {
            lock (_syncRoot)
            {
                _list.Add(item);
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            lock (_syncRoot)
            {
                _list.AddRange(items);
            }
        }

        #endregion

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

        public IEnumerator<T> GetEnumerator()
        {
            lock (_syncRoot)
            {
                return new List<T>(_list).GetEnumerator();
            }
        }

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
            }
        }

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

        #region IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
