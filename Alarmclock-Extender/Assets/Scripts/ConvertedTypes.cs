using System;
using System.Reflection;

public static class CommonReflectedTypeInfo
{
    static CommonReflectedTypeInfo()
    {
        AlarmClockType = ReflectionHelper.FindType("Assets.Scripts.Props.AlarmClock");
        MaxBuzzerTimeField = AlarmClockType.GetField("MaxAlarmClockBuzzerTime", BindingFlags.Public | BindingFlags.Instance);
    }

    #region AlarmClock
    public static Type AlarmClockType
    {
        get;
        private set;
    }

    public static FieldInfo MaxBuzzerTimeField
    {
        get;
        private set;
    }
    #endregion
}
