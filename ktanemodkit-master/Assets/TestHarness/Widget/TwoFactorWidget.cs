using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class TwoFactorWidget : Widget
{
    public int TwoFactorKey;
    public int ExpiryTime;
    public float ElapsedTime;

    private static int _increment = 1;
    private int _id;

    void Start()
    {
        if (ExpiryTime == 0)
            Init();
    }

    public void Init(int ExpiryTime = 60)
    {
        OnQueryRequest += GetResult;
        OnWidgetActivate += Activate;
        if (ExpiryTime < 30)
            ExpiryTime = 30;
        if (ExpiryTime > 999)
            ExpiryTime = 999;

        _id = _increment;
        _increment++;
        this.ExpiryTime = ExpiryTime;
    }

    public string GetResult(string key, string data)
    {
        if (key == "twofactor")
        {
            return JsonConvert.SerializeObject((object)new Dictionary<string, int>()
            {
                {
                    "twofactor_key", TwoFactorKey
                }
            });
        }
        return null;
    }

    public void Activate()
    {
        UpdateKey();
    }

    private void UpdateKey()
    {
        ElapsedTime = 0;
        TwoFactorKey = Random.Range(0, 1000000);
        Debug.LogFormat("Two Factor Key #{0} = {1}", _id, TwoFactorKey);
    }

    void Update()
    {
        ElapsedTime += Time.deltaTime;
        if (ElapsedTime < ExpiryTime) return;
        UpdateKey();
    }
}