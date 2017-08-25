using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Xml.Serialization;
using Microsoft.Win32;
using UnityEditor;
using UnityEngine;

public class TestHarness : MonoBehaviour
{
    public FakeBombInfo FakeInfo;

    public StatusLight StatusLightPrefab;
    public GameObject HighlightPrefab;

    [Serializable]
    public class BombSoundEffects
    {
        public List<AudioClip> ButtonPress = new List<AudioClip>();
        public List<AudioClip> ButtonRelease = new List<AudioClip>();
        public List<AudioClip> BigButtonPress = new List<AudioClip>();
        public List<AudioClip> BigButtonRelease = new List<AudioClip>();
        public List<AudioClip> WireSnip = new List<AudioClip>();
        public List<AudioClip> Strike = new List<AudioClip>();
        public List<AudioClip> AlarmClockBeep = new List<AudioClip>();
        public List<AudioClip> AlarmClockSnooze = new List<AudioClip>();
        public List<AudioClip> Switch = new List<AudioClip>();
        public List<AudioClip> GameOverFanfare = new List<AudioClip>();
        public List<AudioClip> BombDefused = new List<AudioClip>();
        public List<AudioClip> BriefcaseOpen = new List<AudioClip>();
        public List<AudioClip> BriefcaseClose = new List<AudioClip>();
        public List<AudioClip> CorrectChime = new List<AudioClip>();
        public List<AudioClip> BombExplode = new List<AudioClip>();
        public List<AudioClip> NormalTimerBeep = new List<AudioClip>();
        public List<AudioClip> FastTimerBeep = new List<AudioClip>();
        public List<AudioClip> FastestTimerBeep = new List<AudioClip>();
        public List<AudioClip> LightBuzz = new List<AudioClip>();
        public List<AudioClip> LightBuzzShort = new List<AudioClip>();
        public List<AudioClip> Stamp = new List<AudioClip>();
        public List<AudioClip> TypewriterKey = new List<AudioClip>();
        public List<AudioClip> NeedyActivated = new List<AudioClip>();
        public List<AudioClip> WireSequenceMechanism = new List<AudioClip>();
        public List<AudioClip> SelectionTick = new List<AudioClip>();
        public List<AudioClip> PageTurn = new List<AudioClip>();
        public List<AudioClip> DossierOptionPressed = new List<AudioClip>();
        public List<AudioClip> FreeplayDeviceDrop = new List<AudioClip>();
        public List<AudioClip> BombDrop = new List<AudioClip>();
        public List<AudioClip> MenuDrop = new List<AudioClip>();
        public List<AudioClip> BinderDrop = new List<AudioClip>();
        public List<AudioClip> MenuButtonPressed = new List<AudioClip>();
        public List<AudioClip> TitleMenuPressed = new List<AudioClip>();
        public List<AudioClip> CapacitorPop = new List<AudioClip>();
        public List<AudioClip> EmergencyAlarm = new List<AudioClip>();
        public List<AudioClip> NeedyWarning = new List<AudioClip>();

        private readonly Dictionary<KMSoundOverride.SoundEffect, List<AudioClip>> _soundEffects =
            new Dictionary<KMSoundOverride.SoundEffect, List<AudioClip>>();

