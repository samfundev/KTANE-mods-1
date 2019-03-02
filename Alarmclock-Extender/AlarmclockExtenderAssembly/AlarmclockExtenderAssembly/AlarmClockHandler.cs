using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Props;
using DarkTonic.MasterAudio;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;
using static ReflectionHelper;

namespace AlarmClockExtenderAssembly
{
    
    public class AlarmClockHandler
    {
        private readonly AlarmclockExtenderAssembly.ModSettings _modSettings;
        public KMGameInfo.State GameState;

        public class AudioClips
        {
            public AudioClip Clip;
            public bool IsVanilla;

            public AudioClips(AudioClip clip, bool vanilla = false)
            {
                Clip = clip;
                IsVanilla = vanilla;
            }
        }

        private readonly Queue<AudioClips> _newClipQueue = new Queue<AudioClips>();
        private AudioClip _originalClip;

        private readonly GetAvailableTracks _availableTracks = new GetAvailableTracks();

        public AlarmClockHandler(AlarmclockExtenderAssembly.ModSettings settings)
        {
            _modSettings = settings;
        }


        public static void DebugLog(string message, params object[] args)
        {
            AlarmclockExtenderAssembly.ModSettings.DebugLog(message, args);
        }


        public IEnumerator ShutOffAlarmClock(float time, AlarmClock alarmClock, FieldInfo<bool> buzzerStateField)
        {
            var startTime = Time.time;
            yield return new WaitUntil(() => ((Time.time - startTime) >= time) || !buzzerStateField.Get());

            if(buzzerStateField.Get())
                alarmClock.TurnOff();
        }

