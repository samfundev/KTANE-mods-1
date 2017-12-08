using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Debug = UnityEngine.Debug;

public static class ReflectionHelper
{
    public static Type FindType(string fullName)
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetSafeTypes(fullName)).FirstOrDefault(t => t.FullName.Equals(fullName));
    }

    private static IEnumerable<Type> GetSafeTypes(this Assembly assembly, string name)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(x => x != null);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            return null;
        }
    }
}
