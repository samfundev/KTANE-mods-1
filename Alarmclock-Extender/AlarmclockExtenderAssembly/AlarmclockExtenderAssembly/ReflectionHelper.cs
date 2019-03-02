using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;


[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UseStringInterpolation")]
// ReSharper disable once CheckNamespace
public static class ReflectionHelper
{
    private static void DebugLog(string message, params object[] args)
    {
        AlarmclockExtenderAssembly.ModSettings.DebugLog(message, args);
    }

    public static Type FindType(string fullName)
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetSafeTypes()).FirstOrDefault(t => t.FullName != null && t.FullName.Equals(fullName));
    }

    public static Type FindType(string fullName, string assemblyName)
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetSafeTypes()).FirstOrDefault(t => t.FullName != null && t.FullName.Equals(fullName) && t.Assembly.GetName().Name.Equals(assemblyName));
    }

    public static IEnumerable<Type> GetSafeTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(x => x != null);
        }
        catch (Exception)
        {
            return new List<Type>();
        }
    }

    public sealed class FieldInfo<T>
    {
        private readonly object _target;
        public readonly FieldInfo Field;
        public readonly string FieldFullName;

        public FieldInfo(object target, FieldInfo field)
        {
            _target = target;
            Field = field;
            FieldFullName = field.DeclaringType == null ? "<Null>" : field.DeclaringType.FullName;
        }

        public T Get(bool nullAllowed = false)
        {
            var t = (T)Field.GetValue(_target);
            if (!nullAllowed && t == null)
                DebugLog("Field {1}.{0} is null.", Field.Name, FieldFullName);
            return t;
        }

        public T GetFrom(object obj, bool nullAllowed = false)
        {
            var t = (T)Field.GetValue(obj);
            if (!nullAllowed && t == null)
                DebugLog("Field {1}.{0} is null.", Field.Name, FieldFullName);
            return t;
        }

        public void Set(T value) { Field.SetValue(_target, value); }
    }

    public sealed class MethodInfo<T>
    {
        private readonly object _target;
        public MethodInfo Method { get; }

        public MethodInfo(object target, MethodInfo method)
        {
            _target = target;
            Method = method;
        }

        public T Invoke(params object[] arguments)
        {
            return (T)Method.Invoke(_target, arguments);
        }
    }

    public sealed class PropertyInfo<T>
    {
        private readonly object _target;
        public readonly PropertyInfo Property;
        public readonly string PropertyFullName;
        public bool Error { get; private set; }

        public PropertyInfo(object target, PropertyInfo property)
        {
            _target = target;
            Property = property;
            PropertyFullName = property.DeclaringType == null ? "<Null>" : property.DeclaringType.FullName;
            Error = false;
        }

        public T Get(bool nullAllowed = false)
        {
            // “This value should be null for non-indexed properties.” (MSDN)
            return Get(null, nullAllowed);
        }

        public T Get(object[] index, bool nullAllowed = false)
        {
            try
            {
                var t = (T)Property.GetValue(_target, index);
                if (!nullAllowed && t == null)
                    DebugLog("Property {1}.{0} is null.", Property.Name, PropertyFullName);
                Error = false;
                return t;
            }
            catch (Exception e)
            {
                DebugLog("Property {1}.{0} could not be fetched with the specified parameters. Exception: {2}\n{3}", Property.Name, PropertyFullName, e.GetType().FullName, e.StackTrace);
                Error = true;
                return default(T);
            }
        }

        public void Set(T value, object[] index = null)
        {
            try
            {
                Property.SetValue(_target, value, index);
                Error = false;
            }
            catch (Exception e)
            {
                DebugLog("Property {1}.{0} could not be set with the specified parameters. Exception: {2}\n{3}", Property.Name, PropertyFullName, e.GetType().FullName, e.StackTrace);
                Error = true;
            }
        }
    }

    public static FieldInfo<T> GetField<T>(object target, string name, bool isPublic = false)
    {
        if (target != null)
            return GetFieldImpl<T>(target, target.GetType(), name, isPublic, BindingFlags.Instance);

        DebugLog("Attempt to get {1} field {0} of type {2} from a null object.", name, isPublic ? "public" : "non-public", typeof(T).FullName);
        return null;
    }

    public static FieldInfo<T> GetStaticField<T>(Type targetType, string name, bool isPublic = false)
    {
        if (targetType != null)
            return GetFieldImpl<T>(null, targetType, name, isPublic, BindingFlags.Static);

        DebugLog("Attempt to get {0} static field {1} of type {2} from a null type.", isPublic ? "public" : "non-public", name, typeof(T).FullName);
        return null;
    }

    public static FieldInfo<T> GetFieldImpl<T>(object target, Type targetType, string name, bool isPublic, BindingFlags bindingFlags)
    {
        var fld = targetType.GetField(name, (isPublic ? BindingFlags.Public : BindingFlags.NonPublic) | bindingFlags);
        if (fld == null)
        {
            // In case it’s actually an auto-implemented property and not a field.
            fld = targetType.GetField("<" + name + ">k__BackingField", BindingFlags.NonPublic | bindingFlags);
            if (fld == null)
            {
                DebugLog("Type {0} does not contain {1} field {2}. Fields are: {3}", targetType, isPublic ? "public" : "non-public", name,
                    targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Select(f => string.Format("{0} {1} {2}", f.IsPublic ? "public" : "private", f.FieldType.FullName, f.Name)).JoinString(", "));
                return null;
            }
        }

        if (typeof(T).IsAssignableFrom(fld.FieldType))
            return new FieldInfo<T>(target, fld);

        DebugLog("Type {0} has {1} field {2} of type {3} but expected type {4}.", targetType, isPublic ? "public" : "non-public", name, fld.FieldType.FullName, typeof(T).FullName);
        return null;
    }

    public static MethodInfo<T> GetMethod<T>(object target, string name, int numParameters, bool isPublic = false)
    {
        if (target == null)
        {
            DebugLog("Attempt to get {1} method {0} of return type {2} from a null object.", name, isPublic ? "public" : "non-public", typeof(T).FullName);
            return null;
        }
        var bindingFlags = (isPublic ? BindingFlags.Public : BindingFlags.NonPublic) | BindingFlags.Instance;
        var targetType = target.GetType();
        var mths = targetType.GetMethods(bindingFlags).Where(m => m.Name == name && m.GetParameters().Length == numParameters && typeof(T).IsAssignableFrom(m.ReturnType)).Take(2).ToArray();
        switch (mths.Length)
        {
            case 0:
                DebugLog("Type {0} does not contain {1} method {2} with return type {3} and {4} parameters.", targetType, isPublic ? "public" : "non-public", name, typeof(T).FullName, numParameters);
                return null;
            case 1:
                return new MethodInfo<T>(target, mths[0]);
            default:
                DebugLog("Type {0} contains multiple {1} methods {2} with return type {3} and {4} parameters.", targetType, isPublic ? "public" : "non-public", name, typeof(T).FullName, numParameters);
                return null;
        }
    }

    public static PropertyInfo<T> GetProperty<T>(object target, string name, bool isPublic = false)
    {
        if (target != null)
            return GetPropertyImpl<T>(target, target.GetType(), name, isPublic, BindingFlags.Instance);

        DebugLog("Attempt to get {1} property {0} of type {2} from a null object.", name, isPublic ? "public" : "non-public", typeof(T).FullName);
        return null;
    }

    public static PropertyInfo<T> GetStaticProperty<T>(Type targetType, string name, bool isPublic = false)
    {
        if (targetType != null)
            return GetPropertyImpl<T>(null, targetType, name, isPublic, BindingFlags.Static);

        DebugLog("Attempt to get {0} static property {1} of type {2} from a null type.", isPublic ? "public" : "non-public", name, typeof(T).FullName);
        return null;
    }

    public static PropertyInfo<T> GetPropertyImpl<T>(object target, Type targetType, string name, bool isPublic, BindingFlags bindingFlags)
    {
        var fld = targetType.GetProperty(name, (isPublic ? BindingFlags.Public : BindingFlags.NonPublic) | bindingFlags);
        if (fld == null)
        {
            DebugLog("Type {0} does not contain {1} property {2}. Properties are: {3}", targetType, isPublic ? "public" : "non-public", name,
                targetType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Select(f => string.Format("{0} {1} {2}", f.GetGetMethod().IsPublic ? "public" : "private", f.PropertyType.FullName, f.Name)).JoinString(", "));
            return null;
        }

        if (typeof(T).IsAssignableFrom(fld.PropertyType))
            return new PropertyInfo<T>(target, fld);

        DebugLog("Type {0} has {1} field {2} of type {3} but expected type {4}.", targetType, isPublic ? "public" : "non-public", name, fld.PropertyType.FullName, typeof(T).FullName);
        return null;
    }
}


