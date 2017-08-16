using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    // Use this for initialization
    void Start ()
    {
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
        BombModule.LogFormat("Colors in Clockwise order: {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", _colors[0], _colors[1], _colors[2], _colors[3], _colors[4], _colors[5], _colors[6], _colors[7]);
        BombModule.OnActivate += delegate { _activated = true; StartCoroutine(SpinCircle()); StartCoroutine(UpdateSolution());};
    }


    private bool _baseOffsetGenerated;
    private int _baseOffset;
    int GetBaseOffset()
    {
        if (_baseOffsetGenerated)
            return _baseOffset;

        var total = 0;
        foreach (var indicator in BombInfo.GetOnIndicators())
        {
            switch (indicator)
            {
                case "IND":
                    break;
                case "BOB":
                case "CAR":
                case "CLR":
                    BombModule.Log("Lit BOB, CAR, CLR - Adding 1");
                    total += 1;
                    break;
                case "FRK":
                case "FRQ":
                case "MSA":
                case "NSA":
                    BombModule.Log("Lit FRK, FRQ, MSA, NSA - Adding 2");
                    total += 2;
                    break;
                case "SIG":
                case "SND":
                case "TRN":
                    BombModule.Log("Lit SIG, SND, TRN - Adding 3");
                    total += 3;
                    break;
                default:
                    BombModule.Log("Custom Indicator - Adding 6");
                    total += 6;
                    break;
            }
        }
        BombModule.LogFormat("Total after Adding Lit Indicators: {0}", total);

        foreach (var indicator in BombInfo.GetOffIndicators())
        {
            switch (indicator)
            {
                case "IND":
                    break;
                case "BOB":
                case "CAR":
                case "CLR":
                    BombModule.Log("Unlit BOB, CAR, CLR - Subtracting 1");
                    total -= 1;
                    break;
                case "FRK":
                case "FRQ":
                case "MSA":
                case "NSA":
                    BombModule.Log("Unlit FRK, FRQ, MSA, NSA - Subtracting 2");
                    total -= 2;
                    break;
                case "SIG":
                case "SND":
                case "TRN":
                    BombModule.Log("Unlit SIG, SND, TRN - Subtracting 3");
                    total -= 3;
                    break;
                default:
                    BombModule.Log("Custom Indicator - Adding 6");
                    total += 6;
                    break;
            }
        }
        BombModule.LogFormat("Total after Adding Unlit Indicators: {0}", total);

        total += BombInfo.GetBatteryCount() % 2 == 0 ? -4 : 4;
        BombModule.LogFormat("Total after Batteries: {0}", total);

        foreach (var plate in BombInfo.GetPortPlates())
        {
            foreach (var port in plate)
            {
                if (port == KMBombInfoExtensions.KnownPortType.Parallel.ToString())
                {
                    BombModule.Log("Port Plate with Parallel port found");
                    if (plate.Contains(KMBombInfoExtensions.KnownPortType.Serial.ToString()))
                    {
                        BombModule.Log("Paired with a Serial port - Subtracting 5");
                        total -= 5;
                    }
                    else
                    {
                        BombModule.LogFormat("Not paired with a Serial port - Adding 5");
                        total += 5;
                    }
                    continue;
                }
                if (port == KMBombInfoExtensions.KnownPortType.Serial.ToString()) continue;
                if (port == KMBombInfoExtensions.KnownPortType.DVI.ToString())
                {
                    BombModule.Log("Port plate with DVI-D port found");
                    if (plate.Contains(KMBombInfoExtensions.KnownPortType.StereoRCA.ToString()))
                    {
                        BombModule.Log("Paired with Stereo-RCA - Adding 5");
                        total += 5;
                    }
                    else
                    {
                        BombModule.LogFormat("Not paired with Stereo-RCA - Subtracting 5");
                        total -= 5;
                    }
                    continue;
                }
                if (port == KMBombInfoExtensions.KnownPortType.RJ45.ToString()) continue;
                if (port == KMBombInfoExtensions.KnownPortType.StereoRCA.ToString()) continue;
                if (port == KMBombInfoExtensions.KnownPortType.PS2.ToString()) continue;
                BombModule.LogFormat("Special Port {0} found - Subtracting 6", port);
                total = total - 6;
            }
        }
        BombModule.LogFormat("Total after Adding Ports: {0}", total);

        _baseOffset = total;

        _baseOffsetGenerated = true;
        return _baseOffset;
    }


    WedgeColors[] GetSolution(int solved, int twofactor)
    {
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

        var total = GetBaseOffset();
        BombModule.LogFormat("Base Total for current solution: {0}", total);


        total += solved * 4;
        BombModule.LogFormat("Total after Adding Solved Modules: {0}", total);

        total += twofactor;
        BombModule.LogFormat("Total after Adding TwoFactors: {0}", total);

        if (total < 0)
        {
            BombModule.Log("Total is Negative. Multiplying by -1");
            total *= -1;
        }

        var serial = BombInfo.GetSerialNumber().ToUpperInvariant();
        serial += serial.Substring(4, 1) + serial.Substring(3, 1) + serial.Substring(2, 1) + serial.Substring(1, 1);

        var index = serial.Substring(total % 10, 1);
        BombModule.LogFormat("Extended Serial# = {0}. Using Character {1}, which is {2}", serial, total % 10, index);

        var colorIndex = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(index, StringComparison.Ordinal);

        if (colorIndex < 0)
        {
            BombModule.LogFormat("Unrecognized Serial number character: {0} - Passing the module now", index);
            StartCoroutine(FadeCircle(_wedgeColors[(int)WedgeColors.Red]));
            return null;
        }

        var lookup = new List<WedgeColors>(colorLookup[colorIndex / 3]);
        BombModule.LogFormat("Current Solution: {0}, {1}, {2}", lookup[0], lookup[1], lookup[2]);


        if (_rotateCounterClockwise)
        {
            BombModule.LogFormat("Circle is rotating counter-Clockwise. Reversing solution to {0} {1} {2}", lookup[2], lookup[1], lookup[0]);
            lookup.Reverse();
        }

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

            if (BombInfo.GetBatteryCount() == 5 && BombInfo.GetBatteryHolderCount() == 3 && BombInfo.IsIndicatorPresent(KMBombInfoExtensions.KnownIndicatorLabel.BOB))
            {
                var bobColors = BombInfo.GetColoredIndicators(KMBombInfoExtensions.KnownIndicatorLabel.BOB);
                var enumerable = bobColors as string[] ?? bobColors.ToArray();
                if (enumerable.Contains(color.ToString()) 
                    || enumerable.Contains(KMBombInfoExtensions.KnownIndicatorColors.Gray.ToString())
                    || (color == WedgeColors.Magenta && enumerable.Contains(KMBombInfoExtensions.KnownIndicatorColors.Purple.ToString())))
                {
                    BombModule.LogFormat("BOB, our true Saviour has once again come to save the day. Module passed.");
                    StartCoroutine(FadeCircle(_wedgeColors[(int) color]));
                }

                else
                {
                    BombModule.LogFormat("BOB is expecting you to press one of his colors. Strike");
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
        bool faded;
        do
        {
            faded = true;
            foreach (var wedge in WedgeRenderers)
            {
                var r = (int) (wedge.material.color.r * 255);
                var g = (int) (wedge.material.color.g * 255);
                var b = (int) (wedge.material.color.b * 255);
                var solvedR = (int) (solvedColor.r * 255);
                var solvedG = (int) (solvedColor.g * 255);
                var solvedB = (int) (solvedColor.b * 255);

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

public enum WedgeColors
{
    Red,
    Orange,
    Yellow,
    Green,
    Blue,
    Magenta,
    White,
    Black
}
