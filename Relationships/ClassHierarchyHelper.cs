using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BrickSchema.Net.Relationships
{
    public static class ClassHierarchyHelper
    {
        public static Dictionary<string, List<string>> GetClassHierarchy(string? namespaceName = null)
        {
            // If no namespace is specified, use the namespace of this class as the default
            if (string.IsNullOrEmpty(namespaceName))
            {
                namespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            }
            return BrickSchema.Net.ClassHierarchyHelper.GetClassHierarchy(namespaceName);
        }

        public static string GetClassUri(string name, string? namespaceName = null)
        {
            if (string.IsNullOrEmpty(namespaceName))
            {
                namespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            }
            return BrickSchema.Net.ClassHierarchyHelper.GetClassUri(name, namespaceName);
        }
    }
}
