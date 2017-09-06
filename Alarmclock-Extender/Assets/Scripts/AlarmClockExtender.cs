using System;
using UnityEngine;
using System.Collections;
using System.Reflection;
using Object = UnityEngine.Object;

public class AlarmClockExtender : MonoBehaviour
{
    private KMGameInfo _gameInfo = null;
    private KMGameInfo.State _state;
    private ModSettings _modSettings = new ModSettings("AlarmClockExtender");

    public static void DebugLog(string message, params object[] args)
    {
        var debugstring = string.Format("[Alarm Clock Extender] {0}", message);
        Debug.LogFormat(debugstring, args);
    }

    // Use this for initialization
    void Start ()
	{
        DebugLog("Starting service");
	    _gameInfo = GetComponent<KMGameInfo>();
	    _gameInfo.OnStateChange += OnStateChange;
	    _modSettings.ReadSettings();
	}

    void OnStateChange(KMGameInfo.State state)
    {
        DebugLog("Current state = {0}", state.ToString());
        StopAllCoroutines();
        if (state == KMGameInfo.State.Gameplay)
        {

            StartCoroutine(CheckForAlarmClock());
        }
        else
        {
            StopAllCoroutines();
        }
    }

    private IEnumerator CheckForAlarmClock()
    {
        yield return null;
        DebugLog("Attempting to Find Alarm Clock");
        _modSettings.ReadSettings();

        while (true)
        {
            UnityEngine.Object[] alarmClock = FindObjectsOfType(CommonReflectedTypeInfo.AlarmClockType);
            if (alarmClock == null || alarmClock.Length == 0)
            {
                yield return null;
                continue;
            }

            foreach(var alarm in alarmClock) { 
                DebugLog("Alarm Clock found - Hooking into it.");
                CommonReflectedTypeInfo.MaxBuzzerTimeField.SetValue(alarm, _modSettings.Settings.AlarmClockBuzzerTime);
                DebugLog("Done setting up the Alarm Clocks");
                yield break;
            }
        }
    }
}
