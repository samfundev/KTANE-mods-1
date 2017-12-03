using System.ComponentModel.Design.Serialization;
using UnityEngine;

public class VanillaRuleModiferAPI : MonoBehaviour
{
    public delegate int GetRuleSeedHandler();
    public GetRuleSeedHandler HandleGetRuleSeed;

    public delegate string GetRuleManualHandler();
    public GetRuleManualHandler HandleGetRuleManual;

    public delegate void SetRuleSeedHandler(int seed, bool writeSettings);
    public SetRuleSeedHandler HandleSetRuleSeed;

    public bool WriteSettings { get; private set; }
    public int Seed { get; private set; }

    public int GetRuleSeed()
    {
        return HandleGetRuleSeed?.Invoke() ?? 1;
    }

    public string GetRuleManual()
    {
        return HandleGetRuleManual?.Invoke();
    }

    public void SetRuleSeed(int seed, bool writeSettings = false)
    {
        Seed = seed;
        WriteSettings = writeSettings;
        HandleSetRuleSeed?.Invoke(seed, writeSettings);
    }

    public static readonly string VanillaRuleModifierAPIIdentifier = "VanillaRuleModifierAPI";
}