        public BombSoundEffects()
        {
            _soundEffects.Add(KMSoundOverride.SoundEffect.ButtonPress, ButtonPress);
            _soundEffects.Add(KMSoundOverride.SoundEffect.ButtonRelease, ButtonRelease);
            _soundEffects.Add(KMSoundOverride.SoundEffect.BigButtonPress, BigButtonPress);
            _soundEffects.Add(KMSoundOverride.SoundEffect.BigButtonRelease, BigButtonRelease);
            _soundEffects.Add(KMSoundOverride.SoundEffect.WireSnip, WireSnip);
            _soundEffects.Add(KMSoundOverride.SoundEffect.Strike, Strike);
            _soundEffects.Add(KMSoundOverride.SoundEffect.AlarmClockBeep, AlarmClockBeep);
            _soundEffects.Add(KMSoundOverride.SoundEffect.AlarmClockSnooze, AlarmClockSnooze);
            _soundEffects.Add(KMSoundOverride.SoundEffect.Switch, Switch);
            _soundEffects.Add(KMSoundOverride.SoundEffect.GameOverFanfare, GameOverFanfare);
            _soundEffects.Add(KMSoundOverride.SoundEffect.BombDefused, BombDefused);
            _soundEffects.Add(KMSoundOverride.SoundEffect.BriefcaseOpen, BriefcaseOpen);
            _soundEffects.Add(KMSoundOverride.SoundEffect.BriefcaseClose, BriefcaseClose);
            _soundEffects.Add(KMSoundOverride.SoundEffect.CorrectChime, CorrectChime);
            _soundEffects.Add(KMSoundOverride.SoundEffect.BombExplode, BombExplode);
            _soundEffects.Add(KMSoundOverride.SoundEffect.NormalTimerBeep, NormalTimerBeep);
            _soundEffects.Add(KMSoundOverride.SoundEffect.FastTimerBeep, FastTimerBeep);
            _soundEffects.Add(KMSoundOverride.SoundEffect.FastestTimerBeep, FastestTimerBeep);
            _soundEffects.Add(KMSoundOverride.SoundEffect.LightBuzz, LightBuzz);
            _soundEffects.Add(KMSoundOverride.SoundEffect.LightBuzzShort, LightBuzzShort);
            _soundEffects.Add(KMSoundOverride.SoundEffect.Stamp, Stamp);
            _soundEffects.Add(KMSoundOverride.SoundEffect.TypewriterKey, TypewriterKey);
            _soundEffects.Add(KMSoundOverride.SoundEffect.NeedyActivated, NeedyActivated);
            _soundEffects.Add(KMSoundOverride.SoundEffect.WireSequenceMechanism, WireSequenceMechanism);
            _soundEffects.Add(KMSoundOverride.SoundEffect.SelectionTick, SelectionTick);
            _soundEffects.Add(KMSoundOverride.SoundEffect.PageTurn, PageTurn);
            _soundEffects.Add(KMSoundOverride.SoundEffect.DossierOptionPressed, DossierOptionPressed);
            _soundEffects.Add(KMSoundOverride.SoundEffect.FreeplayDeviceDrop, FreeplayDeviceDrop);
            _soundEffects.Add(KMSoundOverride.SoundEffect.BombDrop, BombDrop);
            _soundEffects.Add(KMSoundOverride.SoundEffect.MenuDrop, MenuDrop);
            _soundEffects.Add(KMSoundOverride.SoundEffect.BinderDrop, BinderDrop);
            _soundEffects.Add(KMSoundOverride.SoundEffect.MenuButtonPressed, MenuButtonPressed);
            _soundEffects.Add(KMSoundOverride.SoundEffect.TitleMenuPressed, TitleMenuPressed);
            _soundEffects.Add(KMSoundOverride.SoundEffect.CapacitorPop, CapacitorPop);
            _soundEffects.Add(KMSoundOverride.SoundEffect.EmergencyAlarm, EmergencyAlarm);
            _soundEffects.Add(KMSoundOverride.SoundEffect.NeedyWarning, NeedyWarning);
        }

        public AudioClip GetAudioClip(KMSoundOverride.SoundEffect effect)
        {
            List<AudioClip> clips;
            if (!_soundEffects.TryGetValue(effect, out clips))
                return null;
            if (clips == null || clips.Count == 0)
                return null;
            return clips[UnityEngine.Random.Range(0, clips.Count)];
        }

        public void OverwriteClips(KMSoundOverride clipsOverride)
        {
            List<AudioClip> clips;
            if (!_soundEffects.TryGetValue(clipsOverride.OverrideEffect, out clips))
            {
                clips = new List<AudioClip>();
                _soundEffects.Add(clipsOverride.OverrideEffect, clips);
            }
            else
            {
                clips.Clear();
            }
            clips.Add(clipsOverride.AudioClip);
            clips.AddRange(clipsOverride.AdditionalVariants);
            clips.RemoveAll(t => t == null);
        }
    }

    public BombSoundEffects SoundEffects = new BombSoundEffects();

    TestSelectable currentSelectable;
    TestSelectableArea currentSelectableArea;

    KMBombInfo BombInfo;

    AudioSource audioSource;
    List<AudioClip> audioClips;

