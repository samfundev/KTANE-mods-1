using System.ComponentModel.Design.Serialization;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class VanillaRuleModiferAPI : MonoBehaviour
{
    public delegate int GetRuleSeedHandler();
    public GetRuleSeedHandler HandleGetRuleSeed;

    public delegate string GetRuleManualHandler();
    public GetRuleManualHandler HandleGetRuleManual;

    public delegate void SetRuleSeedHandler(int seed, bool writeSettings);
    public SetRuleSeedHandler HandleSetRuleSeed;

    public delegate bool IsSeedVanillaHandler();
    public IsSeedVanillaHandler HandleIsSeedVanilla;

    public bool WriteSettings { get; private set; }
    public int Seed { get; private set; }

    private static VanillaRuleModiferAPI _instance;
    public static VanillaRuleModiferAPI Instance
    {
        get
        {
            if (_instance != null) return _instance;

            var gameobject = GameObject.Find(VanillaRuleModifierAPIIdentifier);
            if (gameobject == null)
                return null;
            _instance = gameobject.GetComponent<VanillaRuleModiferAPI>();
            return _instance;
        }

        set
        {
            if (value != null)
                _instance = value;
        }
    }

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

    public bool IsSeedVanilla()
    {
        return HandleIsSeedVanilla?.Invoke() ?? false;
    }

    public static readonly string VanillaRuleModifierAPIIdentifier = "VanillaRuleModifierAPI";

    public VanillaRuleModiferAPI(GetRuleManualHandler handleGetRuleManual)
    {
        HandleGetRuleManual = handleGetRuleManual;
    }
}