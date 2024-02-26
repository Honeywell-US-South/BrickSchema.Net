using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.ThreadSafeObjects
{
    public static class ThreadSafeListExtensions
    {
        
        public static ThreadSafeList<T> ToThreadSafeList<T>(this IEnumerable<T> source)
        {
            
            return new ThreadSafeList<T>(source);
        }
    }
}
