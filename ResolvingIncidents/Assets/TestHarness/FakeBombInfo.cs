using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;


public class FakeBombInfo : MonoBehaviour
{
    // ReSharper disable InconsistentNaming
    [Header("Bomb Configuration")]
    public float TimeLeft = (10 * 60f) + 0f;
    public int NumStrikes = 3;

    [Serializable]
    public class ModdedWidgetConfiguration
    {
        public bool TwoFactor = false;
        public bool EncryptedIndicators = false;
        public bool MultipleWidgets = true;
    }
    public ModdedWidgetConfiguration ModdedWidget = new ModdedWidgetConfiguration();


    //WidgetExpanderOptions

    [Serializable]
    public class WidgetExpanderConfiguration
    {
        public bool EnableSerialNumberLettersOY = false;
        public bool EnableCustomIndicators = false;
        public int MinCustomIndicators = 1;
        public bool EnableWidgetExpansion = false;
        public int MinWidgets = 5;
        public int MaxWidgets = 7;
    }
    public WidgetExpanderConfiguration WidgetExpander = new WidgetExpanderConfiguration();

    [Serializable]
    public class MultipleWidgetConfiguration
    {
        public bool EnableTwoFactorMultipleWidgets = true;
        public int MultipleWidgetsTwoFactoryExpiry = 60;
    }
    public MultipleWidgetConfiguration MultipleWidget = new MultipleWidgetConfiguration();

 
    Widget GetRandomWidget()
    {
        global::MultipleWidget.EnableTowFactor = MultipleWidget.EnableTwoFactorMultipleWidgets;
        global::MultipleWidget.TwoFactorExpiry = MultipleWidget.MultipleWidgetsTwoFactoryExpiry;

        var choices = new List<int> { 0, 1, 2 };
        if (ModdedWidget.MultipleWidgets)
            choices.Add(3);
        if (ModdedWidget.TwoFactor)
            choices.Add(4);
        if (ModdedWidget.EncryptedIndicators)
            choices.Add(5);
        var choice = choices[Random.Range(0, choices.Count)];
        switch (choice)
        {
            case 0:
                return gameObject.AddComponent<BatteryWidget>();
            case 1:
                return gameObject.AddComponent<IndicatorWidget>();
            case 2:
                return gameObject.AddComponent<PortWidget>();
            case 4:
                return gameObject.AddComponent<TwoFactorWidget>();
            case 5:
                return gameObject.AddComponent<EncryptedIndicatorWidget>();
            default:
            {
                var widget = gameObject.AddComponent<global::MultipleWidget>();
                widget.Init();
                return widget;
            }
        }
    }

    private List<string> _customIndicators;
    private void InitCustomIndicators()
    {
        string _letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        _customIndicators = new List<string>();

        foreach (char x in _letters)
        {
            foreach (char y in _letters)
            {
                foreach (char z in _letters)
                {
                    _customIndicators.Add(x.ToString() + y + z);
                }
            }
        }

        foreach (string x in IndicatorWidget.possibleValues)
        {
            _customIndicators.Remove(x);
        }
        _customIndicators.Remove("NLL");

        int n = _customIndicators.Count;
        while (n-- > 0)
        {
            int k = UnityEngine.Random.Range(0, n + 1);
            string value = _customIndicators[k];
            _customIndicators[k] = _customIndicators[n];
            _customIndicators[n] = value;
        }
    }

    bool IsSerialNumberValid()
    {
        var allchars = WidgetExpander.EnableSerialNumberLettersOY 
            ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
            : "ABCDEFGHIJKLMNEPQRSTUVWXZ0123456789";

        var letters = allchars.Substring(0, allchars.Length - 10);
        var numbers = allchars.Substring(allchars.Length - 10);
        return serial.Length == 6 && allchars.Contains(serial.Substring(0, 1)) 
            && allchars.Contains(serial.Substring(1, 1)) 
            && numbers.Contains(serial.Substring(2, 1)) 
            && letters.Contains(serial.Substring(3, 1)) 
            && letters.Contains(serial.Substring(4, 1)) 
            && numbers.Contains(serial.Substring(5, 1));
    }

    void Awake()
    {
        if (WidgetExpander.EnableCustomIndicators)
        {
            InitCustomIndicators();
            var custom = Math.Max(WidgetExpander.MinCustomIndicators, WidgetExpander.MaxWidgets - 12);
            custom = Math.Min(custom, _customIndicators.Count);
            IndicatorWidget.possibleValues.Add("NLL");
            for (var i = 0; i < custom; i++)
                IndicatorWidget.possibleValues.Add(_customIndicators[i]);
        }

        var widgetCount = WidgetExpander.EnableWidgetExpansion
            ? Random.Range(WidgetExpander.MinWidgets, WidgetExpander.MaxWidgets + 1)
            : 5;


        widgets.AddRange(FindObjectsOfType<Widget>());
        for (int a = widgets.Count; a < widgetCount; a++)
        {
            //widgets[a] = ForceUnicorn ? GetUnicornWidget(a) : GetRandomWidget();
            widgets.Add(GetRandomWidget());
        }


        if (!IsSerialNumberValid())
        {
            char[] possibleCharArray = WidgetExpander.EnableSerialNumberLettersOY
                ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray()
                : "ABCDEFGHIJKLMNEPQRSTUVWXZ0123456789".ToCharArray();
            string str1 = string.Empty;
            for (int index = 0; index < 2; ++index)
                str1 = str1 + possibleCharArray[Random.Range(0, possibleCharArray.Length)];
            string str2 = str1 + (object) Random.Range(0, 10);
            for (int index = 3; index < 5; ++index)
                str2 = str2 + possibleCharArray[Random.Range(0, possibleCharArray.Length - 10)];
            serial = str2 + Random.Range(0, 10);
        }

        Debug.Log("Serial: " + serial);
    }

