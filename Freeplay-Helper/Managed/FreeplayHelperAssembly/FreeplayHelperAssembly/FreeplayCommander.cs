using System;
using System.Collections;
using System.Reflection;
using DarkTonic.MasterAudio;
using UnityEngine;

public class FreeplayCommander
{
    public static void DebugLog(string message, params object[] args) { FreePlayHelper.DebugLog(message, args); }

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
        FloatingHoldable = FreeplayDevice.GetComponent<FloatingHoldable>();
        _selectableManager = KTInputManager.Instance.SelectableManager;
        

        _originalTimeIncrementHandler = FreeplayDevice.TimeIncrement.OnPush;
        _originalTimeDecrementHandler = FreeplayDevice.TimeDecrement.OnPush;
        _originalModuleIncrementHandler = FreeplayDevice.ModuleCountIncrement.OnPush;
        _originalModuleDecrementHandler = FreeplayDevice.ModuleCountDecrement.OnPush;

        FreeplayDevice.TimeIncrement.OnPush = () => { FreeplayDevice.StartCoroutine(IncrementBombTimer()); };
        FreeplayDevice.TimeDecrement.OnPush = () => { FreeplayDevice.StartCoroutine(DecrementBombTimer()); };
        FreeplayDevice.ModuleCountIncrement.OnPush = () => { FreeplayDevice.StartCoroutine(IncrementModuleCount()); };
        FreeplayDevice.ModuleCountDecrement.OnPush = () => { FreeplayDevice.StartCoroutine(DecrementModuleCount()); };

        if (!MultipleBombs.Installed())
            return;

