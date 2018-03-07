using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.RuleGenerator;
using Rnd = UnityEngine.Random;

public class TheBigCircle : MonoBehaviour
{

    #region Public Variables
    public GameObject Circle;
    public KMSelectable[] Wedges;
    public MeshRenderer[] WedgeRenderers;

    public KMBombModule BombModule;
    public KMAudio Audio;
    public KMBombInfo BombInfo;
    #endregion

    #region Prviate Variables
    private bool _rotateCounterClockwise = false;
    private bool _solved = false;
    private bool _spinning = true;
    private bool _activated = false;


    private Color[] _wedgeColors = Ext.NewArray(
            "CC0000",       //red
            "CC7308",       //orange
            "C9CC21",       //yellow
            "00FF21",       //green
            "1911F3",       //blue
            "CC15C6",       //magenta
            "FFFFFF",       //white
            "121212"        //black
        )
        .Select(c => new Color(Convert.ToInt32(c.Substring(0, 2), 16) / 255f, Convert.ToInt32(c.Substring(2, 2), 16) / 255f, Convert.ToInt32(c.Substring(4, 2), 16) / 255f))
        .ToArray();


    private WedgeColors[] _colors = (WedgeColors[]) Enum.GetValues(typeof(WedgeColors));
    #endregion

    private static BigCircleRuleGenerator BigCircleRuleSet
    {
        get { return BigCircleRuleGenerator.Instance; }
    }

    // Use this for initialization
    void Start ()
    {
        colorLookup = BigCircleRuleSet.Rules;
        BombModule.GenerateLogFriendlyName();
	    _rotateCounterClockwise = Rnd.value < 0.5;
	    WedgeRenderers[0].material.color = Color.cyan;
	    _colors.Shuffle();
	    for (var i = 0; i < 8; i++)
	    {
	        WedgeRenderers[i].material.color = _wedgeColors[(int) _colors[i]];
	        var j = _colors[i];
	        Wedges[i].OnInteract += delegate { HandleWedge(j); return false; };
	    }
        //BombModule.LogFormat("Colors in Clockwise order: {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", _colors[0], _colors[1], _colors[2], _colors[3], _colors[4], _colors[5], _colors[6], _colors[7]);
        BombModule.OnActivate += delegate { _activated = true; StartCoroutine(SpinCircle()); StartCoroutine(UpdateSolution());};
    }


    
    bool IsBobPresent()
    {
        var bobPresent = BombInfo.GetBatteryCount() == 5 && BombInfo.GetBatteryHolderCount() == 3 &&
                          BombInfo.IsIndicatorPresent(KMBombInfoExtensions.KnownIndicatorLabel.BOB);

        if (!bobPresent || _baseOffsetGenerated) return bobPresent;
        _baseOffsetGenerated = true;
        BombModule.LogFormat("Bob, Our true lord and savior has come to save the Day.: Press any solution that would be valid for any characters present on the serial number at any time.");
        var serial = BombInfo.GetSerialNumber().ToUpperInvariant();
        if (_rotateCounterClockwise)
        {
            
            BombModule.Log("Circle is spinning counter-clockwise.");
        }
        else
        {
            BombModule.Log("Circle is spinning clockwise.");
        }
        for (var i = 0; i < serial.Length; i++)
        {
            var index = serial.Substring(i, 1);
            var colorIndex = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(index, StringComparison.Ordinal);
            if (colorIndex < 0)
            {
                BombModule.LogFormat("Unrecognized Serial number character: {0} - Passing the module now", index);
                StartCoroutine(FadeCircle(_wedgeColors[(int)WedgeColors.Red]));
                return false;
            }
            var lookup = new List<WedgeColors>(colorLookup[colorIndex / 3]);
            if(_rotateCounterClockwise)
                lookup.Reverse();

            if (ValidBOBColors.Any(x => x[0] == lookup[0] && x[1] == lookup[1] && x[2] == lookup[2])) continue;

            ValidBOBColors.Add(lookup.ToArray());
        }
        ValidBOBColors = ValidBOBColors.OrderBy(x => x[0].ToString()).ThenBy(x => x[1].ToString()).ToList();
        for (var i = 0; i < ValidBOBColors.Count; i++)
        {
            var lookup = ValidBOBColors[i];
            BombModule.LogFormat("Bob Solution #{3}: {0}, {1}, {2}", lookup[0], lookup[1], lookup[2], i + 1);
        }

        _currentSolution = new WedgeColors[3];
        return true;
    }

