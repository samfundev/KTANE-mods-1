using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ReflectionHelper
{
    public static Type FindType(string fullName)
    {
        return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).FirstOrDefault(t => t.FullName.Equals(fullName));
    }
}
