using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

// ReSharper disable once CheckNamespace
public class BombCreator : MonoBehaviour
{
    public TextMesh TimeText;
    public KMSelectable TimeMinusButton;
    public KMSelectable TimePlusButton;

    public TextMesh ModulesText;
    public KMSelectable ModulesMinusButton;
    public KMSelectable ModulesPlusButton;

    public TextMesh StrikesText;
    public KMSelectable StrikesMinusButton;
    public KMSelectable StrikesPlusButton;

    public TextMesh WidgetsText;
    public KMSelectable WidgetsMinusButton;
    public KMSelectable WidgetsPlusButton;

    public TextMesh ModuleDisableText;
    public KMSelectable ModuleDisableMinusButton;
    public KMSelectable ModuleDisablePlusButton;
    public KMSelectable ModuleDisableButton;

    public TextMesh SeedText;
    public KMSelectable SeedMinusButton;
    public KMSelectable SeedManualButton;
    public KMSelectable SeedPlusButton;

    public TextMesh NeediesText;
    public KMSelectable NeedyMinusButton;
    public KMSelectable NeedyPlusButton;

    public TextMesh PlayModeText;
    public KMSelectable PlayModeButton;

    public TextMesh PacingEventsText;
    public KMSelectable PacingEventsButton;

    public TextMesh FrontFaceText;
    public KMSelectable FrontFaceButton;

    public TextMesh BombsText;
    public KMSelectable BombsMinusButton;
    public KMSelectable BombsPlusButton;

    public TextMesh DuplicateText;
    public KMSelectable DuplicateButton;

    public KMSelectable ResetButton;
    public KMSelectable StartButton;
    public KMSelectable SaveButton;
    private List<KMGameInfo.KMModuleInfo> _vanillaModules;

    public KMAudio Audio;
    public KMGameInfo GameInfo;

    private int _maxModules = 11;
    private int _maxFrontFace = 5;

    private readonly ModSettings _modSettings = new ModSettings("BombCreator");
    private ModuleSettings Settings { get { return _modSettings.Settings; } }

    public static void DebugLog(string message, params object[] args)
    {
        var debugstring = String.Format("[BombCreator] {0}", message);
        Debug.LogFormat(debugstring, args);
    }

    private IEnumerator HideMultipleBombsButtons()
    {
        var installed = MultipleBombs.Refresh();
        while (installed.MoveNext())
        {
            yield return installed.Current;
        }
        if (MultipleBombs.Installed()) yield break;
        BombsMinusButton.gameObject.SetActive(false);
        BombsPlusButton.gameObject.SetActive(false);
    }

    private IEnumerator HideVanillaSeed()
    {
        var installed = VanillaRuleModifier.Refresh();
        while (installed.MoveNext())
        {
            yield return installed.Current;
        }
        if (VanillaRuleModifier.Installed()) yield break;
        SeedManualButton.transform.parent.gameObject.SetActive(false);
    }

