using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class KMColorblindMode : MonoBehaviour
{
	void Awake()
	{
		modConfig = new ModConfig<ColorblindModeSettings>("ColorblindMode");
		_settings = modConfig.Settings;
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
			modConfig.Settings = _settings;
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
			modConfig.Settings = _settings;
		}
		return enabled;
	}

	public void SetModuleEnabled(string moduleID, bool? enable)
	{
		bool? temp;
		if (moduleID != null && _settings.EnabledModules.TryGetValue(moduleID, out temp))
		{
			_settings.EnabledModules[moduleID] = enable;
			modConfig.Settings = _settings;
		}
	}

	private static ModConfig<ColorblindModeSettings> modConfig;
	private static ColorblindModeSettings _settings = new ColorblindModeSettings();
}

internal class ColorblindModeSettings
{
	public bool Enabled = false;
	public Dictionary<string, bool?> EnabledModules = new Dictionary<string, bool?>();
}