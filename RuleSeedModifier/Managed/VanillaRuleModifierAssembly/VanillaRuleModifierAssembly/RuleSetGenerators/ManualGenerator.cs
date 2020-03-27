using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Components.VennWire;
using Assets.Scripts.Rules;
using Assets.Scripts.Utility;
using BombGame;
using Resources = VanillaRuleModifierAssembly.Properties.Resources;
using static VanillaRuleModifierAssembly.CommonReflectedTypeInfo;

namespace VanillaRuleModifierAssembly.RuleSetGenerators
{
    public sealed class ManualGenerator
    {
        private class Nested
        {
            static Nested() { }
            internal static ManualGenerator instance = new ManualGenerator();
        }
        public static ManualGenerator Instance = Nested.instance;

        private readonly List<ManualFileName> _manualFileNames = new List<ManualFileName>()
        {
            //HTML Manuals
            new ManualFileName("Capacitor Discharge.html", Resources.Capacitor_Discharge),
            new ManualFileName("Complicated Wires.html", Resources.Complicated_Wires),
            new ManualFileName("Keypads.html", Resources.Keypads),
            new ManualFileName("Knobs.html", Resources.Knobs),
            new ManualFileName("Mazes.html", Resources.Mazes),
            new ManualFileName("Memory.html", Resources.Memory),
            new ManualFileName("Morse Code.html", Resources.Morse_Code),
            new ManualFileName("Passwords.html", Resources.Passwords),
            new ManualFileName("Simon Says.html", Resources.Simon_Says),
            new ManualFileName("The Button.html", Resources.The_Button),
            new ManualFileName("Venting Gas.html", Resources.Venting_Gas),
            new ManualFileName("Who’s on First.html", Resources.Whos_on_First_html),
            new ManualFileName("Wire Sequences.html", Resources.Wire_Sequences),
            new ManualFileName("Wires.html", Resources.Wires),
            new ManualFileName("index.html", Resources.index),

            //CSS
            new ManualFileName(Path.Combine("css", "dark-theme.css"), Resources.dark_theme),
            new ManualFileName(Path.Combine("css", "font.css"), Resources.font),
            new ManualFileName(Path.Combine("css", "jquery-ui.1.12.1.css"), Resources.jquery_ui_1_12_1),
            new ManualFileName(Path.Combine("css", "main.css"), Resources.main),
            new ManualFileName(Path.Combine("css", "main-before.css"), Resources.main_before),
            new ManualFileName(Path.Combine("css", "main-orig.css"), Resources.main_orig),
            new ManualFileName(Path.Combine("css", "normalize.css"), Resources.normalize),

            //Font
            new ManualFileName(Path.Combine("font", "AnonymousPro-Bold.ttf"), Resources.AnonymousPro_Bold),
            new ManualFileName(Path.Combine("font", "AnonymousPro-BoldItalic.ttf"), Resources.AnonymousPro_BoldItalic),
            new ManualFileName(Path.Combine("font", "AnonymousPro-Italic.ttf"), Resources.AnonymousPro_Italic),
            new ManualFileName(Path.Combine("font", "AnonymousPro-Regular.ttf"), Resources.AnonymousPro_Regular),
            new ManualFileName(Path.Combine("font", "CALIFB.TTF"), Resources.CALIFB),
            new ManualFileName(Path.Combine("font", "CALIFI.TTF"), Resources.CALIFI),
            new ManualFileName(Path.Combine("font", "CALIFR.TTF"), Resources.CALIFR),
            new ManualFileName(Path.Combine("font", "Morse_Font.ttf"), Resources.Morse_Font),
            new ManualFileName(Path.Combine("font", "OpusChordsSansStd.otf"), Resources.OpusChordsSansStd),
            new ManualFileName(Path.Combine("font", "OpusStd.otf"), Resources.OpusStd),
            new ManualFileName(Path.Combine("font", "OstrichSans-Heavy_90.otf"), Resources.OstrichSans_Heavy_90),
            new ManualFileName(Path.Combine("font", "SpecialElite.ttf"), Resources.SpecialElite),
            new ManualFileName(Path.Combine("font", "specialelite-cyrillic.woff2"), Resources.specialelite_cyrillic),
            new ManualFileName(Path.Combine("font", "trebuc.ttf"), Resources.trebuc),
            new ManualFileName(Path.Combine("font", "trebucbd.ttf"), Resources.trebucbd),
            new ManualFileName(Path.Combine("font", "trebucbi.ttf"), Resources.trebucbi),
            new ManualFileName(Path.Combine("font", "trebucit.ttf"), Resources.trebucit),

            //img
            new ManualFileName(Path.Combine("img", "Bomb.svg"), Resources.Bomb),
            new ManualFileName(Path.Combine("img", "BombSide.svg"), Resources.BombSide),
            new ManualFileName(Path.Combine("img", "ktane-logo.png"), Resources.ktane_logo),
            new ManualFileName(Path.Combine("img", "page-bg-noise-01.png"), Resources.page_bg_noise_01),
            new ManualFileName(Path.Combine("img", "page-bg-noise-02.png"), Resources.page_bg_noise_02),
            new ManualFileName(Path.Combine("img", "page-bg-noise-03.png"), Resources.page_bg_noise_03),
            new ManualFileName(Path.Combine("img", "page-bg-noise-04.png"), Resources.page_bg_noise_04),
            new ManualFileName(Path.Combine("img", "page-bg-noise-05.png"), Resources.page_bg_noise_05),
            new ManualFileName(Path.Combine("img", "page-bg-noise-06.png"), Resources.page_bg_noise_06),
            new ManualFileName(Path.Combine("img", "page-bg-noise-07.png"), Resources.page_bg_noise_07),
            new ManualFileName(Path.Combine("img", "web-background.jpg"), Resources.web_background),
            new ManualFileName(Path.Combine("img", Path.Combine("appendix-batteries", "Battery-AA.svg")), Resources.Battery_AA),
            new ManualFileName(Path.Combine("img", Path.Combine("appendix-batteries", "Battery-D.svg")), Resources.Battery_D),
            new ManualFileName(Path.Combine("img", Path.Combine("appendix-ports", "DVI.svg")), Resources.DVI),
            new ManualFileName(Path.Combine("img", Path.Combine("appendix-ports", "Parallel.svg")), Resources.Parallel),
            new ManualFileName(Path.Combine("img", Path.Combine("appendix-ports", "PS2.svg")), Resources.PS2),
            new ManualFileName(Path.Combine("img", Path.Combine("appendix-ports", "RJ45.svg")), Resources.RJ45),
            new ManualFileName(Path.Combine("img", Path.Combine("appendix-ports", "Serial.svg")), Resources.Serial),
            new ManualFileName(Path.Combine("img", Path.Combine("appendix-ports", "StereoRCA.svg")), Resources.StereoRCA),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "Component.svg")), Resources.Component),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "IndicatorWidget.svg")), Resources.IndicatorWidget),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "NeedyComponent.svg")), Resources.NeedyComponent),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "TimerComponent.svg")), Resources.TimerComponent),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "Capacitor Discharge.svg")), Resources.Capacitor_Discharge1),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "Complicated Wires.svg")), Resources.Complicated_Wires1),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "Keypads.svg")), Resources.Keypads1),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "Knobs.svg")), Resources.Knobs1),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "Mazes.svg")), Resources.Mazes1),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "Memory.svg")), Resources.Memory1),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "Morse Code.svg")), Resources.Morse_Code1),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "Passwords.svg")), Resources.Passwords1),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "Simon Says.svg")), Resources.Simon_Says1),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "The Button.svg")), Resources.The_Button1),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "Venting Gas.svg")), Resources.Venting_Gas1),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "Who’s on First.svg")), Resources.Whos_on_First_component),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "Wire Sequences.svg")), Resources.Wire_Sequences1),
            new ManualFileName(Path.Combine("img", Path.Combine("Component", "Wires.svg")), Resources.Wires1),
            new ManualFileName(Path.Combine("img", Path.Combine("Morsematics", "International_Morse_Code.svg")), Resources.International_Morse_Code),
            new ManualFileName(Path.Combine("img", Path.Combine("Simon Says", "SimonComponent_ColourLegend.svg")), Resources.Simon_Says1),
            new ManualFileName(Path.Combine("img", Path.Combine("Who’s on First", "eye-icon.png")), Resources.eye_icon),

            //js
            new ManualFileName(Path.Combine("js", "highlighter.js"), Resources.highlighter_js),
            new ManualFileName(Path.Combine("js", "jquery-ui.1.12.1.min.js"), Resources.jquery_3_1_1_min_js),
            new ManualFileName(Path.Combine("js", "jquery.3.1.1.min.js"), Resources.jquery_3_1_1_min_js),
        };

        private readonly List<ManualFileName> _keypadFiles = new List<ManualFileName>
        {
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "1-copyright.png")), Resources._1_copyright),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "2-filledstar.png")), Resources._2_filledstar),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "3-hollowstar.png")), Resources._3_hollowstar),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "4-smileyface.png")), Resources._4_smileyface),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "5-doublek.png")), Resources._5_doublek),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "6-omega.png")), Resources._6_omega),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "7-squidknife.png")), Resources._7_squidknife),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "8-pumpkin.png")), Resources._8_pumpkin),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "9-hookn.png")), Resources._9_hookn),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "10-teepee.png")), Resources._10_teepee),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "11-six.png")), Resources._11_six),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "12-squigglyn.png")), Resources._12_squigglyn),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "13-at.png")), Resources._13_at),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "14-ae.png")), Resources._14_ae),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "15-meltedthree.png")), Resources._15_meltedthree),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "16-euro.png")), Resources._16_euro),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "17-circle.png")), Resources._17_circle),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "18-nwithhat.png")), Resources._18_nwithhat),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "19-dragon.png")), Resources._19_dragon),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "20-questionmark.png")), Resources._20_questionmark),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "21-paragraph.png")), Resources._21_paragraph),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "22-rightc.png")), Resources._22_rightc),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "23-leftc.png")), Resources._23_leftc),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "24-pitchfork.png")), Resources._24_pitchfork),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "25-tripod.png")), Resources._25_tripod),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "26-cursive.png")), Resources._26_cursive),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "27-tracks.png")), Resources._27_tracks),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "28-balloon.png")), Resources._28_balloon),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "29-weirdnose.png")), Resources._29_weirdnose),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "30-upsidedowny.png")), Resources._30_upsidedowny),
            new ManualFileName(Path.Combine("img", Path.Combine("Round Keypad", "31-bt.png")), Resources._31_bt),
        };

        public const string KeypadSymbols = "©★☆ټҖΩѬѼϗϫϬϞѦӕԆӬ҈Ҋѯ¿¶ϾϿΨѪҨ҂Ϙζƛѣ";

        private void WriteComplicatedWiresManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
        {
            var strokeDashArrays = new List<float[]>
            {
                new [] {20f, 10f, 5f, 10f},
                new float[0],
                new [] {4f, 7f},
                new [] {10f, 4f}
            };
            var labels = new List<string>
            {
                "Manual/venn.legendred",
                "Manual/venn.legendblue",
                "Manual/venn.legendsymbol",
                "Manual/venn.legendled"
            };
            var strokeWidths = new List<float>
            {
                5f,
                6f,
                4f,
                10f
            };

            var ruleset = _ruleManager.VennWireRuleSet;
            var cutInstructionList = new List<CutInstruction>
            {
                ruleset.RuleDict[new VennWireState(true, false, false, false)],
                ruleset.RuleDict[new VennWireState(false, true, false, false)],
                ruleset.RuleDict[new VennWireState(false, false, true, false)],
                ruleset.RuleDict[new VennWireState(false, false, false, true)],
                ruleset.RuleDict[new VennWireState(true, false, true, false)],
                ruleset.RuleDict[new VennWireState(true, true, false, false)],
                ruleset.RuleDict[new VennWireState(false, true, false, true)],
                ruleset.RuleDict[new VennWireState(false, false, true, true)],
                ruleset.RuleDict[new VennWireState(false, true, true, false)],
                ruleset.RuleDict[new VennWireState(true, false, false, true)],
                ruleset.RuleDict[new VennWireState(true, true, true, false)],
                ruleset.RuleDict[new VennWireState(true, true, false, true)],
                ruleset.RuleDict[new VennWireState(false, true, true, true)],
                ruleset.RuleDict[new VennWireState(true, false, true, true)],
                ruleset.RuleDict[new VennWireState(true, true, true, true)],
                ruleset.RuleDict[new VennWireState(false, false, false, false)]
            };

            var vennList = new List<string>();
            using (var enumerator = cutInstructionList.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (enumerator.Current)
                    {
                        case CutInstruction.Cut:
                            vennList.Add("Manual/venn.symbolcut");
                            break;
                        case CutInstruction.DoNotCut:
                            vennList.Add("Manual/venn.symboldonotcut");
                            break;
                        case CutInstruction.CutIfSerialEven:
                            vennList.Add("Manual/venn.symbolserial");
                            break;
                        case CutInstruction.CutIfParallelPortPresent:
                            vennList.Add("Manual/venn.symbolparallel");
                            break;
                        case CutInstruction.CutIfTwoOrMoreBatteriesPresent:
                            vennList.Add("Manual/venn.symbolbattery");
                            break;
                    }
                }
            }

            var vennSVG = new SVGGenerator(800, 650);
            var legendSVG = new SVGGenerator(275, 200);
            vennSVG.Draw4SetVennDiagram(vennList, strokeDashArrays, strokeWidths);
            legendSVG.DrawVennDiagramLegend(labels, strokeDashArrays, strokeWidths);
            replacements.Add(new ReplaceText { Original = "VENNDIAGRAMSVGDATA", Replacement = vennSVG.ToString() });
            replacements.Add(new ReplaceText { Original = "VENNLEGENDSVGDATA", Replacement = legendSVG.ToString() });
            file.WriteFile(path, replacements);
        }

        private void WriteMazesManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
        {
            var mazes = _ruleManager.MazeRuleSet.GetMazes();
            for (var i = 0; i < mazes.Count; i++)
            {
                replacements.Add(new ReplaceText { Original = $"MAZE{i}SVGDATA", Replacement = mazes[i].ToSVG().Replace("<svg ", "<svg class=\"maze\" ") });
            }
            file.WriteFile(path, replacements);
        }

        private void WriteSimonSaysManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
        {
            var rules = _ruleManager.SimonRuleSet.RuleList;
            foreach (var keyValuePair in rules)
            {
                var colors = new[] {"RED", "BLUE", "GREEN", "YELLOW"};
                for (var i = 0; i < keyValuePair.Value.Count; i++)
                {
                    for (var j = 0; j < 4; j++)
                    {
                        replacements.Add(new ReplaceText() {Original = $"{keyValuePair.Key}{i}{colors[j]}", Replacement = keyValuePair.Value[i][j].ToString()});
                    }
                }
            }
            file.WriteFile(path, replacements);
        }

        private void WritePasswordManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
        {
            var passwordrules = _ruleManager.PasswordRuleSet.possibleWords;
            for (var i = 0; i < passwordrules.Count; i++)
            {
                replacements.Add(new ReplaceText {Original = $"PASSWORD{i:00}", Replacement = passwordrules[i]});
            }

            file.WriteFile(path, replacements);
        }

        private void WriteNeedyKnobManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
        {
            var replacement = string.Empty;
            var currentDirection = string.Empty;
            foreach (var rule in _ruleManager.NeedyKnobRuleSet.Rules)
            {
                var direction = rule.Solution.Text;
                if (currentDirection != direction)
                {
                    replacement += $"                            <h4>{direction}:</h4>\n";
                    currentDirection = direction;
                }
                foreach (var query in rule.Queries)
                {
                    var leds = (bool[]) query.Args[NeedyKnobRuleSet.LED_CONFIG_ARG_KEY];
                    replacement += "                            <table style=\"display: inline-table\">\n";
                    for (var i = 0; i < NeedyKnobRuleSetGenerator.LED_ROWS; i++)
                    {
                        replacement += "                                <tr>\n";
                        for (var j = 0; j < NeedyKnobRuleSetGenerator.LED_COLS; j++)
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

            replacements.Add(new ReplaceText {Original = "NEEDYKNOBLIGHTCONFIGURATION", Replacement = replacement});
            file.WriteFile(path, replacements);
        }

        private void WriteKeypadsManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
        {
            var table = string.Empty;
            var rules = _ruleManager.KeypadRuleSet.PrecedenceLists;

            for (var i = 0; i < rules[0].Count; i++)
            {
                table += "                                <tr>\n";
                for (var j = 0; j < rules.Count; j++)
                {
                    table += "                                    <td class=\"keypad-table-column\"><img class=\"keypad-symbol-image\" src=\"";

                    var index = ManualGenerator.KeypadSymbols.IndexOf(rules[j][i], StringComparison.Ordinal);
                    table += _keypadFiles[index].Name.Replace(Path.DirectorySeparatorChar, '/');
                    table += "\"></img>";
                    table += "                                    </td>\n";
                    if (j == (rules.Count - 1))
                        break;
                    table += "                                    <td class=\"keypad-table-spacer\"></td>\n";
                }
                table += "                                </tr>\n";
            }
            replacements.Add(new ReplaceText() {Original = "<!--KEYPADTABLE GOES HERE-->", Replacement = table});
            foreach (var imagefile in _keypadFiles)
            {
                imagefile.WriteFile(path);
            }

            file.WriteFile(path, replacements);
        }

        private void WriteWhosOnFirstManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
        {
            var step1Precedentlist = _ruleManager.WhosOnFirstRuleSet.DisplayTermToButtonIndexMap;

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
                    var word = WhosOnFirstRuleSet.DisplayTerms[(i * 6) + j];
                    var index = step1Precedentlist[word].ToString();

                    replace += "                                    <td>\n";
                    replace += "                                        <table>\n";
                    replace += "                                            <tr>\n";
                    replace += "                                                <th class=\"whos-on-first-look-at-display\" colspan=\"2\">";
                    replace += Localization.GetLocalizedString(word).Trim();
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
            replacements.Add(new ReplaceText {Original = "LOOKATDISPLAYMAP", Replacement = replace});
            replace = string.Empty;

            foreach (var map in _ruleManager.WhosOnFirstRuleSet.TermsPrecedenceMap)
            {
                replace += "                                <tr>\n";
                replace += "                                    <th>";
                replace += Localization.GetLocalizedString(map.Key);
                replace += "</th>\n";
                replace += "                                    <td>";
                replace += string.Join(", ", map.Value.Select(term => Localization.GetLocalizedString(term)).ToArray());
                replace += "</td>";
                replace += "                                </tr>\n";
            }
            replacements.Add(new ReplaceText {Original = "PRECEDENTMAP", Replacement = replace});

            file.WriteFile(path, replacements);
        }

        private void WriteWireSequenceManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
        {
            var wiresequencetable = string.Empty;
            var wireLetters = new[] {"A", "B", "C"};
            for (var i = WireSequenceRuleSetGenerator.NUM_COLOURS - 1; i >= 0; i--)
            {
                var color = (WireColor) i;
                wiresequencetable += $"<table class=\'{color}'>";
                wiresequencetable += $"<tr><th colspan=\'2\' class=\'header\'>{color.ToString().Capitalize()} Wire Occurrences</th></tr>";
                wiresequencetable += "<tr><th class=\'first-col\'>Wire Occurrence</th><th class=\'second-col\'>Cut if connected to:</th></tr>";
                for (var j = 0; j < WireSequenceRuleSetGenerator.NumWiresPerColour; j++)
                {
                    wiresequencetable += $"<tr><td class=\'first-col\'>{Util.OrdinalWord(j + 1)}&nbsp;{color} occurrence</td><td class=\'second-col\'>";
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
                    if (list.Count == 0)
                        wiresequencetable += "Never Cut";
                    wiresequencetable += "</td></tr>";
                }
                wiresequencetable += "</table>\n";
            }

            replacements.Add(new ReplaceText {Original = "WIRESEQUENCETABLES", Replacement = wiresequencetable});
            file.WriteFile(path, replacements);
        }

        private void WriteMorseCodeManaul(string path, ManualFileName file, ref List<ReplaceText> replacements)
        {
            var worddict = _ruleManager.MorseCodeRuleSet.FrequencyTermMap;
            var validFreqs = _ruleManager.MorseCodeRuleSet.ValidFrequencies;
            var morsecodetable = validFreqs.Aggregate(string.Empty, (current, freq) => current + $"<tr><td>{Localization.GetLocalizedString(worddict[freq]) ?? worddict[freq]}</td><td>3.{freq} MHz</td></tr>\n");
            replacements.Add(new ReplaceText {Original = "MORSECODELOOKUP", Replacement = morsecodetable});
            file.WriteFile(path, replacements);
        }

        private void WriteWiresManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
        {
            var wirecuttinginstructions = string.Empty;
            var wirerules = _ruleManager.WireRuleSet.RulesDictionary;

            foreach (var rules in wirerules)
            {
                var rule = new List<Rule>(rules.Value);

                wirecuttinginstructions += $"<tr><td><strong><em>{rules.Key} wires:</em></strong><br />\n";
                if (rule.Count == 1)
                {
                    wirecuttinginstructions += $"{rule[0].GetSolutionString()}.\n";
                }
                else
                {
                    wirecuttinginstructions += $"If {rule[0].GetQueryString()}, {rule[0].GetSolutionString()}.\n";
                    for (var i = 1; i < rule.Count - 1; i++)
                    {
                        wirecuttinginstructions += $"<br />Otherwise, If {rule[i].GetQueryString()}, {rule[i].GetSolutionString()}.\n";
                    }
                    wirecuttinginstructions += $"<br />Otherwise, {rule.Last().GetSolutionString()}.\n";
                }
            }

            replacements.Add(new ReplaceText {Original = "WIRECUTTINGINSTRUCTIONS", Replacement = wirecuttinginstructions});
            file.WriteFile(path, replacements);
        }

        private void WriteMemoryManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
        {
            var memoryinstructions = string.Empty;

            foreach (var stage in _ruleManager.MemoryRuleSet.RulesDictionary)
            {
                memoryinstructions += $"                        <h4>Stage {stage.Key + 1}:</h4><p>";
                for (var i = 0; i < stage.Value.Count; i++)
                {
                    memoryinstructions += $"If {stage.Value[i].GetQueryString()}, {stage.Value[i].GetSolutionString()}.<br />";
                }
                memoryinstructions += "</p>\n";
            }

            replacements.Add(new ReplaceText {Original = "MEMORYRULES", Replacement = memoryinstructions});
            file.WriteFile(path, replacements);
        }

        private void WriteButtonManual(string path, ManualFileName file, ref List<ReplaceText> replacements)
        {
            var initial = string.Empty;
            var onhold = string.Empty;
            var portsused = false;

            foreach (var press in _ruleManager.ButtonRuleSet.RuleList)
            {
                portsused = press.Queries.Aggregate(portsused, (current, query) => current || query.Property == QueryablePorts.EmptyPortPlate);
                portsused = press.Queries.Aggregate(portsused, (current, query) => current || QueryablePorts.PortList.Contains(query.Property));
            }

            initial = _ruleManager.ButtonRuleSet.RuleList.Aggregate(initial, (current, press) => current + $"<li>If {press.GetQueryString()}, {press.GetSolutionString()}.</li>\n");
            onhold = _ruleManager.ButtonRuleSet.HoldRuleList.Aggregate(onhold, (current, hold) => current + $"<li><em>{hold.GetQueryString()}</em> {hold.GetSolutionString()}</li>\n");

            replacements.Add(new ReplaceText {Original = "APPENDIXCREFERENCE", Replacement = portsused ? "<br />See Appendix C for port identification reference." : ""});
            replacements.Add(new ReplaceText {Original = "INITIALBUTTONRULES", Replacement = initial});
            replacements.Add(new ReplaceText {Original = "ONBUTTONHOLDRULES", Replacement = onhold});
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
                case "Mazes.html":
                    WriteMazesManual(path, file, ref replacements);
                    break;
                case "Complicated Wires.html":
                    WriteComplicatedWiresManual(path, file, ref replacements);
                    break;
                case "index.html":
                    file.WriteFile(path, replacements);
                    break;
                default:
                    file.WriteFile(path);
                    break;
            }
        }

        private readonly List<int> PreviousSeeds = new List<int>();
        private RuleManager _ruleManager;

        public void WriteManual(int seed)
        {
            if (PreviousSeeds.Contains(seed))
            {
                if (seed != 1)
                    DebugLog($"Manual already written for seed #{seed}.");
                return; //Seed 1 is the Original Vanilla seed.
            }
            PreviousSeeds.Add(seed);
            _ruleManager = RuleManager.Instance;

            var path = Path.Combine(Application.persistentDataPath, Path.Combine("ModifiedManuals", seed.ToString()));
            //if (Directory.Exists(path))
            //    return;

            DebugLog($"Printing the Rules for seed #{seed}");

            if (!PrintRules(_ruleManager.CurrentRules))
            {
                DebugLog("One or more of the rules failed to print - Writing of manual aborted");
                return;
            }

            if (_manualFileNames == null)
            {
                DebugLog("Can't write any manuals :(");
                return;
            }

            var replacements = new List<ReplaceText>
            {
                new ReplaceText {Original = "VANILLAMODIFICATIONSEED", Replacement = seed.ToString()},
                new ReplaceText {Original = "<span class=\"page-header-doc-title\">Keep Talking and Nobody Explodes v. 1</span>", Replacement = $"<span class=\"page-header-doc-title\">Keep Talking and Nobody Explodes - Seed #{seed}</span>"},
                new ReplaceText {Original = "<span class=\"page-header-doc-title\">Keep Talking and Nobody Explodes</span>", Replacement = $"<span class=\"page-header-doc-title\">Keep Talking and Nobody Explodes - Seed #{seed}</span>"}
            };
            foreach (var manual in _manualFileNames)
            {
                WriteHTML(path, manual, ref replacements);
            }
	        ModRuleSetGenerator.Instance.WriteManuals(path);
        }
    }
}