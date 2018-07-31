using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class KMColorblindMode : MonoBehaviour
{
	void Awake()
	{
		_settingsPath = Path.Combine(Path.Combine(Application.persistentDataPath, "Modsettings"), "ColorblindMode.json");
		ReadSettings();
	}
	

	public bool ColorblindModeGlobalEnable
	{
		get
		{
			return _settings.Enabled;
		}
		set
		{
			_settings.Enabled = value;
			WriteSettings();
		}
	}

	public List<string> ColorblindEnabledModules
	{
		get
		{
			return _settings.EnabledModules.Keys.ToList();
		}
	}

	public bool? IsModuleEnabled(string moduleID, bool addIfNotFound=false)
	{
		bool? enabled = null;
		if (moduleID != null && !_settings.EnabledModules.TryGetValue(moduleID, out enabled) && addIfNotFound)
		{
			_settings.EnabledModules[moduleID] = null;
			WriteSettings();
		}
		return enabled;
	}

	public void SetModuleEnabled(string moduleID, bool? enable)
	{
		bool? temp;
		if (moduleID != null && _settings.EnabledModules.TryGetValue(moduleID, out temp))
		{
			_settings.EnabledModules[moduleID] = enable;
			WriteSettings();
		}
	}

	private static string _settingsPath;
	private static ColorblindModeSettings _settings = new ColorblindModeSettings();

	static void WriteSettings()
	{
		try
		{
			File.WriteAllText(_settingsPath, JsonConvert.SerializeObject(_settings, Formatting.Indented));
		}
		catch
		{
			//
		}
	}

	static void ReadSettings()
	{
		try
		{
			if (!File.Exists(_settingsPath)) WriteSettings();
			_settings = JsonConvert.DeserializeObject<ColorblindModeSettings>(File.ReadAllText(_settingsPath));
		}
		catch
		{
			_settings = new ColorblindModeSettings();
		}
	}

	
}

internal class ColorblindModeSettings
{
	public bool Enabled = false;
	public Dictionary<string, bool?> EnabledModules = new Dictionary<string, bool?>();
}