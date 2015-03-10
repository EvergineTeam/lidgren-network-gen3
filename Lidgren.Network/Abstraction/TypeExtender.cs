using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace System.Reflection
{
    public static class TypeExtender
    {
        public static MethodInfo[] GetMethods(this Type t, BindingFlags flags)
        {
            if (!flags.HasFlag(BindingFlags.Instance) && !flags.HasFlag(BindingFlags.Static)) return null;
            var allMethods = t.GetRuntimeMethods();
            var resultList = new List<MethodInfo>();
            foreach (var method in allMethods)
            {
                var isValid = (flags.HasFlag(BindingFlags.Public) && method.IsPublic)
                || (flags.HasFlag(BindingFlags.NonPublic) && !method.IsPublic);
                isValid &= (flags.HasFlag(BindingFlags.Static) && method.IsStatic) || (flags.HasFlag(BindingFlags.Instance) && !method.IsStatic);
                if (flags.HasFlag(BindingFlags.DeclaredOnly))
                    isValid &= method.DeclaringType == t;
                if (isValid)
                    resultList.Add(method);
            }
            return resultList.ToArray();
        }

        public static PropertyInfo[] GetProperties(this Type t, BindingFlags flags)
        {
            if (!flags.HasFlag(BindingFlags.Instance) && !flags.HasFlag(BindingFlags.Static)) return null;
            var origProperties = t.GetRuntimeProperties();
            var results = new List<PropertyInfo>();
            foreach (var property in origProperties)
            {
                var isPublic = property.GetMethod.IsPublic && property.SetMethod.IsPublic;
                var isStatic = property.GetMethod.IsStatic && property.SetMethod.IsStatic;

                var isValid = (flags.HasFlag(BindingFlags.Public) && isPublic)
                || (flags.HasFlag(BindingFlags.NonPublic) && !isPublic);
                isValid &= (flags.HasFlag(BindingFlags.Static) && isStatic) || (flags.HasFlag(BindingFlags.Instance) && !isStatic);
                if (flags.HasFlag(BindingFlags.DeclaredOnly))
                    isValid &= property.DeclaringType == t;
                results.Add(property);
            }
            return results.ToArray();
        }

        public static FieldInfo[] GetFields(this Type type)
        {
            return GetFields(type, BindingFlags.Default);
        }
        public static FieldInfo[] GetFields(this Type t, BindingFlags flags)
        {
            if (!flags.HasFlag(BindingFlags.Instance) && !flags.HasFlag(BindingFlags.Static)) return null;
            var origFields = t.GetRuntimeFields();
            var results = new List<FieldInfo>();
            foreach (var field in origFields)
            {
                var isValid = (flags.HasFlag(BindingFlags.Public) && field.IsPublic)
                || (flags.HasFlag(BindingFlags.NonPublic) && !field.IsPublic);
                isValid &= (flags.HasFlag(BindingFlags.Static) && field.IsStatic) || (flags.HasFlag(BindingFlags.Instance) && !field.IsStatic);
                if (flags.HasFlag(BindingFlags.DeclaredOnly))
                    isValid &= field.DeclaringType == t;
                results.Add(field);
            }
            return results.ToArray();
        }
        public static MethodInfo GetMethod(this Type type, string name)
        {
            return GetMethod(type, name, BindingFlags.Default, null);
        }
        public static MethodInfo GetMethod(this Type type, string name, Type[] types)
        {
            return GetMethod(type, name, BindingFlags.Default, types);
        }
        public static MethodInfo GetMethod(this Type t, string name, BindingFlags flags)
        {
            return GetMethod(t, name, flags, null);
        }
        public static MethodInfo GetMethod(Type t, string name, BindingFlags flags, Type[] parameters)
        {
            var method = t.GetRuntimeMethod(name, parameters);

            var isValid = (flags.HasFlag(BindingFlags.Public) && method.IsPublic)
                || (flags.HasFlag(BindingFlags.NonPublic) && !method.IsPublic);
            isValid &= (flags.HasFlag(BindingFlags.Static) && method.IsStatic) || (flags.HasFlag(BindingFlags.Instance) && !method.IsStatic);
            if (flags.HasFlag(BindingFlags.DeclaredOnly))
                isValid &= method.DeclaringType == t;

            if (!isValid)
                method = null;

            return method;
        }
        public static Type[] GetGenericArguments(this Type t)
        {
            var ti = t.GetTypeInfo();
            return ti.GenericTypeArguments;
        }
    }
}
