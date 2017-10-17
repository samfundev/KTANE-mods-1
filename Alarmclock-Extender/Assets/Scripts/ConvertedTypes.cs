using System;
using System.Reflection;

public static class CommonReflectedTypeInfo
{
    static CommonReflectedTypeInfo()
    {
        AlarmClockType = ReflectionHelper.FindType("Assets.Scripts.Props.AlarmClock");
        MaxBuzzerTimeField = AlarmClockType.GetField("MaxAlarmClockBuzzerTime", BindingFlags.Public | BindingFlags.Instance);
        BuzzerStateField = AlarmClockType.GetField("isOn", BindingFlags.NonPublic | BindingFlags.Instance);
        PlayResultField = AlarmClockType.GetField("alarmClockBuzzerSound", BindingFlags.NonPublic | BindingFlags.Instance);
        TurnOffMethod = AlarmClockType.GetMethod("TurnOff", BindingFlags.Public | BindingFlags.Instance);

        PlaySoundResultType = ReflectionHelper.FindType("DarkTonic.MasterAudio.PlaySoundResult");
        ActingVariationProperty = PlaySoundResultType.GetProperty("ActingVariation", BindingFlags.Public | BindingFlags.Instance);

        SoundGroupVariationType = ReflectionHelper.FindType("DarkTonic.MasterAudio.SoundGroupVariation");
        VarAudioProperty = SoundGroupVariationType.GetProperty("VarAudio", BindingFlags.Public | BindingFlags.Instance);
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

    public static FieldInfo BuzzerStateField
    {
        get;
        private set;
    }

    public static FieldInfo PlayResultField
    {
        get;
        private set;
    }

    public static MethodInfo TurnOffMethod
    {
        get;
        private set;
    }
    #endregion

    #region DarkTonic.MasterAudio
    public static Type PlaySoundResultType
    {
        get;
        private set;
    }

    public static PropertyInfo ActingVariationProperty
    {
        get;
        private set;
    }

    public static Type SoundGroupVariationType
    {
        get;
        private set;
    }

    public static PropertyInfo VarAudioProperty
    {
        get;
        private set;
    }

    #endregion
}
