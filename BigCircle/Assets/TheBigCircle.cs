using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rnd = UnityEngine.Random;

public class TheBigCircle : MonoBehaviour
{

    public GameObject Circle;
    public KMSelectable[] Wedges;
    public MeshRenderer[] WedgeRenderers;

    public KMBombModule BombModule;
    public KMAudio Audio;
    public KMBombInfo BombInfo;

    private bool _rotateCounterClockwise = false;
    private bool _solved = false;
    private bool _allgray = false;
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


    private Dictionary<string, List<WedgeColors>> _colorLookup = new Dictionary<string, List<WedgeColors>>
    {
        {"0", new List<WedgeColors> {WedgeColors.Red, WedgeColors.Yellow, WedgeColors.Blue}},
        {"1", new List<WedgeColors> {WedgeColors.Red, WedgeColors.Yellow, WedgeColors.Blue}},
        {"2", new List<WedgeColors> {WedgeColors.Red, WedgeColors.Yellow, WedgeColors.Blue}},
        {"3", new List<WedgeColors> {WedgeColors.Orange, WedgeColors.Green, WedgeColors.Magenta}},
        {"4", new List<WedgeColors> {WedgeColors.Orange, WedgeColors.Green, WedgeColors.Magenta }},
        {"5", new List<WedgeColors> {WedgeColors.Orange, WedgeColors.Green, WedgeColors.Magenta }},
        {"6", new List<WedgeColors> {WedgeColors.Blue, WedgeColors.Black, WedgeColors.Red}},
        {"7", new List<WedgeColors> {WedgeColors.Blue, WedgeColors.Black, WedgeColors.Red }},
        {"8", new List<WedgeColors> {WedgeColors.Blue, WedgeColors.Black, WedgeColors.Red }},
        {"9", new List<WedgeColors> {WedgeColors.Magenta, WedgeColors.White, WedgeColors.Orange}},
        {"A", new List<WedgeColors> {WedgeColors.Magenta, WedgeColors.White, WedgeColors.Orange }},
        {"B", new List<WedgeColors> {WedgeColors.Magenta, WedgeColors.White, WedgeColors.Orange }},
        {"C", new List<WedgeColors> {WedgeColors.Orange, WedgeColors.Blue, WedgeColors.Black}},
        {"D", new List<WedgeColors> {WedgeColors.Orange, WedgeColors.Blue, WedgeColors.Black }},
        {"E", new List<WedgeColors> {WedgeColors.Orange, WedgeColors.Blue, WedgeColors.Black }},
        {"F", new List<WedgeColors> {WedgeColors.Green, WedgeColors.Red, WedgeColors.White}},
        {"G", new List<WedgeColors> {WedgeColors.Green, WedgeColors.Red, WedgeColors.White }},
        {"H", new List<WedgeColors> {WedgeColors.Green, WedgeColors.Red, WedgeColors.White }},
        {"I", new List<WedgeColors> {WedgeColors.Magenta, WedgeColors.Yellow, WedgeColors.Black}},
        {"J", new List<WedgeColors> {WedgeColors.Magenta, WedgeColors.Yellow, WedgeColors.Black }},
        {"K", new List<WedgeColors> {WedgeColors.Magenta, WedgeColors.Yellow, WedgeColors.Black }},
        {"L", new List<WedgeColors> {WedgeColors.Red, WedgeColors.Orange, WedgeColors.Yellow}},
        {"M", new List<WedgeColors> {WedgeColors.Red, WedgeColors.Orange, WedgeColors.Yellow }},
        {"N", new List<WedgeColors> {WedgeColors.Red, WedgeColors.Orange, WedgeColors.Yellow }},
        {"O", new List<WedgeColors> {WedgeColors.Yellow, WedgeColors.Green, WedgeColors.Blue}},
        {"P", new List<WedgeColors> {WedgeColors.Yellow, WedgeColors.Green, WedgeColors.Blue }},
        {"Q", new List<WedgeColors> {WedgeColors.Yellow, WedgeColors.Green, WedgeColors.Blue }},
        {"R", new List<WedgeColors> {WedgeColors.Blue, WedgeColors.Magenta, WedgeColors.Red}},
        {"S", new List<WedgeColors> {WedgeColors.Blue, WedgeColors.Magenta, WedgeColors.Red }},
        {"T", new List<WedgeColors> {WedgeColors.Blue, WedgeColors.Magenta, WedgeColors.Red }},
        {"U", new List<WedgeColors> {WedgeColors.Black, WedgeColors.White, WedgeColors.Green}},
        {"V", new List<WedgeColors> {WedgeColors.Black, WedgeColors.White, WedgeColors.Green }},
        {"W", new List<WedgeColors> {WedgeColors.Black, WedgeColors.White, WedgeColors.Green }},
        {"X", new List<WedgeColors> {WedgeColors.White, WedgeColors.Yellow, WedgeColors.Blue}},
        {"Y", new List<WedgeColors> {WedgeColors.White, WedgeColors.Yellow, WedgeColors.Blue }},
        {"Z", new List<WedgeColors> {WedgeColors.White, WedgeColors.Yellow, WedgeColors.Blue }},
    };

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
        BombModule.OnActivate += delegate { _activated = true; };
    }

    List<WedgeColors> GetSolution()
    {
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
        BombModule.LogFormat("Total after Adding Lit Indicators: {0}",total);

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



        total += BombInfo.GetSolvedModuleNames().Count * 4;
        BombModule.LogFormat("Total after Adding Solved Modules: {0}", total);


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
                if (port == KMBombInfoExtensions.KnownPortType.DVI.ToString()) continue;
                if (port == KMBombInfoExtensions.KnownPortType.RJ45.ToString()) continue;
                if (port == KMBombInfoExtensions.KnownPortType.StereoRCA.ToString()) continue;
                if (port == KMBombInfoExtensions.KnownPortType.PS2.ToString()) continue;
                BombModule.LogFormat("Special Port {0} found - Subtracting 6", port);
                total = total - 6;
            }
        }
        BombModule.LogFormat("Total after Adding Ports: {0}", total);

        total += BombInfo.GetTwoFactorCodes().Sum(twofactor => twofactor % 10);
        BombModule.LogFormat("Total after Adding TwoFactors: {0}", total);


        if (total < 0)
        {
            BombModule.Log("Total is Negative. Multiplying by -1");
            total *= -1;
        }

        var serial = BombInfo.GetSerialNumber().ToUpperInvariant();
        serial += serial.Substring(4, 1) + serial.Substring(3, 1) + serial.Substring(2, 1) + serial.Substring(1, 1);

        BombModule.LogFormat("Extended Serial# = {0}. Using Character {1}, which is {2}", serial, total % 10, serial.Substring(total % 10, 1));

        var lookup = new List<WedgeColors>(_colorLookup[serial.Substring(total % 10, 1)]);
        BombModule.LogFormat("Current Solution: {0}, {1}, {2}", lookup[0], lookup[1], lookup[2]);


        if (_rotateCounterClockwise)
        {
            BombModule.LogFormat("Circle is rotating counter-Clockwise. Reversing solution to {0} {1} {2}", lookup[2], lookup[1], lookup[0]);
            lookup.Reverse();
        }

        return lookup;
    }



    private int _currentState = 0;
    private List<WedgeColors> _currentSolution;
    void HandleWedge(WedgeColors color)
    {
        if (!_activated)
        {
            BombModule.LogFormat("Pressed {0} before module has activated",color);
            BombModule.HandleStrike();
        }
        if (_solved)
            return;

        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BombModule.transform);

        if (BombInfo.GetBatteryCount() == 5 && BombInfo.GetBatteryHolderCount() == 3 && BombInfo.IsIndicatorOff("BOB"))
        {
            BombModule.LogFormat("BOB, our true Saviour has once again come to save the day. Module passed.");
            _solved = true;
        }

        if (_currentSolution == null)
            _currentSolution = GetSolution();


        BombModule.LogFormat("Stage {0}: Pressed {1}. I Expected {2}", _currentState+1,color, _currentSolution[_currentState]);
        if (color == _currentSolution[_currentState])
        {
            BombModule.LogFormat("Stage {0} Correct.",_currentState+1);
            _currentState++;
            if (_currentState != _currentSolution.Count) return;
            BombModule.LogFormat("Module Passed");
            _solved = true;
        }
        else
        {
            BombModule.LogFormat("Stage {0} Incorrect. Strike", _currentState + 1);
            _currentState = 0;
            _currentSolution = null;
            BombModule.HandleStrike();
        }
    }

	
	// Update is called once per frame
	void Update ()
	{
	    if (_allgray)
	        return;

	    if (_solved)
	    {
	        var AllGray = true;
            foreach (var wedge in WedgeRenderers)
	        {
	            var r = (int)(wedge.material.color.r * 255);
	            var g = (int) (wedge.material.color.g * 255);
	            var b = (int) (wedge.material.color.b * 255);

	            if (r < 0xA0)
	                r++;
                else if (r > 0xA0)
	                r--;

	            if (g < 0xA0)
	                g++;
                else if (g > 0xA0)
	                g--;

	            if (b < 0xA0)
	                b++;
                else if (b > 0xA0)
	                b--;

	            AllGray &= r == 0xA0;
	            AllGray &= b == 0xA0;
	            AllGray &= g == 0xA0;

	            wedge.material.color = new Color((float)r / 255, (float)g / 255, (float)b / 255);
	        }
	        if (AllGray)
	        {
	            _allgray = true;
	            BombModule.HandlePass();
	        }
        }
	    
	    var y = Circle.transform.localEulerAngles.y;
	    y += _rotateCounterClockwise ? -0.25f : 0.25f;
	    Circle.transform.localEulerAngles = new Vector3(0, y, 0);
    }

    public string TwitchHelpMessage = "Submit the correct response with !{0} press blue black red.  (Valid colors are Red, Orange, Yellow, Green, Blue, Magenta, White, blacK)";
    private IEnumerator ProcessTwitchCommand(string command)
    {
        if (!command.StartsWith("press ", StringComparison.InvariantCultureIgnoreCase))
            yield break;
        var split = command.ToLowerInvariant().Substring(6).Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);

        Dictionary<string, WedgeColors> buttons = new Dictionary<string, WedgeColors>
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