        _bombsIncrementButton = Selectable.Children[3].GetComponent<KeypadButton>();
        _bombsDecrementButton = Selectable.Children[2].GetComponent<KeypadButton>();

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
        if (!state) _index = freeplaySelection.Timer;
        return state;
    }

    public IEnumerator HandleInput()
    {
        Selectable = FreeplayDevice.GetComponent<Selectable>();

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

        if (!Input.GetKeyDown(KeyCode.LeftArrow) && !Input.GetKeyDown(KeyCode.RightArrow))
            yield break;

        IEnumerator handler = null;
        switch (_index)
        {
            case freeplaySelection.Timer:
                SelectObject(Input.GetKeyDown(KeyCode.LeftArrow) ? FreeplayDevice.TimeDecrement : FreeplayDevice.TimeIncrement);
                break;
            case freeplaySelection.Bombs:
                SelectObject(Input.GetKeyDown(KeyCode.LeftArrow) ? _bombsDecrementButton : _bombsIncrementButton);
                break;
            case freeplaySelection.Modules:
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
            default:
                throw new ArgumentOutOfRangeException();
        }
        if (handler == null)
            yield break;
        while (handler.MoveNext())
        {
            yield return handler.Current;
        }
    }

    public void ToggleIndex()
    {
        var currentSettings = FreeplayDevice.CurrentSettings;
        var currentModuleCount = currentSettings.ModuleCount;
        var currentBombsCount = MultipleBombs.GetBombCount();
        var currentTime = currentSettings.Time;
        var onlyMods = currentSettings.OnlyMods;
        switch (_index)
        {
            case freeplaySelection.Timer:
                try
                {
                    SelectObject(FreeplayDevice.TimeIncrement);
                    if (Mathf.FloorToInt(currentTime) == Mathf.FloorToInt(currentSettings.Time))
                        break;
                    SelectObject(FreeplayDevice.TimeDecrement);
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
                    SelectObject(_bombsIncrementButton);
                    if (currentBombsCount == MultipleBombs.GetBombCount())
                        break;
                    SelectObject(_bombsDecrementButton);

                }
                catch (Exception ex)
                {
                    DebugLog("Failed to Select the Bomb count buttons due to Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                }
                break;
            case freeplaySelection.Modules:
                try
                {
                    SelectObject(FreeplayDevice.ModuleCountIncrement);
                    if (currentModuleCount == currentSettings.ModuleCount)
                        break;
                    SelectObject(FreeplayDevice.ModuleCountDecrement);
                }
                catch (Exception ex)
                {
                    DebugLog("Failed to Select the Module count buttons due to Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                }
                break;
            case freeplaySelection.Needy:
                try
                {
                    SelectObject(FreeplayDevice.NeedyToggle);
                    SelectObject(FreeplayDevice.NeedyToggle);
                }
                catch (Exception ex)
                {
                    DebugLog("Failed to Select the Needy toggle due to Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                }
                break;
            case freeplaySelection.Hardcore:
                try
                {
                    SelectObject(FreeplayDevice.HardcoreToggle);
                    SelectObject(FreeplayDevice.HardcoreToggle);
                }
                catch (Exception ex)
                {
                    DebugLog("Failed to Select the Hardcore toggle due to Exception: {0}, Stack Trace: {1}", ex.Message, ex.StackTrace);
                }
                break;
            case freeplaySelection.ModsOnly:
                try
                {
                    SelectObject(FreeplayDevice.ModsOnly);
                    var onlyModsCurrent = currentSettings.OnlyMods;
                    SelectObject(FreeplayDevice.ModsOnly);
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
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static float startDelay = 0.2f;
    private static float Acceleration = 0.005f;
    private static float minDelay = 0.01f;

    public bool IsHeld(KeypadButton button)
    {
        return (bool)(typeof(KeypadButton).GetField("isBeingPushed", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(button) ?? false);
    }

    public IEnumerator HandleButton(KeypadButton button, KeyCode keyCode, PushEvent handler)
    {
        var delay = startDelay;
        while (Input.GetKey(keyCode) || IsHeld(button))
        {
            MasterAudio.PlaySound3DFollowTransformAndForget("singlebeep", FreeplayDevice.transform, 1f, null, 0f, null);
            handler.Invoke();

            yield return new WaitForSeconds(Mathf.Max(delay, minDelay));
            delay -= Acceleration;
        }
    }

    public IEnumerator IncrementBombTimer() { return HandleButton(FreeplayDevice.TimeIncrement, KeyCode.RightArrow, _originalTimeIncrementHandler); }
    public IEnumerator DecrementBombTimer() { return HandleButton(FreeplayDevice.TimeDecrement, KeyCode.LeftArrow, _originalTimeDecrementHandler); }

    public IEnumerator IncrementModuleCount() { return HandleButton(FreeplayDevice.ModuleCountIncrement, KeyCode.RightArrow, _originalModuleIncrementHandler); }
    public IEnumerator DecrementModuleCount() { return HandleButton(FreeplayDevice.ModuleCountDecrement, KeyCode.LeftArrow, _originalModuleDecrementHandler); }

    public IEnumerator IncrementBombCount() { return HandleButton(_bombsIncrementButton, KeyCode.RightArrow, _originalBombIncrementHandler); }
    public IEnumerator DecrementBombCount() { return HandleButton(_bombsDecrementButton, KeyCode.LeftArrow, _originalBombDecrementHandler); }

    public IEnumerator SetNeedy()
    {
        var on = !Input.GetKeyDown(KeyCode.LeftArrow);
        var currentSettings = FreeplayDevice.CurrentSettings;
        var hasNeedy = currentSettings.HasNeedy;
        if (hasNeedy != on)
        {
            SelectObject(FreeplayDevice.NeedyToggle);

        }
        yield return null;
    }

    public IEnumerator SetHardcore()
    {
        var on = !Input.GetKeyDown(KeyCode.LeftArrow);
        var currentSettings = FreeplayDevice.CurrentSettings;
        var isHardcore = currentSettings.IsHardCore;
        if (isHardcore != on)
        {
            SelectObject(FreeplayDevice.HardcoreToggle);
        }
        yield return null;
    }

    public IEnumerator SetModsOnly()
    {
        var on = !Input.GetKeyDown(KeyCode.LeftArrow);
        var currentSettings = FreeplayDevice.CurrentSettings;
        var onlyMods = currentSettings.OnlyMods;
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

    private void SelectObject(Component component)
    {
        var selectable = component.GetComponent<Selectable>();
        selectable.HandleSelect(true);
        _selectableManager.Select(selectable, true);
        _selectableManager.HandleInteract();
        selectable.OnInteractEnded();
    }

    #endregion

    private readonly PushEvent _originalTimeIncrementHandler;
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
    public readonly FloatingHoldable FloatingHoldable = null;
    private readonly SelectableManager _selectableManager = null;
    #endregion
}

