using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BombCreator : MonoBehaviour
{
    public TextMesh TimeText;
    public KMSelectable TimeMinusButton;
    public KMSelectable TimePlusButton;

    public TextMesh ModulesText;
    public KMSelectable ModulesMinusButton;
    public KMSelectable ModulesPlusButton;

    public TextMesh BombsText;
    public KMSelectable BombsMinusButton;
    public KMSelectable BombsPlusButton;

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

    public TextMesh NeediesText;
    public KMSelectable NeedyButton;

    public TextMesh PlayModeText;
    public KMSelectable PlayModeButton;

    public TextMesh PacingEventsText;
    public KMSelectable PacingEventsButton;

    public TextMesh FrontFaceText;
    public KMSelectable FrontFaceButton;

    public KMSelectable StartButton;
    List<KMGameInfo.KMModuleInfo> vanillaModules;

    public KMAudio Audio;

    int maxModules = 11;
    int maxFrontFace = 5;

    private ModSettings settings = new ModSettings("BombCreator");

    void Start()
    {
        settings.ReadSettings();

        vanillaModules = GetComponent<KMGameInfo>().GetAvailableModuleInfo().Where(x => !x.IsMod).ToList();
        maxModules = GetComponent<KMGameInfo>().GetMaximumBombModules();
        maxFrontFace = CommonReflectedTypeInfo.GetMaximumFrontFace();
        if (maxFrontFace < 0)
            maxFrontFace = (maxModules / 2);

        
        settings.Settings.modules = Mathf.Clamp(settings.Settings.modules, 1, settings.Settings.FrontFaceOnly ? maxFrontFace : maxModules);
        if (vanillaModules == null)
        {
            vanillaModules = CreateTempModules();
        }

        UpdateDisplay();
        ChangeModuleDisableIndex(0);

        TimeMinusButton.OnInteract += delegate () { StartCoroutine(AddTimer(-30)); return false; };
        TimePlusButton.OnInteract += delegate () { StartCoroutine(AddTimer(30)); return false; };
        ModulesMinusButton.OnInteract += delegate () { StartCoroutine(AddModules(-1)); return false; };
        ModulesPlusButton.OnInteract += delegate () { StartCoroutine(AddModules(1)); return false; };
        WidgetsMinusButton.OnInteract += delegate () {  StartCoroutine(AddWidgets(-1)); return false; };
        WidgetsPlusButton.OnInteract += delegate () {  StartCoroutine(AddWidgets(1)); return false; };
        BombsMinusButton.OnInteract += delegate () { StartCoroutine(AddBombs(-1)); return false; };
        BombsPlusButton.OnInteract += delegate () { StartCoroutine(AddBombs(1)); return false; };
        StrikesMinusButton.OnInteract += delegate () {  StartCoroutine(AddStrikes(-1)); return false; };
        StrikesPlusButton.OnInteract += delegate () { StartCoroutine(AddStrikes(1)); return false; };

        TimeMinusButton.OnInteractEnded += StopAllCoroutines;
        TimePlusButton.OnInteractEnded += StopAllCoroutines;
        ModulesMinusButton.OnInteractEnded += StopAllCoroutines;
        ModulesPlusButton.OnInteractEnded += StopAllCoroutines;
        WidgetsMinusButton.OnInteractEnded += StopAllCoroutines;
        WidgetsPlusButton.OnInteractEnded += StopAllCoroutines;
        BombsMinusButton.OnInteractEnded += StopAllCoroutines;
        BombsPlusButton.OnInteractEnded += StopAllCoroutines;
        StrikesMinusButton.OnInteractEnded += StopAllCoroutines;
        StrikesPlusButton.OnInteractEnded += StopAllCoroutines;

        ModuleDisableMinusButton.OnInteract += () => ChangeModuleDisableIndex(-1);
        ModuleDisablePlusButton.OnInteract += () => ChangeModuleDisableIndex(1);
        ModuleDisableButton.OnInteract += ModuleDisableButtonPressed;

        NeedyButton.OnInteract += ChangeNeedyMode;
        PlayModeButton.OnInteract += ChangePlayMode;

        PacingEventsButton.OnInteract += ChangePacingEvent;
        FrontFaceButton.OnInteract += ChangeFrontFace;

        StartButton.OnInteract += StartMission;
    }

    private static float startDelay = 0.2f;
    private static float Acceleration = 0.005f;
    private static float minDelay = 0.01f;

    private IEnumerator AddTimer(int timer)
    {
        float delay = startDelay;
        while(true)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            settings.Settings.time += ((timer / 30) * 30);
            settings.Settings.time = Mathf.Max(30, settings.Settings.time);
            UpdateDisplay();
            yield return new WaitForSeconds(Mathf.Max(delay, minDelay));
            delay -= Acceleration;
            if (timer > 0)
                timer++;
            else
                timer--;
        }
    }

    private IEnumerator AddModules(int count)
    {
        maxModules = GetComponent<KMGameInfo>().GetMaximumBombModules();
        maxFrontFace = CommonReflectedTypeInfo.GetMaximumFrontFace();
        float countFloat = count;
        if (maxFrontFace < 0)
            maxFrontFace = maxModules / 2;
        float delay = startDelay;
        while (true)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            settings.Settings.modules += (int)countFloat;
            settings.Settings.modules = Mathf.Clamp(settings.Settings.modules, 1, settings.Settings.FrontFaceOnly ? maxFrontFace : maxModules);
            UpdateDisplay();
            yield return new WaitForSeconds(Mathf.Max(delay, minDelay));
            delay -= Acceleration;
            if (count > 0)
                countFloat += (1.0f / 30.0f);
            else
                countFloat -= (1.0f / 30.0f);
        }
    }

    private IEnumerator AddBombs(int count)
    {
        float delay = startDelay;
        while (true)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            settings.Settings.bombs += count;
            settings.Settings.bombs = Mathf.Max(1, settings.Settings.bombs);
            UpdateDisplay();
            yield return new WaitForSeconds(Mathf.Max(delay, minDelay));
            delay -= Acceleration;
        }
    }

    private IEnumerator AddStrikes(int count)
    {
        float delay = startDelay;
        float countFloat = count;
        while (true)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            settings.Settings.strikes += (int)countFloat;
            settings.Settings.strikes = Mathf.Max(1, settings.Settings.strikes);
            UpdateDisplay();
            yield return new WaitForSeconds(Mathf.Max(delay, minDelay));
            delay -= Acceleration;
            if (count > 0)
                countFloat += (1.0f / 30.0f);
            else
                countFloat -= (1.0f / 30.0f);
        }
    }

    private IEnumerator AddWidgets(int count)
    {
        float delay = startDelay;
        while (true)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
            settings.Settings.widgets += count;
            settings.Settings.widgets = Mathf.Clamp(settings.Settings.widgets, 0, 50);
            UpdateDisplay();
            yield return new WaitForSeconds(Mathf.Max(delay, minDelay));
            delay -= Acceleration;
        }
    }

    private List<KMGameInfo.KMModuleInfo> CreateTempModules()
    {
        List<KMGameInfo.KMModuleInfo> tempModules = new List<KMGameInfo.KMModuleInfo>();

        KMGameInfo.KMModuleInfo info1 = new KMGameInfo.KMModuleInfo();
        info1.DisplayName = "Module 1";
        info1.ModuleId = "Module1";
        tempModules.Add(info1);

        KMGameInfo.KMModuleInfo info2 = new KMGameInfo.KMModuleInfo();
        info2.DisplayName = "Module 2";
        info1.ModuleId = "Module2";
        tempModules.Add(info2);
        
        return tempModules;
    }

    void UpdateDisplay()
    {
        var t = TimeSpan.FromSeconds(settings.Settings.time);
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
        ModulesText.text = "" + settings.Settings.modules;
        WidgetsText.text = "" + settings.Settings.widgets;
        BombsText.text = "" + settings.Settings.bombs;
        StrikesText.text = "" + settings.Settings.strikes;
        NeediesText.text = settings.Settings.neediesEnabled ? "Needy On" : "Needy Off";
        if (settings.Settings.playmode == PlayMode.AllModules)
            PlayModeText.text = "All Modules";
        else if (settings.Settings.playmode == PlayMode.VanillaOnly)
            PlayModeText.text = "Vanilla Only";
        else
            PlayModeText.text = "Mods Only";

        PacingEventsText.text = settings.Settings.PacingEvents ? "Pacing Events On" : "Pacing Events Off";
        FrontFaceText.text = settings.Settings.FrontFaceOnly ? "Front Face Only" : "All Faces";
    }

    bool ChangeModuleDisableIndex(int diff)
    {
        if (diff != 0)
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

        if (vanillaModules.Count == 0)
            return false;

        settings.Settings.moduleDisableIndex += diff;
        if (settings.Settings.moduleDisableIndex < 0)
        {
            settings.Settings.moduleDisableIndex = vanillaModules.Count - 1;
        }
        else if (settings.Settings.moduleDisableIndex >= vanillaModules.Count)
        {
            settings.Settings.moduleDisableIndex = 0;
        }

        UpdateModuleDisableDisplay();
        return false;
    }

    void UpdateModuleDisableDisplay()
    {
        if(vanillaModules.Count > 0)
        {
            KMGameInfo.KMModuleInfo moduleInfo = vanillaModules[settings.Settings.moduleDisableIndex];
            ModuleDisableText.text = moduleInfo.DisplayName;
            if(settings.Settings.disabledModuleIds.Contains(moduleInfo.ModuleId))
            {
                ModuleDisableText.color = Color.red;
            }
            else
            {
                ModuleDisableText.color = Color.white;
            }
        }
    }

    bool ModuleDisableButtonPressed()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (vanillaModules.Count > 0)
        {
            KMGameInfo.KMModuleInfo moduleInfo = vanillaModules[settings.Settings.moduleDisableIndex];
            if(settings.Settings.disabledModuleIds.Contains(moduleInfo.ModuleId))
            {
                settings.Settings.disabledModuleIds.Remove(moduleInfo.ModuleId);
            }
            else
            {
                settings.Settings.disabledModuleIds.Add(moduleInfo.ModuleId);
            }

            UpdateModuleDisableDisplay();
        }
        return false;
    }

    bool ChangeNeedyMode()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        settings.Settings.neediesEnabled = !settings.Settings.neediesEnabled;
        UpdateDisplay();
        return false;
    }

    bool ChangePlayMode()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        settings.Settings.playmode++;
        if (settings.Settings.playmode > PlayMode.ModsOnly)
            settings.Settings.playmode = PlayMode.AllModules;

        if (settings.Settings.playmode == PlayMode.ModsOnly)
        {
            if (GetComponent<KMGameInfo>().GetAvailableModuleInfo().All(x => !x.IsMod))
                settings.Settings.playmode = PlayMode.AllModules;
        }

        UpdateDisplay();
        return false;
    }

    bool ChangePacingEvent()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        settings.Settings.PacingEvents = !settings.Settings.PacingEvents;
        UpdateDisplay();
        return false;
    }

    bool ChangeFrontFace()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        settings.Settings.FrontFaceOnly = !settings.Settings.FrontFaceOnly;
        UpdateDisplay();
        return false;
    }

    bool StartMission()
    {
        var maxAll = GetComponent<KMGameInfo>().GetMaximumBombModules();
        var maxFront = CommonReflectedTypeInfo.GetMaximumFrontFace();
        if (maxFrontFace < 0)
            maxFrontFace = maxModules / 2;

        if (settings.Settings.modules > (settings.Settings.FrontFaceOnly ? maxFront : maxAll))
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
            return false;
        }

        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        KMGeneratorSetting generatorSettings = new KMGeneratorSetting();
        generatorSettings.NumStrikes = settings.Settings.strikes;
        generatorSettings.TimeLimit = settings.Settings.time;
        generatorSettings.FrontFaceOnly = settings.Settings.FrontFaceOnly;
        
        generatorSettings.ComponentPools = BuildComponentPools();
        if (generatorSettings.ComponentPools.Count == 0)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
            return false;
        }

        generatorSettings.OptionalWidgetCount = settings.Settings.widgets;

        KMMission mission = ScriptableObject.CreateInstance<KMMission>() as KMMission;
        mission.DisplayName = "Custom Freeplay";
        mission.GeneratorSetting = generatorSettings;
        mission.PacingEventsEnabled = settings.Settings.PacingEvents;

        //GetComponent<KMGameCommands>().StartMission(mission, "" + UnityEngine.Random.Range(0, int.MaxValue));
        GetComponent<KMGameCommands>().StartMission(mission, "" + -1);
        settings.WriteSettings();
        return false;
    }

    List<KMComponentPool> BuildComponentPools()
    {
        List<KMComponentPool> pools = new List<KMComponentPool>();

        KMComponentPool solvablePool = new KMComponentPool();
        solvablePool.ComponentTypes = new List<KMComponentPool.ComponentTypeEnum>();
        solvablePool.ModTypes = new List<string>();

        KMComponentPool needyPool = new KMComponentPool();
        needyPool.ComponentTypes = new List<KMComponentPool.ComponentTypeEnum>();
        needyPool.ModTypes = new List<string>();

        KMComponentPool bombPool = new KMComponentPool();
        bombPool.ModTypes = new List<string>();

        foreach (KMGameInfo.KMModuleInfo moduleInfo in GetComponent<KMGameInfo>().GetAvailableModuleInfo())
        {
            if(!settings.Settings.disabledModuleIds.Contains(moduleInfo.ModuleId))
            {
                KMComponentPool pool = moduleInfo.IsNeedy ? needyPool : solvablePool;
                if (moduleInfo.IsNeedy && !settings.Settings.neediesEnabled)
                    continue;
                if(moduleInfo.IsMod)
                {
                    if (settings.Settings.playmode == PlayMode.VanillaOnly)
                        continue;
                    pool.ModTypes.Add(moduleInfo.ModuleId);
                }
                else
                {
                    if (settings.Settings.playmode == PlayMode.ModsOnly)
                        continue;
                    pool.ComponentTypes.Add(moduleInfo.ModuleType);
                }
                
            }
        }
        

        solvablePool.Count = needyPool.ComponentTypes.Count + needyPool.ModTypes.Count > 0 ? settings.Settings.modules - 1 : settings.Settings.modules;
        needyPool.Count = needyPool.ComponentTypes.Count + needyPool.ModTypes.Count > 0 ? 1 : 0;

        if(solvablePool.ComponentTypes.Count + solvablePool.ModTypes.Count > 0)
            pools.Add(solvablePool);

        if(needyPool.ComponentTypes.Count + needyPool.ModTypes.Count > 0 && pools.Count > 0)
            pools.Add(needyPool);

        /*
        if (settings.Settings.bombs > 1 && pools.Count > 0)
        {
            bombPool.ModTypes.Add("Multiple Bombs");
            bombPool.Count = settings.Settings.bombs - 1;
            pools.Add(bombPool);
        }*/

        if (settings.Settings.neediesEnabled && !pools.Contains(needyPool))
        {
            pools.Clear();
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
