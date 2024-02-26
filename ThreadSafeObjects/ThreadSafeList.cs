using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.ThreadSafeObjects
{
    public class ThreadSafeList<T> : IList<T>
    {
        private readonly List<T> _list = new List<T>();
        private readonly object _syncRoot = new object();

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

        public bool IsReadOnly => false;

        public void Add(T item)
        {
            lock (_syncRoot)
            {
                _list.Add(item);
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
