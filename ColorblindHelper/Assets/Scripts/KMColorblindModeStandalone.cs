using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class KMColorblindModeStandalone : MonoBehaviour
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
	private static ColorblindModeStandaloneSettings _settings = new ColorblindModeStandaloneSettings();
	private static string SerializeSettings(ColorblindModeStandaloneSettings settings)
	{
		return JsonConvert.SerializeObject(settings, Formatting.Indented);
	}
	static readonly object settingsFileLock = new object();

	static void WriteSettings()
	{
		lock (settingsFileLock)
		{
			File.WriteAllText(_settingsPath, SerializeSettings(_settings));
		}
	}

	static void ReadSettings()
	{
		try
		{
			lock (settingsFileLock)
			{
				if (!File.Exists(_settingsPath))
				{
					File.WriteAllText(_settingsPath, SerializeSettings(Activator.CreateInstance<ColorblindModeStandaloneSettings>()));
				}

				ColorblindModeStandaloneSettings deserialized = JsonConvert.DeserializeObject<ColorblindModeStandaloneSettings>(
					File.ReadAllText(_settingsPath),
					new JsonSerializerSettings { Error = (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) => args.ErrorContext.Handled = true }
				);
				_settings = deserialized != null ? deserialized : Activator.CreateInstance<ColorblindModeStandaloneSettings>();
			}
		}
		catch (Exception e)
		{
			Debug.LogException(e);
			_settings = Activator.CreateInstance<ColorblindModeStandaloneSettings>();
		}
	}

	
}

internal class ColorblindModeStandaloneSettings
{
	public bool Enabled = false;
	public Dictionary<string, bool?> EnabledModules = new Dictionary<string, bool?>();
}