using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


public class AlarmClockExtender : MonoBehaviour
{
    private KMGameInfo _gameInfo = null;
    private KMGameInfo.State _state;
    private ModSettings _modSettings = new ModSettings("AlarmClockExtender");


    public class AudioClips
    {
        public AudioClip clip;
        public bool IsVanilla;

        public AudioClips(AudioClip Clip, bool Vanilla = false)
        {
            clip = Clip;
            IsVanilla = Vanilla;
        }
    }

    private Queue<AudioClips> newClipQueue = new Queue<AudioClips>();
    private AudioClip originalClip = null;

    private GetAvailableTracks AvailableTracks = new GetAvailableTracks();

    public static void DebugLog(string message, params object[] args)
    {
        
        var debugstring = string.Format("[Alarm Clock Extender] {0}", message);
        Debug.LogFormat(debugstring, args);
    }

    // Use this for initialization
    void Start()
    {
        DebugLog("Starting service");
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
            DebugLog("Failed due to Exception: {0} - Stack Trace: {1}", ex.Message, ex.StackTrace);
            failure = true;
        }
        if (failure)
        {
            DebugLog("The reflection component of Alarm Clock Extender failed. Aborting the load");
            return;
        }
        _gameInfo = GetComponent<KMGameInfo>();
        _gameInfo.OnStateChange += OnStateChange;
        _modSettings.ReadSettings();
        StartCoroutine(KeepTrackQueueFull());
    }

    private KMGameInfo.State _gamestate;
    void OnStateChange(KMGameInfo.State state)
    {
        DebugLog("Current state = {0}", state.ToString());
        _gamestate = state;
        if (state == KMGameInfo.State.Gameplay)
        {
            StartCoroutine(CheckForAlarmClock());
        }
    }

    IEnumerator MakeClip(string path)
    {
        DebugLog("Attempting to make a clip from {0}", path);
        MakeAudioClip fileclip = new MakeAudioClip();
        fileclip.FilePath = path;
        fileclip.Start();
        while (!fileclip.IsDone)
            yield return null;
        if (fileclip.Ex != null)
        {
            DebugLog("Failed to load sound sound file {0} due to Exception: {1}\nStack Trace {2}", path, fileclip.Ex.Source, fileclip.Ex.StackTrace);
            yield break;
        }

        if (fileclip.clip)
        {
            AudioClip clip = fileclip.clip;
            clip.name = Path.GetFileName(path);
            DebugLog("Loaded clip {0}", clip.name);
            newClipQueue.Enqueue(new AudioClips(clip));
        }
        else
        {
            DebugLog("Failed to load clip {0}", Path.GetFileName(path));
            AvailableTracks.IgnoredTracks.Add(path);
            AvailableTracks.AudioFiles.Remove(path);
        }
    }

    private IEnumerator _newTrack;
    private IEnumerator KeepTrackQueueFull()
    {
        Random.InitState((int)Time.time);
        if (_newTrack != null)
        {
            while (_newTrack.MoveNext())
                yield return _newTrack.Current;
            _newTrack = null;
        }

        _newTrack = PickNextTrack(_modSettings.Settings.RescanDirectory);
        while (_newTrack.MoveNext())
            yield return _newTrack.Current;
        _newTrack = null;

        while (true)
        {
            yield return null;

            if (newClipQueue.Count < 5)
            {
                _newTrack = PickNextTrack();
                while (_newTrack.MoveNext())
                    yield return _newTrack.Current;
                _newTrack = null;
            }
        }
    }

    private IEnumerator PickNextTrack(bool getTracks = false)
    {
        DebugLog("Picking a new Clip to play");

        if (AvailableTracks.AudioFiles.Count == 0 || getTracks)
        {
            AvailableTracks.FilePath = _modSettings.Settings.SoundFileDirectory;
            AvailableTracks.Start();
            while (!AvailableTracks.Update())
                yield return null;
        }
        if (AvailableTracks.AudioFiles.Count == 0)
        {
            newClipQueue.Enqueue(new AudioClips(null, true));
            DebugLog("No files found to load. Forced to use the Vanilla alarm clock sound instead. :(");
            yield break;
        }
        while (AvailableTracks.AudioFiles.Count > 0 && newClipQueue.Count < 5)
        {
            int nexttrack = Random.Range(0, AvailableTracks.AudioFiles.Count);
            IEnumerator makeclip = MakeClip(AvailableTracks.AudioFiles[nexttrack]);
            while (makeclip.MoveNext())
                yield return makeclip.Current;
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
        _modSettings.ReadSettings();
        MonoBehaviour AlarmClock = null;
        DebugLog("Attempting to Find Alarm Clock");
        while (_gamestate == KMGameInfo.State.Gameplay)
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
                AlarmClock = (MonoBehaviour)alarm;
                break;
            }
            break;
        }
        if (AlarmClock == null)
            yield break;
        while (_gamestate == KMGameInfo.State.Gameplay)
        {
            if ((bool) CommonReflectedTypeInfo.BuzzerStateField.GetValue(AlarmClock))
            {
                bool normalbeep = newClipQueue.Count == 0;

                if (!normalbeep)
                {
                    int forcedBeep = Random.Range(0, 100);
                    DebugLog("Alarm clock beep check. Rolling a D100. Need to roll higher than {0} for normal beep. Rolled a {1}.", 100 - _modSettings.Settings.ChanceOfNormalBeep, 100 - forcedBeep);
                    normalbeep |= forcedBeep < _modSettings.Settings.ChanceOfNormalBeep;
                    if (normalbeep)
                    {
                        DebugLog("Well, it looks like the clock isn't playing music right now. Kappa");
                    }

                    if (_modSettings.Settings.ChanceOfNormalBeep == 0 && forcedBeep == 0)
                    {
                        DebugLog("Wait, why are we rolling for alarm clock beep when that is impossible to get.");
                    }
                }
                else
                {
                    DebugLog("Alarm clock beep check. Rolling a D100. Need to roll higher than {0} for normal beep. Rolled a natural twenty. Wait a minute, that was a D20.", 100 - _modSettings.Settings.ChanceOfNormalBeep);
                    DebugLog("Since there is no music queed anyways, the natural twentry still stands, regardless of the chance.");
                }

                AudioClips clip = normalbeep
                    ? new AudioClips(null, true) 
                    : newClipQueue.Dequeue();

                AlarmClock.StopCoroutine("StopBuzzerAfterTime");
                float clipTime = -1;
                AudioSource varAudio = null;
                try
                {
                    DebugLog("Alarm Clock just turned on. Trying to find out how long the clip length is.");
                    var playingSound = CommonReflectedTypeInfo.PlayResultField.GetValue(AlarmClock);
                    if (playingSound != null)
                    {
                        var soundGroupVariation = CommonReflectedTypeInfo.ActingVariationProperty.GetValue(playingSound, null);
                        if (soundGroupVariation != null)
                        {
                            varAudio = (AudioSource) CommonReflectedTypeInfo.VarAudioProperty.GetValue(soundGroupVariation, null);
                            if (varAudio != null)
                            {
                                //varAudio.clip != null
                                varAudio.Stop();
                                originalClip = varAudio.clip;

                                if (!clip.IsVanilla)
                                {
                                    varAudio.clip = clip.clip;
                                }

                                DebugLog("Retrieved the clip successfully. The Clip name is {0} and the length is {1} seconds", varAudio.clip.name, varAudio.clip.length);
                                bool looped = varAudio.clip.name.ToLowerInvariant().Contains("[looped]");
                                looped |= clip.IsVanilla;
                                bool tracked = varAudio.clip.name.ToLowerInvariant().EndsWith(".it");
                                tracked |= varAudio.clip.name.ToLowerInvariant().EndsWith(".s3m");
                                tracked |= varAudio.clip.name.ToLowerInvariant().EndsWith(".xm");
                                tracked |= varAudio.clip.name.ToLowerInvariant().EndsWith(".mod");
                                if (looped)
                                {
                                    clipTime = _modSettings.Settings.AlarmClockBuzzerTime;
                                    varAudio.loop = true;
                                    DebugLog("The clip specified that it wants to be looped. Playing the Alarm clock for AlarmClockBuzzerTime.");
                                }
                                else
                                {
                                    varAudio.loop = false;
                                    bool shortSong = (tracked ? 600.0f : varAudio.clip.length) < _modSettings.Settings.AlarmClockBuzzerTime;
                                    clipTime = Mathf.Min(varAudio.clip.length, _modSettings.Settings.AlarmClockBuzzerTime);
                                    if (!shortSong)
                                    {
                                        float endTime = tracked ? 600.0f : varAudio.clip.length - _modSettings.Settings.AlarmClockBuzzerTime;
                                        float startTime = Random.Range(0, endTime);
                                        varAudio.time = startTime;
                                        DebugLog("Playing clip from {0} to {1}", startTime, _modSettings.Settings.AlarmClockBuzzerTime + startTime);
                                    }
                                    else
                                    {
                                        DebugLog("Playing Entire clip");
                                    }
                                }
                                varAudio.Play();
                            }
                            else
                            {
                                DebugLog("varAudio is null. Cannot continue.");
                            }
                        }
                        else
                        {
                            DebugLog("soundGroupVariation is null. Cannot continue.");
                        }
                    }
                    else
                    {
                        DebugLog("playingSound is null. Cannot continue.");
                    }
                }
                catch (Exception ex)
                {
                    DebugLog("Failed to get Clip length due to Exception: {0}\n{1}", ex.Message, ex.StackTrace);
                    clipTime = _modSettings.Settings.AlarmClockBuzzerTime;
                }

                AlarmClock.StartCoroutine(ShutOffAlarmClock(clipTime, AlarmClock));
                DebugLog("Alarm Clock will play for {0} seconds provided it isn't shut off sooner. Kappa", clipTime);

                //Wait for Alarm clock to shut off.
                while ((bool)CommonReflectedTypeInfo.BuzzerStateField.GetValue(AlarmClock))
                {
                    yield return null;
                }

                if (varAudio != null && originalClip != null)
                {
                    varAudio.clip = originalClip;
                    if (!clip.IsVanilla)
                    {
                        Destroy(clip.clip);
                    }
                }
                
            }
            yield return null;
        }
    }
}