    private void Start()
    {
        _modSettings.ReadSettings();
        StartCoroutine(HideMultipleBombsButtons());
        StartCoroutine(HideVanillaSeed());

        _vanillaModules = GameInfo.GetAvailableModuleInfo().Where(x => !x.IsMod).ToList();

        UpdateDisplay();
        ChangeModuleDisableIndex(0);

        TimeMinusButton.OnInteract += delegate { StartCoroutine(AddTimer(-30)); return false; };
        TimePlusButton.OnInteract += delegate { StartCoroutine(AddTimer(30)); return false; };

        ModulesMinusButton.OnInteract += delegate { StartCoroutine(AddModules(-1)); return false; };
        ModulesPlusButton.OnInteract += delegate { StartCoroutine(AddModules(1)); return false; };

        WidgetsMinusButton.OnInteract += delegate {  StartCoroutine(AddWidgets(-1)); return false; };
        WidgetsPlusButton.OnInteract += delegate {  StartCoroutine(AddWidgets(1)); return false; };

        StrikesMinusButton.OnInteract += delegate {  StartCoroutine(AddStrikes(-1)); return false; };
        StrikesPlusButton.OnInteract += delegate { StartCoroutine(AddStrikes(1)); return false; };

        ModuleDisableMinusButton.OnInteract += () => ChangeModuleDisableIndex(-1);
        ModuleDisablePlusButton.OnInteract += () => ChangeModuleDisableIndex(1);
        ModuleDisableButton.OnInteract += ModuleDisableButtonPressed;

        SeedMinusButton.OnInteract += delegate { StartCoroutine(AddSeed(-1)); return false; };
        SeedManualButton.OnInteract += OpenManualDirectory;
        SeedPlusButton.OnInteract += delegate { StartCoroutine(AddSeed(1)); return false; };

        NeedyMinusButton.OnInteract += delegate { StartCoroutine(AddNeedyModules(-1)); return false; };
        NeedyPlusButton.OnInteract += delegate { StartCoroutine(AddNeedyModules(1)); return false; };
        PlayModeButton.OnInteract += ChangePlayMode;

        PacingEventsButton.OnInteract += ChangePacingEvent;
        FrontFaceButton.OnInteract += ChangeFrontFace;

        BombsMinusButton.OnInteract += delegate { StartCoroutine(AddBombs(-1)); return false; };
        BombsPlusButton.OnInteract += delegate { StartCoroutine(AddBombs(1)); return false; };
        DuplicateButton.OnInteract += DuplicatesAllowed;

        StartButton.OnInteract += StartMission;
        SaveButton.OnInteract += () => SaveSettings();
        ResetButton.OnInteract += delegate { StartCoroutine(ResetSettings()); return false; };



        TimeMinusButton.OnInteractEnded += () => EndInteract();
        TimePlusButton.OnInteractEnded += () => EndInteract();

        ModulesMinusButton.OnInteractEnded += () => EndInteract();
        ModulesPlusButton.OnInteractEnded += () => EndInteract();

        WidgetsMinusButton.OnInteractEnded += () => EndInteract();
        WidgetsPlusButton.OnInteractEnded += () => EndInteract();

        StrikesMinusButton.OnInteractEnded += () => EndInteract();
        StrikesPlusButton.OnInteractEnded += () => EndInteract();

        ModuleDisableMinusButton.OnInteractEnded += () => EndInteract(false);
        ModuleDisablePlusButton.OnInteractEnded += () => EndInteract(false);
        ModuleDisableButton.OnInteractEnded += () => EndInteract(false);

        SeedMinusButton.OnInteractEnded += () => EndInteract();
        SeedManualButton.OnInteractEnded += () => EndInteract(false);
        SeedPlusButton.OnInteractEnded += () => EndInteract();

        NeedyMinusButton.OnInteractEnded += () => EndInteract();
        NeedyPlusButton.OnInteractEnded += () => EndInteract();
        PlayModeButton.OnInteractEnded += () => EndInteract(false);

        PacingEventsButton.OnInteractEnded += () => EndInteract(false);
        FrontFaceButton.OnInteractEnded += () => EndInteract(false);

        BombsMinusButton.OnInteractEnded += () => EndInteract();
        BombsPlusButton.OnInteractEnded += () => EndInteract();
        DuplicateButton.OnInteractEnded += () => EndInteract(false);

        ResetButton.OnInteractEnded += CancelSettingsReset;
        StartButton.OnInteractEnded += () => EndInteract(false);
        SaveButton.OnInteractEnded += () => EndInteract(false);

    }

