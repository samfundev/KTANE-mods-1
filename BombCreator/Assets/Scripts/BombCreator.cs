using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;
using Random = System.Random;

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
    public KMSelectable WidgetsMinimumMinusButton;
    public KMSelectable WidgetsMinimumPlusButton;
    public KMSelectable WidgetsMaximumMinusButton;
    public KMSelectable WidgetsMaximumPlusButton;

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
    public KMSelectable PlayModeMinusButton;
    public KMSelectable PlayModePlusButton;

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

    private Random _random;

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

        WidgetsMinimumMinusButton.OnInteract += delegate {  StartCoroutine(AddWidgetsMinimum(-1)); return false; };
        WidgetsMinimumPlusButton.OnInteract += delegate {  StartCoroutine(AddWidgetsMinimum(1)); return false; };
        WidgetsMaximumMinusButton.OnInteract += delegate { StartCoroutine(AddWidgetsMaximum(-1)); return false; };
        WidgetsMaximumPlusButton.OnInteract += delegate { StartCoroutine(AddWidgetsMaximum(1)); return false; };

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
        PlayModeMinusButton.OnInteract += delegate { StartCoroutine(AddVanillaModules(1)); return false; };
        PlayModePlusButton.OnInteract += delegate { StartCoroutine(AddVanillaModules(-1)); return false; };


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

        WidgetsMinimumMinusButton.OnInteractEnded += () => EndInteract();
        WidgetsMinimumPlusButton.OnInteractEnded += () => EndInteract();
        WidgetsMaximumMinusButton.OnInteractEnded += () => EndInteract();
        WidgetsMaximumPlusButton.OnInteractEnded += () => EndInteract();

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
        PlayModeMinusButton.OnInteractEnded += () => EndInteract();
        PlayModePlusButton.OnInteractEnded += () => EndInteract();

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

    private IEnumerator AddWidgetsMinimum(int count)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        var delay = StartDelay;
        while (true)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.FastestTimerBeep, transform);
            Settings.WidgetsMinimum += count;
            UpdateDisplay();
            yield return new WaitForSeconds(Mathf.Max(delay, MinDelay));
            delay -= Acceleration;
        }
        // ReSharper disable once IteratorNeverReturns
    }

    private IEnumerator AddWidgetsMaximum(int count)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        var delay = StartDelay;
        while (true)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.FastestTimerBeep, transform);
            Settings.WidgetsMaximum += count;
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
        Settings.WidgetsMinimum = Mathf.Clamp(Settings.WidgetsMinimum, 0, Settings.WidgetsMaximum);
        Settings.WidgetsMaximum = Mathf.Clamp(Settings.WidgetsMaximum, Settings.WidgetsMinimum, 50);
        Settings.NeedyModules = Mathf.Clamp(Settings.NeedyModules, 0, Settings.Modules - 1);

        Settings.VanillaModules = Mathf.Clamp(Settings.VanillaModules, 0, 100);
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
        WidgetsText.text = string.Format("{0} to {1}", Settings.WidgetsMinimum, Settings.WidgetsMaximum);
        BombsText.text = !MultipleBombs.Installed() ? "" : "Bombs: " + Settings.Bombs;
        StrikesText.text = "" + Settings.Strikes;
        NeediesText.text = Settings.NeedyModules > 0 ? string.Format("Needies: {0}",Settings.NeedyModules) : "Needy Off";
        DuplicateText.text = Settings.DuplicatesAllowed ? "Duplicates" : "No Duplicates";
        // ReSharper disable once SwitchStatementMissingSomeCases
        switch (Settings.VanillaModules)
        {
            case 0:
                PlayModeText.text = "Mods Only";
                break;
            case 100:
                PlayModeText.text = "Vanilla Only";
                break;
            default:
                PlayModeText.text = string.Format("Vanilla {0}% - Mods {1}%", Settings.VanillaModules, 100 - Settings.VanillaModules);
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

    private IEnumerator AddVanillaModules(int count)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        var delay = StartDelay;
        while (true)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.FastestTimerBeep, transform);
            Settings.VanillaModules += count;
            UpdateDisplay();
            yield return new WaitForSeconds(Mathf.Max(delay, MinDelay));
            delay -= Acceleration;
        }
        // ReSharper disable once IteratorNeverReturns
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
        _random = new Random(((int)Time.time));
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

        generatorSettings.OptionalWidgetCount = _random.Next(Settings.WidgetsMinimum, Settings.WidgetsMaximum);

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
            ModTypes = new List<string>(),
            Count = 1
        };
        if (module.IsMod)
            pool.ModTypes.Add(module.ModuleId);
        else
            pool.ComponentTypes.Add(module.ModuleType);
        
        return pool;
    }

    private void AddComponent(KMGameInfo.KMModuleInfo module, ref KMComponentPool pool)
    {
        if (pool == null)
        {
            pool = new KMComponentPool
            {
                ComponentTypes = new List<KMComponentPool.ComponentTypeEnum>(),
                ModTypes = new List<string>(),
                Count = 1
            };
        }
        if (module.IsMod)
            pool.ModTypes.Add(module.ModuleId);
        else
            pool.ComponentTypes.Add(module.ModuleType);
    }

    private KMGameInfo.KMModuleInfo PopModule(ICollection<KMGameInfo.KMModuleInfo> modules, ref List<KMGameInfo.KMModuleInfo> output, string moduleType)
    {
        if(modules == null || output == null)
            throw new NullReferenceException();

        if (output.Count == 0)
        {
            output.AddRange(modules);
            output = output.OrderBy(x => _random.NextDouble()).ToList();
            if (output.Count == 0)
                throw new NullModuleException(string.Format("No Modules of type {0} to return", moduleType));
        }
        
        var module = output[0];
        output.RemoveAt(0);
        return module;
    }

    private List<KMComponentPool> BuildNoDuplicatesPool()
    {
        var pools = new List<KMComponentPool>();

        var vanillaModulesChance = Settings.VanillaModules / 100.0f;
        var moddedModulesChance = 1.0f - vanillaModulesChance;

        var vanillaNeedySize = Mathf.FloorToInt(Settings.NeedyModules * vanillaModulesChance);
        var moddedNeedySize = Mathf.FloorToInt(Settings.NeedyModules * moddedModulesChance);
        var mixedNeedySize = Settings.NeedyModules - vanillaNeedySize - moddedNeedySize;
        DebugLog("Initial: VanillaNeedySize = {0}, ModdedNeedySize = {1}, mixedNeedySize = {2}", vanillaNeedySize, moddedNeedySize, mixedNeedySize);

        for (var i = 0; i < mixedNeedySize; i++)
        {
            if (_random.NextDouble() < vanillaModulesChance)
                vanillaNeedySize++;
            else
                moddedNeedySize++;
        }
        DebugLog("Final: VanillaNeedySize = {0}, ModdedNeedySize = {1}", vanillaNeedySize, moddedNeedySize);


        var vanillaSolvableSize = Mathf.FloorToInt((Settings.Modules - Settings.NeedyModules) * vanillaModulesChance);
        var moddedSolvableSize = Mathf.FloorToInt((Settings.Modules - Settings.NeedyModules) * moddedModulesChance);
        var mixedSolvableSize = (Settings.Modules - Settings.NeedyModules) - vanillaSolvableSize - moddedSolvableSize;
        DebugLog("Initial: vanillaSolvableSize = {0}, moddedSolvableSize = {1}, mixedSolvableSize = {2}", vanillaSolvableSize, moddedSolvableSize, mixedSolvableSize);
        
        for (var i = 0; i < mixedSolvableSize; i++)
        {
            if (_random.NextDouble() < vanillaModulesChance)
                vanillaSolvableSize++;
            else
                moddedSolvableSize++;
        }
        DebugLog("Final: vanillaSolvableSize = {0}, moddedSolvableSize = {1}", vanillaSolvableSize, moddedSolvableSize);
        
        var moddedSolvableModules = GameInfo.GetAvailableModuleInfo().Where(x => x.IsMod && !x.IsNeedy).ToList();
        var moddedNeedyModules = GameInfo.GetAvailableModuleInfo().Where(x => x.IsMod && x.IsNeedy).ToList();

        var vanillaSolvableModules = GameInfo.GetAvailableModuleInfo().Where(x => !x.IsMod && !x.IsNeedy && !Settings.DisabledModuleIds.Contains(x.ModuleId)).ToList();
        var vanillaNeedyModules = GameInfo.GetAvailableModuleInfo().Where(x => !x.IsMod && x.IsNeedy && !Settings.DisabledModuleIds.Contains(x.ModuleId)).ToList();

        var modules = new List<KMGameInfo.KMModuleInfo>();

        var maxVanillaSolvablePerPool = Mathf.Max(vanillaNeedyModules.Count / Math.Max(vanillaSolvableSize,1), 1);
        var maxVanillaNeedyPerPool = Mathf.Max(vanillaNeedyModules.Count / Math.Max(vanillaNeedySize, 1), 1);
        var maxModSolvablePerPool = Mathf.Max(moddedSolvableModules.Count / Math.Max(moddedSolvableSize, 1), 1);
        var maxModNeedyPerPool = Mathf.Max(moddedNeedyModules.Count / Math.Max(moddedNeedySize, 1), 1);

        try
        {
            for(var i = 0; i < vanillaSolvableSize; i++)
            {
                var pool = AddComponent(PopModule(vanillaSolvableModules, ref modules, "Vanilla Solvable"));
                for (var j = 1; j < maxVanillaSolvablePerPool; j++)
                {
                    AddComponent(PopModule(vanillaSolvableModules, ref modules, "Vanilla Solvable"), ref pool);
                }
                pools.Add(pool);
            }
            modules.Clear();

            for (var i = 0; i < moddedSolvableSize; i++)
            {
                var pool = AddComponent(PopModule(moddedSolvableModules, ref modules, "Modded Solvable"));
                for (var j = 1; j < maxModSolvablePerPool; j++)
                {
                    AddComponent(PopModule(moddedSolvableModules, ref modules, "Modded Solvable"), ref pool);
                }
                pools.Add(pool);
            }
            modules.Clear();

            for (var i = 0; i < vanillaNeedySize; i++)
            {
                var pool = AddComponent(PopModule(vanillaNeedyModules, ref modules, "Vanilla Needy"));
                for (var j = 1; j < maxVanillaNeedyPerPool; j++)
                {
                    AddComponent(PopModule(vanillaNeedyModules, ref modules, "Vanilla Needy"), ref pool);
                }
                pools.Add(pool);
            }
            modules.Clear();

            for (var i = 0; i < moddedNeedySize; i++)
            {
                var pool = AddComponent(PopModule(moddedNeedyModules, ref modules, "Modded Needy"));
                for (var j = 1; j < maxModNeedyPerPool; j++)
                {
                    AddComponent(PopModule(moddedNeedyModules, ref modules, "Modded Needy"), ref pool);
                }
                pools.Add(pool);
            }
        }
        catch (NullModuleException ex)
        {
            DebugLog("Failure in No Duplicates for the following reason");
            DebugLog(ex.Message);
            pools.Clear();
            return pools;
        }
        catch (Exception ex)
        {
            DebugLog("Failure in No Duplicates due to an exception.");
            DebugLog("Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
            pools.Clear();
            return pools;
        }

        return pools;
    }

    private List<KMComponentPool> BuildComponentPools()
    {
        var pools = new List<KMComponentPool>();

        var vanillaModules = Settings.VanillaModules / 100.0f;
        var moddedModules = 1.0f - vanillaModules;

        var vanillaNeedySize = Mathf.FloorToInt(Settings.NeedyModules * vanillaModules);
        var moddedNeedySize = Mathf.FloorToInt(Settings.NeedyModules * moddedModules);
        var mixedNeedySize = Settings.NeedyModules - vanillaNeedySize - moddedNeedySize;

        for (var i = 0; i < mixedNeedySize; i++)
        {
            if (_random.NextDouble() < vanillaModules)
                vanillaNeedySize++;
            else
                moddedNeedySize++;
        }


        var vanillaSolvableSize = Mathf.FloorToInt((Settings.Modules - Settings.NeedyModules) * vanillaModules);
        var moddedSolvableSize = Mathf.FloorToInt((Settings.Modules - Settings.NeedyModules) * moddedModules);
        var mixedSolvableSize = (Settings.Modules - Settings.NeedyModules) - vanillaSolvableSize - moddedSolvableSize;

        for (var i = 0; i < mixedSolvableSize; i++)
        {
            if (_random.NextDouble() < vanillaModules)
                vanillaSolvableSize++;
            else
                moddedSolvableSize++;
        }

        var moddedSolvableModules = GameInfo.GetAvailableModuleInfo().Where(x => x.IsMod && !x.IsNeedy).ToList();
        var moddedNeedyModules = GameInfo.GetAvailableModuleInfo().Where(x => x.IsMod && x.IsNeedy).ToList();

        var vanillaSolvableModules = GameInfo.GetAvailableModuleInfo().Where(x => !x.IsMod && !x.IsNeedy && !Settings.DisabledModuleIds.Contains(x.ModuleId)).ToList();
        var vanillaNeedyModules = GameInfo.GetAvailableModuleInfo().Where(x => !x.IsMod && x.IsNeedy && !Settings.DisabledModuleIds.Contains(x.ModuleId)).ToList();

        if ((vanillaNeedyModules.Count == 0 && vanillaNeedySize > 0) ||
            (vanillaSolvableModules.Count == 0 && vanillaSolvableSize > 0) ||
            (moddedNeedyModules.Count == 0 && moddedNeedySize > 0) ||
            (moddedSolvableModules.Count == 0 && moddedSolvableSize > 0))
        {
            DebugLog("Duplicates Allowed Failure");

            if (vanillaNeedyModules.Count == 0 && vanillaNeedySize > 0) DebugLog("VanillaNeedyModules.Count = {0}, VanillaNeedySize = {1}", vanillaNeedyModules.Count, vanillaNeedySize);
            if (vanillaSolvableModules.Count == 0 && vanillaSolvableSize > 0) DebugLog("vanillaSolvableModules.Count = {0}, vanillaSolvableSize = {1}", vanillaSolvableModules.Count, vanillaSolvableSize);
            if (moddedNeedyModules.Count == 0 && moddedNeedySize > 0) DebugLog("moddedNeedyModules.Count = {0}, moddedNeedySize = {1}", moddedNeedyModules.Count, moddedNeedySize);
            if (moddedSolvableModules.Count == 0 && moddedSolvableSize > 0) DebugLog("moddedSolvableModules.Count = {0}, moddedSolvableSize = {1}", moddedSolvableModules.Count, moddedSolvableSize);

            return pools;
        }

        if (vanillaNeedySize > 0)
        {
            var pool = new KMComponentPool
            {
                ComponentTypes = new List<KMComponentPool.ComponentTypeEnum>(),
                ModTypes = new List<string>()
            };
            pool.ComponentTypes.AddRange(vanillaNeedyModules.Select(x => x.ModuleType));
            pool.Count = vanillaNeedySize;
            pools.Add(pool);
        }

        if (moddedNeedySize > 0)
        {
            var pool = new KMComponentPool
            {
                ComponentTypes = new List<KMComponentPool.ComponentTypeEnum>(),
                ModTypes = new List<string>()
            };
            pool.ModTypes.AddRange(moddedNeedyModules.Select(x => x.ModuleId));
            pool.Count = moddedNeedySize;
            pools.Add(pool);
        }

        if (vanillaSolvableSize > 0)
        {
            var pool = new KMComponentPool
            {
                ComponentTypes = new List<KMComponentPool.ComponentTypeEnum>(),
                ModTypes = new List<string>()
            };
            pool.ComponentTypes.AddRange(vanillaSolvableModules.Select(x => x.ModuleType));
            pool.Count = vanillaSolvableSize;
            pools.Add(pool);
        }

        if (moddedSolvableSize > 0)
        {
            var pool = new KMComponentPool
            {
                ComponentTypes = new List<KMComponentPool.ComponentTypeEnum>(),
                ModTypes = new List<string>()
            };
            pool.ModTypes.AddRange(moddedSolvableModules.Select(x => x.ModuleId));
            pool.Count = moddedSolvableSize;
            pools.Add(pool);
        }
        

        return pools;
    }
}

public class NullModuleException : Exception
{
    public NullModuleException()
    {
    }

    public NullModuleException(string message) : base(message)
    {
    }

    public NullModuleException(string message, Exception inner) : base(message, inner)
    {
    }
}
