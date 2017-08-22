using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Xml.Serialization;
using Microsoft.Win32;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class FakeBombInfo : MonoBehaviour
{
    //Bomb Configuration
    public float timeLeft = (10 * 60f) + 0f;
    public const int numStrikes = 3;

    //Used with code below to force a particular set of widgets to ALWAYS show up in the test harness
    //Useful for testing various rules, including unicorn rules you may have implemented into the module.
    private bool _forceUnicorn = false;

    //Modded Widgets
    private bool TwoFactor = false;
    private bool EncryptedIndicators = false;
    private bool MultipleWidgets = true;

    //WidgetExpanderOptions
    private bool EnableSerialNumberLettersOY = false;
    private bool EnableCustomIndicators = false;
    private int MinCustomIndicators = 1;
    private bool EnableWidgetExpansion = false;
    private int MinWidgets = 5;
    private int MaxWidgets = 7;

    //Multiple Widgets configuration
    private bool _enableTwoFactorMultipleWidgets = true;
    private int _multipleWidgetsTwoFactoryExpiry = 60;
    


    //Write your custom widget testing rules here.
    Widget GetUnicornWidget(int a)
    {
        switch (a)
        {
            case 0:
                return new EncryptedIndicatorWidget(true, "BOB", "Black");
            case 1:
                return new BatteryWidget(true, 2);
            case 2:
                return new BatteryWidget(true, 1);
            case 3:
                return new BatteryWidget(true, 1);
            case 4:
                return new TwoFactorWidget(30);
            default:
                return GetRandomWidget();
        }
    }

    #region Widgets
    public abstract class Widget : Object
    {
        public abstract string GetResult(string key, string data);
        public abstract void Update();
    }

    public class MultipleWidget : Widget
    {
        private Widget[] widgets = new Widget[2];

        public MultipleWidget(bool enableTowFactor, int twoFactorExpiry=60, Widget widget1=null, Widget widget2=null)
        {
            if (widget1 != null && widget2 != null)
            {
                widgets[0] = widget1;
                widgets[1] = widget2;
                return;
            }
            Debug.Log("Start of Multiple Widgets");
            var choices = new List<int> {0, 1, 2};
            if (enableTowFactor)
                choices.Add(3);
            for (var i = 0; i < 2; i++)
            {
                var choice = choices[Random.Range(0, choices.Count)];
                choices.Remove(choice);
                if(choice == 0)
                    widgets[i] = new BatteryWidget(true);
                else if (choice == 1)
                    widgets[i] = new PortWidget(true);
                else if (choice == 2)
                    widgets[i] = new EncryptedIndicatorWidget(true);
                else
                    widgets[i] = new TwoFactorWidget(twoFactorExpiry);
            }
            Debug.Log("End of Multiple Widgets");
        }

        public override string GetResult(string key, string data)
        {
            return (from widget in widgets where widget.GetResult(key, data) != null select widget.GetResult(key, data)).FirstOrDefault();
        }

        public override void Update()
        {
            widgets[0].Update();
            widgets[1].Update();
        }

    }

    public class TwoFactorWidget : Widget
    {
        private int _expiryTime;
        private float _elapsedTime;
        private int _key;
        private static int _increment = 1;
        private int _id;

        public TwoFactorWidget(int ExpiryTime = 60)
        {
            if (ExpiryTime < 30)
                ExpiryTime = 30;
            if (ExpiryTime > 120)
                ExpiryTime = 120;

            _id = _increment;
            _increment++;
            _expiryTime = ExpiryTime;
            UpdateKey();
        }

        public override string GetResult(string key, string data)
        {
            if (key == "twofactor")
            {
                return JsonConvert.SerializeObject((object)new Dictionary<string, int>()
                {
                    {
                        "twofactor_key", _key
                    }
                });
            }
            return null;
        }

        private void UpdateKey()
        {
            _elapsedTime = 0;
            _key = Random.Range(0, 1000000);
            Debug.LogFormat("Two Factor Key #{0} = {1}", _id, _key);
        }

        public override void Update()
        {
            _elapsedTime += Time.deltaTime;
            if (_elapsedTime < _expiryTime) return;
            UpdateKey();
        }
    }

    public class PortWidget : Widget
    {
        [Flags]
        public enum PortType
        {
            None           = 0,
            DVI            = 1 << 0,
            Parallel       = 1 << 1,
            PS2            = 1 << 2,
            RJ45           = 1 << 3,
            Serial         = 1 << 4,
            StereoRCA      = 1 << 5,
            ComponentVideo = 1 << 6,
            CompositeVideo = 1 << 7,
            USB            = 1 << 8,
            HDMI           = 1 << 9,
            VGA            = 1 << 10,
            AC             = 1 << 11,
            PCMCIA         = 1 << 12
        }

        List<string> ports;

        public PortWidget(bool extended = false, List<PortType> unicornPorts = null)
        {
            ports = new List<string>();
            PortType portList = PortType.None;

            var portPlates = new List<List<PortType>>
            {
                new List<PortType> {PortType.Serial, PortType.Parallel},
                new List<PortType> {PortType.PS2,PortType.DVI,PortType.RJ45,PortType.StereoRCA },
                new List<PortType> {PortType.HDMI,PortType.USB,PortType.ComponentVideo,PortType.AC,PortType.PCMCIA,PortType.VGA,PortType.CompositeVideo },
                new List<PortType> {PortType.DVI,PortType.StereoRCA,PortType.HDMI,PortType.ComponentVideo,PortType.VGA,PortType.CompositeVideo,PortType.AC },
                new List<PortType> {PortType.Parallel,PortType.Serial,PortType.PCMCIA,PortType.VGA,PortType.PS2,PortType.RJ45,PortType.USB,PortType.AC }
            };

            var plate = portPlates[1];
            if (!extended)
            {
                if (Random.value > 0.5)
                {
                    plate = portPlates[0];
                }
            }
            else
            {
                plate = portPlates[Random.Range(0, portPlates.Count)];
            }
            foreach (var port in plate)
            {
                if (!(Random.value > 0.5)) continue;
                ports.Add(port.ToString());
                portList |= port;
            }

            if (portList == PortType.None)
                Debug.Log("Added port widget: Empty plate");
            else
                Debug.Log("Added port widget: " + portList);
        }

        public override string GetResult(string key, string data)
        {
            if (key == KMBombInfo.QUERYKEY_GET_PORTS)
            {
                return JsonConvert.SerializeObject((object) new Dictionary<string, List<string>>()
                {
                    {
                        "presentPorts", ports
                    }
                });
            }
            return null;
        }

        public override void Update()
        {

        }
    }

    public class EncryptedIndicatorWidget : Widget
    {
        private string val;
        private bool on;
        private string color;
        private bool enableColors;

        private static string[] PossibleColors =
        {
            "Black", "White", "Red", "Orange", "Yellow", "Green", "Blue", "Purple", "Mangenta", "Gray"
        };

        private readonly string[] _possibleValues = {"CLR, IND", "TRN", "FRK", "CAR", "FRQ", "NSA", "SIG", "MSA", "SND", "BOB"};

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

        public EncryptedIndicatorWidget(bool enableColors = false, string unicornLabel = null, string unicornColor = null)
        {
            on = Random.value > 0.4;
            if (enableColors)
            {
                if (unicornColor != null)
                {
                    color = unicornColor;
                    @on = unicornColor != PossibleColors[0];
                }
                else
                {
                    if (!@on)
                        color = PossibleColors[0];
                    else
                    {
                        color = PossibleColors[Random.Range(1, PossibleColors.Length)];
                    }
                }
            }
            if (unicornLabel != null)
            {
                val = unicornLabel;
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
                        val = _possibleValues[totalval-1];
                    else
                        val = _columnStrings[0].Substring(val0, 1) + _columnStrings[1].Substring(val1, 1) +
                              _columnStrings[2].Substring(val2, 1);
                }
                catch
                {
                    val = _possibleValues[Random.Range(0, _possibleValues.Length)];
                }

            }

            if (enableColors)
                Debug.Log("Added indicator widget: " + val + " is " + (on ? "ON" : "OFF") + ", Color is " + color);
            else
                Debug.Log("Added indicator widget: " + val + " is " + (on ? "ON" : "OFF"));

        }

        public override string GetResult(string key, string data)
        {
            if (key == KMBombInfo.QUERYKEY_GET_INDICATOR)
            {
                return JsonConvert.SerializeObject((object)new Dictionary<string, string>()
                {
                    {
                        "label", val
                    },
                    {
                        "on", on?bool.TrueString:bool.FalseString
                    }
                });
            }
            if (key == (KMBombInfo.QUERYKEY_GET_INDICATOR + "Color") && enableColors)
            {
                return JsonConvert.SerializeObject((object)new Dictionary<string, string>()
                {
                    {
                        "label", val
                    },
                    {
                        "color", color
                    }
                });
            }
            return null;
        }

        public override void Update()
        {

        }
    }


    public class IndicatorWidget : Widget
    {
        public static List<string> possibleValues = new List<string>(){
            "SND","CLR","CAR",
            "IND","FRQ","SIG",
            "NSA","MSA","TRN",
            "BOB","FRK"
        };

        private string val;
        private bool on;

        public IndicatorWidget(string unicornLabel=null, string unicornValue=null)
        {
           
                if (unicornLabel != null)
                {
                    val = unicornLabel;
                    possibleValues.Remove(val);
                }
                else
                {
                    int pos = Random.Range(0, possibleValues.Count);
                    val = possibleValues[pos];
                    possibleValues.RemoveAt(pos);
                }

                if (unicornValue != null)
                {
                    on = unicornValue == "true";
                }
                else
                {
                    on = Random.value > 0.4f;
                }
            Debug.Log("Added indicator widget: " + val + " is " + (on ? "ON" : "OFF"));
        }

        public override string GetResult(string key, string data)
        {
            if (key == KMBombInfo.QUERYKEY_GET_INDICATOR)
            {
                return JsonConvert.SerializeObject((object) new Dictionary<string, string>()
                {
                    {
                        "label", val
                    },
                    {
                        "on", on?bool.TrueString:bool.FalseString
                    }
                });
            }
            return null;
        }

        public override void Update()
        {

        }
    }

    public class BatteryWidget : Widget
    {
        private int batt;

        public BatteryWidget(bool extended=false, int forceUnicorn = -1)
        {
            batt = extended ? Random.Range(0,5) : Random.Range(1, 3);
            if (forceUnicorn > -1)
                batt = forceUnicorn;

            Debug.Log("Added battery widget: " + batt);
        }

        public override string GetResult(string key, string data)
        {
            if (key == KMBombInfo.QUERYKEY_GET_BATTERIES)
            {
                return JsonConvert.SerializeObject((object) new Dictionary<string, int>()
                {
                    {
                        "numbatteries", batt
                    }
                });
            }
            return null;
        }

        public override void Update()
        {

        }
    }
    #endregion
    Widget GetRandomWidget()
    {
        var choices = new List<int> { 0, 1, 2 };
        if (MultipleWidgets)
            choices.Add(3);
        if (TwoFactor)
            choices.Add(4);
        if (EncryptedIndicators)
            choices.Add(5);
        var choice = choices[Random.Range(0, choices.Count)];
        switch (choice)
        {
            case 0:
                return new BatteryWidget();
            case 1:
                return new IndicatorWidget();
            case 2:
                return new PortWidget();
            case 4:
                return new TwoFactorWidget();
            case 5:
                return new EncryptedIndicatorWidget();
            default:
                return new MultipleWidget(_enableTwoFactorMultipleWidgets, _multipleWidgetsTwoFactoryExpiry);
        }
    }

    public Widget[] widgets;

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

    void Awake()
    {
        if (EnableCustomIndicators)
        {
            InitCustomIndicators();
            var custom = Math.Max(MinCustomIndicators, MaxWidgets - 12);
            custom = Math.Min(custom, _customIndicators.Count);
            IndicatorWidget.possibleValues.Add("NLL");
            for (var i = 0; i < custom; i++)
                IndicatorWidget.possibleValues.Add(_customIndicators[i]);
        }

        widgets = EnableWidgetExpansion ? new Widget[Random.Range(MinWidgets, MaxWidgets + 1)] : new Widget[5]; 
        
        for (int a = 0; a < widgets.Length; a++)
        {
            widgets[a] = _forceUnicorn ? GetUnicornWidget(a) : GetRandomWidget();
        }

        char[] possibleCharArray = EnableSerialNumberLettersOY ?
        new [] {
            'A','B','C','D','E','F','G','H','I','J','K','L',
            'M','N','O','P','Q','R','S','T','U','V','W','X',
            'Y','Z','0','1','2','3','4','5','6','7','8','9'
        } : new []
        {
            'A','B','C','D','E','F','G','H','I','J','K','L',
            'M','N','E','P','Q','R','S','T','U','V','W','X',
            'Z','0','1','2','3','4','5','6','7','8','9'
        };
        string str1 = string.Empty;
        for (int index = 0; index < 2; ++index) str1 = str1 + possibleCharArray[Random.Range(0, possibleCharArray.Length)];
        string str2 = str1 + (object) Random.Range(0, 10);
        for (int index = 3; index < 5; ++index) str2 = str2 + possibleCharArray[Random.Range(0, possibleCharArray.Length - 10)];
        serial = str2 + Random.Range(0, 10);

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
            if (startupTime < 0)
            {
                ActivateLights();
                foreach (KeyValuePair<KMBombModule, bool> m in modules)
                {
                    if (m.Key.OnActivate != null) m.Key.OnActivate();
                }
                foreach (KMNeedyModule m in needyModules)
                {
                    if (m.OnActivate != null) m.OnActivate();
                }
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

            timeLeft -= Time.fixedDeltaTime * multiplier;
            if (timeLeft < 0)
            {
                timeLeft = 0;
                detonated = true;
                Debug.Log("KABOOM!!! - Time Ran out");
            }
        }

        foreach (var widget in widgets)
            widget.Update();
    }

    public bool solved;
    public bool detonated;
    public int strikes = 0;
    public string serial;

    public float GetTime()
    {
        return timeLeft;
    }

    public string GetFormattedTime()
    {
        string time = "";
        if (timeLeft < 60)
        {
            if (timeLeft < 10) time += "0";
            time += (int) timeLeft;
            time += ".";
            int s = (int) (timeLeft * 100);
            if (s < 10) time += "0";
            time += s;
        }
        else
        {
            if (timeLeft < 600) time += "0";
            time += (int) timeLeft / 60;
            time += ":";
            int s = (int) timeLeft % 60;
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
            string r = w.GetResult(queryKey, queryInfo);
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
        Debug.Log(strikes + "/" + numStrikes);
        if (strikes == numStrikes)
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

public class TestHarness : MonoBehaviour
{
    private FakeBombInfo fakeInfo;

    public StatusLight StatusLightPrefab;
    public GameObject HighlightPrefab;
    public AudioClip StrikeAudio;
    public KMAudio Audio;

    TestSelectable currentSelectable;
    TestSelectableArea currentSelectableArea;

    AudioSource audioSource;
    List<AudioClip> audioClips;

    void Awake()
    {
        fakeInfo = gameObject.AddComponent<FakeBombInfo>();
        fakeInfo.ActivateLights += delegate ()
        {
            TurnLightsOn();
            fakeInfo.OnLightsOn();
        };
        TurnLightsOff();

        ReplaceBombInfo();
        AddHighlightables();
        AddSelectables();
    }

    void ReplaceBombInfo()
    {
        MonoBehaviour[] scripts = MonoBehaviour.FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour s in scripts)
        {
            IEnumerable<FieldInfo> fields = s.GetType().GetFields();
            foreach (FieldInfo f in fields)
            {
                if (f.FieldType.Equals(typeof(KMBombInfo)))
                {
                    KMBombInfo component = (KMBombInfo) f.GetValue(s);
                    component.TimeHandler += new KMBombInfo.GetTimeHandler(fakeInfo.GetTime);
                    component.FormattedTimeHandler += new KMBombInfo.GetFormattedTimeHandler(fakeInfo.GetFormattedTime);
                    component.StrikesHandler += new KMBombInfo.GetStrikesHandler(fakeInfo.GetStrikes);
                    component.ModuleNamesHandler += new KMBombInfo.GetModuleNamesHandler(fakeInfo.GetModuleNames);
                    component.SolvableModuleNamesHandler += new KMBombInfo.GetSolvableModuleNamesHandler(fakeInfo.GetSolvableModuleNames);
                    component.SolvedModuleNamesHandler += new KMBombInfo.GetSolvedModuleNamesHandler(fakeInfo.GetSolvedModuleNames);
                    component.WidgetQueryResponsesHandler += new KMBombInfo.GetWidgetQueryResponsesHandler(fakeInfo.GetWidgetQueryResponses);
                    component.IsBombPresentHandler += new KMBombInfo.KMIsBombPresent(fakeInfo.IsBombPresent);
                    continue;
                }
                if (f.FieldType.Equals(typeof(KMGameInfo)))
                {
                    KMGameInfo component = (KMGameInfo) f.GetValue(s);
                    component.OnLightsChange += new KMGameInfo.KMLightsChangeDelegate(fakeInfo.OnLights);
                    //component.OnAlarmClockChange += new KMGameInfo.KMAlarmClockChangeDelegate(fakeInfo.OnAlarm);
                    continue;
                }
                if (f.FieldType.Equals(typeof(KMGameCommands)))
                {
                    KMGameCommands component = (KMGameCommands) f.GetValue(s);
                    component.OnCauseStrike += new KMGameCommands.KMCauseStrikeDelegate(fakeInfo.HandleStrike);
                    continue;
                }
            }
        }
    }

    void Start()
    {
        MonoBehaviour[] scripts = MonoBehaviour.FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour s in scripts)
        {
            IEnumerable<FieldInfo> fields = s.GetType().GetFields();
            foreach (FieldInfo f in fields)
            {
                if (f.FieldType.Equals(typeof(KMBombInfo)))
                {
                    KMBombInfo component = (KMBombInfo) f.GetValue(s);
                    if (component.OnBombExploded != null) fakeInfo.Detonate += new FakeBombInfo.OnDetonate(component.OnBombExploded);
                    if (component.OnBombSolved != null) fakeInfo.HandleSolved += new FakeBombInfo.OnSolved(component.OnBombSolved);
                    continue;
                }
            }
        }

        currentSelectable = GetComponent<TestSelectable>();

        KMBombModule[] modules = FindObjectsOfType<KMBombModule>();
        KMNeedyModule[] needyModules = FindObjectsOfType<KMNeedyModule>();
        fakeInfo.needyModules = needyModules.ToList();
        currentSelectable.Children = new TestSelectable[modules.Length + needyModules.Length];
        for (int i = 0; i < modules.Length; i++)
        {
            KMBombModule mod = modules[i];

            KMStatusLightParent statuslightparent = modules[i].GetComponentInChildren<KMStatusLightParent>();
            var statuslight = Instantiate<StatusLight>(StatusLightPrefab);
            statuslight.transform.parent = statuslightparent.transform;
            statuslight.transform.localPosition = Vector3.zero;
            statuslight.transform.localScale = Vector3.one;
            statuslight.transform.localRotation = Quaternion.identity;
            statuslight.SetInActive();

            currentSelectable.Children[i] = modules[i].GetComponent<TestSelectable>();
            modules[i].GetComponent<TestSelectable>().Parent = currentSelectable;

            fakeInfo.modules.Add(new KeyValuePair<KMBombModule, bool>(modules[i], false));
            modules[i].OnPass = delegate ()
            {
                Debug.Log("Module Passed");
                statuslight.SetPass();

                fakeInfo.modules.Remove(fakeInfo.modules.First(t => t.Key.Equals(mod)));
                fakeInfo.modules.Add(new KeyValuePair<KMBombModule, bool>(mod, true));
                bool allSolved = !fakeInfo.detonated;
                foreach (KeyValuePair<KMBombModule, bool> m in fakeInfo.modules)
                {
                    if (!allSolved)
                        break;
                    allSolved &= m.Value;
                }
                if (allSolved) fakeInfo.Solved();
                return false;
            };
            var j = i;
            modules[i].OnStrike = delegate ()
            {
                Debug.Log("Strike");
                Audio.HandlePlaySoundAtTransform(StrikeAudio.name, transform);
                statuslight.FlashStrike();
                fakeInfo.HandleStrike();
                return false;
            };
        }

        for (int i = 0; i < needyModules.Length; i++)
        {
            currentSelectable.Children[modules.Length + i] = needyModules[i].GetComponent<TestSelectable>();
            needyModules[i].GetComponent<TestSelectable>().Parent = currentSelectable;

            needyModules[i].OnPass = delegate ()
            {
                Debug.Log("Module Passed");
                return false;
            };
            needyModules[i].OnStrike = delegate ()
            {
                Debug.Log("Strike");
                fakeInfo.HandleStrike();
                return false;
            };
        }

        currentSelectable.ActivateChildSelectableAreas();


        //Load all the audio clips in the asset database
        audioClips = new List<AudioClip>();
        string[] audioClipAssetGUIDs = AssetDatabase.FindAssets("t:AudioClip");

        foreach (var guid in audioClipAssetGUIDs)
        {
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AssetDatabase.GUIDToAssetPath(guid));

            if (clip != null)
            {
                audioClips.Add(clip);
            }
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        KMAudio[] kmAudios = FindObjectsOfType<KMAudio>();
        foreach (KMAudio kmAudio in kmAudios)
        {
            kmAudio.HandlePlaySoundAtTransform += PlaySoundHandler;
        }
    }

    protected void PlaySoundHandler(string clipName, Transform t)
    {
        if (audioClips.Count > 0)
        {
            AudioClip clip = audioClips.Where(a => a.name == clipName).First();

            if (clip != null)
            {
                audioSource.transform.position = t.position;
                audioSource.PlayOneShot(clip);
            }
        }
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction);
        RaycastHit hit;
        int layerMask = 1 << 11;
        bool rayCastHitSomething = Physics.Raycast(ray, out hit, 1000, layerMask);
        if (rayCastHitSomething)
        {
            TestSelectableArea hitArea = hit.collider.GetComponent<TestSelectableArea>();
            if (hitArea != null)
            {
                if (currentSelectableArea != hitArea)
                {
                    if (currentSelectableArea != null)
                    {
                        currentSelectableArea.Selectable.Deselect();
                    }

                    hitArea.Selectable.Select();
                    currentSelectableArea = hitArea;
                }
            }
            else
            {
                if (currentSelectableArea != null)
                {
                    currentSelectableArea.Selectable.Deselect();
                    currentSelectableArea = null;
                }
            }
        }
        else
        {
            if (currentSelectableArea != null)
            {
                currentSelectableArea.Selectable.Deselect();
                currentSelectableArea = null;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (currentSelectableArea != null && currentSelectableArea.Selectable.Interact())
            {
                currentSelectable.DeactivateChildSelectableAreas(currentSelectableArea.Selectable);
                currentSelectable = currentSelectableArea.Selectable;
                currentSelectable.ActivateChildSelectableAreas();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (currentSelectableArea != null)
            {
                currentSelectableArea.Selectable.InteractEnded();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (currentSelectable.Parent != null && currentSelectable.Cancel())
            {
                currentSelectable.DeactivateChildSelectableAreas(currentSelectable.Parent);
                currentSelectable = currentSelectable.Parent;
                currentSelectable.ActivateChildSelectableAreas();
            }
        }
    }

    void AddHighlightables()
    {
        List<KMHighlightable> highlightables = new List<KMHighlightable>(GameObject.FindObjectsOfType<KMHighlightable>());

        foreach (KMHighlightable highlightable in highlightables)
        {
            TestHighlightable highlight = highlightable.gameObject.AddComponent<TestHighlightable>();

            highlight.HighlightPrefab = HighlightPrefab;
            highlight.HighlightScale = highlightable.HighlightScale;
            highlight.OutlineAmount = highlightable.OutlineAmount;
        }
    }

    void AddSelectables()
    {
        List<KMSelectable> selectables = new List<KMSelectable>(GameObject.FindObjectsOfType<KMSelectable>());

        foreach (KMSelectable selectable in selectables)
        {
            TestSelectable testSelectable = selectable.gameObject.AddComponent<TestSelectable>();
            testSelectable.Highlight = selectable.Highlight.GetComponent<TestHighlightable>();
        }

        foreach (KMSelectable selectable in selectables)
        {
            TestSelectable testSelectable = selectable.gameObject.GetComponent<TestSelectable>();
            testSelectable.Children = new TestSelectable[selectable.Children.Length];
            for (int i = 0; i < selectable.Children.Length; i++)
            {
                if (selectable.Children[i] != null)
                {
                    testSelectable.Children[i] = selectable.Children[i].GetComponent<TestSelectable>();
                }
            }
        }
    }

    // TPK Methods
    protected void DoInteractionStart(KMSelectable interactable)
    {
        interactable.OnInteract();
    }

    protected void DoInteractionEnd(KMSelectable interactable)
    {
        if (interactable.OnInteractEnded != null)
        {
            interactable.OnInteractEnded();
        }
    }

    Dictionary<Component, HashSet<KMSelectable>> ComponentHelds = new Dictionary<Component, HashSet<KMSelectable>> { };
    IEnumerator SimulateModule(Component component, Transform moduleTransform, MethodInfo method, string command)
    {
        // Simple Command
        if (method.ReturnType == typeof(KMSelectable[]))
        {
            KMSelectable[] selectableSequence = null;
            try
            {
                selectableSequence = (KMSelectable[]) method.Invoke(component, new object[] { command });
                if (selectableSequence == null)
                {
                    yield break;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogErrorFormat("An exception occurred while trying to invoke {0}.{1}; the command invokation will not continue.", method.DeclaringType.FullName, method.Name);
                Debug.LogException(ex);
                yield break;
            }

            int initialStrikes = fakeInfo.strikes;
            int initialSolved = fakeInfo.GetSolvedModuleNames().Count;
            foreach (KMSelectable selectable in selectableSequence)
            {
                DoInteractionStart(selectable);
                yield return new WaitForSeconds(0.1f);
                DoInteractionEnd(selectable);

                if (fakeInfo.strikes != initialStrikes || fakeInfo.GetSolvedModuleNames().Count != initialSolved)
                {
                    break;
                }
            };
        }

        // Complex Commands
        if (method.ReturnType == typeof(IEnumerator))
        {
            IEnumerator responseCoroutine = null;
            try
            {
                responseCoroutine = (IEnumerator) method.Invoke(component, new object[] { command });
            }
            catch (System.Exception ex)
            {
                Debug.LogErrorFormat("An exception occurred while trying to invoke {0}.{1}; the command invokation will not continue.", method.DeclaringType.FullName, method.Name);
                Debug.LogException(ex);
                yield break;
            }

            if (responseCoroutine == null)
                yield break;

            if (!ComponentHelds.ContainsKey(component))
                ComponentHelds[component] = new HashSet<KMSelectable>();
            HashSet<KMSelectable> heldSelectables = ComponentHelds[component];

            int initialStrikes = fakeInfo.strikes;
            int initialSolved = fakeInfo.GetSolvedModuleNames().Count;

            while (responseCoroutine.MoveNext())
            {
                object currentObject = responseCoroutine.Current;
                if (currentObject is KMSelectable)
                {
                    KMSelectable selectable = (KMSelectable) currentObject;
                    if (heldSelectables.Contains(selectable))
                    {
                        DoInteractionEnd(selectable);
                        heldSelectables.Remove(selectable);
                    }
                    else
                    {
                        DoInteractionStart(selectable);
                        heldSelectables.Add(selectable);
                    }
                }
                else if (currentObject is string)
                {
                    Debug.Log("Twitch handler sent: " + currentObject);
                    yield return currentObject;
                }
                else if (currentObject is Quaternion)
                {
                    moduleTransform.localRotation = (Quaternion) currentObject;
                }
                else
                    yield return currentObject;

                if (fakeInfo.strikes != initialStrikes || fakeInfo.GetSolvedModuleNames().Count != initialSolved)
                    yield break;
            }
        }
    }

    string command = "";
    void OnGUI()
    {
        if (GUILayout.Button("Activate Needy Modules"))
        {
            foreach (KMNeedyModule needyModule in GameObject.FindObjectsOfType<KMNeedyModule>())
            {
                if (needyModule.OnNeedyActivation != null)
                {
                    needyModule.OnNeedyActivation();
                }
            }
        }

        if (GUILayout.Button("Deactivate Needy Modules"))
        {
            foreach (KMNeedyModule needyModule in GameObject.FindObjectsOfType<KMNeedyModule>())
            {
                if (needyModule.OnNeedyDeactivation != null)
                {
                    needyModule.OnNeedyDeactivation();
                }
            }
        }

        if (GUILayout.Button("Lights On"))
        {
            TurnLightsOn();
            fakeInfo.OnLightsOn();
        }

        if (GUILayout.Button("Lights Off"))
        {
            TurnLightsOff();
            fakeInfo.OnLightsOff();
        }

        GUILayout.Label("Time remaining: " + fakeInfo.GetFormattedTime());

        GUILayout.Space(10);

        command = GUILayout.TextField(command);
        if ((GUILayout.Button("Simulate Twitch Command") || Event.current.keyCode == KeyCode.Return) && command != "")
        {
            Debug.Log("Twitch Command: " + command);

            foreach (KMBombModule module in FindObjectsOfType<KMBombModule>())
            {
                Component[] allComponents = module.gameObject.GetComponentsInChildren<Component>(true);
                foreach (Component component in allComponents)
                {
                    System.Type type = component.GetType();
                    MethodInfo method = type.GetMethod("ProcessTwitchCommand", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    if (method != null)
                    {
                        StartCoroutine(SimulateModule(component, module.transform, method, command));
                    }
                }
            }
            command = "";
        }
    }

    public void TurnLightsOn()
    {
        RenderSettings.ambientIntensity = 1f;
        DynamicGI.UpdateEnvironment();

        foreach (Light l in FindObjectsOfType<Light>())
            if (l.transform.parent == null)
                l.enabled = true;
    }

    public void TurnLightsOff()
    {
        RenderSettings.ambientIntensity = 0.2f;
        DynamicGI.UpdateEnvironment();

        foreach (Light l in FindObjectsOfType<Light>())
            if (l.transform.parent == null)
                l.enabled = false;
    }
}
