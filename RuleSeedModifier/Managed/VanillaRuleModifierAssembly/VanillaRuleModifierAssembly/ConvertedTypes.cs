using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.Manual;
using Assets.Scripts.Rules;
using log4net;
using UnityEngine;
using VanillaRuleModifierAssembly.RuleSetGenerators;
using Object = UnityEngine.Object;

namespace VanillaRuleModifierAssembly
{
    public static class CommonReflectedTypeInfo
    {
        static CommonReflectedTypeInfo()
        {
            RuleManagerType = typeof(RuleManager);
            if (RuleManagerType == null) return;

            RuleManagerInstanceField = RuleManagerType.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
            GenerateRulesMethod = RuleManagerType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
            CurrentRulesProperty = RuleManagerType.GetProperty("CurrentRules", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            WireRuleSetGenerator = new WireRuleGenerator();
            WhosOnFirstRuleSetGenerator = new WhosOnFirstRuleSetGenerator();
            MemoryRuleSetGenerator = new MemoryRuleSetGenerator();
            KeypadRuleSetGenerator = new KeypadRuleSetGenerator();
            NeedyKnobRuleSetGenerator = new NeedyKnobRuleSetGenerator();
            ButtonRuleSetGenerator = new ButtonRuleGenerator();
            WireSequenceRuleSetGenerator = new WireSetRuleGenerator();
            PasswordRuleSetGenerator = new PasswordRuleGenerator();
            //MorseCodeRuleSetGenerator = new MorseCodeRuleSetGenerator();
            MorseCodeRuleSetGenerator = new MorseCodeRuleGenerator();
            VennWireRuleSetGenerator = new VennWireGenerator();
            RhythmHeavenRuleSetGenerator = new RhythmHeavenRuleSetGenerator();
            MazeRuleSetGenerator = new MazeRuleSetGenerator();
            SimonRuleSetGenerator = new SimonRuleGenerator();
            Seed = -1;
            OriginalBombRules = null;

            BombComponentLogger = typeof(BombComponent).GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            MorseCodeModuleChosenTermField = typeof(MorseCodeComponent).GetField("chosenTerm", BindingFlags.NonPublic | BindingFlags.Instance);
            MorseCodeModuleChosenWordField = typeof(MorseCodeComponent).GetField("chosenWord", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public static void DebugLog(string message, params object[] args)
        {
            var debugstring = $"[Vanilla Rule Modifier] {message}";
            Debug.LogFormat(debugstring, args);
        }

        public static bool Initialize()
        {
            return RuleManagerType != null;
        }

        private static bool TryPrintRules(string name, AbstractRuleSet ruleSet)
        {
            try
            {
                logger.Debug($"{name} Rules:\n{ruleSet}");
                return true;
            }
            catch (Exception ex)
            {
                logger.Debug($"Could not Print rules for {name}", ex);
                return false;
            }
        }

        public static bool PrintRules(BombRules bombRules)
        {
            logger.DebugFormat("BombRules: {0}", bombRules.ManualMetaData);
            var success = true;
            success &= TryPrintRules("WireSet", bombRules.WireRuleSet);
            success &= TryPrintRules("Button", bombRules.ButtonRuleSet);
            success &= TryPrintRules("Keypad", bombRules.KeypadRuleSet);
            success &= TryPrintRules("Simon", bombRules.SimonRuleSet);
            success &= TryPrintRules("Who's On First", bombRules.WhosOnFirstRuleSet);
            success &= TryPrintRules("Memory", bombRules.MemoryRuleSet);
            success &= TryPrintRules("Morse Code", bombRules.MorseCodeRuleSet);
            success &= TryPrintRules("Venn Wire", bombRules.VennWireRuleSet);
            success &= TryPrintRules("Maze", bombRules.MazeRuleSet);
            success &= TryPrintRules("Password", bombRules.PasswordRuleSet);
            success &= TryPrintRules("Wire Sequence", bombRules.WireSequenceRuleSet);
            success &= TryPrintRules("Needy Knob", bombRules.NeedyKnobRuleSet);
            return success;
        }

        private static BombRules Initialize(int seed)
        {
            DebugLog("Generating Rules for seed {0}", seed);
            var rng = new System.Random(seed);
            var bombRules = new BombRules
            {
                ManualMetaData = new ManualMetaData
                {
                    ManualVersion = OriginalBombRules.ManualMetaData.ManualVersion,
                    //ManualVersion = $"Rule Seed Modifier {OriginalBombRules.ManualMetaData.LanguageCode}-{seed}",
                    Seed = seed,
                    LockCode = OriginalBombRules.ManualMetaData.LockCode,
                    LanguageCode = OriginalBombRules.ManualMetaData.LanguageCode,
                    IsValid = OriginalBombRules.ManualMetaData.IsValid
                },
                WireRuleSet = WireRuleSetGenerator.CreateWireRules(seed),
                WhosOnFirstRuleSet = (WhosOnFirstRuleSet) WhosOnFirstRuleSetGenerator.GenerateRuleSet(seed),
                MemoryRuleSet = (MemoryRuleSet) MemoryRuleSetGenerator.GenerateRuleSet(seed),
                KeypadRuleSet = (KeypadRuleSet) KeypadRuleSetGenerator.GenerateRuleSet(seed),
                NeedyKnobRuleSet = (NeedyKnobRuleSet) NeedyKnobRuleSetGenerator.GenerateRuleSet(seed),
                ButtonRuleSet = ButtonRuleSetGenerator.GenerateButtonRules(seed),
                WireSequenceRuleSet = WireSequenceRuleSetGenerator.GenerateWireSequenceRules(seed),
                PasswordRuleSet = PasswordRuleSetGenerator.GeneratePasswordRules(seed),
                MorseCodeRuleSet = MorseCodeRuleSetGenerator.GenerateMorseCodeRuleSet(seed),
                //MorseCodeRuleSet = (MorseCodeRuleSet) MorseCodeRuleSetGenerator.GenerateRuleSet(seed),
                VennWireRuleSet = VennWireRuleSetGenerator.GenerateVennWireRules(seed),
                RhythmHeavenRuleSet = (RhythmHeavenRuleSet) RhythmHeavenRuleSetGenerator.GenerateRuleSet(seed),
                MazeRuleSet = (MazeRuleSet) MazeRuleSetGenerator.GenerateRuleSet(seed),
                SimonRuleSet = SimonRuleSetGenerator.GenerateSimonRuleSet(seed)
			};
	        ModRuleSetGenerator.Instance.CreateRules(seed);

            try
            {
                bombRules.CacheStringValues();
            }
            catch (Exception ex)
            {
                DebugLog("Could not Print rules due to Exception: {0}", ex.Message);
                DebugLog(ex.StackTrace);
            }
            PrintRules(bombRules);

            DebugLog("Done Generating Rules for seed {0}", seed);

            return bombRules;
        }

        public static IEnumerator FixMorseCodeModule()
        {
            yield return null;
            DebugLog("Attempting to Fix morse code modules. Waiting for the Gameplay room");
            while (SceneManager.Instance.CurrentState != SceneManager.State.Gameplay)
                yield return null;

            DebugLog("Waiting for the bomb list to be available");
            while (SceneManager.Instance.GameplayState.Bombs == null || SceneManager.Instance.GameplayState.Bombs.Count == 0)
                yield return null;

            //Processing each bomb
            var bombsProcessed = new HashSet<Bomb>();
            while (SceneManager.Instance.CurrentState == SceneManager.State.Gameplay)
            {
                yield return null;
                foreach (var bomb in SceneManager.Instance.GameplayState.Bombs)
                {
                    if (!bomb.gameObject.activeSelf) continue;
                    if (bombsProcessed.Contains(bomb)) continue;
                    var morseCodeModules = bomb.GetComponentsInChildren<MorseCodeComponent>();
                    if (morseCodeModules.Length == 0)
                    {
                        bombsProcessed.Add(bomb);
                        continue;
                    }
                    foreach (var module in morseCodeModules)
                    {
                        if (string.IsNullOrEmpty((string) MorseCodeModuleChosenTermField.GetValue(module)))
                            break;

                        var moduleLogger = (ILog) BombComponentLogger.GetValue(module);

                        var chosenTerm = (string)MorseCodeModuleChosenTermField.GetValue(module);
                        if(chosenTerm.Length < 5 || chosenTerm.Length > 6)
                            continue;

                        MorseCodeModuleChosenWordField.SetValue(module, chosenTerm);
                        moduleLogger.DebugFormat("Chosen word is: {0} (\"{1}\") (freq={2}).", "RuleModifierWord", chosenTerm, module.ChosenFrequency);

                        bombsProcessed.Add(bomb);
                    }
                }
            }
        }

        public static IEnumerator AddWidgetToBomb(KMWidget widget)
        {
            DebugLog("Started AddWidgetToBomb");
            var modwidget = widget.GetComponent<ModWidget>();
            DebugLog("Tried to Get Modwidget");
            if (modwidget == null)
            {
                DebugLog("Modwidget not defined. Creating it now.");
                modwidget = widget.gameObject.AddComponent<ModWidget>();
            }

            DebugLog("Definitely have mod widget");
            var generators = Object.FindObjectsOfType<WidgetGenerator>();
            DebugLog($"{generators.Length} Widget Generators found");
            while (generators.Length == 0)
            {
                yield return null;
                generators = Object.FindObjectsOfType<WidgetGenerator>();
                DebugLog($"{generators.Length} Widget Generators found");
            }
            
            foreach(var g in generators)
            {
                if (modwidget == null) break;
                DebugLog("Adding required widget");
                if(!g.RequiredWidgets.Contains(modwidget))
                    g.RequiredWidgets.Add(modwidget);
            }

            yield break;
        }

        public static RuleManager GenerateRules(int seed)
        {
            var ruleManager = RuleManager.Instance;
            var bombRules = (BombRules) CurrentRulesProperty.GetValue(ruleManager, null);

            //If the official manual metadata has changed mid-game, grab the most current one.
            if (OriginalBombRules == null || 
                bombRules.ManualMetaData.LanguageCode != OriginalBombRules.ManualMetaData.LanguageCode ||
                bombRules.ManualMetaData.LockCode != OriginalBombRules.ManualMetaData.LockCode ||
                bombRules.ManualMetaData.ManualVersion != OriginalBombRules.ManualMetaData.ManualVersion ||
                bombRules.ManualMetaData.IsValid != OriginalBombRules.ManualMetaData.IsValid)
                OriginalBombRules = bombRules;

            else if (bombRules.ManualMetaData.ManualVersion.StartsWith("Rule Seed Modifier ") && bombRules.ManualMetaData.Seed == seed)
                return ruleManager;

            if (seed == Seed)
            {
                DebugLog("Rule Manager already initialized with seed {0}. Skipping initialization.", seed);
                return ruleManager;
            }
            Seed = seed;

            CurrentRulesProperty.SetValue(ruleManager, Initialize(seed == int.MinValue ? 0 : seed), null);
            return ruleManager;
        }

        public static void UnloadRuleManager()
        {
            if (OriginalBombRules == null) return;
            var ruleManager = RuleManager.Instance;
            CurrentRulesProperty.SetValue(ruleManager, OriginalBombRules, null);
            OriginalBombRules = null;
        }

        public static bool IsVanillaSeed => Seed == 1;
        public static bool IsModdedSeed => Seed != 1;

        public static bool IsRulesReady()
        {
            return RuleManagerInstanceField.GetValue(null) != null;
        }

        public static int Seed { get; private set; }
        public static BombRules OriginalBombRules { get; private set; }

        #region RuleManager


        public static Type RuleManagerType
        {
            get;
        }

        public static FieldInfo RuleManagerInstanceField
        {
            get;
        }

        public static MethodInfo GenerateRulesMethod
        {
            get;
        }

        public static PropertyInfo CurrentRulesProperty
        {
            get;
        }

        private static FieldInfo MorseCodeModuleChosenTermField
        {
            get;
        }

        private static FieldInfo MorseCodeModuleChosenWordField
        {
            get;
        }

        private static FieldInfo BombComponentLogger
        {
            get; 
        }

        private static readonly WireRuleGenerator WireRuleSetGenerator;
        private static readonly WhosOnFirstRuleSetGenerator WhosOnFirstRuleSetGenerator;
        private static readonly MemoryRuleSetGenerator MemoryRuleSetGenerator;
        private static readonly KeypadRuleSetGenerator KeypadRuleSetGenerator;
        private static readonly NeedyKnobRuleSetGenerator NeedyKnobRuleSetGenerator;
        private static readonly ButtonRuleGenerator ButtonRuleSetGenerator;
        private static readonly WireSetRuleGenerator WireSequenceRuleSetGenerator;
        private static readonly PasswordRuleGenerator PasswordRuleSetGenerator;
        //private static readonly MorseCodeRuleSetGenerator MorseCodeRuleSetGenerator;
        private static readonly MorseCodeRuleGenerator MorseCodeRuleSetGenerator;
        private static readonly VennWireGenerator VennWireRuleSetGenerator;
        private static readonly RhythmHeavenRuleSetGenerator RhythmHeavenRuleSetGenerator;
        private static readonly MazeRuleSetGenerator MazeRuleSetGenerator;
        private static readonly SimonRuleGenerator SimonRuleSetGenerator;
        private static ILog logger = LogManager.GetLogger(typeof(BombRules));

        #endregion

    }
}
