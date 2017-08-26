using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

public class BatteryWidget : Widget
{
    public BatteryType Batteries = BatteryType.NotSet;

    public enum BatteryType
    {
        NotSet = -1,
        EmptyHolder,
        OneDCell,
        TwoAA,
        ThreeAA,
        FourAA
    }

    void Start()
    {
        OnQueryRequest += GetResult;
        if (Batteries == BatteryType.NotSet)
        {
            Init();
        }
    }

    public void Init(bool extended = false)
    {
        Batteries = (BatteryType)(extended ? Random.Range(0,5) : Random.Range(1, 3));
        Debug.Log("Added battery widget: " + (int)Batteries);
    }

    public string GetResult(string key, string data)
    {
        if (key == KMBombInfo.QUERYKEY_GET_BATTERIES)
        {
            return JsonConvert.SerializeObject((object)new Dictionary<string, int>()
            {
                {
                    "numbatteries", (int)Batteries
                }
            });
        }
        return null;
    }
}