    float startupTime = 3f;

    public delegate void LightsOn();
    public LightsOn ActivateLights;

    void FixedUpdate()
    {
        if (solved) return;
        if (detonated) return;
        if (startupTime > 0)
        {
            startupTime -= Time.fixedDeltaTime;
            if (!(startupTime < 0)) return;

            ActivateLights();

            foreach (Widget w in widgets)
            {
                if (w.OnWidgetActivate != null) w.OnWidgetActivate();
            }
            foreach (KMWidget w in kmWidgets)
            {
                if (w.OnWidgetActivate != null) w.OnWidgetActivate();
            }
            for (var i = 0; i < modules.Count; i++)
            {
                if (modules[i].Key.OnActivate != null)
                    modules[i].Key.OnActivate();
            }
            foreach (KMNeedyModule m in needyModules)
            {
                if (m.OnActivate != null) m.OnActivate();
            }
        }
        else
        {
            var multiplier = 1f;
            switch (strikes)
            {
                case 0:
                    multiplier = 1;
                    break;
                case 1:
                    multiplier = 1.25f;
                    break;
                case 2:
                    multiplier = 1.5f;
                    break;
                case 3:
                    multiplier = 3f;
                    break;
                default:
                    multiplier = 6f;
                    break;
            }

            TimeLeft -= Time.fixedDeltaTime * multiplier;
            if (TimeLeft > 0) return;
            TimeLeft = 0;
            detonated = true;
            if (Detonate != null) Detonate();
            Debug.Log("KABOOM!!! - Time Ran out");
        }
    }

    public bool solved;
    public bool detonated;
    public int strikes = 0;
    public string serial;

    public float GetTime()
    {
        return TimeLeft;
    }

    public string GetFormattedTime()
    {
        string time = "";
        if (TimeLeft < 60)
        {
            if (TimeLeft < 10) time += "0";
            time += (int) TimeLeft;
            time += ".";
            int s = (int) (TimeLeft * 100);
            if (s < 10) time += "0";
            time += s;
        }
        else
        {
            if (TimeLeft < 600) time += "0";
            time += (int) TimeLeft / 60;
            time += ":";
            int s = (int) TimeLeft % 60;
            if (s < 10) time += "0";
            time += s;
        }
        return time;
    }

    public int GetStrikes()
    {
        return strikes;
    }

    public List<KeyValuePair<KMBombModule, bool>> modules = new List<KeyValuePair<KMBombModule, bool>>();
    public List<KMNeedyModule> needyModules = new List<KMNeedyModule>();
    public List<KMWidget> kmWidgets = new List<KMWidget>();
    public List<Widget> widgets = new List<Widget>();

    public List<string> GetModuleNames()
    {
        List<string> moduleList = new List<string>();
        foreach (KeyValuePair<KMBombModule, bool> m in modules)
        {
            moduleList.Add(m.Key.ModuleDisplayName);
        }
        foreach (KMNeedyModule m in needyModules)
        {
            moduleList.Add(m.ModuleDisplayName);
        }
        return moduleList;
    }

    public List<string> GetSolvableModuleNames()
    {
        List<string> moduleList = new List<string>();
        foreach (KeyValuePair<KMBombModule, bool> m in modules)
        {
            moduleList.Add(m.Key.ModuleDisplayName);
        }
        return moduleList;
    }

    public List<string> GetSolvedModuleNames()
    {
        List<string> moduleList = new List<string>();
        foreach (KeyValuePair<KMBombModule, bool> m in modules)
        {
            if (m.Value) moduleList.Add(m.Key.ModuleDisplayName);
        }
        return moduleList;
    }

    public List<string> GetWidgetQueryResponses(string queryKey, string queryInfo)
    {
        List<string> responses = new List<string>();
        if (queryKey == KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER)
        {
            responses.Add(JsonConvert.SerializeObject((object) new Dictionary<string, string>()
            {
                {
                    "serial", serial
                }
            }));
        }
        foreach (Widget w in widgets)
        {
            string r = null;
            if (w.OnQueryRequest != null)
                r = w.OnQueryRequest(queryKey, queryInfo);
            if (r != null) responses.Add(r);
        }

        foreach (KMWidget w in kmWidgets)
        {
            string r = null;
            if (w.OnQueryRequest != null)
                r = w.OnQueryRequest(queryKey, queryInfo);
            if (r != null) responses.Add(r);
        }

        return responses;
    }

    public bool IsBombPresent()
    {
        return true;
    }

    public void HandleStrike()
    {
        strikes++;
        Debug.Log(strikes + "/" + NumStrikes);
        if (strikes == NumStrikes)
        {
            if (Detonate != null) Detonate();
            Debug.Log("KABOOM!");
            detonated = true;
        }
    }

    public delegate void OnDetonate();
    public OnDetonate Detonate;

    public void HandleStrike(string reason)
    {
        Debug.Log("Strike: " + reason);
        HandleStrike();
    }

    public delegate void OnSolved();
    public OnSolved HandleSolved;

    public void Solved()
    {
        solved = true;
        if (HandleSolved != null) HandleSolved();
        Debug.Log("Bomb defused!");
    }

    public delegate void LightState(bool state);
    public LightState OnLights;
    public void OnLightsOn()
    {
        if (OnLights != null) OnLights(true);
    }

    public void OnLightsOff()
    {
        if (OnLights != null) OnLights(false);
    }
}