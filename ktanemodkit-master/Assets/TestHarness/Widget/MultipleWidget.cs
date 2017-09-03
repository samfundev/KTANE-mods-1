using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MultipleWidget : Widget
{
    public Widget[] widgets;

    public static bool EnableTowFactor = true;
    public static int TwoFactorExpiry = 60;

    void Start()
    {
        OnQueryRequest += GetResult;
        OnWidgetActivate += Activate;
        if (widgets == null)
        {
            Init();
        }
    }

    public void Init()
    {
        
        widgets = new Widget[2];
        Debug.Log("Start of Multiple Widgets");
        var choices = new List<int> { 0, 1, 2 };
        if (EnableTowFactor)
            choices.Add(3);
        for (var i = 0; i < 2; i++)
        {
            var choice = choices[Random.Range(0, choices.Count)];
            choices.Remove(choice);
            switch (choice)
            {
                case 0:
                    var batteryWidget = gameObject.AddComponent<BatteryWidget>();
                    batteryWidget.Init(true);
                    widgets[i] = batteryWidget; 
                    break;
                case 1:
                    var portWidget = gameObject.AddComponent<PortWidget>();
                    portWidget.Init(true);
                    widgets[i] = portWidget;
                    break;
                case 2:
                    var encryptedIndicator = gameObject.AddComponent<EncryptedIndicatorWidget>();
                    encryptedIndicator.Init(true);
                    widgets[i] = encryptedIndicator;
                    break;
                default:
                    var twofactor = gameObject.AddComponent<TwoFactorWidget>();
                    twofactor.Init(TwoFactorExpiry);
                    widgets[i] = twofactor;
                    break;
            }
        }
        Debug.Log("End of Multiple Widgets");
    }

    public void Activate()
    {
        foreach(var w in widgets)
            if (w.OnWidgetActivate != null)
                w.OnWidgetActivate();
    }

    public string GetResult(string key, string data)
    {
        return (from w in widgets where w.OnQueryRequest != null where w.OnQueryRequest(key, data) != null select w.OnQueryRequest(key, data)).FirstOrDefault();
    }
}