    private bool _baseOffsetGenerated;
    private int _baseOffset;
    int GetBaseOffset()
    {
        if (_baseOffsetGenerated)
            return _baseOffset;

        var total = 0;



        var customIndicatorRule = new List<string>();
        var litIndicators = BombInfo.GetOnIndicators().ToList();
        var indicators = BombInfo.GetIndicators().ToList();
        indicators.Sort();
        int score;


        foreach (var indicator in indicators)
        {
            var lit = litIndicators.Contains(indicator);
            if (lit)
            {
                litIndicators.Remove(indicator);
            }
            switch (indicator)
            {
                case "IND":
                    score = 0;
                    break;
                case "BOB":
                case "CAR":
                case "CLR":
                    score = lit ? 1 : -1;
                    break;
                case "FRK":
                case "FRQ":
                case "MSA":
                case "NSA":
                    score = lit ? 2 : -2;
                    break;
                case "SIG":
                case "SND":
                case "TRN":
                    score = lit ? 3 : -3;
                    break;
                default:
                    score = 6;
                    break;
            }
            if(score < 6)
                BombModule.LogFormat("{0} {1}: {2:+#;-#}", lit ? "Lit" : "Unlit", indicator, score);
            else
                customIndicatorRule.Add(string.Format("Custom indicator: {0} {1}: +6", lit ? "Lit":"Unlit",indicator));
            total += score;
        }
        score = BombInfo.GetBatteryCount() % 2 == 0 ? -4 : 4;
        BombModule.LogFormat("{0} Batteries ({1}): {2}", BombInfo.GetBatteryCount(),score < 0 ? "Even" : "Odd",  score);
        total += score;

        var dviRule = new List<string>();
        var customPorts = new List<string>();
        foreach (var plate in BombInfo.GetPortPlates())
        {
            foreach (var port in plate)
            {
                if (port == KMBombInfoExtensions.KnownPortType.Parallel.ToString())
                {
                    if (plate.Contains(KMBombInfoExtensions.KnownPortType.Serial.ToString()))
                    {
                        BombModule.Log("Port plate with both parallel and serial: -4");
                        total -= 4;
                    }
                    else
                    {
                        BombModule.LogFormat("Port plate with only parallel: +5");
                        total += 5;
                    }
                    continue;
                }
                if (port == KMBombInfoExtensions.KnownPortType.Serial.ToString()) continue;
                if (port == KMBombInfoExtensions.KnownPortType.DVI.ToString())
                {
                    if (plate.Contains(KMBombInfoExtensions.KnownPortType.StereoRCA.ToString()))
                    {
                        dviRule.Add("Port plate with both DVI-D and Stereo RCA: +4");
                        total += 4;
                    }
                    else
                    {
                        dviRule.Add("Port plate with DVI-D: -5");
                        total -= 5;
                    }
                    continue;
                }
                if (port == KMBombInfoExtensions.KnownPortType.RJ45.ToString()) continue;
                if (port == KMBombInfoExtensions.KnownPortType.StereoRCA.ToString()) continue;
                if (port == KMBombInfoExtensions.KnownPortType.PS2.ToString()) continue;
                customPorts.Add(string.Format("Special port {0}: -6",port));
                total = total - 6;
            }
        }
        foreach (var s in dviRule)
            BombModule.Log(s);
        foreach (var s in customIndicatorRule)
            BombModule.Log(s);
        foreach (var s in customPorts)
            BombModule.Log(s);

        _baseOffset = total;

        _baseOffsetGenerated = true;
        return _baseOffset;
    }

