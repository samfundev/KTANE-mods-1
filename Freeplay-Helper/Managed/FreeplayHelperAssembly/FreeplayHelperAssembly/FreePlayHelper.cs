using UnityEngine;
using System.Collections;

// ReSharper disable once CheckNamespace
public class FreePlayHelper : MonoBehaviour
{
    private FreeplayCommander _freeplayCommander;
    private KMGameInfo _gameInfo;

    public static void DebugLog(string message, params object[] args)
    {
        var debugstring = $"[Freeplay Helper] {message}";
        Debug.LogFormat(debugstring, args);
    }

    // Use this for initialization
    void Start ()
	{
        DebugLog("Starting service");
	    _gameInfo = GetComponent<KMGameInfo>();
	    _gameInfo.OnStateChange += OnStateChange;
	}
	
	// Update is called once per frame
    private IEnumerator _handler;
	void Update ()
	{
	    if (_freeplayCommander == null) return;
	    if (!_freeplayCommander.UpdateHeldState()) return;
        
	    if (!Input.GetKeyDown(KeyCode.LeftArrow) && !Input.GetKeyDown(KeyCode.RightArrow) && 
	        !Input.GetKeyDown(KeyCode.UpArrow) && !Input.GetKeyDown(KeyCode.DownArrow) && 
	        !Input.GetKeyDown(KeyCode.KeypadEnter) && !Input.GetKeyDown(KeyCode.Return))
	            return;

	    if (_handler != null)
	        StopCoroutine(_handler);

	    _handler = _freeplayCommander.HandleInput();
	    StartCoroutine(_handler);
	}

    void OnStateChange(KMGameInfo.State state)
    {
        DebugLog("Current state = {0}", state.ToString());
        StopAllCoroutines();
        if (state == KMGameInfo.State.Setup)
        {
            StartCoroutine(CheckForFreeplayDevice());
        }
        else
        {
            _freeplayCommander = null;
            _handler = null;
        }
    }

    private IEnumerator CheckForFreeplayDevice()
    {
        yield return null;
        yield return null;
        DebugLog("Attempting to find Freeplay device");
        
        var multipleBombs = MultipleBombs.Refresh();
        while (multipleBombs.MoveNext())
            yield return multipleBombs.Current;
        if (MultipleBombs.Installed())
            DebugLog("Multiple Bombs is also installed");

        var setupRoom = (SetupRoom)SceneManager.Instance.CurrentRoom;
        _freeplayCommander = new FreeplayCommander(setupRoom.FreeplayDevice);
    }
}
