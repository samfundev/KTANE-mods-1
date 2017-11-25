using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;


public class ModuleSettings
{
    public int SettingsVersion;
    public string HowToUse0 = "Don't Touch this value. It is used by the mod internally to determine if there are new settings to be saved.";

    
    public bool ResetToDefault = false;
    public string HowToReset = "Changing this setting to true will reset ALL your setting back to default.";

    //Add your own settings here.  If you wish to have explanations, define them as strings similar to as shown above.
    //Make sure those strings are JSON compliant.
    public int time = 300;
    public int modules = 3;
    public int bombs = 1;
    public int strikes = 3;
    public int widgets = 5;
    public int moduleDisableIndex = 0;
    public bool neediesEnabled = false;
    public PlayMode playmode = PlayMode.AllModules;
    public List<string> disabledModuleIds = new List<string>();
    public bool PacingEvents = true;
    public bool FrontFaceOnly = false;

    public string HowToUse1 = "The settings are saved automatically and persist across multiple sessions. No need to modify them here.";
    public string HowToUse2 = "The only thing you might need to do here is use ResetToDefault, or simply remove the variables outright.";
}

public class ModSettings
{
    public readonly int ModSettingsVersion = 1;
    public ModuleSettings Settings = new ModuleSettings();

    public string ModuleName { get; private set; }
    //Update this line each time you make changes to the Settings version.


    public ModSettings(KMBombModule module)
    {
        ModuleName = module.ModuleType;
    }

    public ModSettings(KMNeedyModule module)
    {
        ModuleName = module.ModuleType;
    }

    public ModSettings(string moduleName)
    {
        ModuleName = moduleName;
    }

    private string GetModSettingsPath(bool directory)
    {
        string ModSettingsDirectory = Path.Combine(Application.persistentDataPath, "Modsettings");
        return directory ? ModSettingsDirectory : Path.Combine(ModSettingsDirectory, ModuleName + "-settings.txt");
    }

    public bool WriteSettings()
    {
        Debug.LogFormat("Writing Settings File: {0}", GetModSettingsPath(false));
        try
        {
            if (!Directory.Exists(GetModSettingsPath(true)))
            {
                Directory.CreateDirectory(GetModSettingsPath(true));
            }

            Settings.SettingsVersion = ModSettingsVersion;
            string settings = JsonConvert.SerializeObject(Settings, Formatting.Indented, new StringEnumConverter());
            File.WriteAllText(GetModSettingsPath(false), settings);
            Debug.LogFormat("New settings = {0}", settings);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogFormat("Failed to Create settings file due to Exception:\n{0}\nStack Trace:\n{1}", ex.Message,
                ex.StackTrace);
            return false;
        }
    }

    public bool ReadSettings()
    {
        string ModSettings = GetModSettingsPath(false);
        try
        {
            if (File.Exists(ModSettings))
            {
                string settings = File.ReadAllText(ModSettings);
                Settings = JsonConvert.DeserializeObject<ModuleSettings>(settings, new StringEnumConverter());

                if (Settings.SettingsVersion != ModSettingsVersion)
                    return WriteSettings();
                if (!Settings.ResetToDefault) return true;
                Settings = new ModuleSettings();
                return WriteSettings();
            }
            Settings = new ModuleSettings();
            return WriteSettings();
        }
        catch (Exception ex)
        {
            Debug.LogFormat(
                "Settings not loaded due to Exception:\n{0}\nStack Trace:\n{1}\nLoading default settings instead.",
                ex.Message, ex.StackTrace);
            Settings = new ModuleSettings();
            return WriteSettings();
        }
    }
}