    WedgeColors[][] colorLookup =
    {
        new[] {WedgeColors.Red, WedgeColors.Yellow, WedgeColors.Blue},
        new[] {WedgeColors.Orange, WedgeColors.Green, WedgeColors.Magenta},
        new[] {WedgeColors.Blue, WedgeColors.Black, WedgeColors.Red},
        new[] {WedgeColors.Magenta, WedgeColors.White, WedgeColors.Orange},
        new[] {WedgeColors.Orange, WedgeColors.Blue, WedgeColors.Black},
        new[] {WedgeColors.Green, WedgeColors.Red, WedgeColors.White},
        new[] {WedgeColors.Magenta, WedgeColors.Yellow, WedgeColors.Black},
        new[] {WedgeColors.Red, WedgeColors.Orange, WedgeColors.Yellow},
        new[] {WedgeColors.Yellow, WedgeColors.Green, WedgeColors.Blue},
        new[] {WedgeColors.Blue, WedgeColors.Magenta, WedgeColors.Red},
        new[] {WedgeColors.Black, WedgeColors.White, WedgeColors.Green},
        new[] {WedgeColors.White, WedgeColors.Yellow, WedgeColors.Blue}
    };

    private List<WedgeColors[]> ValidBOBColors = new List<WedgeColors[]>();

    WedgeColors[] GetSolution(int solved, int twofactor)
    {

        var total = GetBaseOffset();
        if (IsBobPresent())
        {
            return null;
        }

        BombModule.LogFormat("Base total: {0}", total);


        const int solvedMultiplier = 3;
        total += solved * solvedMultiplier;
        if (solved > 0)
            BombModule.LogFormat("{0} solved modules: +{1}", solved, solved * solvedMultiplier);

        total += twofactor;
        foreach (var twoFA in BombInfo.GetTwoFactorCodes())
            BombModule.LogFormat("Two Factor {0}: +{1}", twoFA, twoFA % 10);

        BombModule.LogFormat("Total: {0}", total);

        if (total < 0)
        {
            total *= -1;
            BombModule.LogFormat("Total is Negative.  Multiplying by -1.  New number is {0}.", total);
        }

        var serial = BombInfo.GetSerialNumber().ToUpperInvariant();
        serial += serial.Substring(4, 1) + serial.Substring(3, 1) + serial.Substring(2, 1) + serial.Substring(1, 1);

        var index = serial.Substring(total % 10, 1);
        BombModule.LogFormat("Extended serial number is {0}.", serial);
        BombModule.LogFormat("Using Character {0}, which is {1}.", total % 10, index);

        var colorIndex = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(index, StringComparison.Ordinal);

        if (colorIndex < 0)
        {
            BombModule.LogFormat("Unrecognized Serial number character: {0} - Passing the module now", index);
            StartCoroutine(FadeCircle(_wedgeColors[(int)WedgeColors.Red]));
            return null;
        }

        var lookup = new List<WedgeColors>(colorLookup[colorIndex / 3]);

        if (_rotateCounterClockwise)
        {
            lookup.Reverse();
            BombModule.Log("Circle is spinning counter-clockwise.");
        }
        else
        {
            BombModule.Log("Circle is spinning clockwise.");
        }
        BombModule.LogFormat("Solution: {0}, {1}, {2}", lookup[0], lookup[1], lookup[2]);

        return lookup.ToArray();
    }



