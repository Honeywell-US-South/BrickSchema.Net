using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BrickSchema.Net
{
    public static class ClassHierarchyHelper
    {
        // Gets the names of all classes and their derived classes within the assembly, optionally filtered by namespace
        public static Dictionary<string, List<string>> GetClassHierarchy(string? namespaceName = null)
        {
            // If no namespace is specified, use the namespace of this class as the default
            if (string.IsNullOrEmpty(namespaceName))
            {
                namespaceName = MethodBase.GetCurrentMethod().DeclaringType.Namespace;
            }

            Dictionary<string, List<string>> classHierarchies = new Dictionary<string, List<string>>();
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Filter types based on namespace if specified, otherwise include all types
            var allTypes = string.IsNullOrEmpty(namespaceName) ?
                            assembly.GetTypes() :
                            assembly.GetTypes().Where(t => t.Namespace != null && t.Namespace.StartsWith(namespaceName));

            foreach (var type in allTypes)
            {
                if (type.BaseType != null && allTypes.Contains(type.BaseType))
                {
                    string baseTypeName = type.BaseType.Name;

                    if (!classHierarchies.ContainsKey(baseTypeName))
                    {
                        classHierarchies[baseTypeName] = new List<string>();
                    }

                    classHierarchies[baseTypeName].Add(type.Name);
                }
            }

            return classHierarchies;
        }
    }
}
