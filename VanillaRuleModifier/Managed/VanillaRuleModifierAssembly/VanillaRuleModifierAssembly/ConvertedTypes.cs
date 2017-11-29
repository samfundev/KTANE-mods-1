using System;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Manual;
using Assets.Scripts.Rules;
using BombGame;
using UnityEngine;
using VanillaRuleModifierAssembly.RuleSetGenerators;

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
            SeedProperty = RuleManagerType.GetProperty("Seed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            CurrentRulesProperty = RuleManagerType.GetProperty("CurrentRules", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            WireRuleSetGenerator = new WireRuleSetGenerator();
            WhosOnFirstRuleSetGenerator = new WhosOnFirstRuleSetGenerator();
            MemoryRuleSetGenerator = new MemoryRuleSetGenerator();
            KeypadRuleSetGenerator = new KeypadRuleSetGenerator();
            NeedyKnobRuleSetGenerator = new NeedyKnobRuleSetGenerator();
            ButtonRuleSetGenerator = new ButtonRuleGenerator();
            WireSequenceRuleSetGenerator = new WireSequenceRuleSetGenerator();
            PasswordRuleSetGenerator = new PasswordRuleGenerator();
            MorseCodeRuleSetGenerator = new MorseCodeRuleGenerator();
            VennWireRuleSetGenerator = new VennWireGenerator();
            RhythmHeavenRuleSetGenerator = new RhythmHeavenRuleSetGenerator();
            MazeRuleSetGenerator = new MazeRuleSetGenerator();
            SimonRuleSetGenerator = new SimonRuleGenerator();
            Seed = -1;
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

        public static void SimonTwoDistinct(int seed, RuleManager ruleManager)
        {
            if (seed == 1 || seed == 2)
                return;
            var fieldInfo = ruleManager.GetType().GetField("simonRuleSetGenerator", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo == null) return;
            var simonrulesetgenerator = fieldInfo.GetValue(ruleManager);
            var field = simonrulesetgenerator.GetType().GetField("rand", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) return;
            var rng = (System.Random) field.GetValue(simonrulesetgenerator);
            var vowel = ruleManager.SimonRuleSet.RuleList[SimonRuleSet.HAS_VOWEL_STRING];
            var otherwise = ruleManager.SimonRuleSet.RuleList[SimonRuleSet.OTHERWISE_STRING];
            for (var i = 0; i < 3; i++)
            {
                if (vowel[i].Distinct().Count() == 1)
                {
                    vowel[i][rng.Next(4)] = (SimonColor) rng.Next(4);
                }
                if (otherwise[i].Distinct().Count() == 1)
                {
                    otherwise[i][rng.Next(4)] = (SimonColor)rng.Next(4);
                }
            }
        }

        private static BombRules Initialize(int seed)
        {
            DebugLog("Generating Rules for seed {0}", seed);

            var bombRules = new BombRules
            {
                ManualMetaData = new ManualMetaData
                {
                    BombClassification = ManualMetaData.BOMB_CLASSIFICATION,
                    LockCode = ManualMetaData.LOCK_CODE
                },
                WireRuleSet = (WireRuleSet) WireRuleSetGenerator.GenerateRuleSet(seed),
                WhosOnFirstRuleSet = (WhosOnFirstRuleSet) WhosOnFirstRuleSetGenerator.GenerateRuleSet(seed),
                MemoryRuleSet = (MemoryRuleSet) MemoryRuleSetGenerator.GenerateRuleSet(seed),
                KeypadRuleSet = (KeypadRuleSet) KeypadRuleSetGenerator.GenerateRuleSet(seed),
                NeedyKnobRuleSet = (NeedyKnobRuleSet) NeedyKnobRuleSetGenerator.GenerateRuleSet(seed),
                ButtonRuleSet = ButtonRuleSetGenerator.GenerateButtonRules(seed),
                WireSequenceRuleSet = (WireSequenceRuleSet) WireSequenceRuleSetGenerator.GenerateRuleSet(seed),
                PasswordRuleSet = PasswordRuleSetGenerator.GeneratePasswordRules(seed),
                MorseCodeRuleSet = MorseCodeRuleSetGenerator.GenerateMorseCodeRuleSet(seed),
                VennWireRuleSet = VennWireRuleSetGenerator.GenerateVennWireRules(seed),
                RhythmHeavenRuleSet = (RhythmHeavenRuleSet) RhythmHeavenRuleSetGenerator.GenerateRuleSet(seed),
                MazeRuleSet = (MazeRuleSet) MazeRuleSetGenerator.GenerateRuleSet(seed),
                SimonRuleSet = SimonRuleSetGenerator.GenerateSimonRuleSet(seed)
            };
            bombRules.PrintRules();
            bombRules.CacheStringValues();

            DebugLog("Done Generating Rules for seed {0}", seed);

            return bombRules;
        }

        public static RuleManager GenerateRules(int seed)
        {
            var ruleManager = RuleManager.Instance;

            if (seed == Seed)
            {
                DebugLog("Rule Manager already initialized with seed {0}. Skipping initialization.", seed);
                return ruleManager;
            }
            Seed = seed;

            CurrentRulesProperty.SetValue(ruleManager, Initialize(seed), null);
            SeedProperty.SetValue(ruleManager, RuleManager.DEFAULT_SEED, null);
            return ruleManager;
        }


        public static bool IsRulesReady()
        {
            return RuleManagerInstanceField.GetValue(null) != null;
        }

        public static int Seed { get; private set; }

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

        public static PropertyInfo SeedProperty
        {
            get;
        }

        public static PropertyInfo CurrentRulesProperty
        {
            get;
        }

        private static readonly WireRuleSetGenerator WireRuleSetGenerator;
        private static readonly WhosOnFirstRuleSetGenerator WhosOnFirstRuleSetGenerator;
        private static readonly MemoryRuleSetGenerator MemoryRuleSetGenerator;
        private static readonly KeypadRuleSetGenerator KeypadRuleSetGenerator;
        private static readonly NeedyKnobRuleSetGenerator NeedyKnobRuleSetGenerator;
        private static readonly ButtonRuleGenerator ButtonRuleSetGenerator;
        private static readonly WireSequenceRuleSetGenerator WireSequenceRuleSetGenerator;
        private static readonly PasswordRuleGenerator PasswordRuleSetGenerator;
        private static readonly MorseCodeRuleGenerator MorseCodeRuleSetGenerator;
        private static readonly VennWireGenerator VennWireRuleSetGenerator;
        private static readonly RhythmHeavenRuleSetGenerator RhythmHeavenRuleSetGenerator;
        private static readonly MazeRuleSetGenerator MazeRuleSetGenerator;
        private static readonly SimonRuleGenerator SimonRuleSetGenerator;

        #endregion

    }
}
