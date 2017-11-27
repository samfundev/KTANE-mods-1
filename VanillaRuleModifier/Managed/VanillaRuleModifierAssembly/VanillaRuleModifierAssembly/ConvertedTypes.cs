using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Assets.Scripts.Components.VennWire;
using Assets.Scripts.Rules;
using Assets.Scripts.Tournaments;
using BombGame;
using VanillaRuleModifierAssembly;

public static class CommonReflectedTypeInfo
{
    static CommonReflectedTypeInfo()
    {
        RuleManagerType = typeof(RuleManager);
        if (RuleManagerType != null)
        {
            RuleManagerInstanceField = RuleManagerType.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
            GenerateRulesMethod = RuleManagerType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
            SeedProperty = RuleManagerType.GetProperty("Seed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            _possibilities = new List<string>()
            {
                "aloof","arena","bleat","boxed","butts","caley","crate",
                "feret","freak","humus","jewel","joule","joust","knobs",
                "local","pause","press","prime","rings","sails","snake",
                "space","splat","spoon","steel","tangy","texas","these",
                "those","toons","tunes","walks","weird","wodar","words",

                "quick","timwi","pingu","thief","cluck","rubik","ktane",
                "phone","decoy","debit","death","fails","flunk","flush",
                "games","gangs","goals","hotel","india","joker","lemon",
                "level","maker","mains","major","noble","noose","obese",
                "olive","paste","party","peace","quest","quack","radar",

                "react","ready","spawn","safer","scoop","ulcer","unban",
                "unite","vinyl","virus","wagon","wrong","xerox","yawns",
                "years","youth","zilch","zones"
            };
            _possibilities.AddRange(((PasswordRuleSet)(new PasswordRuleSetGenerator().GenerateRuleSet(1))).possibilities);
            Seed = -1;
        }
    }

    public static void DebugLog(string message, params object[] args)
    {
        var debugstring = string.Format("[Vanilla Rule Modifier] {0}", message);
        Debug.LogFormat(debugstring, args);
    }

    public static bool Initialize()
    {
        return RuleManagerType != null;
    }

    private static List<string> _possibilities;

    public static void GeneratePasswords(int seed, PasswordRuleSet passwordRuleSet)
    {
        if (seed == 1)
            return;
        if (passwordRuleSet == null)
        {
            DebugLog("Can't set password as that object is null");
            return;
        }
        try
        {
            if (seed == 2)
            {
                passwordRuleSet.possibilities = _possibilities.Take(35).ToList();
            }
            else
            {
                var rng = new System.Random(seed);
                passwordRuleSet.possibilities = _possibilities.Distinct().OrderBy(x => rng.Next()).Take(35).OrderBy(x => x).ToList();
            }
            DebugLog("Setting list of passwords to password ruleset");

        }
        catch (Exception ex)
        {
            DebugLog("Exception: {0} - Stack Trace: {1}", ex.Message, ex.StackTrace);
            return;
        }
    }

    public static void GenerateMorseCode(int seed, RuleManager ruleManager)
    {
        if (seed == 1 || seed == 2)
        {
            DebugLog("Not modifiying Morse code list of seeds 1 nor 2");
            return;
        }
        var fieldInfo = ruleManager.GetType().GetField("morseCodeRuleSetGenerater", BindingFlags.NonPublic | BindingFlags.Instance);
        if (fieldInfo == null)
        {
            DebugLog("Failed to get MorseCodeRuleSetGenerater.");
            return;
        }
        var morsecoderulesetgenerator = fieldInfo.GetValue(ruleManager);
        var randField = morsecoderulesetgenerator.GetType().GetField("rand", BindingFlags.NonPublic | BindingFlags.Instance);
        if (randField == null)
        {
            DebugLog("Failed to get MorseCodeRuleSetGenerater's initialized RNG");
            return;
        }
        var rng = (System.Random) randField.GetValue(morsecoderulesetgenerator);
        var morseWordListField = morsecoderulesetgenerator.GetType().GetField("possibleWords", BindingFlags.NonPublic | BindingFlags.Instance);
        if (morseWordListField == null)
        {
            DebugLog("Failed to get MorseCodeRuleSetGenerater's possible Words List");
            return;
        }
        var possibleWords = (List<string>) morseWordListField.GetValue(morsecoderulesetgenerator);
        var morseWords = new List<string>(_possibilities);
        morseWords.AddRange(possibleWords);
        morseWords.AddRange(new[] {"strike", "tango", "timer", "penguin", "elias", "manual", "zulu", "november", "kaboom", "unicorn", "quebec", "bashly", "slick", "victor", "timwi", "kitty", "bitstim", "market", "oxtrot", "foxtrot","hexicube","lthummus","caitsith","samfun","rexkix"});
        //DebugLog("-----BEFORE-----: {0}", ruleManager.MorseCodeRuleSet.ToString());
        morseWords = morseWords.Distinct().OrderBy(x => rng.Next()).ToList();
        for (var i = 0; i < ruleManager.MorseCodeRuleSet.ValidFrequencies.Count; i++)
        {
            var freq = ruleManager.MorseCodeRuleSet.ValidFrequencies[i];
            ruleManager.MorseCodeRuleSet.WordDict[freq] = morseWords[i];
        }
        //DebugLog("-----AFTER-----: {0}", ruleManager.MorseCodeRuleSet.ToString());

    }

    private static void SetFirstVennWireCutInstruction(int seed, RuleManager ruleManager)
    {

        try
        {
            DebugLog("Getting Rule Dictionary");
            //var RuleDict = (IDictionary) VennWireRuleDictionaryProperty.GetValue(vennruleset, new object[0]);
            var RuleDict = ruleManager.VennWireRuleSet.RuleDict;
            DebugLog("Getting ALL States");
            List<VennWireState> states = RuleDict.Keys.ToList();

            /*foreach (var state in states)
            {
                DebugLog("Dumping State");
                bool red = (bool) VennWireStateRedField.GetValue(state);
                bool blue = (bool) VennWireStateBlueField.GetValue(state);
                bool symbol = (bool) VennWireStateSymbolField.GetValue(state);
                bool led = (bool) VennWireStateLEDField.GetValue(state);
                DebugLog("Dumped Venn Wires State: Red:{0}, Blue:{1}, Symbol:{2}, LED:{3}, Cut Instruction:{4}", red, blue, symbol, led, (int) RuleDict[state]);
                RuleDict[state] = Enum.ToObject(RuleDict[state].GetType(), led ? 1 : 0);
            }*/

            /*
             *      0 = Cut,
		            1 = DoNotCut,
		            2 = CutIfSerialEven,
		            3 = CutIfParallelPortPresent,
		            4 = CutIfTwoOrMoreBatteriesPresent
             */


            switch (seed)
            {
                case 1:
                    return;
                case 2:
                    //Set ALL 16 cut instructions for this seed to match the LTHummus manual hack.
                    //(The Code that generated complicated wires may have changed, and as such seed 2 does NOT match
                    // the LTHummus manual.  This fixex that.)
                    //int[] instructions = new[] {
                    //0, 4, 0, 3, 
                    //1, 2, 0, 3, 
                    //2, 4, 4, 1, 
                    //2, 0, 3, 0};
                    var instructions = new[]
                    {
                        CutInstruction.Cut, CutInstruction.CutIfTwoOrMoreBatteriesPresent, CutInstruction.Cut, CutInstruction.CutIfParallelPortPresent,
                        CutInstruction.DoNotCut, CutInstruction.CutIfSerialEven, CutInstruction.Cut, CutInstruction.CutIfParallelPortPresent,
                        CutInstruction.CutIfSerialEven, CutInstruction.CutIfTwoOrMoreBatteriesPresent, CutInstruction.CutIfTwoOrMoreBatteriesPresent, CutInstruction.DoNotCut,
                        CutInstruction.CutIfSerialEven, CutInstruction.Cut, CutInstruction.CutIfParallelPortPresent, CutInstruction.Cut
                    };

                    for(var i = 0; i < 16; i++)
                    {
                        RuleDict[states[i]] = instructions[i];
                    }
                    break;
                default:
                    List<CutInstruction> possibleInstructions = new List<CutInstruction>((CutInstruction[])Enum.GetValues(typeof(CutInstruction)));
                    //Modify the White no LED no symbol cut instruction so that it is not always cut.
                    //var rng = new System.Random(seed);
                    //RuleDict[states[0]] =  (CutInstruction) rng.Next(0, 5);
                    var fieldInfo = ruleManager.GetType().GetField("vennWireRuleSetGenerator", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fieldInfo == null) return;
                    var vennWireRuleSetGenerator = fieldInfo.GetValue(ruleManager);
                    var getWeightCutInstructionsMethod = vennWireRuleSetGenerator.GetType().GetMethod("GetWeightedRandomCutInstruction", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (getWeightCutInstructionsMethod == null) return;
                    RuleDict[states[0]] = (CutInstruction) getWeightCutInstructionsMethod.Invoke(vennWireRuleSetGenerator, new object[] {possibleInstructions});
                    break;
            }
        }
        catch (Exception ex)
        {
            DebugLog("Failed due to exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
            return;
        }

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

    public static RuleManager GenerateRules(int seed)
    {
        
        //var ruleManager = (RuleManager)RuleManagerInstanceField.GetValue(null);
        //if (ruleManager == null)
        //    return null;
        var ruleManager = RuleManager.Instance;

        if (seed == Seed)
        {
            DebugLog("Rule Manager already initialized with seed {0}. Skipping initialization.", seed);
            return ruleManager;
        }

        if (seed == RuleManager.DEFAULT_SEED)
        {
            DebugLog("Forcing the seed to something else so that the Rule generator will initialize with vanilla rules");
            SeedProperty.SetValue(ruleManager, RuleManager.DEFAULT_SEED + 1, null);
        }

        DebugLog("Generating Rules for seed {0}", seed);
        var previousLabels = IndicatorWidget.Labels;
        IndicatorWidget.Labels = IndicatorLabels;
        GenerateRulesMethod.Invoke(ruleManager, new object[] {seed});

        //Run custom rule generators after the official ones have done their thing.
        SetFirstVennWireCutInstruction(seed, ruleManager);
        GeneratePasswords(seed, ruleManager.PasswordRuleSet);
        ButtonRuleGenerator.Instance.GenerateRules(seed);
        if (ButtonRuleGenerator.Instance.ruleSet != null)
            ruleManager.CurrentRules.ButtonRuleSet = ButtonRuleGenerator.Instance.ruleSet;
        SimonTwoDistinct(seed, ruleManager);
        GenerateMorseCode(seed, ruleManager);



        DebugLog("Done Generating Rules for seed {0}", seed);
        Seed = seed;
        SeedProperty.SetValue(ruleManager, RuleManager.DEFAULT_SEED, null);
        IndicatorLabels = previousLabels;
        return ruleManager;
    }

    public static bool IsRulesReady()
    {
        return RuleManagerInstanceField.GetValue(null) != null;
    }

    public static int Seed { get; private set; }

    public static List<string> IndicatorLabels = new List<string>
    {
        "SND",
        "CLR",
        "CAR",
        "IND",
        "FRQ",
        "SIG",
        "NSA",
        "MSA",
        "TRN",
        "BOB",
        "FRK"
    };

    #region RuleManager


    public static Type RuleManagerType
    {
        get;
        private set;
    }

    public static FieldInfo RuleManagerInstanceField
    {
        get;
        private set;
    }

    public static MethodInfo GenerateRulesMethod
    {
        get;
        private set;
    }

    public static PropertyInfo SeedProperty
    {
        get;
        private set;
    }

    #endregion

}
