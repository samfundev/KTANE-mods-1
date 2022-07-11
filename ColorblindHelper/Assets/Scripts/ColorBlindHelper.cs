using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Assets.Scripts;
using Newtonsoft.Json;
using UnityEngine;
using Random = System.Random;

// ReSharper disable once CheckNamespace
public class ColorBlindHelper : MonoBehaviour
{
    public TextMesh ModuleDisableText;
    public KMSelectable ModuleDisableMinusButton;
    public KMSelectable ModuleDisablePlusButton;
    public KMSelectable ModuleDisableButton;

    public TextMesh GlobalEnableText;
    public KMSelectable GlobalEnableButton;

	public KMSelectable ColorblindHoldable;

    public KMAudio Audio;

	public KMColorblindMode ColorblindMode;

	private List<string> _modules;
	private int _moduleIndex;
	private bool _isFocused;

    public static void DebugLog(string message, params object[] args)
    {
        var debugstring = String.Format("[Colorblind Helper] {0}", message);
        Debug.LogFormat(debugstring, args);
    }

	private void FindColorblindModules()
	{
		DebugLog(transform.name);
		var mod = ReflectionHelper.FindType("Mod");
		var modManager = ReflectionHelper.FindType("ModManager");
		if (mod != null && modManager != null)
		{
			var gameObjectsField = mod.GetProperty("ModObjects", BindingFlags.Public | BindingFlags.Instance);
			var instanceField = modManager.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
			var loadModsField = modManager.GetField("loadedMods", BindingFlags.NonPublic | BindingFlags.Instance);
			if (instanceField == null || loadModsField == null || gameObjectsField == null) return;


			var instance = instanceField.GetValue(null);
			if (instance == null) return;
			IDictionary loadedMods = (IDictionary) loadModsField.GetValue(instance);
			DebugLog("Finding KTaNE modules that support Colorblind mode.");
			foreach (var kvp in loadedMods.Values)
			{
				var go = (List<GameObject>) gameObjectsField.GetValue(kvp, null);
				foreach (var g in go.Where(x => x.GetComponentsInChildren<Component>(true).Any(y => y != null && y.GetType().Name.Equals("KMColorblindMode"))))
				{
					if (g.GetComponent<ColorBlindHelper>()) continue;
					if (g.GetComponent<KMBombModule>())
					{
						DebugLog("Bomb Module: {0}", g.GetComponent<KMBombModule>().ModuleType);
						ColorblindMode.IsModuleEnabled(g.GetComponent<KMBombModule>().ModuleType, true);
					}
					else if (g.GetComponent<KMNeedyModule>())
					{
						DebugLog("Needy Module: {0}", g.GetComponent<KMNeedyModule>().ModuleType);
						ColorblindMode.IsModuleEnabled(g.GetComponent<KMNeedyModule>().ModuleType, true);
					}
					else
					{
						DebugLog("Other: {0}", g.name);
						ColorblindMode.IsModuleEnabled(g.name, true);
					}
					
				}
			}

		}
	}

    private void Start()
    {
	    FindColorblindModules();

	    _modules = ColorblindMode.ColorblindEnabledModules;
		_modules.Sort();
        ChangeModuleDisableIndex(0);

		ColorblindHoldable.OnInteract += () => _isFocused = true;
		ColorblindHoldable.OnDefocus += () => _isFocused = false;

        ModuleDisableMinusButton.OnInteract += () => ChangeModuleDisableIndex(-1);
        ModuleDisablePlusButton.OnInteract += () => ChangeModuleDisableIndex(1);
        ModuleDisableButton.OnInteract += ModuleDisableButtonPressed;

        GlobalEnableButton.OnInteract += GlobalColorBlindEnable;

        ModuleDisableMinusButton.OnInteractEnded += () => EndInteract(false);
        ModuleDisablePlusButton.OnInteractEnded += () => EndInteract(false);
        ModuleDisableButton.OnInteractEnded += () => EndInteract(false);

        GlobalEnableButton.OnInteractEnded += () => EndInteract(false);
	    UpdateDisplay();
    }

