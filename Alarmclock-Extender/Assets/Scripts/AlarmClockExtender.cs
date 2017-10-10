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
    void Start()
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

    private IEnumerator ShutOffAlarmClock(float time, Object AlarmClock)
    {
        yield return new WaitForSeconds(time);
        CommonReflectedTypeInfo.TurnOffMethod.Invoke(AlarmClock, null);
    }

    private IEnumerator CheckForAlarmClock()
    {
        yield return null;
        DebugLog("Attempting to Find Alarm Clock");
        _modSettings.ReadSettings();
        Object AlarmClock = null;

        while (true)
        {
            UnityEngine.Object[] alarmClock = FindObjectsOfType(CommonReflectedTypeInfo.AlarmClockType);
            if (alarmClock == null || alarmClock.Length == 0)
            {
                yield return null;
                continue;
            }

            foreach (var alarm in alarmClock)
            {
                DebugLog("Alarm Clock found - Hooking into it.");
                CommonReflectedTypeInfo.MaxBuzzerTimeField.SetValue(alarm, _modSettings.Settings.AlarmClockBuzzerTime);
                DebugLog("Done setting up the Alarm Clocks");
                AlarmClock = alarm;
                break;
            }
            break;
        }
        if (AlarmClock == null)
            yield break;
        while (true)
        {
            if ((bool) CommonReflectedTypeInfo.BuzzerStateField.GetValue(AlarmClock))
            {
                float clipTime = -1;
                AudioSource varAudio = null;
                yield return new WaitForSeconds(0.1f);
                try
                {
                    DebugLog("Alarm Clock just turned on. Trying to find out how long the clip length is.");
                    var playingSound = CommonReflectedTypeInfo.PlayResultField.GetValue(AlarmClock);
                    DebugLog("Retrieved the playingSound object: type = {0}", playingSound == null ? "null" : playingSound.GetType().FullName );
                    PropertyInfo soundGroupVariationField = playingSound.GetType().GetProperty("ActingVariation");
                    var soundGroupVariation = soundGroupVariationField.GetValue(playingSound, null);
                    DebugLog("Retrieved the Acting Variation object: type = {0}", soundGroupVariation == null ? "null" : soundGroupVariation.GetType().FullName);
                    PropertyInfo varAudioField = soundGroupVariation.GetType().GetProperty("VarAudio");
                    varAudio = (AudioSource)varAudioField.GetValue(soundGroupVariation, null);
                    DebugLog("Retrieved AudioSource: type = {0}", varAudio == null ? "null" : varAudio.name);
                    if (varAudio != null && varAudio.clip != null)
                    {
                        clipTime = varAudio.clip.length;
                    }
                }
                catch (Exception ex)
                {
                    DebugLog("Failed to get Clip length due to Exception: {0}\n{1}", ex.Message, ex.StackTrace);
                    clipTime = -1;
                }
                if (clipTime > 0)
                {
                    Coroutine shutoff = StartCoroutine(ShutOffAlarmClock(clipTime, AlarmClock));
                    DebugLog("Alarm Clock will play for {0} seconds provided it isn't shut off sooner. Kappa", Mathf.CeilToInt(Mathf.Min(clipTime, _modSettings.Settings.AlarmClockBuzzerTime)));
                    while ((bool) CommonReflectedTypeInfo.BuzzerStateField.GetValue(AlarmClock))
                    {
                        yield return null;
                    }
                    if(shutoff != null)
                        StopCoroutine(shutoff);
                }
                else
                {
                    DebugLog("Alarm Clock will play for {0} seconds provided it isn't shut off sooner. Kappa", Mathf.CeilToInt(_modSettings.Settings.AlarmClockBuzzerTime));
                    while ((bool) CommonReflectedTypeInfo.BuzzerStateField.GetValue(AlarmClock))
                    {
                        yield return null;
                    }
                }
            }
            yield return null;
        }
    }
}