    private int _currentState = 0;
    private WedgeColors[] _currentSolution;
    void HandleWedge(WedgeColors color)
    {
        try
        {

            if (!_activated)
            {
                BombModule.LogFormat("Pressed {0} before module has activated", color);
                BombModule.HandleStrike();
                return;
            }
            if (_solved)
                return;

            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BombModule.transform);

            if (IsBobPresent())
            {
                bool result = false;
                string[] validColors;
                if (_currentState == 0)
                {
                    validColors = ValidBOBColors.Select(x => x[0].ToString()).Distinct().ToArray();
                    result = ValidBOBColors.Any(x => x[0] == color);
                }
                else if (_currentState == 1)
                {
                    validColors = ValidBOBColors.Where(x => x[0] == _currentSolution[0]).Select(x => x[1].ToString()).Distinct().ToArray();
                    result = ValidBOBColors.Any(x => x[0] == _currentSolution[0] && x[1] == color);
                }
                else
                {
                    validColors = ValidBOBColors.Where(x => x[0] == _currentSolution[0] && x[1] == _currentSolution[1]).Select(x => x[2].ToString()).Distinct().ToArray();
                    result = ValidBOBColors.Any(x => x[0] == _currentSolution[0] && x[1] == _currentSolution[1] && x[2] == color);
                }
                BombModule.LogFormat("Stage {0}: Pressed {1}. Bob Expected {2}", _currentState + 1, color, string.Join(", ", validColors));
                if (result)
                {
                    _currentSolution[_currentState] = color;
                    BombModule.LogFormat("Stage {0} Correct.", _currentState + 1);
                    _currentState++;
                    if (_currentState != _currentSolution.Length) return;
                    BombModule.LogFormat("Module Passed");
                    var bobColors = ValidBOBColors.SelectMany(x => x).ToArray();
                    StartCoroutine(FadeCircle(_wedgeColors[(int)bobColors[Rnd.Range(0,bobColors.Length)]]));
                }
                else
                {
                    BombModule.LogFormat("Stage {0} Incorrect. Strike", _currentState + 1);
                    _currentState = 0;

                    BombModule.HandleStrike();
                }

                return;
            }

            if (_currentSolution == null)
                return;


            BombModule.LogFormat("Stage {0}: Pressed {1}. I Expected {2}", _currentState + 1, color, _currentSolution[_currentState]);
            if (color == _currentSolution[_currentState])
            {
                BombModule.LogFormat("Stage {0} Correct.", _currentState + 1);
                _currentState++;
                if (_currentState != _currentSolution.Length) return;
                BombModule.LogFormat("Module Passed");
                StartCoroutine(FadeCircle(new Color(160f / 255, 160f / 255, 160f / 255)));
            }
            else
            {
                BombModule.LogFormat("Stage {0} Incorrect. Strike", _currentState + 1);
                _currentState = 0;

                BombModule.HandleStrike();
            }
        }
        catch (Exception ex)
        {
            BombModule.LogFormat("Exception caused by {0}\n{1}", ex.Message, ex.StackTrace);
            BombModule.Log("Module passed by exception");
            StartCoroutine(FadeCircle(_wedgeColors[(int)WedgeColors.Red]));
        }
    }

    private IEnumerator FadeCircle(Color solvedColor)
    {
        _solved = true;
        const float targetTime = 1.0f / 60f;
        var totalTime = 0f;
        var faded = false;
        do
        {
            totalTime += Time.deltaTime;
            if (totalTime < targetTime)
            {
                yield return null;
                continue;
            }
            faded = true;
            var count = 0;
            while (totalTime >= targetTime)
            {
                totalTime -= targetTime;
                count++;
            }
            foreach (var wedge in WedgeRenderers)
            {
                var r = (int) (wedge.material.color.r * 255);
                var g = (int) (wedge.material.color.g * 255);
                var b = (int) (wedge.material.color.b * 255);
                var solvedR = (int) (solvedColor.r * 255);
                var solvedG = (int) (solvedColor.g * 255);
                var solvedB = (int) (solvedColor.b * 255);

                for (var i = 0; i < count; i++)
                {
                    if (r < solvedR)
                        r++;
                    else if (r > solvedR)
                        r--;

                    if (g < solvedG)
                        g++;
                    else if (g > solvedG)
                        g--;

                    if (b < solvedB)
                        b++;
                    else if (b > solvedB)
                        b--;
                }

                faded &= r == solvedR;
                faded &= g == solvedG;
                faded &= b == solvedB;

                wedge.material.color = new Color((float) r / 255, (float) g / 255, (float) b / 255);
            }
            yield return null;
        } while (!faded);
        BombModule.HandlePass();
        _spinning = false;
    }

    private IEnumerator UpdateSolution()
    {
        if (IsBobPresent())
            yield break;

        var solved = BombInfo.GetSolvedModuleNames().Count;
        var twofactorsum = BombInfo.GetTwoFactorCodes().Sum(twofactor => twofactor % 10);
        BombModule.LogFormat("Getting solution for 0 modules solved and Two Factor sum of {0}", twofactorsum);
        _currentSolution = GetSolution(solved, twofactorsum);

        do
        {
            yield return new WaitForSeconds(0.1f);
            var solvedUpdate = BombInfo.GetSolvedModuleNames().Count;
            var twofactorUpdate = BombInfo.GetTwoFactorCodes().Sum(twofactor => twofactor % 10);
            if (_currentState > 0 || (solved == solvedUpdate && twofactorsum == twofactorUpdate))
                continue;
            if(solved != solvedUpdate)
                BombModule.LogFormat("Updating solution for {0} modules solved",solvedUpdate);
            if(twofactorsum != twofactorUpdate)
                BombModule.LogFormat("Updating solution for new two factor sum of {0}",twofactorUpdate);
            solved = solvedUpdate;
            twofactorsum = twofactorUpdate;
            _currentSolution = GetSolution(solved, twofactorsum);
        } while (!_solved);
    }

    private IEnumerator SpinCircle()
    {
        do
        {
            var framerate = 1f / Time.deltaTime;
            var rotation = 6f / framerate; //6 degrees per second.
            if (_rotateCounterClockwise)
                rotation *= -1;

            var y = Circle.transform.localEulerAngles.y;
            y += rotation;
            Circle.transform.localEulerAngles = new Vector3(0, y, 0);

            yield return null;
        } while (_spinning);
    }


    public string TwitchHelpMessage = "Submit the correct response with !{0} press blue black red.  (Valid colors are Red, Orange, Yellow, Green, Blue, Magenta, White, blacK)";
    public string[] TwitchValidCommands ={"press(?> (?>red|orange|yellow|green|blue|magenta|purple|white|black|[roygbmwkp])){1,3}"};
    public IEnumerator ProcessTwitchCommand(string command)
    {
        if (!command.StartsWith("press ", StringComparison.InvariantCultureIgnoreCase))
            yield break;
        var split = command.ToLowerInvariant().Substring(6).Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);

        var buttons = new Dictionary<string, WedgeColors>
        {
            {"red", WedgeColors.Red},
            {"r", WedgeColors.Red},
            {"orange", WedgeColors.Orange},
            {"o", WedgeColors.Orange},
            {"yellow", WedgeColors.Yellow},
            {"y", WedgeColors.Yellow},
            {"green", WedgeColors.Green},
            {"g", WedgeColors.Green},
            {"blue", WedgeColors.Blue},
            {"b", WedgeColors.Blue},
            {"magenta", WedgeColors.Magenta},
            {"m", WedgeColors.Magenta},
            {"purple",WedgeColors.Magenta },
            {"p",WedgeColors.Magenta },
            {"white", WedgeColors.White},
            {"w", WedgeColors.White},
            {"black", WedgeColors.Black},
            {"k", WedgeColors.Black}
        };

        foreach(var c in split)
            if (!buttons.ContainsKey(c))
                yield break;

        yield return null;
        foreach (var c in split)
        {
            HandleWedge(buttons[c]);
            yield return new WaitForSeconds(0.1f);
            if (!_solved) continue;
            yield return "solve";
            yield break;
        }
    }

}