    private void Update()
    {
		if (_isFocused)
		{
			switch (MatchKey())
			{
				case KeyCode.UpArrow:
					if (_moduleIndex > 10)
						_moduleIndex -= 10;
					else if (_moduleIndex == 0)
						_moduleIndex = _modules.Count - 1;
					else
						_moduleIndex = 0;
					Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
					UpdateDisplay();
					break;
				case KeyCode.DownArrow:
					if ((_modules.Count > 10) && (_moduleIndex < (_modules.Count - 10)))
						_moduleIndex += 10;
					else if (_moduleIndex == (_modules.Count - 1))
						_moduleIndex = 0;
					else
						_moduleIndex = _modules.Count - 1;
					Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
					UpdateDisplay();
					break;
				case KeyCode.LeftArrow:
					ModuleDisableMinusButton.OnInteract();
					break;
				case KeyCode.RightArrow:
					ModuleDisablePlusButton.OnInteract();
					break;
				default:
					if (Input.anyKeyDown)
					{
						int ix = -2;
						foreach (char c in Input.inputString)
						{
							if (_modules[_moduleIndex].ToLowerInvariant()[0] == c)
								_moduleIndex = (_moduleIndex + 1) % _modules.Count;
							if (_modules[ix = _moduleIndex].ToLowerInvariant()[0] != c)
								ix = _modules.FindIndex(s => Regex.IsMatch(s, "^" + c, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase));
							if (ix != -1)
							{
								_moduleIndex = ix;
								UpdateDisplay();
								Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
								Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
								break;
							}
						}
						if (ix == -1)
							Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
					}
					break;
			}
		}
    }

	private KeyCode MatchKey()
    {
		KeyCode code;
        if (Input.GetKeyDown(code = KeyCode.UpArrow))
			return code;
		else if (Input.GetKeyDown(code = KeyCode.LeftArrow))
			return code;
		else if (Input.GetKeyDown(code = KeyCode.RightArrow))
			return code;
		else if (Input.GetKeyDown(code = KeyCode.DownArrow))
			return code;
		return KeyCode.None;
    }

