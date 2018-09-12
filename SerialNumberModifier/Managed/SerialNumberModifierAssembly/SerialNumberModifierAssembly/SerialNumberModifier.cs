using UnityEngine;
using static SerialNumberModifierAssembly.CommonReflectedTypeInfo;
using Settings = SerialNumberModifierAssembly.ModSettings;


// ReSharper disable once CheckNamespace
public class SerialNumberModifier : MonoBehaviour
{
    private KMGameInfo _gameInfo;
    private Settings _modSettings;

    private bool _started;
    private void Start ()
    {
        _started = true;
	    DebugLog("Service prefab Instantiated.");
        _modSettings = new Settings(GetComponent<KMModSettings>());

	    if (!_modSettings.ReadSettings())
	    {
	        DebugLog("Failed to initialize Mod settings. Aborting load");
	        return;
	    }
        Settings = _modSettings;

		_gameInfo = GetComponent<KMGameInfo>();
        LoadMod();
        
        
	    DebugLog("Service started");
    }

    private bool _enabled;
    // ReSharper disable once UnusedMember.Local
    private void OnDestroy()
    {
        DebugLog("Service prefab destroyed. Shutting down.");
        if (!_enabled) return;

        UnloadMod();
        _started = false;
    }

    private void OnEnable()
    {
        if (!_started || _enabled) return;
        DebugLog("Service prefab Enabled.");
        LoadMod();
    }

    private void OnDisable()
    {
        if (!_enabled) return;
        DebugLog("Service Prefab Disabled.");
        UnloadMod();
    }

    private void LoadMod()
    {
        if (!InitializeSerialNumberWidget(SerialNumberWidget))
            return;

        _gameInfo.OnStateChange += OnStateChange;
        _enabled = true;
        CurrentState = KMGameInfo.State.Setup;
        _prevState = KMGameInfo.State.Setup;
    }

    private void UnloadMod()
    {
        // ReSharper disable once DelegateSubtraction
        _gameInfo.OnStateChange -= OnStateChange;
        _enabled = false;

        StopAllCoroutines();
    }

    public KMGameInfo.State CurrentState = KMGameInfo.State.Unlock;
    private KMGameInfo.State _prevState = KMGameInfo.State.Unlock;
    private Coroutine _addWidget;
    private void OnStateChange(KMGameInfo.State state)
    {
        if (_addWidget != null)
        {
            StopCoroutine(_addWidget);
            _addWidget = null;
        }

        DebugLog("Transitioning from {1} to {0}", state, CurrentState);
        if((_prevState == KMGameInfo.State.Setup || _prevState == KMGameInfo.State.PostGame) && CurrentState == KMGameInfo.State.Transitioning && state == KMGameInfo.State.Transitioning)
        {
            _modSettings.ReadSettings();
            _addWidget = StartCoroutine(AddWidgetToBomb());
        }

        if (CurrentState == KMGameInfo.State.Gameplay && state == KMGameInfo.State.Transitioning)
        {
            RemoveCapturedSerialTag();
        }

        _prevState = CurrentState;
        CurrentState = state;
    }

    public SerialNumberModWidget SerialNumberWidget;
    public static Settings Settings;
}