using System;
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
    public float AlarmClockBuzzerTime = 60f;

    public string HowToUse1 = "Sets the number of seconds the Alarm clock will buzz for.";

    public string SoundFileDirectory = null;
    public string HowToUse2 = "Set this directory to where you will put all your music tracks used for the Alarm clock.";

    public bool RescanDirectory = true;
    public string HowToUse3 = "When Enabled, the directory where the tracks are contained will be rescanned each bomb.";

    public int ChanceOfNormalBeep = 5;
    public string HowToUse4_1 = "Set this between 0 and 100. This determines how likely you will get the annoying beep instead of music.";
    public string HowToUse4_2 = "Note: If there are no tracks loaded, you will ALWAYS get the beep instead.";


}

public class ModSettings
{
    public readonly int ModSettingsVersion = 4;
    public ModuleSettings Settings = new ModuleSettings();

    public string ModuleName { get; private set; }
    //Update this line each time you make changes to the Settings version.

    public bool InitializeSettings()
    {
        bool RewriteFile = false;

        if (Settings.ResetToDefault)
        {
            DebugLog("Factory Reset requested.");
            Settings = new ModuleSettings();
            RewriteFile = true;
        }

        if (Settings.SettingsVersion != ModSettingsVersion)
        {
            DebugLog("New settings added since previous version.  Previous = {0}, Current = {1}", Settings.SettingsVersion, ModSettingsVersion);
            Settings.SettingsVersion = ModSettingsVersion;
            RewriteFile = true;
        }

        //Set up things that are not allowed to be done in the constructor here, such as Application.persistantDataPath related items.
        //Although the code would run and work correctly if done in the constructor, the Unity editor will complain with an "error".
        if (string.IsNullOrEmpty(Settings.SoundFileDirectory))
        {
            Settings.SoundFileDirectory = Path.Combine(Application.persistentDataPath, "AlarmClockExtender");
            DebugLog("SoundFileDiretory is Null or Empty. Resetting to {0}", Settings.SoundFileDirectory);
            RewriteFile = true;
        }

        //This is also a good place to enforce limits
        if (Settings.ChanceOfNormalBeep < 0)
        {
            DebugLog("ChanceOfNormalBeep < 0%.");
            Settings.ChanceOfNormalBeep = 0;
            RewriteFile = true;
        }
        if (Settings.ChanceOfNormalBeep > 100)
        {
            DebugLog("ChanceOfNormalBeep > 100%.");
            Settings.ChanceOfNormalBeep = 100;
            RewriteFile = true;
        }

        return RewriteFile;
    }

    public void DebugLog(string message, params object[] args)
    {
        var debugstring = string.Format("[{0}] {1}", ModuleName,  message);
        Debug.LogFormat(debugstring, args);
    }


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
        InitializeSettings();
        DebugLog("Writing Settings File: {0}", GetModSettingsPath(false));
        try
        {
            if (!Directory.Exists(GetModSettingsPath(true)))
            {
                Directory.CreateDirectory(GetModSettingsPath(true));
            }

            string settings = JsonConvert.SerializeObject(Settings, Formatting.Indented, new StringEnumConverter());
            File.WriteAllText(GetModSettingsPath(false), settings);
            DebugLog("New settings = {0}", settings);
            return true;
        }
        catch (Exception ex)
        {
            DebugLog("Failed to Create settings file due to Exception:\n{0}\nStack Trace:\n{1}", ex.Message,
                ex.StackTrace);
            return false;
        }
    }

    public bool ReadSettings()
    {
        DebugLog("Attempting to read Settings file");
        string ModSettings = GetModSettingsPath(false);
        try
        {
            if (File.Exists(ModSettings))
            {
                string settings = File.ReadAllText(ModSettings);
                Settings = JsonConvert.DeserializeObject<ModuleSettings>(settings, new StringEnumConverter());
                DebugLog("Settings loaded. Settings = {0}", settings);

                return !InitializeSettings() || WriteSettings();
            }
            Settings = new ModuleSettings();
            return WriteSettings();
        }
        catch (Exception ex)
        {
            DebugLog(
                "Settings not loaded due to Exception:\n{0}\nStack Trace:\n{1}\nLoading default settings instead.",
                ex.Message, ex.StackTrace);
            Settings = new ModuleSettings();
            return WriteSettings();
        }
    }
}
