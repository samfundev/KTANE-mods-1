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
        //RuleManagerType = ReflectionHelper.FindType("Assets.Scripts.Rules.RuleManager");
        RuleManagerType = typeof(RuleManager);
        if (RuleManagerType != null)
        {
            RuleManagerInstanceField = RuleManagerType.GetField("instance", BindingFlags.NonPublic | BindingFlags.Static);
            GenerateRulesMethod = RuleManagerType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Instance);
        }
        else
        {
            //If getting the rule manager fails, then reflection failed, no point in getting the rest of the items and continuing.
            return;
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

    public static void GeneratePasswords(int seed, PasswordRuleSet passwordRuleSet)
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
            Possibilites.AddRange(passwordRuleSet.possibilities);
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

            passwordRuleSet.possibilities = Possibilites;
        }
        catch (Exception ex)
        {
            DebugLog("Exception: {0} - Stack Trace: {1}", ex.Message, ex.StackTrace);
            return;
        }
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

    public static RuleManager GenerateRules(int seed)
    {
        DebugLog("Generating Rules for seed {0}", seed);
        var ruleManager = (RuleManager)RuleManagerInstanceField.GetValue(null);
        if (ruleManager == null)
            return null;

        GenerateRulesMethod.Invoke(ruleManager, new object[] {seed});

        //Run custom rule generators after the official ones have done their thing.
        SetFirstVennWireCutInstruction(seed, ruleManager.VennWireRuleSet);
        GeneratePasswords(seed, ruleManager.PasswordRuleSet);

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

}