        public IEnumerator CheckForAlarmClock()
        {
            AlarmClock alarmClock = null;

            yield return null;
            _modSettings.ReadSettings();
            
            DebugLog("Attempting to Find Alarm Clock");
            while (SceneManager.Instance.CurrentState == SceneManager.State.Gameplay)
            {


                AlarmClock[] alarmClocks = (AlarmClock[])Object.FindObjectsOfType(typeof(AlarmClock));
                if (alarmClocks == null || alarmClocks.Length == 0)
                {
                    yield return null;
                    continue;
                }

                foreach (var alarm in alarmClocks)
                {
                    DebugLog("Alarm Clock found - Hooking into it.");
                    alarm.MaxAlarmClockBuzzerTime = _modSettings.Settings.AlarmClockBuzzerTime;
                    DebugLog("Done setting up the Alarm Clocks");
                    alarmClock = alarm;
                    break;
                }
                break;
            }

            FieldInfo<bool> buzzerStateField = GetField<bool>(alarmClock, "isOn");
            FieldInfo<PlaySoundResult> playResultField = GetField<PlaySoundResult>(alarmClock, "alarmClockBuzzerSound");

            if (alarmClock == null || buzzerStateField == null || playResultField == null)
                yield break;

            if (_modSettings.Settings.RescanDirectory)
            {
                DebugLog("Rescanning the Sound Directory");
                var rescan = PickNextTrack(_modSettings.Settings.RescanDirectory);
                while (rescan.MoveNext())
                    yield return rescan.Current;
                DebugLog("Rescan complete");
            }

            while (SceneManager.Instance.CurrentState == SceneManager.State.Gameplay)
            {
                if (buzzerStateField.Get())
                {
                    var normalBeep = _newClipQueue.Count == 0;

                    if (!normalBeep)
                    {
                        var forcedBeep = Random.Range(0, 100);
                        DebugLog("Alarm clock beep check. Rolling a D100. Need to roll higher than {0} for normal beep. Rolled a {1}.", 100 - _modSettings.Settings.ChanceOfNormalBeep, 100 - forcedBeep);
                        normalBeep |= forcedBeep < _modSettings.Settings.ChanceOfNormalBeep;
                        if (normalBeep)
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
                        DebugLog("Since there is no music queued anyways, the natural twenty still stands, regardless of the chance.");
                    }

                    AudioClips clip = normalBeep
                        ? new AudioClips(null, true)
                        : _newClipQueue.Dequeue();

                    // ReSharper disable once Unity.IncorrectMethodSignatureInStringLiteral
                    alarmClock.StopCoroutine("StopBuzzerAfterTime");
                    float clipTime = -1;
                    AudioSource varAudio = null;
                    try
                    {
                        DebugLog("Alarm Clock just turned on. Trying to find out how long the clip length is.");
                        var playingSound = playResultField.Get();
                        if (playingSound != null)
                        {
                            var soundGroupVariation = playingSound.ActingVariation;
                            if (soundGroupVariation != null)
                            {
                                varAudio = soundGroupVariation.VarAudio;
                                if (varAudio != null)
                                {
                                    //varAudio.clip != null
                                    varAudio.Stop();
                                    _originalClip = varAudio.clip;

                                    if (!clip.IsVanilla)
                                    {
                                        varAudio.clip = clip.Clip;
                                    }

                                    DebugLog("Retrieved the clip successfully. The Clip name is {0} and the length is {1} seconds", varAudio.clip.name, varAudio.clip.length);
                                    var looped = varAudio.clip.name.ToLowerInvariant().Contains("[looped]");
                                    looped |= varAudio.clip.name.Equals("alarm_clock_beep");
                                    looped |= (clip.IsVanilla && _availableTracks.AudioFiles.Count > 0);
                                    var tracked = varAudio.clip.name.ToLowerInvariant().EndsWith(".it");
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
                                        var shortSong = (tracked ? 600.0f : varAudio.clip.length) < _modSettings.Settings.AlarmClockBuzzerTime;
                                        clipTime = Mathf.Min(varAudio.clip.length, _modSettings.Settings.AlarmClockBuzzerTime);
                                        if (!shortSong)
                                        {
                                            var endTime = tracked ? 600.0f : varAudio.clip.length - _modSettings.Settings.AlarmClockBuzzerTime;
                                            var startTime = Random.Range(0, endTime);
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

                    alarmClock.StartCoroutine(ShutOffAlarmClock(clipTime, alarmClock, buzzerStateField));

                    DebugLog("Alarm Clock will play for {0} seconds provided it isn't shut off sooner. Kappa", clipTime);

                    //Wait for Alarm clock to shut off.
                    yield return new WaitUntil(() => !buzzerStateField.Get());

                    if (varAudio != null && _originalClip != null)
                    {
                        varAudio.clip = _originalClip;
                        if (!clip.IsVanilla)
                        {
                            Object.Destroy(clip.Clip);
                        }
                    }

                }
                yield return null;
            }
        }

        private IEnumerator MakeClip(string path)
        {
            DebugLog("Attempting to make a clip from {0}", path);
            MakeAudioClip fileClip = new MakeAudioClip {FilePath = path};
            fileClip.Start();
            while (!fileClip.IsDone)
                yield return null;
            if (fileClip.Ex != null)
            {
                DebugLog("Failed to load sound sound file {0} due to Exception: {1}\nStack Trace {2}", path, fileClip.Ex.Source, fileClip.Ex.StackTrace);
                yield break;
            }

            if (path != null && fileClip.clip)
            {
                AudioClip clip = fileClip.clip;
                clip.name = Path.GetFileName(path);
                DebugLog("Loaded clip {0}", clip.name);
                _newClipQueue.Enqueue(new AudioClips(clip));
            }
            else
            {
                DebugLog("Failed to load clip {0}", path == null ? "<null>" : Path.GetFileName(path));
                _availableTracks.IgnoredTracks.Add(path);
                _availableTracks.AudioFiles.Remove(path);
            }
        }

        private IEnumerator _newTrack;
        public IEnumerator KeepTrackQueueFull()
        {
            yield return new WaitUntil(() => SceneManager.Instance.CurrentState == SceneManager.State.Setup);
            yield return new WaitUntil(() => SceneManager.Instance.CurrentState == SceneManager.State.Transitioning);
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

                if (_newClipQueue.Count >= 5) continue;
                _newTrack = PickNextTrack();
                while (_newTrack.MoveNext())
                    yield return _newTrack.Current;
                _newTrack = null;
            }
            // ReSharper disable once IteratorNeverReturns
        }

        public IEnumerator PickNextTrack(bool getTracks = false)
        {
            DebugLog("Picking a new Clip to play");

            if (_availableTracks.AudioFiles.Count == 0 || getTracks)
            {
                _availableTracks.FilePath = _modSettings.Settings.SoundFileDirectory;
                if (getTracks)
                    _availableTracks.AudioFiles.Clear();
                _availableTracks.Start();
                while (!_availableTracks.Update())
                    yield return null;
            }
            if (_availableTracks.AudioFiles.Count == 0)
            {
                _newClipQueue.Enqueue(new AudioClips(null, true));
                DebugLog("No files found to load. Forced to use the Vanilla alarm clock sound instead. :(");
                yield break;
            }
            while (_availableTracks.AudioFiles.Count > 0 && _newClipQueue.Count < 5)
            {
                var nexttrack = Random.Range(0, _availableTracks.AudioFiles.Count);
                IEnumerator makeclip = MakeClip(_availableTracks.AudioFiles[nexttrack]);
                while (makeclip.MoveNext())
                    yield return makeclip.Current;
            }
        }
    }
}