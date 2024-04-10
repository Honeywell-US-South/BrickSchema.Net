﻿using System;
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
                if (type != null & type.BaseType != null) {
                    if (allTypes.Contains(type.BaseType) || (type.FullName.StartsWith(namespaceName) && type.BaseType.Name.StartsWith("Brick")))
                    {
                        string baseTypeName = type.BaseType.Name;

                        if (!classHierarchies.ContainsKey(baseTypeName))
                        {
                            classHierarchies[baseTypeName] = new List<string>();
                        }

                        classHierarchies[baseTypeName].Add(type.Name);
                    }
                }
            }

            return classHierarchies;
        }

        public static string GetClassUri(string name, string? namespaceName = null)
        {
            var classHierarchies = GetClassHierarchy(namespaceName);
            // Temporary dictionary to hold reversed mappings for quick lookup.
            return GetClassUri(name, classHierarchies);
        }

        public static string GetClassUri(string name, Dictionary<string, List<string>> classHierarchies)
        {
            
            // Temporary dictionary to hold reversed mappings for quick lookup.
            Dictionary<string, string> childToParentMap = new Dictionary<string, string>();

            // Populate the childToParentMap for quick reverse lookup.
            foreach (var parent in classHierarchies)
            {
                foreach (var child in parent.Value)
                {
                    if (!childToParentMap.ContainsKey(child))
                    {
                        childToParentMap[child] = parent.Key;
                    }
                }
            }

            // Starting with the target name, build the path backwards.
            string currentName = name;
            List<string> path = new List<string>();
            while (childToParentMap.ContainsKey(currentName))
            {
                path.Insert(0, currentName); // Insert at the beginning to build the path backwards.
                currentName = childToParentMap[currentName]; // Move up to the parent.
            }

            // Add the top-level parent if it's not already part of the path.
            if (!path.Contains(currentName) && classHierarchies.ContainsKey(currentName))
            {
                path.Insert(0, currentName);
            }

            return string.Join(".", path); // Join the path components with dots.
        }
    }
}
