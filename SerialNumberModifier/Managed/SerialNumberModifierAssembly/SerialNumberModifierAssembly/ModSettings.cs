using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace SerialNumberModifierAssembly
{
    public class ModuleSettings
    {
        private static readonly ModuleSettings Instance = new ModuleSettings();

        public static bool IsMatch(ref List<string> list1, List<string> list2)
        {
            if (list1 == null || list1.Count != list2.Count)
            {
                list1 = new List<string>(list2);
                return false;
            }

            for (var i = 0; i < list1.Count; i++)
            {
                if (list1[i].Equals(list2[i])) continue;
                list1 = new List<string>(list2);
                return false;
            }

            return true;
        }

        public bool AreUsageStringsClean()
        {
            var result = true;
            result &= IsMatch(ref HowToUse0, Instance.HowToUse0);
            result &= IsMatch(ref HowToUse1, Instance.HowToUse1);
            result &= IsMatch(ref HowToUse2, Instance.HowToUse2);
            result &= IsMatch(ref HowToUse3, Instance.HowToUse3);
            result &= IsMatch(ref HowToUse4, Instance.HowToUse4);
            result &= HowToReset.Equals(Instance.HowToReset);
            HowToReset = Instance.HowToReset;
            return result;
        }

        public int SettingsVersion;
        public List<string> HowToUse0 = new List<string> {"Don't Touch this value. It is used by the mod internally to determine if there are new settings to be saved."};
        
        public bool ResetToDefault = false;
        public string HowToReset = "Changing this setting to true will reset ALL your setting back to default.";

        //Add your own settings here.  If you wish to have explanations, define them as strings similar to as shown above.
        //Make sure those strings are JSON compliant.
        public bool Enabled = true;

        public List<string> HowToUse1 = new List<string>
        {
            "If disabled, the built in serial number widget will be used instead."
        };

        public string ForbiddenSerialNumberLettersNumbers = "OY";

        public List<string> HowToUse2 = new List<string>
        {
            "Serial number letters/digits specified here will be disallowed to appear",
            "Note, at least one letter and one digit MUST be allowed.",
            "If every letter/digit is marked as forbidden, the widget will silently forbid NONE of them.",
            "The default letters that are forbidden are 'O' and 'Y'"
        };

        public bool ShowSerialNumberBeforeLightsTurnOn = false;

        public List<string> HowToUse3 = new List<string>
        {
            "If Enabled, the serial number text is readable before the lights turn on for the first time."
        };

        public List<string> SerialNumberOverride = new List<string>();
        public List<string> HowToUse4 = new List<string>
        {
            "Fill out the above list like this list is filled out.",
            "If there are ANY entries in this list that are invalid according to the game rules",
            "or if ALL of them get used in the current session, the mod will start using random serial numbers again.",
            "The format is XXDAAD, where X = Any Letter or Digit, D = Any Digit, A = Any Letter",
            "",
            "000AA0","000AA1","000AA2"
        };
    }

    public class ModSettings
    {
        public readonly int ModSettingsVersion = 3;
        public ModuleSettings Settings = new ModuleSettings();

        public string ModuleName { get; }
        //Update this line each time you make changes to the Settings version.

        public ModSettings(KMModSettings settings)
        {
            var fileName = Path.GetFileName(settings.SettingsPath);
            if (fileName != null) ModuleName = fileName.Replace("-settings.txt", "");
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
            var modSettingsDirectory = Path.Combine(Application.persistentDataPath, "Modsettings");
            return directory ? modSettingsDirectory : Path.Combine(modSettingsDirectory, ModuleName + "-settings.txt");
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
                var settings = JsonConvert.SerializeObject(Settings, Formatting.Indented, new StringEnumConverter());
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
            var modSettings = GetModSettingsPath(false);
            try
            {
                if (File.Exists(modSettings))
                {
                    var settings = File.ReadAllText(modSettings);
                    Settings = JsonConvert.DeserializeObject<ModuleSettings>(settings, new StringEnumConverter());

                    if (Settings.SettingsVersion != ModSettingsVersion || !Settings.AreUsageStringsClean())
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
}