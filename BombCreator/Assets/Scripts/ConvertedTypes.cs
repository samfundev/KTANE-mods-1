using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class CommonReflectedTypeInfo
{
    static CommonReflectedTypeInfo()
    {
        ModManagerType = ReflectionHelper.FindType("ModManager");
        if (ModManagerType != null)
        {
            ModManagerInstanceField = ModManagerType.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
            ModManagerGetMaxFrontFaceMethod = ModManagerType.GetMethod("GetMaximumModulesFrontFace", BindingFlags.Public | BindingFlags.Instance);
        }
        else
        {
            DebugLog("Failed to reflect ModManager");
        }
    }

    public static void DebugLog(string message, params object[] args)
    {
        var debugstring = string.Format("[BombCreator] {0}", message);
        Debug.LogFormat(debugstring, args);
    }

    public static int GetMaximumFrontFace()
    {
        if (ModManagerType == null || ModManagerInstanceField == null || ModManagerGetMaxFrontFaceMethod == null)
            return -1;
        var instance = ModManagerInstanceField.GetValue(null);
        if (instance == null)
            return -1;
        try
        {
            return (int) ModManagerGetMaxFrontFaceMethod.Invoke(instance, null);
        }
        catch (Exception ex)
        {
            DebugLog("Failed to Get Maximum Front Face count due to Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
            return -1;
        }
    }
    

    

    #region ModManager
    

    public static Type ModManagerType
    {
        get;
        private set;
    }

    public static FieldInfo ModManagerInstanceField
    {
        get;
        private set;
    }

    public static MethodInfo ModManagerGetMaxFrontFaceMethod
    {
        get;
        private set;
    }

    #endregion
    
}
