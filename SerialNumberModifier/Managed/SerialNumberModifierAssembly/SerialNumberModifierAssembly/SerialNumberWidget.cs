using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using static SerialNumberModifierAssembly.CommonReflectedTypeInfo;

// ReSharper disable once CheckNamespace
public class SerialNumberWidget : Widget
{
    public Transform OriginalSerialNumberWidgetCaptured;
    public List<SerialNumberTag> Tags;
    private SerialNumberTag _tag;


    private string _serialNumber = string.Empty;
    private string _tagSerial = string.Empty;

    private string GetAllowedCharacters(string characterset, string exclusions, string defaultset)
    {
        exclusions = exclusions.ToUpperInvariant();
        foreach (var c in exclusions)
            characterset = c == 'O'
                ? characterset.Replace("O", exclusions.Contains("E") ? "" : "E")
                : characterset.Replace(c.ToString(), "");
        return characterset == string.Empty ? defaultset : characterset;
    }

    private void Awake()
    {
        if (Tags == null || Tags.Count == 0)
        {
            DebugLog("Serial number widget was just instantiated without a proper set of tags");
            return;
        }

        //DebugLog("Picking a random tag");
        _tag = Tags[Random.Range(0, Tags.Count)];

        var settings = SerialNumberModifier.Settings.Settings;

        var letters = GetAllowedCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZ", settings.ForbiddenSerialNumberLettersNumbers + _tag.ForcedLetterNumberExclusions, "ABCDEFGHIJKLMNEPQRSTUVWXZ");
        var digits = GetAllowedCharacters("0123456789", settings.ForbiddenSerialNumberLettersNumbers + _tag.ForcedLetterNumberExclusions, "0123456789");

        letters = GetAllowedCharacters(letters, _tag.ForcedLetterNumberExclusions, letters);
        digits = GetAllowedCharacters(digits, _tag.ForcedLetterNumberExclusions, digits);

        var lettersDigits = letters + digits;

        _serialNumber += lettersDigits.Substring(Random.Range(0, lettersDigits.Length), 1);
        _serialNumber += lettersDigits.Substring(Random.Range(0, lettersDigits.Length), 1);
        _serialNumber += digits.Substring(Random.Range(0, digits.Length), 1);
        _serialNumber += letters.Substring(Random.Range(0, letters.Length), 1);
        _serialNumber += letters.Substring(Random.Range(0, letters.Length), 1);
        _serialNumber += digits.Substring(Random.Range(0, digits.Length), 1);

        _activated = settings.ShowSerialNumberBeforeLightsTurnOn;

        _tagSerial = SetTagSerialNumber();

        //if (_tag.name.Equals("Vanilla Serial Number"))
            Debug.LogFormat("[SerialNumber] Randomizing Serial Number: {0}", _serialNumber);
        //else
        //    DebugLog($"Tag: [{_tag.name}] BaseSerial: [{_serialNumber}] TagFormattedSerial: [{_tagSerial}]");

        

        //DebugLog("Hiding all unused tags");
        foreach (var t in Tags)
            t.gameObject.SetActive(false);

        //DebugLog("Showing the selected tag");
        _tag.gameObject.SetActive(true);

        //DebugLog("Setting screw positions and rotations");
        foreach (var screw in _tag.Screws)
        {
            screw.localEulerAngles = new Vector3(0, Random.Range(0f, 360f), 0);
            if (Random.value > 0.25f) continue;
            {
                if (Random.value > 0.5f) screw.gameObject.SetActive(false);
                screw.localPosition = Vector3.Lerp(new Vector3(screw.localPosition.x, screw.localPosition.y, screw.localPosition.z),
                    new Vector3(screw.localPosition.x, _tag.UnscrewOffset, screw.localPosition.z), Random.value);
            }
        }

        WriteTextMesh(string.Empty);
        if(_tag.TextMeshes != null)
            foreach (var t in _tag.TextMeshes)
                t.text = string.Empty;
        
        StartCoroutine(DelayWriteTextMesh(_tagSerial));
    }

    private Bomb GetBomb()
    {
        var t = transform;
        var bomb = t.GetComponentInChildren<Bomb>();
        while (bomb == null && t.parent != null)
        {
            t = t.parent;
            bomb = t.GetComponentInChildren<Bomb>();
        }

        return bomb;
    }

    private bool _activated;
    private IEnumerator UpdateTractorTag()
    {
        while (transform.parent == null)
            yield return null;

        while (!_activated)
            yield return null;

        var bomb = GetBomb();
        if (bomb == null) yield break;

        _tag.TextMeshes[1].text = $"{bomb.BombComponents.Count}M / {bomb.NumStrikesToLose}S";
        var hours = (int) (bomb.TotalTime / 3600);
        var minutes = (int) ((bomb.TotalTime % 3600) / 60);
        var seconds = (int) (bomb.TotalTime % 60);
        _tag.TextMeshes[2].text = $"{hours}:{minutes:00}:{seconds:00}";
    }

    private IEnumerator DelayWriteTextMesh(string text)
    {
        //DebugLog("Setting serial number text if allowed to do so before lights go on");
        while (!_activated)
            yield return null;
        //DebugLog("Now showing the serial number if it is not already shown");
        WriteTextMesh(text);
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
            case LicensePlates licenseplate:
                licenseplate.SerialNumber = text;
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
                if (_tag.TextMeshes == null || _tag.TextMeshes.Length == 0) break;
                _tag.TextMeshes[0].text = text;
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

            case "TractorTag":
                StartCoroutine(UpdateTractorTag());
                return _serialNumber;
        }
    }

    public override void Activate()
    {
        _activated = true;
        base.Activate();
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