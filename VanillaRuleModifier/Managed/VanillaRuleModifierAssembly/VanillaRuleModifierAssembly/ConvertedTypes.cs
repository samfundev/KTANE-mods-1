using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Assets.Scripts.Components.VennWire;
using Assets.Scripts.Rules;

public static class CommonReflectedTypeInfo
{
    static CommonReflectedTypeInfo()
    {
        RuleManagerType = ReflectionHelper.FindType("Assets.Scripts.Rules.RuleManager");
        if (RuleManagerType != null)
        {
            RuleManagerWireRuleSet = RuleManagerType.GetProperty("WireRuleSet", BindingFlags.Public | BindingFlags.Instance);
            RuleManagerWhosOnFirstRuleSet = RuleManagerType.GetProperty("WhosOnFirstRuleSet", BindingFlags.Public | BindingFlags.Instance);
            RuleManagerKeypadRuleSet = RuleManagerType.GetProperty("KeypadRuleSet", BindingFlags.Public | BindingFlags.Instance);
            RuleManagerMemoryRuleSet = RuleManagerType.GetProperty("MemoryRuleSet", BindingFlags.Public | BindingFlags.Instance);
            RuleManagerNeedyKnobRuleSet = RuleManagerType.GetProperty("NeedyKnobRuleSet", BindingFlags.Public | BindingFlags.Instance);
            RuleManagerButtonRuleSet = RuleManagerType.GetProperty("ButtonRuleSet", BindingFlags.Public | BindingFlags.Instance);
            RuleManagerWireSequenceRuleSet = RuleManagerType.GetProperty("WireSequenceRuleSet", BindingFlags.Public | BindingFlags.Instance);
            RuleManagerPasswordRuleSet = RuleManagerType.GetProperty("PasswordRuleSet", BindingFlags.Public | BindingFlags.Instance);
            RuleManagerMorseCodeRuleSet = RuleManagerType.GetProperty("MorseCodeRuleSet", BindingFlags.Public | BindingFlags.Instance);
            RuleManagerVennWireRuleSet = RuleManagerType.GetProperty("VennWireRuleSet", BindingFlags.Public | BindingFlags.Instance);
            RuleManagerMazeRuleSet = RuleManagerType.GetProperty("MazeRuleSet", BindingFlags.Public | BindingFlags.Instance);
            RuleManagerSimonRuleSet = RuleManagerType.GetProperty("SimonRuleSet", BindingFlags.Public | BindingFlags.Instance);

            RuleManagerInstanceField = RuleManagerType.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
            GenerateRulesMethod = RuleManagerType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
        }
        else
        {
            //If getting the rule manager fails, then reflection failed, no point in getting the rest of the items and continuing.
            return;
        }

        KeypadRuleSetType = ReflectionHelper.FindType("Assets.Scripts.Rules.KeypadRuleSet");
        if (KeypadRuleSetType != null)
        {
            KeypadRulesToStringMethod = KeypadRuleSetType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance);
        }

        MazeType = ReflectionHelper.FindType("Maze");
        if (MazeType != null)
        {
            MazeGenerateSVGMetohd = MazeType.GetMethod("ToSVG", BindingFlags.Public | BindingFlags.Instance);
            MazeRuleSetType = ReflectionHelper.FindType("Assets.Scripts.Rules.MazeRuleSet");
            if (MazeRuleSetType != null)
            {
                MazeRuleSetMazesField = MazeRuleSetType.GetField("mazes", BindingFlags.NonPublic | BindingFlags.Instance);
            }
        }

        VennWireRuleSetType = ReflectionHelper.FindType("Assets.Scripts.Rules.VennWireRuleSet");
        if (VennWireRuleSetType != null)
        {
            VennWireToStringMethod = VennWireRuleSetType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance);

            VennWireStateType = ReflectionHelper.FindType("Assets.Scripts.Components.VennWire.VennWireState");
            if (VennWireStateType != null)
            {
                VennWireStateRedField = VennWireStateType.GetField("HasRed", BindingFlags.Public | BindingFlags.Instance);
                VennWireStateBlueField = VennWireStateType.GetField("HasBlue", BindingFlags.Public | BindingFlags.Instance);
                VennWireStateSymbolField = VennWireStateType.GetField("HasSymbol", BindingFlags.Public | BindingFlags.Instance);
                VennWireStateLEDField = VennWireStateType.GetField("HasLED", BindingFlags.Public | BindingFlags.Instance);
            }