    private bool OpenManualDirectory()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (VanillaRuleModifier.Installed())
        {
            Application.OpenURL("file:///" + VanillaRuleModifier.GetRuleManualDirectory());
        }
        return false;
    }

    private bool DuplicatesAllowed()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Settings.DuplicatesAllowed = !Settings.DuplicatesAllowed;
        UpdateDisplay();
        return false;
    }

    private void EndInteract(bool stop=true)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
        if(stop)
            StopAllCoroutines();
        UpdateDisplay();
        UpdateModuleDisableDisplay();
    }

    private int GetMaxModules()
    {
        _maxModules = GameInfo.GetMaximumBombModules();
        _maxFrontFace = GameInfo.GetMaximumModulesFrontFace();
        return Settings.FrontFaceOnly ? _maxFrontFace : _maxModules;
    }

    private const float StartDelay = 0.2f;
    private const float Acceleration = 0.005f;
    private const float MinDelay = 0.01f;

    private IEnumerator AddTimer(int timer)
    {
        var delay = StartDelay;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        while (true)
        {
            
            Settings.Time += ((timer / 30) * 30);
            UpdateDisplay();
            yield return new WaitForSeconds(Mathf.Max(delay, MinDelay));
            delay -= Acceleration;
            if (timer > 0)
                timer++;
            else
                timer--;
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.FastestTimerBeep, transform);
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private IEnumerator AddModules(int count)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        float countFloat = count;
        var delay = StartDelay;
        while (true)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.FastestTimerBeep, transform);
            Settings.Modules += (int)countFloat;
            UpdateDisplay();
            yield return new WaitForSeconds(Mathf.Max(delay, MinDelay));
            delay -= Acceleration;
            if (count > 0)
                countFloat += (1.0f / 30.0f);
            else
                countFloat -= (1.0f / 30.0f);
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private IEnumerator AddBombs(int count)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        var delay = StartDelay;
        while (true)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.FastestTimerBeep, transform);
            Settings.Bombs += count;
            UpdateDisplay();
            yield return new WaitForSeconds(Mathf.Max(delay, MinDelay));
            delay -= Acceleration;
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private IEnumerator AddStrikes(int count)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        var delay = StartDelay;
        float countFloat = count;
        while (true)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.FastestTimerBeep, transform);
            Settings.Strikes += (int)countFloat;
            UpdateDisplay();
            yield return new WaitForSeconds(Mathf.Max(delay, MinDelay));
            delay -= Acceleration;
            if (count > 0)
                countFloat += (1.0f / 30.0f);
            else
                countFloat -= (1.0f / 30.0f);
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private IEnumerator AddWidgets(int count)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        var delay = StartDelay;
        while (true)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.FastestTimerBeep, transform);
            Settings.Widgets += count;
            UpdateDisplay();
            yield return new WaitForSeconds(Mathf.Max(delay, MinDelay));
            delay -= Acceleration;
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private IEnumerator AddSeed(int count)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (!VanillaRuleModifier.Installed())
            yield break;
        var delay = StartDelay;
        float countFloat = count;
        var seed = VanillaRuleModifier.GetRuleSeed();
        while (true)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.FastestTimerBeep, transform);
            //seedAPI.SetRuleSeed(seedAPI.GetRuleSeed() + (int)countFloat);
            seed += (int) countFloat;
            VanillaRuleModifier.SetRuleSeed(seed);
            UpdateDisplay();
            yield return new WaitForSeconds(Mathf.Max(delay, MinDelay));
            delay -= Acceleration;
            if (count > 0)
                countFloat += (1.0f / 30.0f);
            else
                countFloat -= (1.0f / 30.0f);
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private bool _resetting = false;

    private IEnumerator ResetSettings()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.NormalTimerBeep, transform);
        _resetting = true;
        var prev = 4;
        for (var i = 5f; i > 0; i-=Time.deltaTime)
        {
            if (prev > i)
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.NormalTimerBeep, transform);
                prev = Mathf.FloorToInt(i);
            }
            BombsText.text = string.Format("Reset in {0:0.00}", i);
            yield return null;
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BombExplode, transform);
        _modSettings.Settings = new ModuleSettings();
        _resetting = false;

        UpdateDisplay();
        UpdateModuleDisableDisplay();

        BombsText.text = "Settings Reset";
        yield return new WaitForSeconds(3);

        UpdateDisplay();
        UpdateModuleDisableDisplay();
    }

    private void CancelSettingsReset()
    {
        if (!_resetting)
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
        else
            EndInteract();

        _resetting = false;
    }
    
    private bool SaveSettings(bool sound = true)
    {
        if (sound)
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        _modSettings.WriteSettings();
        VanillaRuleModifier.SetRuleSeed(VanillaRuleModifier.GetRuleSeed(), true);
        return false;
    }

    private void ClampSettings()
    {
        Settings.Time = Mathf.Max(30, Settings.Time);
        Settings.Modules = Mathf.Clamp(Settings.Modules, 1, GetMaxModules());
        Settings.Strikes = Mathf.Max(1, Settings.Strikes);
        Settings.Bombs = Mathf.Clamp(Settings.Bombs, 1, MultipleBombs.GetMaximumBombCount());
        Settings.Widgets = Mathf.Clamp(Settings.Widgets, 0, 50);
        Settings.NeedyModules = Mathf.Clamp(Settings.NeedyModules, 0, Settings.Modules - 1);

        if (Settings.Playmode > PlayMode.ModsOnly)
            Settings.Playmode = PlayMode.AllModules;
        if (Settings.Playmode != PlayMode.ModsOnly) return;

        if (GameInfo.GetAvailableModuleInfo().All(x => !x.IsMod))
            Settings.Playmode = PlayMode.AllModules;
    }

    private void UpdateDisplay()
    {
        ClampSettings();

        var t = TimeSpan.FromSeconds(Settings.Time);
        TimeText.text = t.ToString();
        if (TimeText.text.StartsWith("00:0"))
        {
            TimeText.text = TimeText.text.Remove(0, 4);
        }
        else if (TimeText.text.StartsWith("00:"))
        {
            TimeText.text = TimeText.text.Remove(0, 3);
        }
        else if (TimeText.text.StartsWith("0"))
        {
            TimeText.text = TimeText.text.Remove(0, 1);
        }
        ModulesText.text = "" + Settings.Modules;
        WidgetsText.text = "" + Settings.Widgets;
        BombsText.text = !MultipleBombs.Installed() ? "" : "Bombs: " + Settings.Bombs;
        StrikesText.text = "" + Settings.Strikes;
        NeediesText.text = Settings.NeedyModules > 0 ? String.Format("Needies: {0}",Settings.NeedyModules) : "Needy Off";
        DuplicateText.text = Settings.DuplicatesAllowed ? "Duplicates" : "No Duplicates";
        // ReSharper disable once SwitchStatementMissingSomeCases
        switch (Settings.Playmode)
        {
            case PlayMode.AllModules:
                PlayModeText.text = "All Modules";
                break;
            case PlayMode.VanillaOnly:
                PlayModeText.text = "Vanilla Only";
                break;
            default:
                PlayModeText.text = "Mods Only";
                break;
        }

        PacingEventsText.text = Settings.PacingEvents ? "Pacing Events On" : "Pacing Events Off";
        FrontFaceText.text = Settings.FrontFaceOnly ? "Front Face Only" : "All Faces";

        SeedText.text = VanillaRuleModifier.GetRuleSeed().ToString();
    }

    private bool ChangeModuleDisableIndex(int diff)
    {
        if (diff != 0)
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

        if (_vanillaModules.Count == 0)
            return false;

        Settings.ModuleDisableIndex += diff;
        if (Settings.ModuleDisableIndex < 0)
        {
            Settings.ModuleDisableIndex = _vanillaModules.Count - 1;
        }
        else if (Settings.ModuleDisableIndex >= _vanillaModules.Count)
        {
            Settings.ModuleDisableIndex = 0;
        }

        UpdateModuleDisableDisplay();
        return false;
    }

    private void UpdateModuleDisableDisplay()
    {
        if (_vanillaModules.Count <= 0) return;

        var moduleInfo = _vanillaModules[Settings.ModuleDisableIndex];
        ModuleDisableText.text = moduleInfo.DisplayName;
        ModuleDisableText.color = Settings.DisabledModuleIds.Contains(moduleInfo.ModuleId) 
            ? Color.red 
            : Color.white;
    }

    private bool ModuleDisableButtonPressed()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (_vanillaModules.Count <= 0) return false;

        var moduleInfo = _vanillaModules[Settings.ModuleDisableIndex];
        if(Settings.DisabledModuleIds.Contains(moduleInfo.ModuleId))
        {
            Settings.DisabledModuleIds.Remove(moduleInfo.ModuleId);
        }
        else
        {
            Settings.DisabledModuleIds.Add(moduleInfo.ModuleId);
        }

        UpdateModuleDisableDisplay();
        return false;
    }

    private IEnumerator AddNeedyModules(int count)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        float countFloat = count;
        var delay = StartDelay;
        while (true)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.FastestTimerBeep, transform);
            Settings.NeedyModules += (int)countFloat;
            if (Settings.NeedyModules >= Settings.Modules)
                Settings.Modules = Settings.NeedyModules + 1;
            UpdateDisplay();
            yield return new WaitForSeconds(Mathf.Max(delay, MinDelay));
            delay -= Acceleration;
            if (count > 0)
                countFloat += (1.0f / 30.0f);
            else
                countFloat -= (1.0f / 30.0f);
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private bool ChangePlayMode()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Settings.Playmode++;
        UpdateDisplay();
        return false;
    }

    private bool ChangePacingEvent()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Settings.PacingEvents = !Settings.PacingEvents;
        UpdateDisplay();
        return false;
    }

    private bool ChangeFrontFace()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        Settings.FrontFaceOnly = !Settings.FrontFaceOnly;
        UpdateDisplay();
        return false;
    }

    private bool StartMission()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (Settings.Modules > GetMaxModules())
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
            return false;
        }

        var generatorSettings = new KMGeneratorSetting
        {
            NumStrikes = Settings.Strikes,
            TimeLimit = Settings.Time,
            FrontFaceOnly = Settings.FrontFaceOnly,
            ComponentPools = Settings.DuplicatesAllowed ? BuildComponentPools() : BuildNoDuplicatesPool()
        };

        if (generatorSettings.ComponentPools.Count == 0)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
            return false;
        }

        if (Settings.Bombs > 1)
        {
            generatorSettings.ComponentPools.Add(new KMComponentPool
            {
                ModTypes = new List<string> { "Multiple Bombs" },
                Count = Settings.Bombs - 1
            });
        }

        generatorSettings.OptionalWidgetCount = Settings.Widgets;

        var mission = ScriptableObject.CreateInstance<KMMission>();
        mission.DisplayName = "Custom Freeplay";
        mission.GeneratorSetting = generatorSettings;
        mission.PacingEventsEnabled = Settings.PacingEvents;

        SaveSettings();
        GetComponent<KMGameCommands>().StartMission(mission, "" + -1);
        return false;
    }

    private KMComponentPool AddComponent(KMGameInfo.KMModuleInfo module)
    {
        var pool = new KMComponentPool
        {
            ComponentTypes = new List<KMComponentPool.ComponentTypeEnum>(),
            ModTypes = new List<string>()
        };
        if (module.IsMod)
            pool.ModTypes.Add(module.ModuleId);
        else
            pool.ComponentTypes.Add(module.ModuleType);
        pool.Count = 1;
        return pool;
    }

    private KMGameInfo.KMModuleInfo PopModule(ICollection<KMGameInfo.KMModuleInfo> vanillaModules, ICollection<KMGameInfo.KMModuleInfo> moddedModules, ref List<KMGameInfo.KMModuleInfo> output)
    {
        if(vanillaModules == null || moddedModules == null || output == null)
            throw new NullReferenceException();

        if (output.Count == 0)
        {
            switch (Settings.Playmode)
            {
                case PlayMode.AllModules:
                    if (vanillaModules.Count > 0)
                        output.AddRange(vanillaModules);
                    if (moddedModules.Count > 0)
                        output.AddRange(moddedModules);
                    break;
                case PlayMode.VanillaOnly:
                    if (vanillaModules.Count > 0)
                        output.AddRange(vanillaModules);
                    break;
                case PlayMode.ModsOnly:
                    if (moddedModules.Count > 0)
                        output.AddRange(moddedModules);
                    break;
            }
            if (output.Count == 0)
                throw new Exception("No Modules to return");
            output = output.OrderBy(x => Random.value).ToList();
        }
        var module = output[0];
        output.RemoveAt(0);
        return module;
    }

    private List<KMComponentPool> BuildNoDuplicatesPool()
    {
        var pools = new List<KMComponentPool>();

        var moddedSolvableModules = GameInfo.GetAvailableModuleInfo().Where(x => x.IsMod && !x.IsNeedy).ToList();
        var moddedNeedyModules = GameInfo.GetAvailableModuleInfo().Where(x => x.IsMod && x.IsNeedy).ToList();

        var vanillaSolvableModules = GameInfo.GetAvailableModuleInfo().Where(x => !x.IsMod && !x.IsNeedy && !Settings.DisabledModuleIds.Contains(x.ModuleId)).ToList();
        var vanillaNeedyModules = GameInfo.GetAvailableModuleInfo().Where(x => !x.IsMod && x.IsNeedy && !Settings.DisabledModuleIds.Contains(x.ModuleId)).ToList();

        var solvableModules = new List<KMGameInfo.KMModuleInfo>();
        var needyModules = new List<KMGameInfo.KMModuleInfo>();

        try
        {
            for (var i = 0; i < Settings.NeedyModules; i++)
            {
                pools.Add(AddComponent(PopModule(vanillaNeedyModules, moddedNeedyModules, ref needyModules)));
            }

            for (var i = Settings.NeedyModules; i < Settings.Modules; i++)
            {
                pools.Add(AddComponent(PopModule(vanillaSolvableModules, moddedSolvableModules, ref solvableModules)));
            }

        }
        catch
        {
            pools.Clear();
            return pools;
        }

        return pools;
    }

    private List<KMComponentPool> BuildComponentPools()
    {
        var pools = new List<KMComponentPool>();

        var solvablePool = new KMComponentPool
        {
            ComponentTypes = new List<KMComponentPool.ComponentTypeEnum>(),
            ModTypes = new List<string>()
        };

        var needyPool = new KMComponentPool
        {
            ComponentTypes = new List<KMComponentPool.ComponentTypeEnum>(),
            ModTypes = new List<string>()
        };

        foreach (var moduleInfo in GameInfo.GetAvailableModuleInfo())
        {
            if (Settings.DisabledModuleIds.Contains(moduleInfo.ModuleId)) continue;
            KMComponentPool pool;

            if (moduleInfo.IsNeedy)
            {
                if (Settings.NeedyModules < 1)
                    continue;
                pool = needyPool;
            }
            else
            {
                pool = solvablePool;
            }

            if(moduleInfo.IsMod)
            {
                if (Settings.Playmode == PlayMode.VanillaOnly)
                    continue;
                pool.ModTypes.Add(moduleInfo.ModuleId);
            }
            else
            {
                if (Settings.Playmode == PlayMode.ModsOnly)
                    continue;
                pool.ComponentTypes.Add(moduleInfo.ModuleType);
            }
        }
        

        solvablePool.Count = ((needyPool.ComponentTypes.Count + needyPool.ModTypes.Count) > 0) 
            ? Settings.Modules - Settings.NeedyModules 
            : Settings.Modules;

        needyPool.Count = ((needyPool.ComponentTypes.Count + needyPool.ModTypes.Count) > 0) 
            ? Settings.NeedyModules 
            : 0;

        if ((solvablePool.ComponentTypes.Count + solvablePool.ModTypes.Count) > 0)
            pools.Add(solvablePool);
        else
            return pools;

        if((needyPool.ComponentTypes.Count + needyPool.ModTypes.Count) > 0)
        {
            pools.Add(needyPool);
        }
        else if (Settings.NeedyModules > 0)
        {
            pools.Clear();
            return pools;
        }

        return pools;
    }
}

public enum PlayMode
{
    AllModules,
    VanillaOnly,
    ModsOnly
}
