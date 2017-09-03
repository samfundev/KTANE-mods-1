using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class IndicatorWidget : Widget
{
    [HideInInspector]
    public static List<string> possibleValues = new List<string>(){
        "SND","CLR","CAR",
        "IND","FRQ","SIG",
        "NSA","MSA","TRN",
        "BOB","FRK"
    };

    public string Label;
    public bool Lit;

    void Start()
    {
        OnQueryRequest += GetResult;
        Init();
    }

    public void Init()
    {
        if (string.IsNullOrEmpty(Label))
        {
            int pos = Random.Range(0, possibleValues.Count);
            Label = possibleValues[pos];
            possibleValues.RemoveAt(pos);

            Lit = Random.value > 0.4f;
        }
        else
        {
            possibleValues.Remove(Label);
        }
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
        return null;
    }
}