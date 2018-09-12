using System;
using System.Collections;
using System.Reflection;
using DarkTonic.MasterAudio;
using UnityEngine;

public class FreeplayCommander
{
    public static void DebugLog(string message, params object[] args)
    {
        var debugstring = $"[Freeplay Helper] {message}";
        Debug.LogFormat(debugstring, args);
    }

    public enum freeplaySelection
    {
        Timer = 0,
        Bombs,
        Modules,
        Needy,
        Hardcore,
        ModsOnly
    };

    #region Constructors
    public FreeplayCommander(FreeplayDevice freeplayDevice)
    {
        FreeplayDevice = freeplayDevice;
        Selectable = FreeplayDevice.GetComponent<Selectable>();
        DebugLog("Freeplay device: Attempting to get the Selectable list.");
        SelectableChildren = Selectable.Children;
        FloatingHoldable = FreeplayDevice.GetComponent<FloatingHoldable>();
        _selectableManager = KTInputManager.Instance.SelectableManager;
        

        _oringinalTimeIncrementHandler = FreeplayDevice.TimeIncrement.OnPush;
        _originalTimeDecrementHandler = FreeplayDevice.TimeDecrement.OnPush;
        _originalModuleIncrementHandler = FreeplayDevice.ModuleCountIncrement.OnPush;
        _originalModuleDecrementHandler = FreeplayDevice.ModuleCountDecrement.OnPush;

        FreeplayDevice.TimeIncrement.OnPush = () => { FreeplayDevice.StartCoroutine(IncrementBombTimer()); };
        FreeplayDevice.TimeDecrement.OnPush = () => { FreeplayDevice.StartCoroutine(DecrementBombTimer()); };
        FreeplayDevice.ModuleCountIncrement.OnPush = () => { FreeplayDevice.StartCoroutine(IncrementModuleCount()); };
        FreeplayDevice.ModuleCountDecrement.OnPush = () => { FreeplayDevice.StartCoroutine(DecrementModuleCount()); };

        if (!MultipleBombs.Installed())
            return;

        _bombsIncrementButton = SelectableChildren[3].GetComponent<KeypadButton>();
        _bombsDecrementButton = SelectableChildren[2].GetComponent<KeypadButton>();

        _originalBombIncrementHandler = _bombsIncrementButton.OnPush;
        _originalBombDecrementHandler = _bombsDecrementButton.OnPush;

        _bombsIncrementButton.OnPush = () => { FreeplayDevice.StartCoroutine(IncrementBombCount()); };
        _bombsDecrementButton.OnPush = () => { FreeplayDevice.StartCoroutine(DecrementBombCount()); };
    }
    #endregion


    #region Helper Methods

    private freeplaySelection _index = freeplaySelection.Timer;

    public bool UpdateHeldState()
    {
        var state = FloatingHoldable.HoldState == FloatingHoldable.HoldStateEnum.Held;
        if (!state) _index = 0;
        return state;
    }

