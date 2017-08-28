using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FakeStatusLight : MonoBehaviour
{

    public GameObject GreenLight;
    public GameObject RedLight;
    public GameObject OffLight;

    public StatusLightState PassColor = StatusLightState.Green;
    public StatusLightState FailColor = StatusLightState.Red;
    public StatusLightState OffColor = StatusLightState.Off;
    public StatusLightState MorseTransmitColor = StatusLightState.Green;

    public KMBombModule Module;

    public bool IsFakeStatusLightReady { get; private set; }
    public bool HasFakeStatusLightFailed { get; private set; }

    private bool _green;
    private bool _off;
    private bool _red;

    private bool _flashingStrike;
    private bool _passedForReal;

    public StatusLightState HandlePass(StatusLightState state = StatusLightState.Off)
    {
        _passedForReal = true;
        _flashingStrike = false;
        if (Module == null) return SetLightColor(state);
        Module.HandlePass();
        return SetLightColor(state);
    }

    public StatusLightState SetLightColor(StatusLightState color)
    {
        switch (color)
        {
            case StatusLightState.Random:
                color = (StatusLightState) Random.Range(0, 3);
                if (color == StatusLightState.Red) goto case StatusLightState.Red;
                if (color == StatusLightState.Green) goto case StatusLightState.Green;
                goto case StatusLightState.Off;
            case StatusLightState.Red:
                _red = true;
                _green = false;
                _off = false;
                break;
            case StatusLightState.Green:
                _red = false;
                _green = true;
                _off = false;
                break;
            case StatusLightState.Off:
            default:
                _red = false;
                _green = false;
                _off = true;
                break;
        }
        return color;
    }

    public void GetStatusLights(Transform statusLightParent)
    {
        StartCoroutine(GetStatusLight(statusLightParent));
    }

    protected IEnumerator GetStatusLight(Transform statusLightParent)
    {
        for (var i = 0; i < 60; i++)
        {
            var off = statusLightParent.FindDeepChild("Component_LED_OFF");
            var pass = statusLightParent.FindDeepChild("Component_LED_PASS");
            var fail = statusLightParent.FindDeepChild("Component_LED_STRIKE");
            if (off == null || pass == null || fail == null)
            {
                yield return null;
                continue;
            }
            IsFakeStatusLightReady = true;
            OffLight = off.gameObject;
            GreenLight = pass.gameObject;
            RedLight = fail.gameObject;
            yield break;
        }
        HasFakeStatusLightFailed = true;
    }

    public void HandleStrike()
    {
        if (Module == null) return;
        Module.HandleStrike();
        FlashStrike();
    }


    void Update()
    {
        if (_flashingStrike) return;
        if (GreenLight != null)
            GreenLight.SetActive(_green);
        if (OffLight != null)
            OffLight.SetActive(_off);
        if (RedLight != null)
            RedLight.SetActive(_red);
    }

    public void SetPass()
    {
        SetLightColor(PassColor);
    }

    public void SetInActive()
    {
        SetLightColor(OffColor);
    }

    public void SetStrike()
    {
        SetLightColor(FailColor);
    }

    private IEnumerator _flashingStrikeCoRoutine;
    public void FlashStrike()
    {
        if (_passedForReal) return;
        if (!gameObject.activeInHierarchy) return;
        if (_flashingStrikeCoRoutine != null)
            StopCoroutine(_flashingStrikeCoRoutine);
        _flashingStrike = false;
        _flashingStrikeCoRoutine = StrikeFlash(1f);
        StartCoroutine(_flashingStrikeCoRoutine);
    }

    protected IEnumerator StrikeFlash(float blinkTime)
    {
        SetStrike();
        Update();
        _flashingStrike = true;
        yield return new WaitForSeconds(blinkTime);
        _flashingStrike = false;
        _flashingStrikeCoRoutine = null;
    }

    private static IEnumerable<int> Morsify(string text)
    {
        var values = text.ToUpperInvariant().ToCharArray();
        var data = new List<int>();
        for (int a = 0; a < values.Length; a++)
        {
            if (a > 0) data.Add(-1);
            char c = values[a];
            switch (c)
            {
                /*case ' ':
                    data.Add(-1);
                    break;*/
                case 'A':
                    data.AddRange(new [] {0,1});
                    break;
                case 'B':
                    data.AddRange(new[] { 1,0,0,0 });
                    break;
                case 'C':
                    data.AddRange(new[] { 1,0,1,0 });
                    break;
                case 'D':
                    data.AddRange(new[] { 1,0,0 });
                    break;
                case 'E':
                    data.AddRange(new[] { 0 });
                    break;
                case 'F':
                    data.AddRange(new[] { 0,0,1,0 });
                    break;
                case 'G':
                    data.AddRange(new[] { 1,1,0 });
                    break;
                case 'H':
                    data.AddRange(new[] { 0,0,0,0 });
                    break;
                case 'I':
                    data.AddRange(new[] { 0,0 });
                    break;
                case 'J':
                    data.AddRange(new[] { 0,1,1,1 });
                    break;
                case 'K':
                    data.AddRange(new[] { 1,0,1 });
                    break;
                case 'L':
                    data.AddRange(new[] { 0,1,0,0 });
                    break;
                case 'M':
                    data.AddRange(new[] { 1,1 });
                    break;
                case 'N':
                    data.AddRange(new[] { 1,0 });
                    break;
                case 'O':
                    data.AddRange(new[] { 1,1,1 });
                    break;
                case 'P':
                    data.AddRange(new[] { 0,1,1,0 });
                    break;
                case 'Q':
                    data.AddRange(new[] { 1,1,0,1 });
                    break;
                case 'R':
                    data.AddRange(new[] { 0,1,0 });
                    break;
                case 'S':
                    data.AddRange(new[] { 0,0,0 });
                    break;
                case 'T':
                    data.AddRange(new[] { 1 });
                    break;
                case 'U':
                    data.AddRange(new[] { 0,0,1 });
                    break;
                case 'V':
                    data.AddRange(new[] { 0,0,0,1 });
                    break;
                case 'W':
                    data.AddRange(new[] { 0,1,1 });
                    break;
                case 'X':
                    data.AddRange(new[] { 1,0,0,1 });
                    break;
                case 'Y':
                    data.AddRange(new[] { 1,0,1,1 });
                    break;
                case 'Z':
                    data.AddRange(new[] { 1,1,0,0 });
                    break;
                case '1':
                    data.AddRange(new[] { 0,1,1,1,1 });
                    break;
                case '2':
                    data.AddRange(new[] { 0,0,1,1,1 });
                    break;
                case '3':
                    data.AddRange(new[] { 0,0,0,1,1 });
                    break;
                case '4':
                    data.AddRange(new[] { 0,0,0,0,1 });
                    break;
                case '5':
                    data.AddRange(new[] { 0,0,0,0,0 });
                    break;
                case '6':
                    data.AddRange(new[] { 1,0,0,0,0 });
                    break;
                case '7':
                    data.AddRange(new[] { 1,1,0,0,0 });
                    break;
                case '8':
                    data.AddRange(new[] { 1,1,1,0,0 });
                    break;
                case '9':
                    data.AddRange(new[] { 1,1,1,1,0 });
                    break;
                case '0':
                    data.AddRange(new[] { 1,1,1,1,1 });
                    break;
                case '-':
                    data.AddRange(new[] { 1,0,0,0,0,1 });
                    break;
            }
        }
        return data.ToArray();
    }

    //private bool _transmitting;
    public IEnumerator PlayWord(string word)
    {
        //_transmitting = true;
        while (MorseTransmitColor == OffColor || MorseTransmitColor == StatusLightState.Random || OffColor == StatusLightState.Random)
        {
            if (MorseTransmitColor == OffColor || MorseTransmitColor == StatusLightState.Random)
                MorseTransmitColor = (StatusLightState) Random.Range(0, 3);
            if (OffColor == MorseTransmitColor || OffColor == StatusLightState.Random)
                OffColor = (StatusLightState)Random.Range(0, 3);
        }
        SetLightColor(OffColor);
        foreach (var d in Morsify(word))
        {
            if (d == -1)
            {
                yield return new WaitForSeconds(0.75f);
                continue;
            }
            SetLightColor(MorseTransmitColor);
            yield return d == 0 ? new WaitForSeconds(0.25f) : new WaitForSeconds(0.75f);
            SetLightColor(OffColor);
            yield return new WaitForSeconds(0.25f);
        }
        //_transmitting = false;
    }
}

public enum StatusLightState
{
    Off,
    Green,
    Red,
    Random
}