    private bool GlobalColorBlindEnable()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
	    ColorblindMode.ColorblindModeGlobalEnable = !ColorblindMode.ColorblindModeGlobalEnable;
		UpdateDisplay();
        return false;
    }

    private void EndInteract(bool stop=true)
    {
        _twitchPlays = false;
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, transform);
        if(stop)
            StopAllCoroutines();
        UpdateDisplay();
    }
	
	private bool _twitchPlays = false;
	

    private void UpdateDisplay()
    {
	    GlobalEnableText.text = ColorblindMode.ColorblindModeGlobalEnable ? "Enabled" : "Disabled";
	    if (_modules.Count == 0)
	    {
		    ModuleDisableText.text = "No colorblind enabled modules present";
	    }
	    else
	    {
		    bool? enabled = ColorblindMode.IsModuleEnabled(_modules[_moduleIndex]);
		    if (enabled.HasValue)
		    {
			    ModuleDisableText.text = string.Format("{0}: {1}", _modules[_moduleIndex], enabled.Value ? "Enabled" : "Disabled");
			    ModuleDisableText.color = enabled.Value ? Color.green : Color.red;
		    }
		    else
		    {
			    ModuleDisableText.text = string.Format("{0}: Default", _modules[_moduleIndex]);
			    ModuleDisableText.color = Color.white;
		    }
	    }
    }

    private bool ChangeModuleDisableIndex(int diff)
    {
        if (diff != 0)
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

        if (_modules.Count == 0)
            return false;

	    _moduleIndex += _modules.Count + diff;
	    _moduleIndex %= _modules.Count;
	    UpdateDisplay();

        return false;
    }

    private bool ModuleDisableButtonPressed()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (_modules.Count <= 0) return false;

	    var moduleEnabled = ColorblindMode.IsModuleEnabled(_modules[_moduleIndex]);
	    if (!moduleEnabled.HasValue)
		    ColorblindMode.SetModuleEnabled(_modules[_moduleIndex], true);
		else if (moduleEnabled.Value)
		    ColorblindMode.SetModuleEnabled(_modules[_moduleIndex], false);
		else
		    ColorblindMode.SetModuleEnabled(_modules[_moduleIndex], null);

		UpdateDisplay();
        return false;
    }

	private enum ActionAllowed
	{
		Unfinished,
		Allowed,
		Denied
	}

	private IEnumerator AllowPowerUsers(Permissions permission, PowerLevel power, string errorIfNotAllowed)
	{
		ActionAllowed allowed = ActionAllowed.Unfinished;
		yield return null;
		yield return new object[]
		{
			permission.ToString(),
			new Action(() => allowed = ActionAllowed.Allowed),
			new Action(() => allowed = ActionAllowed.Denied)
		};
        yield return null;
	    DebugLog("Permission = {0}, Result = {1}", permission, allowed);

	    if (allowed == ActionAllowed.Allowed) yield break;
	    allowed = ActionAllowed.Unfinished;
	    yield return new object[]
	    {
	        EnumUtils.StringValueOf(power),
	        new Action(() => allowed = ActionAllowed.Allowed),
	        new Action(() => allowed = ActionAllowed.Denied)
	    };
	    yield return null;
	    DebugLog("Power override {0}, Result = {1}", power, allowed);

		if (allowed == ActionAllowed.Allowed) yield break;
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
		yield return "sendtochaterror " + errorIfNotAllowed;
	}

    private enum Permissions
    {
        ColorblindHelperEnabled,
    }

    private static Dictionary<string, bool> _permissions = new Dictionary<string, bool>
    {
        { Permissions.ColorblindHelperEnabled.ToString(), true },
    };

    private IEnumerator AllowColorblindHelper(bool front=true)
    {
        yield return _permissions;
        yield return AllowPowerUsers(Permissions.ColorblindHelperEnabled, PowerLevel.Mod, "Only mods or higher are allowed to use Colorblind Helper");
        yield return front ? "show front" : "show back";
    }

    private string TwitchHelpMessage = "Enable Colorblind mode with !{0} enable. Disable Colorblind mode with !{0} disable. Enable Colorblind mode for a specific module with !{0} BigCircle enable. Disable Colorblind mode for a specific module with !{0} Morse-a-Maze disable. Use the global enable setting for a specific module with !{0} theBulbModule default. Cycle availble modules with !{0} cycle.";
    private bool TwitchShouldCancelCommand;

    private IEnumerator ProcessTwitchCommand(string command)
    {

		TwitchShouldCancelCommand = false;
        command = command.ToLowerInvariant();
        DebugLog("Received command !colorblindhelper {0}", command);
        string[] split = command.Split(' ');
		
        if (command.Equals("enable") || command.Equals("disable"))
        {
            yield return AllowColorblindHelper();
            yield return new KMSelectable[] {GlobalEnableButton};
            yield break;
        }
		else if (command.Equals("cycle"))
        {
	        yield return AllowColorblindHelper();
	        for (int i = 0; i < _modules.Count; i++)
	        {
		        yield return new WaitForSeconds(2.0f);
		        yield return new KMSelectable[] {ModuleDisablePlusButton};
	        }
        }
		else if (split.Length > 1 && new[] {"enabled", "disabled", "default"}.Contains(split.Last()))
        {
	        var state = new List<string> {"enabled", "disabled", "default"};
			var bools = new List<bool?> {true, false, null};
	        yield return null;
	        var module = string.Join(" ", split.Take(split.Length - 1).ToArray());
	        foreach (var moduleID in _modules)
	        {
				yield return new KMSelectable[] { ModuleDisablePlusButton };

		        if (module.ToLowerInvariant().Equals(moduleID.ToLowerInvariant()))
		        {
			        while (ColorblindMode.IsModuleEnabled(moduleID) != bools[state.IndexOf(split.Last())])
				        yield return new KMSelectable[] { ModuleDisableButton };
			        yield break;
		        }
			}
        }
    }
}

public class NullModuleException : Exception
{
    public NullModuleException()
    {
    }

    public NullModuleException(string message) : base(message)
    {
    }

    public NullModuleException(string message, Exception inner) : base(message, inner)
    {
    }
}