    public IEnumerator HandleInput()
    {
        Selectable = FreeplayDevice.GetComponent<Selectable>();
        SelectableChildren = Selectable.Children;

        if (!Input.GetKeyDown(KeyCode.LeftArrow) && !Input.GetKeyDown(KeyCode.RightArrow) &&
            !Input.GetKeyDown(KeyCode.UpArrow) && !Input.GetKeyDown(KeyCode.DownArrow) &&
            !Input.GetKeyDown(KeyCode.Return) && !Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            yield break;
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            StartBomb();
            yield break;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _index--;
            if (_index == freeplaySelection.Bombs && !MultipleBombs.Installed())
                _index = freeplaySelection.Timer;
            if (_index < freeplaySelection.Timer)
                _index = freeplaySelection.ModsOnly;
            ToggleIndex();
            yield break;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _index++;
            if (_index == freeplaySelection.Bombs && !MultipleBombs.Installed())
                _index = freeplaySelection.Modules;
            if (_index > freeplaySelection.ModsOnly)
                _index = freeplaySelection.Timer;
            ToggleIndex();
            yield break;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            IEnumerator handler = null;
            switch (_index)
            {
                case freeplaySelection.Timer:
                    //handler = Input.GetKeyDown(KeyCode.LeftArrow) ? DecrementBombTimer() : IncrementBombTimer();
                    SelectObject(Input.GetKeyDown(KeyCode.LeftArrow) ? FreeplayDevice.TimeDecrement : FreeplayDevice.TimeIncrement);
                    break;
                case freeplaySelection.Bombs:
                    //handler = Input.GetKeyDown(KeyCode.LeftArrow) ? DecrementBombCount() : IncrementBombCount();
                    SelectObject(Input.GetKeyDown(KeyCode.LeftArrow) ? _bombsDecrementButton : _bombsIncrementButton);
                    break;
                case freeplaySelection.Modules:
                    //handler = Input.GetKeyDown(KeyCode.LeftArrow) ? DecrementModuleCount() : IncrementModuleCount();
                    SelectObject(Input.GetKeyDown(KeyCode.LeftArrow) ? FreeplayDevice.ModuleCountDecrement : FreeplayDevice.ModuleCountIncrement);
                    break; 
                case freeplaySelection.Needy:
                    handler = SetNeedy();
                    break;
                case freeplaySelection.Hardcore:
                    handler = SetHardcore();
                    break;
                case freeplaySelection.ModsOnly:
                    handler = SetModsOnly();
                    break;
            }
            if (handler == null)
                yield break;
            while (handler.MoveNext())
            {
                yield return handler.Current;
            }
        }
    }

