using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

public class EncryptedIndicatorWidget : Widget
{
    public string Label;
    public bool Lit;

    public IndicatorColors Color = IndicatorColors.None;

    public enum IndicatorColors
    {
        None,
        Black,
        White,
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Purple,
        Magenta,
        Gray
    }


    private static readonly IndicatorColors[] PossibleColors = (IndicatorColors[])Enum.GetValues(typeof(IndicatorColors));

    private readonly string[] _possibleValues = { "CLR, IND", "TRN", "FRK", "CAR", "FRQ", "NSA", "SIG", "MSA", "SND", "BOB" };

    private readonly int[][] _columnInts =
    {
        new [] {5, 4,  0, 0, 2, -2, 4, 3, 4, 3, -1, -1, 5},
        new [] {0, 0, -1, 2, 1,  5, 1, 5, 4, 2,  3, -2, 0},
        new [] {4, 5,  4, 5, 2,  5, 2, 4, 2, 3,  4,  4, 5}
    };

    private readonly string[] _columnStrings =
    {
        "GZCJVTLGFPKDQ",
        "DDSXBLAASOQNO",
        "GROYLJOSMFKLZ"
    };

    void Start()
    {
        OnQueryRequest += GetResult;
        if (string.IsNullOrEmpty(Label))
        {
            Init();
        }

    }

    public void Init(bool enableColors = false)
    {
        Lit = Random.value > 0.4;
        if (enableColors)
        {
            Color = !Lit ? IndicatorColors.Black : PossibleColors[Random.Range(1, PossibleColors.Length)];
        }
        else
        {

            var val0 = Random.Range(0, 13);
            var val1 = Random.Range(0, 13);
            while (val1 == val0)
                val1 = Random.Range(0, 13);
            var val2 = Random.Range(0, 13);
            while (val2 == val0 || val2 == val1)
                val2 = Random.Range(0, 13);
            try
            {
                var totalval = _columnInts[0][val0] + _columnInts[1][val1] + _columnInts[2][val2];
                if (totalval > 0 && totalval <= _possibleValues.Length)
                    Label = _possibleValues[totalval - 1];
                else
                    Label = _columnStrings[0].Substring(val0, 1) + _columnStrings[1].Substring(val1, 1) +
                          _columnStrings[2].Substring(val2, 1);
            }
            catch
            {
                Label = _possibleValues[Random.Range(0, _possibleValues.Length)];
            }

        }

        if (enableColors)
            Debug.Log("Added indicator widget: " + Label + " is " + (Lit ? "ON" : "OFF") + ", Color is " + Color);
        else
            Debug.Log("Added indicator widget: " + Label + " is " + (Lit ? "ON" : "OFF"));

    }

    public string GetResult(string key, string data)
    {
        if (key == KMBombInfo.QUERYKEY_GET_INDICATOR)
        {
            return JsonConvert.SerializeObject((object)new Dictionary<string, string>()
            {
                {
                    "label", Label
                },
                {
                    "on", Lit?bool.TrueString:bool.FalseString
                }
            });
        }
        if (key == (KMBombInfo.QUERYKEY_GET_INDICATOR + "Color") && Color != IndicatorColors.None)
        {
            return JsonConvert.SerializeObject((object)new Dictionary<string, string>()
            {
                {
                    "label", Label
                },
                {
                    "color", Color.ToString()
                }
            });
        }
        return null;
    }
}