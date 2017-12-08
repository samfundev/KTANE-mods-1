using System;
using System.Reflection;

public static class CommonReflectedTypeInfo
{
    static CommonReflectedTypeInfo()
    {
        PlayerSettingManagerType = ReflectionHelper.FindType("Assets.Scripts.Settings.PlayerSettingsManager");
        if (PlayerSettingManagerType != null)
        {
            PlayerSettingsManagerInstanceField = PlayerSettingManagerType.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
            PlayerSettingsManagerPlayerSettingsField = PlayerSettingManagerType.GetField("playerSettings", BindingFlags.NonPublic | BindingFlags.Instance);
            PlayerSettingManagerSaveSettingsMethod = PlayerSettingManagerType.GetMethod("SavePlayerSettings", BindingFlags.Public | BindingFlags.Instance);
        }

        PlayerSettingsType = ReflectionHelper.FindType("Assets.Scripts.Settings.PlayerSettings");
        if (PlayerSettingsType != null)
        {
            PlayerSettingLockMouseField = PlayerSettingsType.GetField("LockMouseToWindow", BindingFlags.Public | BindingFlags.Instance);
        }
    }

    public static Type PlayerSettingManagerType
    {
        get;
        private set;
    }

    public static FieldInfo PlayerSettingsManagerInstanceField
    {
        get;
        private set;
    }

    public static FieldInfo PlayerSettingsManagerPlayerSettingsField
    {
        get;
        private set;
    }

    public static MethodInfo PlayerSettingManagerSaveSettingsMethod
    {
        get;
        private set;
    }

    public static Type PlayerSettingsType
    {
        get;
        private set;
    }

    public static FieldInfo PlayerSettingLockMouseField
    {
        get;
        private set;
    }
}