    public void ToggleIndex()
    {
        var currentSettings = FreeplayDevice.CurrentSettings;
        int currentModuleCount = currentSettings.ModuleCount;
        int currentBombsCount = MultipleBombs.GetBombCount();
        float currentTime = currentSettings.Time;
        bool onlyMods = currentSettings.OnlyMods;
        switch (_index)
        {
            case freeplaySelection.Timer:
                try
                {
                    MonoBehaviour timerUp = FreeplayDevice.TimeIncrement;
                    MonoBehaviour timerDown = FreeplayDevice.TimeDecrement;
                    SelectObject( timerUp.GetComponent<Selectable>());
                    if (Mathf.FloorToInt(currentTime) == Mathf.FloorToInt(currentSettings.Time))
                        break;
                    SelectObject( timerDown.GetComponent<Selectable>());
                }
                catch (Exception ex)
                {
                    DebugLog("Failed to Select the Timer buttons due to Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                }
                break;
            case freeplaySelection.Bombs:
                try
                {
                    if (!MultipleBombs.Installed())
                        break;
                    var bombsUp = SelectableChildren[3];
                    var bombsDown = SelectableChildren[2];
                    SelectObject(bombsUp);
                    if (currentBombsCount == MultipleBombs.GetBombCount())
                        break;
                    SelectObject(bombsDown);

                }
                catch (Exception ex)
                {
                    DebugLog("Failed to Select the Bomb count buttons due to Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                }
                break;
            case freeplaySelection.Modules:
                try
                {
                    MonoBehaviour moduleUp = FreeplayDevice.ModuleCountIncrement;
                    MonoBehaviour moduleDown = FreeplayDevice.ModuleCountDecrement;
                    SelectObject( moduleUp.GetComponent<Selectable>());
                    if (currentModuleCount == currentSettings.ModuleCount)
                        break;
                    SelectObject( moduleDown.GetComponent<Selectable>());
                }
                catch (Exception ex)
                {
                    DebugLog("Failed to Select the Module count buttons due to Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                }
                break;
            case freeplaySelection.Needy:
                try
                {
                    MonoBehaviour needyToggle = FreeplayDevice.NeedyToggle;
                    SelectObject( needyToggle.GetComponent<Selectable>());
                    SelectObject( needyToggle.GetComponent<Selectable>());
                }
                catch (Exception ex)
                {
                    DebugLog("Failed to Select the Needy toggle due to Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                }
                break;
            case freeplaySelection.Hardcore:
                try
                {
                    MonoBehaviour hardcoreToggle = FreeplayDevice.HardcoreToggle;
                    SelectObject( hardcoreToggle.GetComponent<Selectable>());
                    SelectObject( hardcoreToggle.GetComponent<Selectable>());
                }
                catch (Exception ex)
                {
                    DebugLog("Failed to Select the Hardcore toggle due to Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                }
                break;
            case freeplaySelection.ModsOnly:
                try
                {
                    MonoBehaviour modsToggle = FreeplayDevice.ModsOnly;
                    SelectObject( modsToggle.GetComponent<Selectable>());
                    bool onlyModsCurrent = currentSettings.OnlyMods;
                    SelectObject( modsToggle.GetComponent<Selectable>());
                    if (onlyMods == onlyModsCurrent)
                    {
                        if (Input.GetKey(KeyCode.DownArrow))
                        {
                            _index = freeplaySelection.Timer;
                            goto case freeplaySelection.Timer;
                        }
                        else
                        {
                            _index = freeplaySelection.Hardcore;
                            goto case freeplaySelection.Hardcore;
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebugLog("Failed to Select the Mods only toggle due to Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                }
                break;
        }
    }

    private static float startDelay = 0.2f;
    private static float Acceleration = 0.005f;
    private static float minDelay = 0.01f;

    public bool IsHeld(KeypadButton button)
    {
        return (bool)(typeof(KeypadButton).GetField("isBeingPushed", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(button) ?? false);
    }

    public IEnumerator IncrementBombTimer()
    {
        var button = FreeplayDevice.TimeIncrement;
        float delay = startDelay;
        while (Input.GetKey(KeyCode.RightArrow) || IsHeld(button))
        {
                MasterAudio.PlaySound3DFollowTransformAndForget("singlebeep", FreeplayDevice.transform, 1f, null, 0f, null);
                _oringinalTimeIncrementHandler.Invoke();

            yield return new WaitForSeconds(Mathf.Max(delay, minDelay));
            delay -= Acceleration;
        }
    }



    public IEnumerator DecrementBombTimer()
    {
        var button = FreeplayDevice.TimeDecrement;
        float delay = startDelay;
        while (Input.GetKey(KeyCode.LeftArrow) || IsHeld(button))
        {
                MasterAudio.PlaySound3DFollowTransformAndForget("singlebeep", FreeplayDevice.transform, 1f, null, 0f, null);
                _originalTimeDecrementHandler.Invoke();
            yield return new WaitForSeconds(Mathf.Max(delay, minDelay));
            delay -= Acceleration;
        }
    }

    public IEnumerator IncrementModuleCount()
    {
        var button = FreeplayDevice.ModuleCountIncrement;
        float delay = startDelay;
        while (Input.GetKey(KeyCode.RightArrow) || IsHeld(button))
        {
                MasterAudio.PlaySound3DFollowTransformAndForget("singlebeep", FreeplayDevice.transform, 1f, null, 0f, null);
                _originalModuleIncrementHandler.Invoke();
            yield return new WaitForSeconds(Mathf.Max(delay, minDelay));
            delay -= Acceleration;
        }
    }

    public IEnumerator DecrementModuleCount()
    {
        var button = FreeplayDevice.ModuleCountDecrement;
        float delay = startDelay;
        while (Input.GetKey(KeyCode.LeftArrow) || IsHeld(button))
        {
                MasterAudio.PlaySound3DFollowTransformAndForget("singlebeep", FreeplayDevice.transform, 1f, null, 0f, null);
                _originalModuleDecrementHandler.Invoke();
            yield return new WaitForSeconds(Mathf.Max(delay, minDelay));
            delay -= Acceleration;
        }
    }

    public IEnumerator IncrementBombCount()
    {
        var button = _bombsIncrementButton;

        float delay = startDelay;
        while (Input.GetKey(KeyCode.RightArrow) || IsHeld(button))
        {
                MasterAudio.PlaySound3DFollowTransformAndForget("singlebeep", FreeplayDevice.transform, 1f, null, 0f, null);
                _originalBombIncrementHandler.Invoke();
            yield return new WaitForSeconds(Mathf.Max(delay, minDelay));
            delay -= Acceleration;
        }
    }

    public IEnumerator DecrementBombCount()
    {
        var button = _bombsDecrementButton;

        float delay = startDelay;
        while (Input.GetKey(KeyCode.LeftArrow) || IsHeld(button))
        {
                MasterAudio.PlaySound3DFollowTransformAndForget("singlebeep", FreeplayDevice.transform, 1f, null, 0f, null);
                _originalBombDecrementHandler.Invoke();
            yield return new WaitForSeconds(Mathf.Max(delay, minDelay));
            delay -= Acceleration;
        }
    }

    public IEnumerator SetNeedy()
    {
        bool on = !Input.GetKeyDown(KeyCode.LeftArrow);
        var currentSettings = FreeplayDevice.CurrentSettings;
        bool hasNeedy = currentSettings.HasNeedy;
        if (hasNeedy != on)
        {
            SelectObject(FreeplayDevice.NeedyToggle);

        }
        yield return null;
    }

    public IEnumerator SetHardcore()
    {
        bool on = !Input.GetKeyDown(KeyCode.LeftArrow);
        var currentSettings = FreeplayDevice.CurrentSettings;
        bool isHardcore = currentSettings.IsHardCore;
        if (isHardcore != on)
        {
            SelectObject(FreeplayDevice.HardcoreToggle);
        }
        yield return null;
    }

    public IEnumerator SetModsOnly()
    {
        bool on = !Input.GetKeyDown(KeyCode.LeftArrow);
        var currentSettings = FreeplayDevice.CurrentSettings;
        bool onlyMods = currentSettings.OnlyMods;
        if (onlyMods != on)
        {
            SelectObject(FreeplayDevice.ModsOnly);
        }
        yield return null;
    }

    public void StartBomb()
    {
        SelectObject(FreeplayDevice.StartButton);
    }

    public IEnumerator HoldFreeplayDevice()
    {
        var holdState = FloatingHoldable.HoldState;

        if (holdState != FloatingHoldable.HoldStateEnum.Held)
        {
            SelectObject(Selectable);

            float holdTime = FloatingHoldable.PickupTime;
            IEnumerator forceRotationCoroutine = ForceHeldRotation(holdTime);
            while (forceRotationCoroutine.MoveNext())
            {
                yield return forceRotationCoroutine.Current;
            }
        }
    }

    public void LetGoFreeplayDevice()
    {
        var holdState = FloatingHoldable.HoldState;
        if (holdState == FloatingHoldable.HoldStateEnum.Held)
        {
            DeselectObject(Selectable);
        }
    }

    private void SelectObject(MonoBehaviour selectable)
    {
        SelectObject(selectable.GetComponent<Selectable>());
    }

    private void SelectObject(Selectable selectable)
    {
        selectable.HandleSelect(true);
        _selectableManager.Select(selectable, true);
        _selectableManager.HandleInteract();
        selectable.OnInteractEnded();
    }

    private void DeselectObject(Selectable selectable)
    {
        _selectableManager.HandleCancel();
    }

    private IEnumerator ForceHeldRotation(float duration)
    {
        Transform baseTransform = _selectableManager.GetBaseHeldObjectTransform();

        float initialTime = Time.time;
        while (Time.time - initialTime < duration)
        {
            Quaternion currentRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

            _selectableManager.SetZSpin(0.0f);
            _selectableManager.SetControlsRotation(baseTransform.rotation * currentRotation);
            _selectableManager.HandleFaceSelection();
            yield return null;
        }

        _selectableManager.SetZSpin(0.0f);
        _selectableManager.SetControlsRotation(baseTransform.rotation * Quaternion.Euler(0.0f, 0.0f, 0.0f));
        _selectableManager.HandleFaceSelection();
    }

    #endregion

    private readonly PushEvent _oringinalTimeIncrementHandler;
    private readonly PushEvent _originalTimeDecrementHandler;
    private readonly PushEvent _originalModuleIncrementHandler;
    private readonly PushEvent _originalModuleDecrementHandler;

    private readonly KeypadButton _bombsIncrementButton;
    private readonly KeypadButton _bombsDecrementButton;
    private readonly PushEvent _originalBombIncrementHandler;
    private readonly PushEvent _originalBombDecrementHandler;

    #region Readonly Fields
    public readonly FreeplayDevice FreeplayDevice = null;
    public Selectable Selectable = null;
    public Selectable[] SelectableChildren = null;
    public readonly FloatingHoldable FloatingHoldable = null;
    private readonly SelectableManager _selectableManager = null;
    #endregion
}

