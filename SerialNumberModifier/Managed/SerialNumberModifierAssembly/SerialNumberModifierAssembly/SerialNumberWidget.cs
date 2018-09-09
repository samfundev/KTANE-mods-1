using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using static SerialNumberModifierAssembly.CommonReflectedTypeInfo;

public class SerialNumberWidget : Widget
{
    public Transform OriginalSerialNumberWidgetCaptured = null;
    public List<SerialNumberTag> Tags;
    private SerialNumberTag _tag;

    private string _serialNumber = string.Empty;

    private void Awake()
    {
        if (Tags == null || Tags.Count == 0)
        {
            DebugLog("Serial number widget was just instantiated without a proper set of tags");
            return;
        }

        var settings = SerialNumberModifier.Settings.Settings;
        var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        foreach (var c in settings.ForbiddenSerialNumberLettersNumbers.ToUpperInvariant())
            letters = c == 'O' 
                ? letters.Replace("O", settings.ForbiddenSerialNumberLettersNumbers.ToUpperInvariant().Contains("E") ? "" : "E") 
                : letters.Replace(c.ToString(), "");
        if (letters == string.Empty)
            letters = "ABCDEFGHIJKLMNEPQRSTUVWXZ";

        var digits = "0123456789";
        foreach (var c in settings.ForbiddenSerialNumberLettersNumbers.ToUpperInvariant())
            digits = digits.Replace(c.ToString(), "");
        if (digits == string.Empty)
            digits = "0123456789";

        var lettersDigits = letters + digits;

        _serialNumber += lettersDigits.Substring(Random.Range(0, lettersDigits.Length), 1);
        _serialNumber += lettersDigits.Substring(Random.Range(0, lettersDigits.Length), 1);
        _serialNumber += digits.Substring(Random.Range(0, digits.Length), 1);
        _serialNumber += letters.Substring(Random.Range(0, letters.Length), 1);
        _serialNumber += letters.Substring(Random.Range(0, letters.Length), 1);
        _serialNumber += digits.Substring(Random.Range(0, digits.Length), 1);
        Debug.LogFormat("[SerialNumber] Randomizing Serial Number: {0}", _serialNumber);

        DebugLog("Picking a random tag");
        _tag = Tags[Random.Range(0, Tags.Count)];

        DebugLog("Hiding all unused tags");
        foreach (var t in Tags)
            t.gameObject.SetActive(false);

        DebugLog("Showing the selected tag");
        _tag.gameObject.SetActive(true);

        DebugLog("Setting screw positions and rotations");
        foreach (var screw in _tag.Screws)
        {
            screw.localEulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
            //screw.localPosition = new Vector3(screw.localPosition.x, Random.Range(0.0135f, 0.0223f), screw.localPosition.z);
        }

        DebugLog("Setting serial number text if allowed to do so before lights go on");
        WriteTextMesh(settings.ShowSerialNumberBeforeLightsTurnOn 
            ? SetTagSerialNumber() 
            : string.Empty);
    }

    private void WriteTextMesh(string text)
    {
        switch (_tag.SerialNumber)
        {
            case TextMesh textmesh:
                textmesh.text = text;
                break;
            case TMPro.TextMeshPro textmeshpro:
                textmeshpro.text = text;
                break;
            case Transform t:
                var tmt = t.GetComponentInChildren<TextMesh>(true);
                if (tmt == null) break;
                tmt.text = text;
                break;
            case GameObject go:
                var tmgo = go.GetComponentInChildren<TextMesh>(true);
                if (tmgo == null) break;
                tmgo.text = text;
                break;
            
            case null:
                break;
        }
    }

    private string SetTagSerialNumber()
    {
        switch (_tag.transform.name)
        {
            default:
                return _serialNumber;

            case "HandWritten":
                // ReSharper disable once StringLiteralTypo
                var letterDigits = new[] {"ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", "abcdefghijklmnopqrstuvwxyz)!@#$%^&*("};
                var newSerial = string.Empty;
                foreach (var c in _serialNumber)
                {
                    if (Random.value < 0.5f)
                        newSerial += c;
                    else
                        newSerial += letterDigits[1][letterDigits[0].IndexOf(c)];
                }
                return newSerial;

            case "Terrorist":

                return $"{_serialNumber[0]} {_serialNumber[1]} {_serialNumber[2]} {_serialNumber[3]} {_serialNumber[4]} {_serialNumber[5]}";
        }
    }

    public void Test()
    {
        Awake();
    }

    public override void Activate()
    {
        DebugLog("Now showing the serial number if it is not already shown");
        if(!SerialNumberModifier.Settings.Settings.ShowSerialNumberBeforeLightsTurnOn)
            WriteTextMesh(SetTagSerialNumber());
    }

    public override string GetQueryResponse(string queryKey, string queryInfo)
    {
        if (queryKey == KMBombInfo.QUERYKEY_GET_SERIAL_NUMBER)
        {
            return JsonConvert.SerializeObject(new Dictionary<string, string>
            {
                {
                    "serial",
                    _serialNumber
                }
            });
        }
        return null;
    }
}