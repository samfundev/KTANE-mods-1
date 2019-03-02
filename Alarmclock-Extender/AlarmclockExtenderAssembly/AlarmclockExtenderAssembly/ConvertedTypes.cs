using System;
using System.Collections;
using System.Reflection;
using Assets.Scripts.Props;
using UnityEngine;

public static class CommonReflectedTypeInfo
{
    static CommonReflectedTypeInfo()
    {
        AlarmClockType = typeof(AlarmClock);
        //BuzzerStateField = AlarmClockType.GetField("isOn", BindingFlags.NonPublic | BindingFlags.Instance);
        //PlayResultField = AlarmClockType.GetField("alarmClockBuzzerSound", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    #region AlarmClock
    public static Type AlarmClockType
    {
        get;
        private set;
    }


    #endregion

    #region DarkTonic.MasterAudio


    #endregion
}
