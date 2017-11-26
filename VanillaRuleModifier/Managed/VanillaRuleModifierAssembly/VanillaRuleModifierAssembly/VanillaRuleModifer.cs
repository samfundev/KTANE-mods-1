using System;
using System.CodeDom;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Components.VennWire;
using Assets.Scripts.Rules;
using Assets.Scripts.Utility;
using Assets.Scripts.Manual;
using BombGame;
using VanillaRuleModifierAssembly;

public class VanillaRuleModifer : MonoBehaviour
{
    private KMGameInfo _gameInfo = null;
    private KMGameInfo.State _state;
    private ModSettings _modSettings = new ModSettings("VanillaRuleModifier");

    //public TextAsset[] ManualDataFiles;
    private List<ManualFileName> ManualFileNames = new List<ManualFileName>()  
    {
        //HTML Manuals
        new ManualFileName("Capacitor Discharge.html",VanillaRuleModifierAssembly.Properties.Resources.Capacitor_Discharge),
        new ManualFileName("Complicated Wires.html",VanillaRuleModifierAssembly.Properties.Resources.Complicated_Wires),
        new ManualFileName("Keypads.html",VanillaRuleModifierAssembly.Properties.Resources.Keypads),
        new ManualFileName("Knobs.html",VanillaRuleModifierAssembly.Properties.Resources.Knobs),
        new ManualFileName("Mazes.html",VanillaRuleModifierAssembly.Properties.Resources.Mazes),
        new ManualFileName("Memory.html",VanillaRuleModifierAssembly.Properties.Resources.Memory),
        new ManualFileName("Morse Code.html",VanillaRuleModifierAssembly.Properties.Resources.Morse_Code),
        new ManualFileName("Passwords.html",VanillaRuleModifierAssembly.Properties.Resources.Passwords),
        new ManualFileName("Simon Says.html",VanillaRuleModifierAssembly.Properties.Resources.Simon_Says),
        new ManualFileName("The Button.html",VanillaRuleModifierAssembly.Properties.Resources.The_Button),
        new ManualFileName("Venting Gas.html",VanillaRuleModifierAssembly.Properties.Resources.Venting_Gas),
        new ManualFileName("Who’s on First.html",VanillaRuleModifierAssembly.Properties.Resources.Whos_on_First_html),
        new ManualFileName("Wire Sequences.html",VanillaRuleModifierAssembly.Properties.Resources.Wire_Sequences),
        new ManualFileName("Wires.html",VanillaRuleModifierAssembly.Properties.Resources.Wires),
        new ManualFileName("index.html",VanillaRuleModifierAssembly.Properties.Resources.index),

        //CSS
        new ManualFileName(Path.Combine("css","dark-theme.css"),VanillaRuleModifierAssembly.Properties.Resources.dark_theme),
        new ManualFileName(Path.Combine("css","font.css"),VanillaRuleModifierAssembly.Properties.Resources.font),
        new ManualFileName(Path.Combine("css","jquery-ui.1.12.1.css"),VanillaRuleModifierAssembly.Properties.Resources.jquery_ui_1_12_1),
        new ManualFileName(Path.Combine("css","main.css"),VanillaRuleModifierAssembly.Properties.Resources.main),
        new ManualFileName(Path.Combine("css","main-before.css"),VanillaRuleModifierAssembly.Properties.Resources.main_before),
        new ManualFileName(Path.Combine("css","main-orig.css"),VanillaRuleModifierAssembly.Properties.Resources.main_orig),
        new ManualFileName(Path.Combine("css","normalize.css"),VanillaRuleModifierAssembly.Properties.Resources.normalize),

        //Font
        new ManualFileName(Path.Combine("font","AnonymousPro-Bold.ttf"),VanillaRuleModifierAssembly.Properties.Resources.AnonymousPro_Bold),
        new ManualFileName(Path.Combine("font","AnonymousPro-BoldItalic.ttf"),VanillaRuleModifierAssembly.Properties.Resources.AnonymousPro_BoldItalic),
        new ManualFileName(Path.Combine("font","AnonymousPro-Italic.ttf"),VanillaRuleModifierAssembly.Properties.Resources.AnonymousPro_Italic),
        new ManualFileName(Path.Combine("font","AnonymousPro-Regular.ttf"),VanillaRuleModifierAssembly.Properties.Resources.AnonymousPro_Regular),
        new ManualFileName(Path.Combine("font","CALIFB.TTF"),VanillaRuleModifierAssembly.Properties.Resources.CALIFB),
        new ManualFileName(Path.Combine("font","CALIFI.TTF"),VanillaRuleModifierAssembly.Properties.Resources.CALIFI),
        new ManualFileName(Path.Combine("font","CALIFR.TTF"),VanillaRuleModifierAssembly.Properties.Resources.CALIFR),
        new ManualFileName(Path.Combine("font","Morse_Font.ttf"),VanillaRuleModifierAssembly.Properties.Resources.Morse_Font),
        new ManualFileName(Path.Combine("font","OpusChordsSansStd.otf"),VanillaRuleModifierAssembly.Properties.Resources.OpusChordsSansStd),
        new ManualFileName(Path.Combine("font","OpusStd.otf"),VanillaRuleModifierAssembly.Properties.Resources.OpusStd),
        new ManualFileName(Path.Combine("font","OstrichSans-Heavy_90.otf"),VanillaRuleModifierAssembly.Properties.Resources.OstrichSans_Heavy_90),
        new ManualFileName(Path.Combine("font","SpecialElite.ttf"),VanillaRuleModifierAssembly.Properties.Resources.SpecialElite),
        new ManualFileName(Path.Combine("font","specialelite-cyrillic.woff2"),VanillaRuleModifierAssembly.Properties.Resources.specialelite_cyrillic),
        new ManualFileName(Path.Combine("font","trebuc.ttf"),VanillaRuleModifierAssembly.Properties.Resources.trebuc),
        new ManualFileName(Path.Combine("font","trebucbd.ttf"),VanillaRuleModifierAssembly.Properties.Resources.trebucbd),
        new ManualFileName(Path.Combine("font","trebucbi.ttf"),VanillaRuleModifierAssembly.Properties.Resources.trebucbi),
        new ManualFileName(Path.Combine("font","trebucit.ttf"),VanillaRuleModifierAssembly.Properties.Resources.trebucit),

        //img
        new ManualFileName(Path.Combine("img","Bomb.svg"),VanillaRuleModifierAssembly.Properties.Resources.Bomb),
        new ManualFileName(Path.Combine("img","BombSide.svg"),VanillaRuleModifierAssembly.Properties.Resources.BombSide),
        new ManualFileName(Path.Combine("img","ktane-logo.png"),VanillaRuleModifierAssembly.Properties.Resources.ktane_logo),
        new ManualFileName(Path.Combine("img","page-bg-noise-01.png"),VanillaRuleModifierAssembly.Properties.Resources.page_bg_noise_01),
        new ManualFileName(Path.Combine("img","page-bg-noise-02.png"),VanillaRuleModifierAssembly.Properties.Resources.page_bg_noise_02),
        new ManualFileName(Path.Combine("img","page-bg-noise-03.png"),VanillaRuleModifierAssembly.Properties.Resources.page_bg_noise_03),
        new ManualFileName(Path.Combine("img","page-bg-noise-04.png"),VanillaRuleModifierAssembly.Properties.Resources.page_bg_noise_04),
        new ManualFileName(Path.Combine("img","page-bg-noise-05.png"),VanillaRuleModifierAssembly.Properties.Resources.page_bg_noise_05),
        new ManualFileName(Path.Combine("img","page-bg-noise-06.png"),VanillaRuleModifierAssembly.Properties.Resources.page_bg_noise_06),
        new ManualFileName(Path.Combine("img","page-bg-noise-07.png"),VanillaRuleModifierAssembly.Properties.Resources.page_bg_noise_07),
        new ManualFileName(Path.Combine("img","web-background.jpg"),VanillaRuleModifierAssembly.Properties.Resources.web_background),
        new ManualFileName(Path.Combine("img",Path.Combine("appendix-batteries","Battery-AA.svg")),VanillaRuleModifierAssembly.Properties.Resources.Battery_AA),
        new ManualFileName(Path.Combine("img",Path.Combine("appendix-batteries","Battery-D.svg")),VanillaRuleModifierAssembly.Properties.Resources.Battery_D),
        new ManualFileName(Path.Combine("img",Path.Combine("appendix-ports","DVI.svg")),VanillaRuleModifierAssembly.Properties.Resources.DVI),
        new ManualFileName(Path.Combine("img",Path.Combine("appendix-ports","Parallel.svg")),VanillaRuleModifierAssembly.Properties.Resources.Parallel),
        new ManualFileName(Path.Combine("img",Path.Combine("appendix-ports","PS2.svg")),VanillaRuleModifierAssembly.Properties.Resources.PS2),
        new ManualFileName(Path.Combine("img",Path.Combine("appendix-ports","RJ45.svg")),VanillaRuleModifierAssembly.Properties.Resources.RJ45),
        new ManualFileName(Path.Combine("img",Path.Combine("appendix-ports","Serial.svg")),VanillaRuleModifierAssembly.Properties.Resources.Serial),
        new ManualFileName(Path.Combine("img",Path.Combine("appendix-ports","StereoRCA.svg")),VanillaRuleModifierAssembly.Properties.Resources.StereoRCA),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","Component.svg")),VanillaRuleModifierAssembly.Properties.Resources.Component),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","IndicatorWidget.svg")),VanillaRuleModifierAssembly.Properties.Resources.IndicatorWidget),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","NeedyComponent.svg")),VanillaRuleModifierAssembly.Properties.Resources.NeedyComponent),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","TimerComponent.svg")),VanillaRuleModifierAssembly.Properties.Resources.TimerComponent),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","Capacitor Discharge.svg")),VanillaRuleModifierAssembly.Properties.Resources.Capacitor_Discharge1),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","Complicated Wires.svg")),VanillaRuleModifierAssembly.Properties.Resources.Complicated_Wires1),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","Keypads.svg")),VanillaRuleModifierAssembly.Properties.Resources.Keypads1),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","Knobs.svg")),VanillaRuleModifierAssembly.Properties.Resources.Knobs1),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","Mazes.svg")),VanillaRuleModifierAssembly.Properties.Resources.Mazes1),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","Memory.svg")),VanillaRuleModifierAssembly.Properties.Resources.Memory1),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","Morse Code.svg")),VanillaRuleModifierAssembly.Properties.Resources.Morse_Code1),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","Passwords.svg")),VanillaRuleModifierAssembly.Properties.Resources.Passwords1),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","Simon Says.svg")),VanillaRuleModifierAssembly.Properties.Resources.Simon_Says1),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","The Button.svg")),VanillaRuleModifierAssembly.Properties.Resources.The_Button1),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","Venting Gas.svg")),VanillaRuleModifierAssembly.Properties.Resources.Venting_Gas1),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","Who’s on First.svg")),VanillaRuleModifierAssembly.Properties.Resources.Whos_on_First_component),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","Wire Sequences.svg")),VanillaRuleModifierAssembly.Properties.Resources.Wire_Sequences1),
        new ManualFileName(Path.Combine("img",Path.Combine("Component","Wires.svg")),VanillaRuleModifierAssembly.Properties.Resources.Wires1),
        new ManualFileName(Path.Combine("img",Path.Combine("Morsematics","International_Morse_Code.svg")),VanillaRuleModifierAssembly.Properties.Resources.International_Morse_Code),
        new ManualFileName(Path.Combine("img",Path.Combine("Simon Says","SimonComponent_ColourLegend.svg")),VanillaRuleModifierAssembly.Properties.Resources.Simon_Says1),
        new ManualFileName(Path.Combine("img",Path.Combine("Who’s on First","eye-icon.png")),VanillaRuleModifierAssembly.Properties.Resources.eye_icon),

        //js
        new ManualFileName(Path.Combine("js","highlighter.js"),VanillaRuleModifierAssembly.Properties.Resources.highlighter_js),
        new ManualFileName(Path.Combine("js","jquery-ui.1.12.1.min.js"),VanillaRuleModifierAssembly.Properties.Resources.jquery_3_1_1_min_js),
        new ManualFileName(Path.Combine("js","jquery.3.1.1.min.js"),VanillaRuleModifierAssembly.Properties.Resources.jquery_3_1_1_min_js),
    };
    
    private List<ManualFileName> KeypadFiles = new List<ManualFileName>
    {
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","1-copyright.png")),VanillaRuleModifierAssembly.Properties.Resources._1_copyright),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","2-filledstar.png")),VanillaRuleModifierAssembly.Properties.Resources._2_filledstar),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","3-hollowstar.png")),VanillaRuleModifierAssembly.Properties.Resources._3_hollowstar),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","4-smileyface.png")),VanillaRuleModifierAssembly.Properties.Resources._4_smileyface),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","5-doublek.png")),VanillaRuleModifierAssembly.Properties.Resources._5_doublek),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","6-omega.png")),VanillaRuleModifierAssembly.Properties.Resources._6_omega),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","7-squidknife.png")),VanillaRuleModifierAssembly.Properties.Resources._7_squidknife),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","8-pumpkin.png")),VanillaRuleModifierAssembly.Properties.Resources._8_pumpkin),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","9-hookn.png")),VanillaRuleModifierAssembly.Properties.Resources._9_hookn),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","10-teepee.png")),VanillaRuleModifierAssembly.Properties.Resources._10_teepee),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","11-six.png")),VanillaRuleModifierAssembly.Properties.Resources._11_six),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","12-squigglyn.png")),VanillaRuleModifierAssembly.Properties.Resources._12_squigglyn),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","13-at.png")),VanillaRuleModifierAssembly.Properties.Resources._13_at),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","14-ae.png")),VanillaRuleModifierAssembly.Properties.Resources._14_ae),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","15-meltedthree.png")),VanillaRuleModifierAssembly.Properties.Resources._15_meltedthree),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","16-euro.png")),VanillaRuleModifierAssembly.Properties.Resources._16_euro),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","17-circle.png")),VanillaRuleModifierAssembly.Properties.Resources._17_circle),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","18-nwithhat.png")),VanillaRuleModifierAssembly.Properties.Resources._18_nwithhat),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","19-dragon.png")),VanillaRuleModifierAssembly.Properties.Resources._19_dragon),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","20-questionmark.png")),VanillaRuleModifierAssembly.Properties.Resources._20_questionmark),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","21-paragraph.png")),VanillaRuleModifierAssembly.Properties.Resources._21_paragraph),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","22-rightc.png")),VanillaRuleModifierAssembly.Properties.Resources._22_rightc),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","23-leftc.png")),VanillaRuleModifierAssembly.Properties.Resources._23_leftc),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","24-pitchfork.png")),VanillaRuleModifierAssembly.Properties.Resources._24_pitchfork),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","25-tripod.png")),VanillaRuleModifierAssembly.Properties.Resources._25_tripod),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","26-cursive.png")),VanillaRuleModifierAssembly.Properties.Resources._26_cursive),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","27-tracks.png")),VanillaRuleModifierAssembly.Properties.Resources._27_tracks),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","28-balloon.png")),VanillaRuleModifierAssembly.Properties.Resources._28_balloon),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","29-weirdnose.png")),VanillaRuleModifierAssembly.Properties.Resources._29_weirdnose),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","30-upsidedowny.png")),VanillaRuleModifierAssembly.Properties.Resources._30_upsidedowny),
        new ManualFileName(Path.Combine("img",Path.Combine("Round Keypad","31-bt.png")),VanillaRuleModifierAssembly.Properties.Resources._31_bt),
    };

    private string KeypadSymbols = "©★☆ټҖΩѬѼϗϫϬϞѦӕԆӬ҈Ҋѯ¿¶ϾϿΨѪҨ҂Ϙζƛѣ";

    public static void DebugLog(string message, params object[] args)
    {
        CommonReflectedTypeInfo.DebugLog(message, args);
    }

    // Use this for initialization
    void Start ()
	{
        //Unofficial bug fix for cutting the 5th Wire.
        WireSolutions.WireIndex4 = new Solution {Text = "cut the fifth wire", SolutionMethod = ((BombComponent comp, Dictionary<string, object> args) => 4)};

        DebugLog("Starting service");
	    if (!CommonReflectedTypeInfo.Initialize())
	    {
	        DebugLog("Failed to initialize the reflection component of Vanilla Rule Modifier. Aborting load");
	        return;
	    }
	    if (!_modSettings.ReadSettings())
	    {
	        DebugLog("Failed to initialize Mod settings. Aborting load");
	        return;
	    }

	    _gameInfo = GetComponent<KMGameInfo>();
	    _gameInfo.OnStateChange += OnStateChange;
	    StartCoroutine(ModifyVanillaRules());
	}

    private RuleManager _ruleManager;

    private KMGameInfo.State _currentState = KMGameInfo.State.Unlock;
    void OnStateChange(KMGameInfo.State state)
    {
        DebugLog("Transitioning to {0}", state);
        _currentState = state;
    }

    private void WriteComplicatedWiresManual(string path)
    {
        var vennpath = Path.Combine(path, Path.Combine("img", "Complicated Wires"));
        Directory.CreateDirectory(vennpath);

        List<string> lineTypes = new List<string>
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

        var ruleset = _ruleManager.VennWireRuleSet;
        var CutInstructionList = new List<CutInstruction>();
        CutInstructionList.Add(ruleset.RuleDict[new VennWireState(true, false, false, false)]);
        CutInstructionList.Add(ruleset.RuleDict[new VennWireState(false, true, false, false)]);
        CutInstructionList.Add(ruleset.RuleDict[new VennWireState(false, false, true, false)]);
        CutInstructionList.Add(ruleset.RuleDict[new VennWireState(false, false, false, true)]);
        CutInstructionList.Add(ruleset.RuleDict[new VennWireState(true, false, true, false)]);
        CutInstructionList.Add(ruleset.RuleDict[new VennWireState(true, true, false, false)]);
        CutInstructionList.Add(ruleset.RuleDict[new VennWireState(false, true, false, true)]);
        CutInstructionList.Add(ruleset.RuleDict[new VennWireState(false, false, true, true)]);
        CutInstructionList.Add(ruleset.RuleDict[new VennWireState(false, true, true, false)]);
        CutInstructionList.Add(ruleset.RuleDict[new VennWireState(true, false, false, true)]);
        CutInstructionList.Add(ruleset.RuleDict[new VennWireState(true, true, true, false)]);
        CutInstructionList.Add(ruleset.RuleDict[new VennWireState(true, true, false, true)]);
        CutInstructionList.Add(ruleset.RuleDict[new VennWireState(false, true, true, true)]);
        CutInstructionList.Add(ruleset.RuleDict[new VennWireState(true, false, true, true)]);
        CutInstructionList.Add(ruleset.RuleDict[new VennWireState(true, true, true, true)]);
        CutInstructionList.Add(ruleset.RuleDict[new VennWireState(false, false, false, false)]);


        var CutLetters = new[] {"C", "D", "S", "P", "B"};
        var VennList = CutInstructionList.Select(instruction => CutLetters[(int) instruction]).ToList();

        SVGGenerator vennSVG = new SVGGenerator(800, 650);
        SVGGenerator legendSVG = new SVGGenerator(275, 200);
        vennSVG.Draw4SetVennDiagram(VennList, lineTypes);
        legendSVG.DrawVennDiagramLegend(labels, lineTypes);

        File.WriteAllText(Path.Combine(vennpath, "venndiagram.svg"), vennSVG.ToString());
        File.WriteAllText(Path.Combine(vennpath, "legend.svg"), legendSVG.ToString());
    }

    private void WriteMazesManual(string path)
    {
        var mazepath = Path.Combine(path, Path.Combine("img", "Mazes"));
        Directory.CreateDirectory(mazepath);
        var mazes = _ruleManager.MazeRuleSet.GetMazes();
        for (int i = 0; i < mazes.Count; i++)
        {
            File.WriteAllText(Path.Combine(mazepath, $"maze{i}.svg"), mazes[i].ToSVG());
        }

        /*
        if (string.IsNullOrEmpty(CommonReflectedTypeInfo.Mazes[0]))
            return;
        for (int i = 0; i < 9; i++)
        {
            File.WriteAllText(Path.Combine(mazepath, string.Format("maze{0}.svg",i)), CommonReflectedTypeInfo.Mazes[i]);
        }*/
    }

    private void WriteSimonSaysManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
    {
        //var simonrules = CommonReflectedTypeInfo.SimonRules.Replace("Strikes","").Replace("\n", "").Replace("0", "").Replace("1", "").Replace("2", "").Replace("HASHASVOWEL", "").Replace("OTHERWISE", "").Replace(":", "").Replace(" ", "").Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
        /*string[] SimonReplacements = new[]
        {
            "HASVOWEL0RED","HASVOWEL0BLUE","HASVOWEL0GREEN","HASVOWEL0YELLOW",
            "HASVOWEL1RED","HASVOWEL1BLUE","HASVOWEL1GREEN","HASVOWEL1YELLOW",
            "HASVOWEL2RED","HASVOWEL2BLUE","HASVOWEL2GREEN","HASVOWEL2YELLOW",
            "OTHERWISE0RED", "OTHERWISE0BLUE", "OTHERWISE0GREEN", "OTHERWISE0YELLOW",
            "OTHERWISE1RED", "OTHERWISE1BLUE", "OTHERWISE1GREEN", "OTHERWISE1YELLOW",
            "OTHERWISE2RED", "OTHERWISE2BLUE", "OTHERWISE2GREEN", "OTHERWISE2YELLOW",
        };*/
        var rules = _ruleManager.SimonRuleSet.RuleList;
        foreach (var keyValuePair in rules)
        {
            var Colors = new[] {"RED", "BLUE", "GREEN", "YELLOW"};
            for (var i = 0; i < keyValuePair.Value.Count; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    replacements.Add(new ReplaceText() { original = $"{keyValuePair.Key}{i}{Colors[j]}", replacement = keyValuePair.Value[i][j].ToString()});
                }
            }
        }


        //for(int i = 0; i < simonrules.Length; i++)
        //    replacements.Add(new ReplaceText() { original = SimonReplacements[i], replacement = simonrules[i]});
        file.WriteFile(path, replacements);
    }

    private void WritePasswordManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
    {
        var passwordrules = _ruleManager.PasswordRuleSet.possibilities;
        for(int i = 0; i < passwordrules.Count; i++)
            replacements.Add(new ReplaceText {original = string.Format("PASSWORD{0:00}", i), replacement = passwordrules[i]});

        //var passwordrules = CommonReflectedTypeInfo.PasswordRules.Replace("Possibilities: ", "").Replace(" ", "").Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries);
        //for(int i = 0; i < passwordrules.Length; i++)
        //    replacements.Add(new ReplaceText {original = string.Format("PASSWORD{0:00}",i), replacement = passwordrules[i]});
        file.WriteFile(path, replacements);
    }

    private void WriteNeedyKnobManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
    {
        /*var knobrules = CommonReflectedTypeInfo.NeedyKnobRules.Replace(Environment.NewLine,"").Replace("Left", "\nLeft").Replace("Right", "\nRight").Replace("Up", "\nUp").Replace("Down", "\nDown").Replace("O"," ").Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
        var replacement = string.Empty;
        foreach (var pos in knobrules)
        {
            var header = pos.Split(':')[0];
            var leds = pos.Split(':')[1];

            replacement += string.Format("                            <h4>{0}:</h4>\n", header);
            for (int i = 0; i < 2; i++)
            {
                var led = leds.Substring(i * 12, 12);
                replacement += "                            <table style=\"display: inline-table\">\n";
                for (int j = 0; j < 2; j++)
                {
                    var half = led.Substring(j * 6, 6);
                    replacement += "                                <tr>\n";
                    foreach (char l in half)
                        replacement += string.Format("                                <td>{0}</td>\n", l);
                    replacement += "                                </tr>\n";
                }
                replacement += "                            </table>\n";
            }
        }*/
        var replacement = string.Empty;
        var currentDirection = string.Empty;
        foreach (var rule in _ruleManager.NeedyKnobRuleSet.Rules)
        {
            var direction = rule.Solution.Text;
            if (currentDirection != direction)
            {
                replacement += string.Format("                            <h4>{0}:</h4>\n", direction);
                currentDirection = direction;
            }
            foreach (var query in rule.Queries)
            {
                var leds = (bool[]) query.Args[NeedyKnobRuleSet.LED_CONFIG_ARG_KEY];
                replacement += "                            <table style=\"display: inline-table\">\n";
                for (int i = 0; i < NeedyKnobRuleSetGenerator.LED_ROWS; i++)
                {
                    replacement += "                                <tr>\n";
                    for (int j = 0; j < NeedyKnobRuleSetGenerator.LED_COLS; j++)
                    {
                        if (leds[NeedyKnobRuleSetGenerator.LED_COLS * i + j])
                        {
                            replacement += "                                <td>X</td>\n";
                        }
                        else
                        {
                            replacement += "                                <td>&nbsp;</td>\n";
                        }
                    }
                    replacement += "                                </tr>\n";
                }
                replacement += "                            </table>\n";
            }
        }


        replacements.Add(new ReplaceText {original = "NEEDYKNOBLIGHTCONFIGURATION", replacement = replacement});
        file.WriteFile(path, replacements);

    }

    private void WriteKeypadsManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
    {
        //string ruleset = CommonReflectedTypeInfo.KeypadRules;
        //if (string.IsNullOrEmpty(ruleset))
        //    return;
        string table = string.Empty;

        //string[] rules = ruleset.Replace(",", "").Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
        var rules = _ruleManager.KeypadRuleSet.PrecedenceLists;


        for (int i = 0; i < rules[0].Count; i++)
        {
            table += "                                <tr>\n";
            for (int j = 0; j < rules.Count; j++)
            {
                table += "                                    <td class=\"keypad-table-column\"><img class=\"keypad-symbol-image\" src=\"";

                int index = KeypadSymbols.IndexOf(rules[j][i]);
                table += KeypadFiles[index].Name.Replace(Path.DirectorySeparatorChar, '/');
                table += "\"></img>";
                table += "                                    </td>\n";
                if (j == (rules.Count - 1))
                    break;
                table += "                                    <td class=\"keypad-table-spacer\"></td>\n";
            }
            table += "                                </tr>\n";
        }
        replacements.Add(new ReplaceText() { original = "<!--KEYPADTABLE GOES HERE-->", replacement = table});
        foreach (var imagefile in KeypadFiles)
        {
            imagefile.WriteFile(path);
        }

        file.WriteFile(path, replacements);
    }

    private void WriteWhosOnFirstManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
    {


        //var whosonfirstrules = _ruleManager.WhosOnFirstRuleSet.ToString().Split(new [] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
        //var step1 = whosonfirstrules[1].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);

        var step1precedentlist = _ruleManager.WhosOnFirstRuleSet.displayWordToButtonIndexMap;

        var replace = string.Empty;
        for (var i = 0; i < 5; i++)
        {
            replace += "                                <tr>\n";
            if (i == 4)
            {
                replace += "                                    <td></td>\n";
            }
            for (var j = 0; j < ((i == 4) ? 4 : 6); j++)
            {
                //var word = step1[(i * 6) + j].Split(':')[0];
                //var index = step1[(i * 6) + j].Split(':')[1];
                var word = WhosOnFirstRuleSet.DisplayWords[(i*6)+j];
                var index = step1precedentlist[word].ToString();

                replace += "                                    <td>\n";
                replace += "                                        <table>\n";
                replace += "                                            <tr>\n";
                replace += "                                                <th class=\"whos-on-first-look-at-display\" colspan=\"2\">";
                    replace += word.Trim();
                    replace += "</th>\n";
                replace += "                                            </tr>\n";
                for (var k = 0; k < 3; k++)
                {
                    replace += "                                            <tr>\n";
                    for (var l = 0; l < 2; l++)
                    {
                        if (index.Trim().Equals(((k * 2) + l).ToString()))
                        {
                            replace += "<td class=\"whos-on-first-look-at\"><img src=\"img/Who’s on First/eye-icon.png\" alt=\"Look At\" style=\"height: 1em;\" /></td>";
                        }
                        else
                        {
                            replace += "                                                <td><br /></td>\n";
                        }
                    }
                    replace += "                                            </tr>\n";
                }

                replace += "                                        </table>\n";
                replace += "                                    </td>\n";
            }
            replace += "                                </tr>\n";
        }
        replacements.Add(new ReplaceText {original = "LOOKATDISPLAYMAP", replacement = replace });
        replace = string.Empty;

        foreach (var map in _ruleManager.WhosOnFirstRuleSet.precedenceMap)
        {
            replace += "                                <tr>\n";
            replace += "                                    <th>";
            //replace += map.Split(':')[0].Trim();
            replace += map.Key;
            replace += "</th>\n";
            replace += "                                    <td>";
            //replace += map.Split(':')[1].Trim();
            replace += string.Join(", ", map.Value.ToArray());
            replace += "</td>";
            replace += "                                </tr>\n";
        }
        replacements.Add(new ReplaceText { original = "PRECEDENTMAP", replacement = replace });

        file.WriteFile(path, replacements);
    }

    private void WriteWireSequenceManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
    {
        var wiresequencetable = string.Empty;

        /*var sequences = CommonReflectedTypeInfo.WireSequenceRules.Replace(Environment.NewLine, "\n").Replace("BLACK Wires: ", "").Replace("BLUE Wires: ", "").Replace("RED Wires: ", "").Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
        var tablenames = new[] {"Black", "Blue", "Red"};
        

        for (var i = sequences.Length - 1; i >= 0; i--)
        {
            wiresequencetable += "                        <table class=\'";
            wiresequencetable += tablenames[i].ToLowerInvariant();
            wiresequencetable += "'>";

            wiresequencetable += "<tr><th colspan=\'2\' class=\'header\'>";
            wiresequencetable += tablenames[i];
            wiresequencetable += " Wire Occurrences</th></tr>";

            wiresequencetable += "<tr><th class=\'first-col\'>Wire Occurrence</th><th class=\'second-col\'>Cut if connected to:</th></tr>";

            foreach (var wire in sequences[i].Trim().Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries))
            {
                if (wire.Contains('['))
                {
                    wiresequencetable += "<tr><td class=\'first-col\'>";
                    wiresequencetable += wire.Split(':')[0].Trim();
                    wiresequencetable += "&nbsp;";
                    wiresequencetable += tablenames[i].ToLowerInvariant();
                    wiresequencetable += " occurrence</td><td class=\'second-col\'>";
                    wiresequencetable += wire.Split(':')[1].Replace("[", "").Replace("]", "");
                    if (wire.Contains(']'))
                    {
                        wiresequencetable += "</td></tr>";
                    }
                    else
                    {
                        wiresequencetable += ",";
                    }
                }
                else
                {
                    wiresequencetable += wire.Replace("]", "");
                    wiresequencetable += "</td></tr>";
                }
            }
            wiresequencetable += "</table>\n";
        }*/

        var wireLetters = new[] {"A", "B", "C"};
        for (int i = WireSequenceRuleSetGenerator.NUM_COLOURS - 1; i >= 0 ; i--)
        {
            var color = (WireColor) i;
            wiresequencetable += "                        <table class=\'";
            wiresequencetable += color.ToString();
            wiresequencetable += "'>";

            wiresequencetable += "<tr><th colspan=\'2\' class=\'header\'>";
            wiresequencetable += color.ToString().Capitalize();
            wiresequencetable += " Wire Occurrences</th></tr>";

            wiresequencetable += "<tr><th class=\'first-col\'>Wire Occurrence</th><th class=\'second-col\'>Cut if connected to:</th></tr>";
            for (int j = 0; j < WireSequenceRuleSetGenerator.NumWiresPerColour; j++)
            {
                wiresequencetable += "<tr><td class=\'first-col\'>";
                wiresequencetable += Util.OrdinalWord(j + 1);
                wiresequencetable += "&nbsp;";
                wiresequencetable += color.ToString();
                wiresequencetable += " occurrence</td><td class=\'second-col\'>";
                var list = new List<string>();
                for (var k = 0; k < WireSequenceRuleSetGenerator.NUM_PER_PAGE; k++)
                {
                    if (_ruleManager.WireSequenceRuleSet.ShouldBeSnipped(color, j, k))
                    {
                        list.Add(wireLetters[k]);
                    }
                }
                for (var k = 0; k < list.Count; k++)
                {
                    wiresequencetable += list[k];
                    if (k == list.Count - 2)
                        wiresequencetable += " or ";
                    else if (k < list.Count - 2 && list.Count > 2)
                        wiresequencetable += ", ";
                }
                wiresequencetable += "</td></tr>";
            }
            wiresequencetable += "</table>\n";
        }

        replacements.Add(new ReplaceText { original = "WIRESEQUENCETABLES", replacement = wiresequencetable });
        file.WriteFile(path, replacements);
    }

    private void WriteMorseCodeManaul(string path, ManualFileName file, ref List<ReplaceText> replacements)
    {
        var worddict = _ruleManager.MorseCodeRuleSet.WordDict;
        var validFreqs = _ruleManager.MorseCodeRuleSet.ValidFrequencies;
        //var worddict = CommonReflectedTypeInfo.MorseCodeRules.Replace(": ", ":").Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries).Skip(1);
        var morsecodetable = string.Empty;
        foreach (var freq in validFreqs)
        {
            morsecodetable += "                        <tr>\n";
            morsecodetable += "                            <td>";
            //morsecodetable += word.Split(':')[0];
            morsecodetable += worddict[freq];
            morsecodetable += "</td>\n";
            morsecodetable += "                            <td>3.";
            //morsecodetable += word.Split(':')[1];
            morsecodetable += freq.ToString();
            morsecodetable += " MHz</td>\n";
            morsecodetable += "                        </tr>\n";
        }
        replacements.Add(new ReplaceText {original = "MORSECODELOOKUP", replacement = morsecodetable });
        file.WriteFile(path, replacements);
    }

    private bool IsWireQueryValid(Rule rule)
    {
        if (rule.Queries.Count == 1)
            return true;
        var query = rule.GetQueryString();
        var lastwirecolor = QueryableWireProperty.LastWireIsColor.Text;
        var exactlyonecolor = QueryableWireProperty.IsExactlyOneColorWire.Text;
        var morethanonecolor = QueryableWireProperty.MoreThanOneColorWire.Text;
        var nocolor = QueryableWireProperty.IsExactlyZeroColorWire.Text;
        for (var i = 0; i < 4; i++)
        {
            if (query.Contains(exactlyonecolor.Replace("{color}", ((WireColor) i).ToString())) && query.Contains(nocolor.Replace("{color}", ((WireColor) i).ToString())))
                return false;
            if (query.Contains(exactlyonecolor.Replace("{color}", ((WireColor) i).ToString())) && query.Contains(morethanonecolor.Replace("{color}", ((WireColor) i).ToString())))
                return false;
            if (query.Contains(morethanonecolor.Replace("{color}", ((WireColor) i).ToString())) && query.Contains(nocolor.Replace("{color}", ((WireColor) i).ToString())))
                return false;

            if (!query.Contains(lastwirecolor.Replace("{color}", ((WireColor) i).ToString()))) continue;
            if (query.Contains(nocolor.Replace("{color}", ((WireColor) i).ToString()))) return false;
            for (var j = i + 1; j < 5; j++)
            {
                if (query.Contains(lastwirecolor.Replace("{color}", ((WireColor) j).ToString())))
                    return false;
            }
        }
        return true;
    }

    private void WriteWiresManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
    {
        var wirecuttinginstructions = string.Empty;
        var wirerules = _ruleManager.WireRuleSet.RulesDictionary;

        /*
        var wirerules = CommonReflectedTypeInfo.WireRules.Split(new[] {"\n\n"}, StringSplitOptions.RemoveEmptyEntries);

        foreach (var rule in wirerules)
        {
            var instructions = rule.Split('\n');
            wirecuttinginstructions += "                        <tr>";
            wirecuttinginstructions += "<td><strong><em>";
            wirecuttinginstructions += instructions[0];
            wirecuttinginstructions += "</em></strong><br />";
            wirecuttinginstructions += instructions[1];
            for(var i = 2; i < instructions.Length; i++)
            {
                if (i == (instructions.Length - 2))
                {
                    //Remove redundant 2nd last instruction if last instruction cuts the exact same wire.
                    if (instructions[i].EndsWith(instructions[instructions.Length - 1].Replace("Otherwise, ", "")))
                        continue;
                }
                wirecuttinginstructions += "<br />";
                if (instructions[i].StartsWith("If "))
                    wirecuttinginstructions += instructions[i].Replace("If ", "Otherwise, if ");
                else
                    wirecuttinginstructions += instructions[i];
            }
            wirecuttinginstructions += "</tr>\n";
        }*/

        foreach (var rules in wirerules)
        {
            var rule = new List<Rule>(rules.Value);

            var lastrule = rule.Last();
            var remainder = rule.Take(rule.Count - 1).ToList();

            for(var i = remainder.Count - 1; i >= 0; i--)
                if (!IsWireQueryValid(remainder[i]))
                    remainder.Remove(remainder[i]);

            while (remainder.Last().GetSolutionString().Equals(lastrule.GetSolutionString()))
                remainder.Remove(remainder.Last());

            rule = remainder;
            rule.Add(lastrule);

            wirecuttinginstructions += "                        <tr>";
            wirecuttinginstructions += "<td><strong><em>";
            wirecuttinginstructions += rules.Key.ToString();
            wirecuttinginstructions += " wires:</em></strong><br />";
            if (rule.Count == 1)
                wirecuttinginstructions += $"{rule[0].GetSolutionString()}.";
            else
            {
                wirecuttinginstructions += $"If {rule[0].GetQueryString()}, {rule[0].GetSolutionString()}.";
                for (var i = 1; i < rule.Count - 1; i++)
                {
                    wirecuttinginstructions += $"<br />Otherwise, If {rule[i].GetQueryString()}, {rule[i].GetSolutionString()}.";
                }
                wirecuttinginstructions += $"<br />Otherwise, {rule.Last().GetSolutionString()}.";
            }
        }


        replacements.Add(new ReplaceText { original = "WIRECUTTINGINSTRUCTIONS", replacement = wirecuttinginstructions });
        file.WriteFile(path, replacements);
    }

    private void WriteMemoryManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
    {
        var memoryinstructions = string.Empty;
        /*
        var memoryrules = CommonReflectedTypeInfo.MemoryRules.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var stage in memoryrules)
        {
            var instructions = stage.Split('\n');
            memoryinstructions += "                        <h4>";
            memoryinstructions += instructions[0];
            memoryinstructions += "</h4><p>";
            foreach (var instruction in instructions.Skip(1))
            {
                memoryinstructions += instruction;
                memoryinstructions += "<br />";
            }
            memoryinstructions += "</p>\n";
        }*/

        foreach (var stage in _ruleManager.MemoryRuleSet.RulesDictionary)
        {
            memoryinstructions += $"                        <h4>Stage {stage.Key + 1}:</h4><p>";
            for (var i = 0; i < stage.Value.Count; i++)
            {
                memoryinstructions += $"If {stage.Value[i].GetQueryString()}, {stage.Value[i].GetSolutionString()}.<br />";
            }
            memoryinstructions += "</p>\n";
        }

        replacements.Add(new ReplaceText { original = "MEMORYRULES", replacement = memoryinstructions });
        file.WriteFile(path, replacements);
    }

    private void WriteButtonManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
    {
        var buttonrules = _ruleManager.ButtonRuleSet.ToString().Split(new[] {"\n\n"}, StringSplitOptions.RemoveEmptyEntries);
        var initial = string.Empty;
        var onhold = string.Empty;

        var unconditionalBatteryTap = "If there is more than 1 battery on the bomb, press and immediately release the button.";
        var unconditionalBatteryHold = "If there is more than 1 battery on the bomb, hold the button and refer to \"Releasing a Held Button\".";
        bool unconditionalBattery = false;

        foreach (var press in buttonrules[0].Split('\n').Skip(1))
        {
            //More than 2 batteries is redundant, if more than 1 batteries came first, with no other conditions attached.
            if (press.Contains("more than 2 batteries") && unconditionalBattery)
                continue;

            initial += "                        <li>";
            initial += press;
            initial += "</li>\n";

            unconditionalBattery |= press.Equals(unconditionalBatteryTap) | press.Equals(unconditionalBatteryHold);
        }

        foreach (var hold in buttonrules[1].Split('\n').Skip(1))
        {
            if (!hold.Contains(':'))
                continue;
            onhold += "                        <li><em>";
            onhold += hold.Replace(":", "</em>:").Replace("..",".");
            onhold += "</li>\n";
        }

        replacements.Add(new ReplaceText { original = "INITIALBUTTONRULES", replacement = initial });
        replacements.Add(new ReplaceText { original = "ONBUTTONHOLDRULES", replacement = onhold });
        file.WriteFile(path, replacements);
    }

    private void WriteHTML(string path, ManualFileName file, ref List<ReplaceText> replacements)
    {
        if (string.IsNullOrEmpty(file.Name))
            return;
        switch (file.Name)
        {
            case "The Button.html":
                WriteButtonManual(path, file, ref replacements);
                break;
            case "Memory.html":
                WriteMemoryManual(path, file, ref replacements);
                break;
            case "Wires.html":
                WriteWiresManual(path, file, ref replacements);
                break;
            case "Wire Sequences.html":
                WriteWireSequenceManual(path, file, ref replacements);
                break;
            case "Morse Code.html":
                WriteMorseCodeManaul(path, file, ref replacements);
                break;
            case "Who’s on First.html":
                WriteWhosOnFirstManual(path, file, ref replacements);
                break;
            case "Knobs.html":
                WriteNeedyKnobManual(path, file, ref replacements);
                break;
            case "Passwords.html":
                WritePasswordManual(path, file, ref replacements);
                break;
            case "Simon Says.html":
                WriteSimonSaysManual(path, file, ref replacements);
                break;
            case "Keypads.html":
                WriteKeypadsManual(path, file, ref replacements);
                break;
            case "index.html":
                file.WriteFile(path, replacements);
                break;
            default:
                file.WriteFile(path);
                break;
        }
    }

    private static List<int> _previousSeeds = new List<int> { 1 };
    private void WriteManual(int seed)
    {
        if (_previousSeeds.Contains(seed))
            return; //Seed 1 is the Original Vanilla seed.

        _previousSeeds.Add(seed);

        var path = Path.Combine(Application.persistentDataPath, Path.Combine("ModifiedVanillaManuals", seed.ToString()));
        //if (Directory.Exists(path))
        //    return;

        WriteComplicatedWiresManual(path);
        WriteMazesManual(path);
        if (ManualFileNames == null)
        {
            DebugLog("Can't write any manuals :(");
            return;
        }

        List<ReplaceText> replacements = new List<ReplaceText>();
        replacements.Add(new ReplaceText { original = "VANILLAMODIFICATIONSEED", replacement = seed.ToString() });
        foreach (var manual in ManualFileNames)
        {
            WriteHTML(path, manual, ref replacements);
        }
    }
    
    private IEnumerator ModifyVanillaRules()
    {
        DebugLog("ModifyVaniilaRules Coroutine Started");

        yield return new WaitUntil(() => _currentState == KMGameInfo.State.Setup);

        DebugLog("Done waiting for Setup");

        DebugLog("Going into loop to enforce the new seed");
        while (true)
        {
            CommonReflectedTypeInfo.RuleManagerInstanceField.SetValue(null, null);
            yield return new WaitUntil(() => CommonReflectedTypeInfo.RuleManagerInstanceField.GetValue(null) != null);

            _modSettings.ReadSettings();
            var seed = _modSettings.Settings.RuleSeed;
            DebugLog("Generating Rules based on Seed {0}", seed);
            _ruleManager = CommonReflectedTypeInfo.GenerateRules(seed);
            WriteManual(seed);

            yield return new WaitUntil(() => _currentState == KMGameInfo.State.PostGame || _currentState == KMGameInfo.State.Setup);
            DebugLog("Resetting the Rule Generator");
        }
    }
}