            VennWireRuleDictionaryProperty = VennWireRuleSetType.GetProperty("RuleDict", BindingFlags.Public | BindingFlags.Instance);
            VennWireCutInstructionType = ReflectionHelper.FindType("Assets.Scripts.Components.VennWire.CutInstruction");
        }

        PasswordRuleSetType = ReflectionHelper.FindType("Assets.Scripts.Rules.PasswordRuleSet");
        if (PasswordRuleSetType != null)
        {
            PasswordToStringMethod = PasswordRuleSetType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance);
            PasswordPossibilitesField = PasswordRuleSetType.GetField("possibilities", BindingFlags.Public | BindingFlags.Instance);
        }

        WireRuleSetType = ReflectionHelper.FindType("Assets.Scripts.Rules.WireRuleSet");
        if (WireRuleSetType != null)
        {
            WireRulesToStringMethod = WireRuleSetType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance);
        }

        MemoryRuleSetType = ReflectionHelper.FindType("Assets.Scripts.Rules.MemoryRuleSet");
        if (MemoryRuleSetType != null)
        {
            MemoryRulesToStringMethod = MemoryRuleSetType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance);
        }

        WhosOnFirstRuleSetType = ReflectionHelper.FindType("Assets.Scripts.Rules.WhosOnFirstRuleSet");
        if (WhosOnFirstRuleSetType != null)
        {
            WhosOnFirstRulesToStringMethod = WhosOnFirstRuleSetType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance);
        }

        NeedyKnobRuleSetType = ReflectionHelper.FindType("NeedyKnobRuleSet");
        if (NeedyKnobRuleSetType != null)
        {
            NeedyKnobRulesToStringMethod = NeedyKnobRuleSetType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance);
        }

        ButtonRuleSetType = ReflectionHelper.FindType("Assets.Scripts.Rules.ButtonRuleSet");
        if (ButtonRuleSetType != null)
        {
            ButtonRulesToStringMethod = ButtonRuleSetType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance);
        }

        WireSequenceRuleSetType = ReflectionHelper.FindType("Assets.Scripts.Rules.WireSequenceRuleSet");
        if (WireSequenceRuleSetType != null)
        {
            WireSequenceRulesToStringMethod = WireSequenceRuleSetType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance);
        }

        MorseCodeRuleSetType = ReflectionHelper.FindType("Assets.Scripts.Rules.MorseCodeRuleSet");
        if (MorseCodeRuleSetType != null)
        {
            MorseCodeRulesToStringMethod = MorseCodeRuleSetType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance);
        }

        SimonRuleSetType = ReflectionHelper.FindType("SimonRuleSet");
        if (SimonRuleSetType != null)
        {
            SimonRulesToStringMethod = SimonRuleSetType.GetMethod("ToString", BindingFlags.Public | BindingFlags.Instance);
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

    public static void GeneratePasswords(int seed, object passwordRuleSet)
    {
        if (seed == 1)
            return;
        if (passwordRuleSet == null)
        {
            DebugLog("Can't set password as that object is null");
            return;
        }
        List<string> Possibilites = new List<string>();
        Possibilites.AddRange(new[]{
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
        });
        try
        {
            DebugLog("Adding vanilla passwords to password pool");
            Possibilites.AddRange((List<string>) PasswordPossibilitesField.GetValue(passwordRuleSet));
            DebugLog("Randomizing password pool");
            
            if (seed == 2)
            {
                Possibilites = Possibilites.Take(35).ToList();
            }
            else
            {
                var rng = new System.Random(seed);
                Possibilites = Possibilites.OrderBy(x => rng.Next()).Take(35).OrderBy(x => x).ToList();
            }
            DebugLog("Setting list of passwords to password ruleset");
            PasswordPossibilitesField.SetValue(passwordRuleSet, Possibilites);
        }
        catch (Exception ex)
        {
            DebugLog("Exception: {0} - Stack Trace: {1}", ex.Message, ex.StackTrace);
            return;
        }
        PasswordRules = (string) PasswordToStringMethod.Invoke(passwordRuleSet, new object[0]);
    }

    public static string VennDiagram;
    public static string VennDiagramLegend;

    public static string KeypadRules;

    public static string PasswordRules;

    public static string[] Mazes = new string[9];

    public static string WireRules;
    public static string WhosOnFirstRules;
    public static string MemoryRules;
    public static string NeedyKnobRules;
    public static string ButtonRules;
    public static string WireSequenceRules;
    public static string MorseCodeRules;
    public static string SimonRules;

    private static void GenerateVennWireSVG(string rules)
    {
        /*List<string> lineTypes = new List<string>
        {
            "15,40,4,10",
            string.Empty,
            "3",
            "8"
        };
        List<string> labels = new List<string>
        {
            "Wire has red\ncoloring",
            "Wire has blue\ncoloring",
            "Has ★ symbol",
            "LED is on"
        };

        rules = rules.Replace("[", "").Replace(" ", "").Replace("]", "").Replace("True", "T").Replace("False", "F");
        rules = rules.Replace("Red:", "").Replace("Blue:", "").Replace("Symbol:", "").Replace("LED:", "");
        rules = rules.Replace("DoNotCut", "D").Replace("CutIfTwoOrMoreBatteriesPresent", "B");
        rules = rules.Replace("CutIfParallelPortPresent", "P").Replace("CutIfSerialEven", "S").Replace("Cut", "C");

        Dictionary<string, string> RuleLookup = new Dictionary<string, string>();
        foreach (string rule in rules.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
        {

            var halves = rule.Split(new[] { ":" }, StringSplitOptions.None);
            if (halves.Length != 2) continue;
            RuleLookup.Add(halves[0], halves[1]);
        }

        List<string> VennList = new List<string>();
        VennList.Add(RuleLookup["TFFF"]);
        VennList.Add(RuleLookup["FTFF"]);
        VennList.Add(RuleLookup["FFTF"]);
        VennList.Add(RuleLookup["FFFT"]);
        VennList.Add(RuleLookup["TFTF"]);
        VennList.Add(RuleLookup["TTFF"]);
        VennList.Add(RuleLookup["FTFT"]);
        VennList.Add(RuleLookup["FFTT"]);
        VennList.Add(RuleLookup["FTTF"]);
        VennList.Add(RuleLookup["TFFT"]);
        VennList.Add(RuleLookup["TTTF"]);
        VennList.Add(RuleLookup["TTFT"]);
        VennList.Add(RuleLookup["FTTT"]);
        VennList.Add(RuleLookup["TFTT"]);
        VennList.Add(RuleLookup["TTTT"]);
        VennList.Add(RuleLookup["FFFF"]);
        SVGGenerator vennSVG = new SVGGenerator(800, 650);
        SVGGenerator legendSVG = new SVGGenerator(275, 200);
        vennSVG.Draw4SetVennDiagram(VennList, lineTypes);
        legendSVG.DrawVennDiagramLegend(labels, lineTypes);
        VennDiagram = vennSVG.ToString();
        VennDiagramLegend = legendSVG.ToString();*/
    }

    private static void SetFirstVennWireCutInstruction(int seed, VennWireRuleSet vennruleset)
    {

        try
        {
            DebugLog("Getting Rule Dictionary");
            //var RuleDict = (IDictionary) VennWireRuleDictionaryProperty.GetValue(vennruleset, new object[0]);
            var RuleDict = vennruleset.RuleDict;
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


            if (seed == 1)
                return;
            if (seed == 2)
            {
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
            }
            else
            {
                //Modify the White no LED no symbol cut instruction so that it is not always cut.
                var rng = new System.Random(seed);
                RuleDict[states[0]] =  (CutInstruction) rng.Next(0, 5);
            }
        }
        catch (Exception ex)
        {
            DebugLog("Failed due to exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
            return;
        }

    }


    
    private static List<int> _previousSeeds = new List<int> {1};

    public static RuleManager GenerateRules(int seed)
    {
        DebugLog("Generating Rules for seed {0}", seed);
        var ruleManager = (RuleManager)RuleManagerInstanceField.GetValue(null);
        if (ruleManager == null)
            return null;

        GenerateRulesMethod.Invoke(ruleManager, new object[] {seed});
        _previousSeeds.Add(seed);

        //var vennruleset = RuleManagerVennWireRuleSet.GetValue(ruleManager, null);
        //SetFirstVennWireCutInstruction(seed, vennruleset);
        //GenerateVennWireSVG((string) VennWireToStringMethod.Invoke(vennruleset, new object[0]));
        SetFirstVennWireCutInstruction(seed, ruleManager.VennWireRuleSet);
        GenerateVennWireSVG(ruleManager.VennWireRuleSet.ToString());


        var mazeruleset = RuleManagerMazeRuleSet.GetValue(ruleManager, null);
        var mazelist = (IList) CommonReflectedTypeInfo.MazeRuleSetMazesField.GetValue(mazeruleset);
        int i = 0;
        foreach (var maze in mazelist)
        {
            Mazes[i++] = (string) (MazeGenerateSVGMetohd.Invoke(maze, new object[0]));
        }

        var keypadruleset = RuleManagerKeypadRuleSet.GetValue(ruleManager, null);
        KeypadRules = (string) KeypadRulesToStringMethod.Invoke(keypadruleset, new object[0]);

        var passwordruleset = RuleManagerPasswordRuleSet.GetValue(ruleManager, null);
        GeneratePasswords(seed, passwordruleset);
        //DebugLog("---{0} Rules---\n{1}\n---End of {0} Rules---\n", "Password", PasswordRules);

        var whosonfirstruleset = RuleManagerWhosOnFirstRuleSet.GetValue(ruleManager, null);
        WhosOnFirstRules = (string)WhosOnFirstRulesToStringMethod.Invoke(whosonfirstruleset, new object[0]);
        //DebugLog("---{0} Rules---\n{1}\n---End of {0} Rules---\n", "Who's On First", WhosOnFirstRules);

        var needyknobruleset = RuleManagerNeedyKnobRuleSet.GetValue(ruleManager, null);
        NeedyKnobRules = (string)NeedyKnobRulesToStringMethod.Invoke(needyknobruleset, new object[0]);
        //DebugLog("---{0} Rules---\n{1}\n---End of {0} Rules---\n", "Needy Knob", NeedyKnobRules);

        var simonruleset = RuleManagerSimonRuleSet.GetValue(ruleManager, null);
        SimonRules = (string)SimonRulesToStringMethod.Invoke(simonruleset, new object[0]);
        //DebugLog("---{0} Rules---\n{1}\n---End of {0} Rules---\n", "Simon Says", SimonRules);

        var wiresequenceruleset = RuleManagerWireSequenceRuleSet.GetValue(ruleManager, null);
        WireSequenceRules = (string)WireSequenceRulesToStringMethod.Invoke(wiresequenceruleset, new object[0]);
        //DebugLog("---{0} Rules---\n{1}\n---End of {0} Rules---\n", "Wire Sequence", WireSequenceRules);

        var morsecoderuleset = RuleManagerMorseCodeRuleSet.GetValue(ruleManager, null);
        MorseCodeRules = (string)MorseCodeRulesToStringMethod.Invoke(morsecoderuleset, new object[0]);
        //DebugLog("---{0} Rules---\n{1}\n---End of {0} Rules---\n", "Morse Code", MorseCodeRules);

        var wireruleset = RuleManagerWireRuleSet.GetValue(ruleManager, null);
        WireRules = (string) WireRulesToStringMethod.Invoke(wireruleset, null);
        //DebugLog("---{0} Rules---\n{1}\n---End of {0} Rules---\n", "Wires", WireRules);

        var memoryruleset = RuleManagerMemoryRuleSet.GetValue(ruleManager, null);
        MemoryRules = (string) MemoryRulesToStringMethod.Invoke(memoryruleset, new object[0]);
        //DebugLog("---{0} Rules---\n{1}\n---End of {0} Rules---\n", "Memory", MemoryRules);

        var buttonruleset = RuleManagerButtonRuleSet.GetValue(ruleManager, null);
        ButtonRules = (string) ButtonRulesToStringMethod.Invoke(buttonruleset, new object[0]);
        DebugLog("---{0} Rules---\n{1}\n---End of {0} Rules---\n", "Button", ButtonRules);

        

        

        DebugLog("Done Generating Rules for seed {0}", seed);
        return ruleManager;
    }

    public static bool IsRulesReady()
    {
        return RuleManagerInstanceField.GetValue(null) != null;
    }

    #region RuleManager
    

    public static Type RuleManagerType
    {
        get;
        private set;
    }

    public static PropertyInfo RuleManagerWireRuleSet
    {
        get;
        private set;
    }

    public static PropertyInfo RuleManagerWhosOnFirstRuleSet
    {
        get;
        private set;
    }



    public static PropertyInfo RuleManagerKeypadRuleSet
    {
        get;
        private set;
    }

    public static PropertyInfo RuleManagerMemoryRuleSet
    {
        get;
        private set;
    }

    public static PropertyInfo RuleManagerNeedyKnobRuleSet
    {
        get;
        private set;
    }

    public static PropertyInfo RuleManagerButtonRuleSet
    {
        get;
        private set;
    }

    public static PropertyInfo RuleManagerWireSequenceRuleSet
    {
        get;
        private set;
    }

    public static PropertyInfo RuleManagerPasswordRuleSet
    {
        get;
        private set;
    }

    public static PropertyInfo RuleManagerMorseCodeRuleSet
    {
        get;
        private set;
    }

    public static PropertyInfo RuleManagerVennWireRuleSet
    {
        get;
        private set;
    }

    public static PropertyInfo RuleManagerMazeRuleSet
    {
        get;
        private set;
    }

    public static PropertyInfo RuleManagerSimonRuleSet
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

    #endregion

    #region Password

    public static Type PasswordRuleSetType
    {
        get;
        private set;
    }

    public static MethodInfo PasswordToStringMethod
    {
        get;
        private set;
    }

    public static FieldInfo PasswordPossibilitesField
    {
        get;
        private set;
    }

    #endregion

    #region VennWires
    public static Type VennWireRuleSetType
    {
        get;
        private set;
    }

    public static MethodInfo VennWireToStringMethod
    {
        get;
        private set;
    }

    public static Type VennWireCutInstructionType
    {
        get;
        private set;
    }

    public static Type VennWireStateType
    {
        get;
        private set;
    }

    public static FieldInfo VennWireStateRedField
    {
        get;
        private set;
    }

    public static FieldInfo VennWireStateBlueField
    {
        get;
        private set;
    }

    public static FieldInfo VennWireStateSymbolField
    {
        get;
        private set;
    }

    public static FieldInfo VennWireStateLEDField
    {
        get;
        private set;
    }

    public static PropertyInfo VennWireRuleDictionaryProperty
    {
        get;
        private set;
    }

    #endregion

    #region Keypad
    public static Type KeypadRuleSetType
    {
        get;
        private set;
    }

    public static MethodInfo KeypadRulesToStringMethod
    {
        get;
        private set;
    }
    #endregion

    #region Maze
    public static Type MazeRuleSetType
    {
        get;
        private set;
    }

    public static FieldInfo MazeRuleSetMazesField
    {
        get;
        private set;
    }

    public static Type MazeType
    {
        get;
        private set;
    }

    public static MethodInfo MazeGenerateSVGMetohd
    {
        get;
        private set;
    }
    #endregion

    #region Wire
    public static Type WireRuleSetType
    {
        get;
        private set;
    }

    public static MethodInfo WireRulesToStringMethod
    {
        get;
        private set;
    }
    #endregion

    #region WhosOnFirst
    public static Type WhosOnFirstRuleSetType
    {
        get;
        private set;
    }

    public static MethodInfo WhosOnFirstRulesToStringMethod
    {
        get;
        private set;
    }
    #endregion

    #region Memory
    public static Type MemoryRuleSetType
    {
        get;
        private set;
    }

    public static MethodInfo MemoryRulesToStringMethod
    {
        get;
        private set;
    }
    #endregion

    #region NeedyKnob
    public static Type NeedyKnobRuleSetType
    {
        get;
        private set;
    }

    public static MethodInfo NeedyKnobRulesToStringMethod
    {
        get;
        private set;
    }
    #endregion

    #region Button
    public static Type ButtonRuleSetType
    {
        get;
        private set;
    }

    public static MethodInfo ButtonRulesToStringMethod
    {
        get;
        private set;
    }
    #endregion

    #region WireSequence
    public static Type WireSequenceRuleSetType
    {
        get;
        private set;
    }

    public static MethodInfo WireSequenceRulesToStringMethod
    {
        get;
        private set;
    }
    #endregion

    #region MorseCode
    public static Type MorseCodeRuleSetType
    {
        get;
        private set;
    }

    public static MethodInfo MorseCodeRulesToStringMethod
    {
        get;
        private set;
    }
    #endregion

    #region Simon
    public static Type SimonRuleSetType
    {
        get;
        private set;
    }

    public static MethodInfo SimonRulesToStringMethod
    {
        get;
        private set;
    }
    #endregion
}
