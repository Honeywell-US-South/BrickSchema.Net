using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.ThreadSafeObjects
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ThreadSafeListRefAttribute : Attribute
    {
    }
}
