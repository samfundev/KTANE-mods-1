using System;
using UnityEngine;
using AlarmClockExtenderAssembly;

public class AlarmClockExtender : MonoBehaviour
{
    private KMGameInfo _gameInfo;
    private readonly AlarmclockExtenderAssembly.ModSettings _modSettings = new AlarmclockExtenderAssembly.ModSettings("AlarmClockExtender");
    private AlarmClockHandler _alarmClockHandler;

    // Use this for initialization
    private void Start()
    {
        AlarmClockHandler.DebugLog("Starting service");
        _alarmClockHandler = new AlarmClockHandler(_modSettings);
        var failure = false;
        try
        {
            if (CommonReflectedTypeInfo.AlarmClockType == null)
            {
                failure = true;
            }
        }
        catch (Exception ex)
        {
            AlarmClockHandler.DebugLog("Failed due to Exception: {0} - Stack Trace: {1}", ex.Message, ex.StackTrace);
            failure = true;
        }
        if (failure)
        {
            AlarmClockHandler.DebugLog("The reflection component of Alarm Clock Extender failed. Aborting the load");
            return;
        }
        _gameInfo = GetComponent<KMGameInfo>();
        _gameInfo.OnStateChange += OnStateChange;
        _modSettings.ReadSettings();
        StartCoroutine(_alarmClockHandler.KeepTrackQueueFull());
    }

    
    void OnStateChange(KMGameInfo.State state)
    {
        AlarmClockHandler.DebugLog("Current state = {0}", state.ToString());
        _alarmClockHandler.GameState = state;
        if (state == KMGameInfo.State.Gameplay)
        {
            StartCoroutine(_alarmClockHandler.CheckForAlarmClock());
        }
    }
}