    void Awake()
    {
        //FakeInfo = gameObject.AddComponent<FakeBombInfo>();
        FakeInfo.ActivateLights += delegate ()
        {
            TurnLightsOn();
            FakeInfo.OnLightsOn();
            StartCoroutine(TimerTick());
            StartCoroutine(LastMinuteWarning());
        };
        TurnLightsOff();

        ReplaceBombInfo();
        AddHighlightables();
        AddSelectables();
    }

    void ReplaceBombInfo()
    {
        MonoBehaviour[] scripts = MonoBehaviour.FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour s in scripts)
        {
            IEnumerable<FieldInfo> fields = s.GetType().GetFields();
            foreach (FieldInfo f in fields)
            {
                if (f.FieldType.Equals(typeof(KMBombInfo)))
                {
                    KMBombInfo component = (KMBombInfo) f.GetValue(s);
                    component.TimeHandler += new KMBombInfo.GetTimeHandler(FakeInfo.GetTime);
                    component.FormattedTimeHandler += new KMBombInfo.GetFormattedTimeHandler(FakeInfo.GetFormattedTime);
                    component.StrikesHandler += new KMBombInfo.GetStrikesHandler(FakeInfo.GetStrikes);
                    component.ModuleNamesHandler += new KMBombInfo.GetModuleNamesHandler(FakeInfo.GetModuleNames);
                    component.SolvableModuleNamesHandler += new KMBombInfo.GetSolvableModuleNamesHandler(FakeInfo.GetSolvableModuleNames);
                    component.SolvedModuleNamesHandler += new KMBombInfo.GetSolvedModuleNamesHandler(FakeInfo.GetSolvedModuleNames);
                    component.WidgetQueryResponsesHandler += new KMBombInfo.GetWidgetQueryResponsesHandler(FakeInfo.GetWidgetQueryResponses);
                    component.IsBombPresentHandler += new KMBombInfo.KMIsBombPresent(FakeInfo.IsBombPresent);
                    continue;
                }
                if (f.FieldType.Equals(typeof(KMGameInfo)))
                {
                    KMGameInfo component = (KMGameInfo) f.GetValue(s);
                    component.OnLightsChange += new KMGameInfo.KMLightsChangeDelegate(FakeInfo.OnLights);
                    //component.OnAlarmClockChange += new KMGameInfo.KMAlarmClockChangeDelegate(fakeInfo.OnAlarm);
                    continue;
                }
                if (f.FieldType.Equals(typeof(KMGameCommands)))
                {
                    KMGameCommands component = (KMGameCommands) f.GetValue(s);
                    component.OnCauseStrike += new KMGameCommands.KMCauseStrikeDelegate(FakeInfo.HandleStrike);
                    continue;
                }
            }
        }
    }

    void OnBombExploded()
    {
        PlayGameSoundHandler(KMSoundOverride.SoundEffect.BombExplode, transform);
    }

    void OnBombSolved()
    {
        PlayGameSoundHandler(KMSoundOverride.SoundEffect.BombDefused, transform);
        PlayGameSoundHandler(KMSoundOverride.SoundEffect.GameOverFanfare, transform);
    }

    private float _previousTimer;

    IEnumerator TimerTick()
    {
        _previousTimer = FakeInfo.TimeLeft;
        while (!FakeInfo.detonated)
        {
            yield return new WaitUntil(() => Mathf.FloorToInt(FakeInfo.TimeLeft) != Mathf.FloorToInt(_previousTimer) && !FakeInfo.detonated);
            if (FakeInfo.detonated) yield break;
            switch (FakeInfo.strikes)
            {
                case 0:
                    PlayGameSoundHandler(KMSoundOverride.SoundEffect.NormalTimerBeep, transform);
                    break;
                case 1:
                    PlayGameSoundHandler(KMSoundOverride.SoundEffect.FastTimerBeep, transform);
                    break;
                default:
                    PlayGameSoundHandler(KMSoundOverride.SoundEffect.FastestTimerBeep, transform);
                    break;
            }
            _previousTimer = FakeInfo.TimeLeft;
        }
    }

    IEnumerator LastMinuteWarning()
    {
        yield return new WaitUntil(() => (FakeInfo.TimeLeft <= 60) && !FakeInfo.detonated);
        while (!FakeInfo.detonated)
        {
            PlayGameSoundHandler(KMSoundOverride.SoundEffect.EmergencyAlarm, transform);
            yield return new WaitForSeconds(2.5f);
        }
    }


    void Start()
    {
        MonoBehaviour[] scripts = MonoBehaviour.FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour s in scripts)
        {
            IEnumerable<FieldInfo> fields = s.GetType().GetFields();
            foreach (FieldInfo f in fields)
            {
                if (f.FieldType.Equals(typeof(KMBombInfo)))
                {
                    KMBombInfo component = (KMBombInfo) f.GetValue(s);
                    if (component.OnBombExploded != null) FakeInfo.Detonate += new FakeBombInfo.OnDetonate(component.OnBombExploded);
                    if (component.OnBombSolved != null) FakeInfo.HandleSolved += new FakeBombInfo.OnSolved(component.OnBombSolved);
                    continue;
                }
            }
        }

        FakeInfo.Detonate += OnBombExploded;
        FakeInfo.HandleSolved += OnBombSolved;

        currentSelectable = GetComponent<TestSelectable>();

        KMBombModule[] modules = FindObjectsOfType<KMBombModule>();
        KMNeedyModule[] needyModules = FindObjectsOfType<KMNeedyModule>();
        KMWidget[] widgets = FindObjectsOfType<KMWidget>();
        KMSoundOverride[] overrides = FindObjectsOfType<KMSoundOverride>();
        FakeInfo.needyModules = needyModules.ToList();
        currentSelectable.Children = new TestSelectable[modules.Length + needyModules.Length];

        FakeInfo.kmWidgets.AddRange(widgets);

        foreach (KMSoundOverride sound in overrides)
            SoundEffects.OverwriteClips(sound);

        for (int i = 0; i < modules.Length; i++)
        {
            KMBombModule mod = modules[i];

            KMStatusLightParent statuslightparent = modules[i].GetComponentInChildren<KMStatusLightParent>();
            var statuslight = Instantiate<StatusLight>(StatusLightPrefab);
            statuslight.transform.parent = statuslightparent.transform;
            statuslight.transform.localPosition = Vector3.zero;
            statuslight.transform.localScale = Vector3.one;
            statuslight.transform.localRotation = Quaternion.identity;
            statuslight.SetInActive();

            currentSelectable.Children[i] = modules[i].GetComponent<TestSelectable>();
            modules[i].GetComponent<TestSelectable>().Parent = currentSelectable;

            FakeInfo.modules.Add(new KeyValuePair<KMBombModule, bool>(modules[i], false));
            modules[i].OnPass = delegate ()
            {
                Debug.Log("Module Passed");
                statuslight.SetPass();

                FakeInfo.modules.Remove(FakeInfo.modules.First(t => t.Key.Equals(mod)));
                FakeInfo.modules.Add(new KeyValuePair<KMBombModule, bool>(mod, true));
                bool allSolved = !FakeInfo.detonated;
                foreach (KeyValuePair<KMBombModule, bool> m in FakeInfo.modules)
                {
                    if (!allSolved)
                        break;
                    allSolved &= m.Value;
                }
                if (allSolved) FakeInfo.Solved();
                return false;
            };
            modules[i].OnStrike = delegate ()
            {
                Debug.Log("Strike");
                statuslight.FlashStrike();
                FakeInfo.HandleStrike();
                if (!FakeInfo.detonated)
                    PlayGameSoundHandler(KMSoundOverride.SoundEffect.Strike, transform);
                return false;
            };
        }

        for (int i = 0; i < needyModules.Length; i++)
        {
            currentSelectable.Children[modules.Length + i] = needyModules[i].GetComponent<TestSelectable>();
            needyModules[i].GetComponent<TestSelectable>().Parent = currentSelectable;

            needyModules[i].OnPass = delegate ()
            {
                Debug.Log("Module Passed");
                return false;
            };
            needyModules[i].OnStrike = delegate ()
            {
                Debug.Log("Strike");
                FakeInfo.HandleStrike();
                if (!FakeInfo.detonated)
                    PlayGameSoundHandler(KMSoundOverride.SoundEffect.Strike, transform);
                return false;
            };
        }

        currentSelectable.ActivateChildSelectableAreas();


        //Load all the audio clips in the asset database
        audioClips = new List<AudioClip>();
        string[] audioClipAssetGUIDs = AssetDatabase.FindAssets("t:AudioClip");

        foreach (var guid in audioClipAssetGUIDs)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));

            if (clip != null)
            {
                audioClips.Add(clip);
            }
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        KMAudio[] kmAudios = FindObjectsOfType<KMAudio>();
        foreach (KMAudio kmAudio in kmAudios)
        {
            kmAudio.HandlePlaySoundAtTransform += PlaySoundHandler;
            kmAudio.HandlePlayGameSoundAtTransform += PlayGameSoundHandler;
            kmAudio.HandlePlaySoundAtTransformWithRef += PlaySoundwithRefHandler;
            kmAudio.HandlePlayGameSoundAtTransformWithRef += PlayGameSoundHandlerWithRef;
        }
    }

    protected KMAudio.KMAudioRef PlaySoundwithRefHandler(string clipName, Transform t, bool loop)
    {
        KMAudio.KMAudioRef kmaudiorRef = new KMAudio.KMAudioRef();
        if (audioClips.Count <= 0) return kmaudiorRef;
        AudioClip clip = audioClips.Where(a => a.name == clipName).First();

        if (clip == null) return kmaudiorRef;
        if(t != null)
            audioSource.transform.position = t.position;
        audioSource.loop = loop;
        audioSource.PlayOneShot(clip);
        KMAudio.KMAudioRef kmaudioRef2 = kmaudiorRef;
        kmaudioRef2.StopSound = (Action) Delegate.Combine(kmaudioRef2.StopSound, new Action(delegate
        {
            if (audioSource.isPlaying)
                audioSource.Stop();
        }));
        return kmaudiorRef;
    }

    protected void PlaySoundHandler(string clipName, Transform t)
    {
        PlaySoundwithRefHandler(clipName, t, false);
    }

    protected void PlayGameSoundHandler(KMSoundOverride.SoundEffect sound, Transform t)
    {
        PlayGameSoundHandlerWithRef(sound, t);
    }

    protected KMAudio.KMAudioRef PlayGameSoundHandlerWithRef(KMSoundOverride.SoundEffect sound, Transform t)
    {
        KMAudio.KMAudioRef kmaudioRef = new KMAudio.KMAudioRef();
        var clip = SoundEffects.GetAudioClip(sound);
        if (clip == null) return kmaudioRef;
        if(t != null)
            audioSource.transform.position = t.position;
        audioSource.loop = false;
        audioSource.PlayOneShot(clip);
        KMAudio.KMAudioRef kmaudioRef2 = kmaudioRef;
        kmaudioRef2.StopSound = (Action)Delegate.Combine(kmaudioRef2.StopSound, new Action(delegate
        {
            if(audioSource.isPlaying)
                audioSource.Stop();
        }));
        return kmaudioRef;
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction);
        RaycastHit hit;
        int layerMask = 1 << 11;
        bool rayCastHitSomething = Physics.Raycast(ray, out hit, 1000, layerMask);
        if (rayCastHitSomething)
        {
            TestSelectableArea hitArea = hit.collider.GetComponent<TestSelectableArea>();
            if (hitArea != null)
            {
                if (currentSelectableArea != hitArea)
                {
                    if (currentSelectableArea != null)
                    {
                        currentSelectableArea.Selectable.Deselect();
                    }

                    hitArea.Selectable.Select();
                    currentSelectableArea = hitArea;
                }
            }
            else
            {
                if (currentSelectableArea != null)
                {
                    currentSelectableArea.Selectable.Deselect();
                    currentSelectableArea = null;
                }
            }
        }
        else
        {
            if (currentSelectableArea != null)
            {
                currentSelectableArea.Selectable.Deselect();
                currentSelectableArea = null;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (currentSelectableArea != null && currentSelectableArea.Selectable.Interact())
            {
                currentSelectable.DeactivateChildSelectableAreas(currentSelectableArea.Selectable);
                currentSelectable = currentSelectableArea.Selectable;
                currentSelectable.ActivateChildSelectableAreas();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (currentSelectableArea != null)
            {
                currentSelectableArea.Selectable.InteractEnded();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (currentSelectable.Parent != null && currentSelectable.Cancel())
            {
                currentSelectable.DeactivateChildSelectableAreas(currentSelectable.Parent);
                currentSelectable = currentSelectable.Parent;
                currentSelectable.ActivateChildSelectableAreas();
            }
        }
    }

    void AddHighlightables()
    {
        List<KMHighlightable> highlightables = new List<KMHighlightable>(GameObject.FindObjectsOfType<KMHighlightable>());

        foreach (KMHighlightable highlightable in highlightables)
        {
            TestHighlightable highlight = highlightable.gameObject.AddComponent<TestHighlightable>();

            highlight.HighlightPrefab = HighlightPrefab;
            highlight.HighlightScale = highlightable.HighlightScale;
            highlight.OutlineAmount = highlightable.OutlineAmount;
        }
    }

    void AddSelectables()
    {
        List<KMSelectable> selectables = new List<KMSelectable>(GameObject.FindObjectsOfType<KMSelectable>());

        foreach (KMSelectable selectable in selectables)
        {
            TestSelectable testSelectable = selectable.gameObject.AddComponent<TestSelectable>();
            testSelectable.Highlight = selectable.Highlight.GetComponent<TestHighlightable>();
        }

        foreach (KMSelectable selectable in selectables)
        {
            TestSelectable testSelectable = selectable.gameObject.GetComponent<TestSelectable>();
            testSelectable.Children = new TestSelectable[selectable.Children.Length];
            for (int i = 0; i < selectable.Children.Length; i++)
            {
                if (selectable.Children[i] != null)
                {
                    testSelectable.Children[i] = selectable.Children[i].GetComponent<TestSelectable>();
                }
            }
        }
    }

    // TPK Methods
    protected void DoInteractionStart(KMSelectable interactable)
    {
        interactable.OnInteract();
    }

    protected void DoInteractionEnd(KMSelectable interactable)
    {
        if (interactable.OnInteractEnded != null)
        {
            interactable.OnInteractEnded();
        }
    }

    Dictionary<Component, HashSet<KMSelectable>> ComponentHelds = new Dictionary<Component, HashSet<KMSelectable>> { };
    IEnumerator SimulateModule(Component component, Transform moduleTransform, MethodInfo method, string command)
    {
        // Simple Command
        if (method.ReturnType == typeof(KMSelectable[]))
        {
            KMSelectable[] selectableSequence = null;
            try
            {
                selectableSequence = (KMSelectable[]) method.Invoke(component, new object[] { command });
                if (selectableSequence == null)
                {
                    yield break;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogErrorFormat("An exception occurred while trying to invoke {0}.{1}; the command invokation will not continue.", method.DeclaringType.FullName, method.Name);
                Debug.LogException(ex);
                yield break;
            }

            int initialStrikes = FakeInfo.strikes;
            int initialSolved = FakeInfo.GetSolvedModuleNames().Count;
            foreach (KMSelectable selectable in selectableSequence)
            {
                DoInteractionStart(selectable);
                yield return new WaitForSeconds(0.1f);
                DoInteractionEnd(selectable);

                if (FakeInfo.strikes != initialStrikes || FakeInfo.GetSolvedModuleNames().Count != initialSolved)
                {
                    break;
                }
            };
        }

        // Complex Commands
        if (method.ReturnType == typeof(IEnumerator))
        {
            IEnumerator responseCoroutine = null;
            try
            {
                responseCoroutine = (IEnumerator) method.Invoke(component, new object[] { command });
            }
            catch (System.Exception ex)
            {
                Debug.LogErrorFormat("An exception occurred while trying to invoke {0}.{1}; the command invokation will not continue.", method.DeclaringType.FullName, method.Name);
                Debug.LogException(ex);
                yield break;
            }

            if (responseCoroutine == null)
                yield break;

            if (!ComponentHelds.ContainsKey(component))
                ComponentHelds[component] = new HashSet<KMSelectable>();
            HashSet<KMSelectable> heldSelectables = ComponentHelds[component];

            int initialStrikes = FakeInfo.strikes;
            int initialSolved = FakeInfo.GetSolvedModuleNames().Count;

            while (responseCoroutine.MoveNext())
            {
                object currentObject = responseCoroutine.Current;
                if (currentObject is KMSelectable)
                {
                    KMSelectable selectable = (KMSelectable) currentObject;
                    if (heldSelectables.Contains(selectable))
                    {
                        DoInteractionEnd(selectable);
                        heldSelectables.Remove(selectable);
                    }
                    else
                    {
                        DoInteractionStart(selectable);
                        heldSelectables.Add(selectable);
                    }
                }
                else if (currentObject is string)
                {
                    Debug.Log("Twitch handler sent: " + currentObject);
                    yield return currentObject;
                }
                else if (currentObject is Quaternion)
                {
                    moduleTransform.localRotation = (Quaternion) currentObject;
                }
                else
                    yield return currentObject;

                if (FakeInfo.strikes != initialStrikes || FakeInfo.GetSolvedModuleNames().Count != initialSolved)
                    yield break;
            }
        }
    }

    string command = "";
    void OnGUI()
    {
        var needyModules = GameObject.FindObjectsOfType<KMNeedyModule>();
        if (needyModules.Length > 0)
        {
            if (GUILayout.Button("Activate Needy Modules"))
            {
                foreach (KMNeedyModule needyModule in needyModules)
                {
                    if (needyModule.OnNeedyActivation != null)
                    {
                        needyModule.OnNeedyActivation();
                    }
                }
            }

            if (GUILayout.Button("Deactivate Needy Modules"))
            {
                foreach (KMNeedyModule needyModule in needyModules)
                {
                    if (needyModule.OnNeedyDeactivation != null)
                    {
                        needyModule.OnNeedyDeactivation();
                    }
                }
            }
        }

        if (GUILayout.Button("Lights On"))
        {
            TurnLightsOn();
            FakeInfo.OnLightsOn();
        }

        if (GUILayout.Button("Lights Off"))
        {
            TurnLightsOff();
            FakeInfo.OnLightsOff();
        }

        GUILayout.Label("Time remaining: " + FakeInfo.GetFormattedTime());

        GUILayout.Space(10);

        command = GUILayout.TextField(command);
        if ((GUILayout.Button("Simulate Twitch Command") || Event.current.keyCode == KeyCode.Return) && command != "")
        {
            Debug.Log("Twitch Command: " + command);

            //if(currentSelectable != )
            Component[] allComponents = currentSelectable.gameObject.GetComponentsInChildren<Component>(true);
            foreach (Component component in allComponents)
            {
                System.Type type = component.GetType();
                MethodInfo method = type.GetMethod("ProcessTwitchCommand", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (method != null)
                {
                    StartCoroutine(SimulateModule(component, currentSelectable.transform, method, command));
                }
            }

            /*
            foreach (KMBombModule module in FindObjectsOfType<KMBombModule>())
            {
                Component[] allComponents = module.gameObject.GetComponentsInChildren<Component>(true);
                foreach (Component component in allComponents)
                {
                    System.Type type = component.GetType();
                    MethodInfo method = type.GetMethod("ProcessTwitchCommand", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (method != null)
                    {
                        StartCoroutine(SimulateModule(component, module.transform, method, command));
                    }
                }
            }

            foreach (KMNeedyModule needyModule in needyModules)
            {
                Component[] allComponents = needyModule.gameObject.GetComponentsInChildren<Component>(true);
                foreach (Component component in allComponents)
                {
                    System.Type type = component.GetType();
                    MethodInfo method = type.GetMethod("ProcessTwitchCommand", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (method != null)
                    {
                        StartCoroutine(SimulateModule(component, needyModule.transform, method, command));
                    }
                }
            }*/
            command = "";
        }
    }

    public void TurnLightsOn()
    {
        RenderSettings.ambientIntensity = 1f;
        DynamicGI.UpdateEnvironment();

        foreach (Light l in FindObjectsOfType<Light>())
            if (l.transform.parent == null)
                l.enabled = true;
    }

    public void TurnLightsOff()
    {
        RenderSettings.ambientIntensity = 0.2f;
        DynamicGI.UpdateEnvironment();

        foreach (Light l in FindObjectsOfType<Light>())
            if (l.transform.parent == null)
                l.enabled = false;
    }
}
