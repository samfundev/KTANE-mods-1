using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Assets.Scripts;
using Assets.Scripts.RuleGenerator;
using Random = UnityEngine.Random;

// ReSharper disable once CheckNamespace
public class MorseAMaze : MonoBehaviour
{
    public FakeStatusLight FakeStatusLight;

    public Transform StatusLight;
    public Transform StatusLightCorner;
    public Transform[] Locations;

    public Transform HorizontalWalls;
    public Transform VerticalWalls;

    public KMSelectable Left;
    public KMSelectable Right;
    public KMSelectable Up;
    public KMSelectable Down;

    public KMBombModule BombModule;
    public KMBombInfo BombInfo;
    public KMAudio Audio;

    public AudioClip GlassBreak;

    public KMModSettings ModSettings;

    private Transform _currentLocation;
    private Transform _destination;

    private CoroutineQueue _movements;
    private bool _solved;
    private bool _strikePending;

    private string _souvenirQuestionStartingLocation;
    private string _souvenirQuestionEndingLocation;
    private string _souvenirQuestionWordPlaying;

    private ModSettings _modSettings;

    private static readonly MorseAMazeRuleGenerator MazeRuleSet = new MorseAMazeRuleGenerator();

    private enum EdgeworkRules
    {
        None,
        SolveCount,
        Batteries,
        BatteryHolders,
        UniquePorts,
        TotalPorts,
        LitIndicators,
        UnlitIndicators,
        TotalIndicators,
        TwoFactor,
        PortPlates,
        SerialLastDigit,
        SerialSum,
        Strikes,
        FirstSerialDigit,
        StartingTime,
        DayOfWeek,
        EmptyPortPlates,
        SerialNumberLetter
    }

    private int _rule;
    private readonly string[] _morseCodeWords =
    {
        //No Edgework
        "kaboom","unicorn","quebec","bashly","slick","vector","flick","timwi","strobe",
        "bombs","bravo","laundry","brick","kitty","halls","steak","break","beats",

        //Edgework required
        "leaks","sting","hello",
        "victor","alien3","bistro",
        "tango","timer","shell",
        "boxes","trick","penguin",
        "strike","elias","ktane",
        "manual","zulu","november"
    };

    private readonly EdgeworkRules[] _edgeworkRules =
    {
        EdgeworkRules.None, EdgeworkRules.None, EdgeworkRules.None, EdgeworkRules.None, EdgeworkRules.None, EdgeworkRules.None, EdgeworkRules.None, EdgeworkRules.None, EdgeworkRules.None,
        EdgeworkRules.None, EdgeworkRules.None, EdgeworkRules.None, EdgeworkRules.None, EdgeworkRules.None, EdgeworkRules.None, EdgeworkRules.None, EdgeworkRules.None, EdgeworkRules.None,
        EdgeworkRules.SolveCount, EdgeworkRules.Batteries, EdgeworkRules.BatteryHolders,
        EdgeworkRules.UniquePorts, EdgeworkRules.TotalPorts, EdgeworkRules.LitIndicators,
        EdgeworkRules.UnlitIndicators, EdgeworkRules.TotalIndicators, EdgeworkRules.TwoFactor,
        EdgeworkRules.PortPlates, EdgeworkRules.SerialLastDigit, EdgeworkRules.SerialSum,
        EdgeworkRules.Strikes, EdgeworkRules.FirstSerialDigit, EdgeworkRules.StartingTime,
        EdgeworkRules.DayOfWeek, EdgeworkRules.EmptyPortPlates, EdgeworkRules.SerialNumberLetter
    };


    private int GetRuleSeed()
    {
        GameObject vanillaRuleModifierAPIGameObject = GameObject.Find("VanillaRuleModifierProperties");
        if (vanillaRuleModifierAPIGameObject == null) //If the Vanilla Rule Modifer is not installed, return.
            return 1;
        IDictionary<string, object> vanillaRuleModifierAPI = vanillaRuleModifierAPIGameObject.GetComponent<IDictionary<string, object>>();
        object seed;
        if (vanillaRuleModifierAPI.TryGetValue("RuleSeed", out seed))
            return (int)seed;
        return 1;
    }

    private string GetRuleManualPath()
    {
        GameObject vanillaRuleModifierAPIGameObject = GameObject.Find("VanillaRuleModifierProperties");
        if (vanillaRuleModifierAPIGameObject == null) //If the Vanilla Rule Modifer is not installed, return.
            return null;
        IDictionary<string, object> vanillaRuleModifierAPI = vanillaRuleModifierAPIGameObject.GetComponent<IDictionary<string, object>>();
        object manual;
        if (vanillaRuleModifierAPI.TryGetValue("GetRuleManual", out manual))
            return (string)manual;
        return null;
    }

    private static int _currentSeed;

    // Use this for initialization
    // ReSharper disable once UnusedMember.Local
    private void Start()
    {
        StartCoroutine(TwitchPlays.Refresh());

        _modSettings = new ModSettings(BombModule);
        _modSettings.ReadSettings();
        _movements = gameObject.AddComponent<CoroutineQueue>();
        BombModule.GenerateLogFriendlyName();

        if (!MazeRuleSet.Initialized || _currentSeed != GetRuleSeed())
        {
            _currentSeed = GetRuleSeed();
            
            try
            {
                MazeRuleSet.InitializeRNG(_currentSeed);
                MazeRuleSet.CreateRules();
                if (_currentSeed != 1)
                {
                    try
                    {
                        var manualPath = GetRuleManualPath();
                        if (Application.isEditor)
                            manualPath = "";
                        if (manualPath != null)
                        {
                            if (!Application.isEditor)
                                if (!Directory.Exists(manualPath))
                                    Directory.CreateDirectory(manualPath);
                            string htmlFileName;
                            var htmlData = MazeRuleSet.GetHTMLManual(out htmlFileName);
                            var htmlPath = Path.Combine(manualPath, htmlFileName);
                            File.WriteAllText(htmlPath, htmlData);
                            for (var i = 0; i < MorseAMazeManual.TextAssetPaths.Length; i++)
                            {
                                var imagePath = Path.Combine(manualPath, MorseAMazeManual.ImagePaths[i]);
                                if (!Directory.Exists(imagePath))
                                    Directory.CreateDirectory(imagePath);
                                var imageFile = Path.Combine(manualPath, MorseAMazeManual.TextAssetPaths[i]);
                                File.WriteAllText(imageFile, MorseAMazeManual.TextAssets[i]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        BombModule.LogFormat("Failed to Generate manual for Seed {0} due to Exception: {1}, Stack Trace: {2}", _currentSeed, ex.Message, ex.StackTrace);
                    }
                }
            }
            catch
            {
                if (_currentSeed == 1)
                    StartCoroutine(InstantlySolveModule("Could not Generate ruleset"));
                else
                {
                    try
                    {
                        MazeRuleSet.InitializeRNG(1);
                        MazeRuleSet.CreateRules();
                    }
                    catch
                    {
                        StartCoroutine(InstantlySolveModule("Could not Generate ruleset"));
                    }
                }
            }
        }
        
	    Locations.Shuffle();
        SetMaze(0); //Hide the walls now.

        BombModule.OnActivate += Activate;
	    FakeStatusLight = Instantiate(FakeStatusLight);

        if (BombModule != null)
            FakeStatusLight.Module = BombModule;

        FakeStatusLight.PassColor = _modSettings.Settings.SolvedState;
        FakeStatusLight.FailColor = _modSettings.Settings.StrikeState;
        FakeStatusLight.OffColor = _modSettings.Settings.OffState;
        FakeStatusLight.MorseTransmitColor = _modSettings.Settings.MorseXmitState;


        FakeStatusLight.GetStatusLights(StatusLight);
        FakeStatusLight.SetInActive();

        _currentLocation = Locations[0];
        _destination = Locations[1];
        StartCoroutine(MoveStatusLightToStart());
    }

    private Vector3 _originalStatusLightLocation;
    private float _statusLightMoveFrames;
    private const float MoveTime = 0.75f;
    private IEnumerator MoveStatusLightToStart()
    {
        while (!FakeStatusLight.IsFakeStatusLightReady)
        {
            yield return null;
            if (!FakeStatusLight.HasFakeStatusLightFailed) continue;
            StartCoroutine(InstantlySolveModule("Status light not able to be manipulated."));
            yield break;
        }
        _originalStatusLightLocation = StatusLight.localPosition;

        var targetMove = _originalStatusLightLocation.y * -1;
        while (StatusLight.localPosition.y > targetMove)
        {
            _statusLightMoveFrames = MoveTime / Time.deltaTime;
            var move = _originalStatusLightLocation.y / _statusLightMoveFrames;
            StatusLight.localPosition = new Vector3(StatusLight.localPosition.x, StatusLight.localPosition.y - move,
                StatusLight.localPosition.z);
            yield return null;
        }

        StatusLight.localPosition = new Vector3(_currentLocation.parent.localPosition.x, targetMove,
            _currentLocation.localPosition.z);
        yield return null;
        targetMove *= -1;

        while (StatusLight.localPosition.y < targetMove)
        {
            _statusLightMoveFrames = MoveTime / Time.deltaTime;
            var move = _originalStatusLightLocation.y / _statusLightMoveFrames;
            StatusLight.localPosition = new Vector3(StatusLight.localPosition.x, StatusLight.localPosition.y + move,
                StatusLight.localPosition.z);
            yield return null;
        }

        StatusLight.localPosition = new Vector3(_currentLocation.parent.localPosition.x, targetMove,
            _currentLocation.localPosition.z);
        yield return null;
    }

    private IEnumerator MoveStatusLightToCorner()
    {
        var targetMove = _originalStatusLightLocation.y * -1;
        while (StatusLight.localPosition.y > targetMove)
        {
            _statusLightMoveFrames = MoveTime / Time.deltaTime;
            var move = _originalStatusLightLocation.y / _statusLightMoveFrames;
            StatusLight.localPosition = new Vector3(StatusLight.localPosition.x, StatusLight.localPosition.y - move,
                StatusLight.localPosition.z);
            yield return null;
        }

        StatusLight.localPosition = new Vector3(StatusLightCorner.localPosition.x, targetMove,
            StatusLightCorner.localPosition.z);
        yield return null;
        targetMove *= -1;

        while (StatusLight.localPosition.y < targetMove)
        {
            _statusLightMoveFrames = MoveTime / Time.deltaTime;
            var move = _originalStatusLightLocation.y / _statusLightMoveFrames;
            StatusLight.localPosition = new Vector3(StatusLight.localPosition.x, StatusLight.localPosition.y + move,
                StatusLight.localPosition.z);
            yield return null;
        }

        StatusLight.localPosition = new Vector3(StatusLightCorner.localPosition.x, targetMove,
            StatusLightCorner.localPosition.z);
        yield return null;

        HandleModulePass();
    }

    private bool _alreadyHandledPass;
    private void HandleModulePass()
    {
        if (_alreadyHandledPass) return;
        _alreadyHandledPass = true;
        switch (FakeStatusLight.SetLightColor(FakeStatusLight.PassColor))
        {
            case StatusLightState.Red:
                BombModule.LogFormat("Setting the status light to Red. Did you want that with a side of strikes? Keepo");
                FakeStatusLight.HandlePass(StatusLightState.Red);
                break;
            case StatusLightState.Green:
                BombModule.LogFormat("Setting the status light to its normal Green color for solved.");
                FakeStatusLight.HandlePass(StatusLightState.Green);
                break;
            case StatusLightState.Off:
            default:
                BombModule.LogFormat("Turning off the Status light. Kappa");
                FakeStatusLight.HandlePass();
                break;
        }
        BombModule.Log("Module Solved");
    }

    private void SetWall(int x, int y, bool right, bool active)
    {
        var letter = "ABCDEF".Substring(x, 1);
        var letters = letter + "BCDEFG".Substring(x, 1);
        var number = "123456".Substring(y, 1);
        var numbers = number + "234567".Substring(y, 1);

        if (right)
        {
            var wall = VerticalWalls.Find(number).Find(letters);
            if (wall == null) return;
            wall.gameObject.SetActive(active);
            StartCoroutine(HideWall(wall.gameObject.GetComponent<MeshRenderer>()));
        }
        else
        {
            var wall = HorizontalWalls.Find(letter).Find(numbers);
            if (wall == null) return;
            wall.gameObject.SetActive(active);
            StartCoroutine(HideWall(wall.gameObject.GetComponent<MeshRenderer>()));
        }
    }

    private void SetMaze(int maze)
    {
        maze %= 18;
        for (var x = 0; x < 6; x++)
        {
            for (var y = 0; y < 6; y++)
            {
                /*SetWall(x, y, false, !_mazes[maze, y, x].Contains("d"));
                SetWall(x, y, true, !_mazes[maze, y, x].Contains("r"));*/
                SetWall(x, y, false, MazeRuleSet.Mazes[maze].GetCell(x, y).WallDown);
                SetWall(x, y, true, MazeRuleSet.Mazes[maze].GetCell(x, y).WallRight);
            }
        }
    }


    private Transform CheckHorizontalWall(string x, string y)
    {
        var hwall = HorizontalWalls.Find(x);
        var wall = hwall.Find(y);
        
        return wall.gameObject.activeSelf ? wall : null;
    }

    private Transform CheckVerticalWalls(string x, string y)
    {
        var hwall = VerticalWalls.Find(x);
        var wall = hwall.Find(y);

        return wall.gameObject.activeSelf ? wall : null;
    }

    private static string GetCoordinates(Transform location)
    {
        return location.parent.name + location.name;
    }

    private string GetDirection(Transform from, Transform to)
    {
        var fromCoordinates = GetCoordinates(from);
        var toCoordinates = GetCoordinates(to);
        var fromX = "ABCDEF".IndexOf(fromCoordinates.Substring(0, 1), StringComparison.Ordinal);
        var toX = "ABCDEF".IndexOf(toCoordinates.Substring(0, 1), StringComparison.Ordinal);
        var fromY = int.Parse(fromCoordinates.Substring(1));
        var toY = int.Parse(toCoordinates.Substring(1));

        if (fromX == toX && fromY == toY) return "";

        if (fromX == toX)
            return (fromY < toY) ? "Down" : "Up";

        return (fromX > toX) ? "Left" : "Right";
    }

    private IEnumerator GiveStrike(Transform wall, Transform from, Transform to)
    {
        BombModule.LogFormat("Tried to move from {0} to {1} - {2}, but there was a wall in the way. Strike", GetCoordinates(from),
            GetCoordinates(to),GetDirection(from,to));
        

        var hitWall = 8;
        var moveFrom = new Vector3(from.parent.transform.localPosition.x, StatusLight.localPosition.y, from.localPosition.z);
        var moveto = new Vector3(to.parent.transform.localPosition.x, StatusLight.localPosition.y, to.localPosition.z);
        var movement = (moveto - moveFrom) / 4 / hitWall;
        for (var i = 0; i < hitWall; i++)
        {
            StatusLight.localPosition += movement;
            yield return null;
        }
        StartCoroutine(ShowWall(wall.gameObject.GetComponent<MeshRenderer>()));
        if (!_unicorn)
        {
            FakeStatusLight.HandleStrike();
        }
        else
        {
            FakeStatusLight.FlashStrike();
            TwitchPlays.CauseFakeStrike(BombModule);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Strike, transform);
        }
        Audio.HandlePlaySoundAtTransform(GlassBreak.name, transform);
        for (var i = 0; i < hitWall; i++)
        {
            StatusLight.localPosition -= movement;
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f);
        _strikePending = false;
    }

    private IEnumerator InstantlySolveModule(string reason, params object[] args)
    {
        BombModule.LogFormat("Instantly solving the module because the following error happened:");
        BombModule.LogFormat(reason, args);
        _movements.CancelFutureSubcoroutines();
        _movements.StopQueue();

        //Kill the Souvenir questions because of crash. This will make Souvenir ignore this Morse-A-Maze instance.
        _souvenirQuestionEndingLocation = null;
        _souvenirQuestionStartingLocation = null;
        _souvenirQuestionWordPlaying = null;

        _solved = true;
        if (!FakeStatusLight.HasFakeStatusLightFailed)
        {
            StartCoroutine(MoveStatusLightToCorner());
            FakeStatusLight.PassColor = StatusLightState.Green;
            HandleModulePass();
        }
        else
        {
            BombModule.HandlePass();
        }

        foreach (var location in Locations)
            location.gameObject.SetActive(false);

        yield return new WaitForSecondsRealtime(2);
        _unicorn = true;
        for (var x = 0; x < 6; x++)
        {
            for (var y = 0; y < 6; y++)
            {
                SetWall(x, y, false, true);
                SetWall(x, y, true, true);
            }
        }
        yield return new WaitForSecondsRealtime(8);
        StopAllCoroutines();
    }

    private IEnumerator MoveToLocation(Transform location, Transform from)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        //yield return null;
        BombModule.LogFormat("Moving from {0} to {1} - {2}", GetCoordinates(from), GetCoordinates(location), GetDirection(from, location));
        var lx = location.parent.localPosition.x;
        var ly = location.localPosition.z;
        var sx = StatusLight.localPosition.x;
        var sy = StatusLight.localPosition.z;
        var sz = StatusLight.localPosition.y;

        var x = lx - sx;
        var y = ly - sy;

        const float eta = 7.5f;
        var fps = (1 / Time.deltaTime) * (eta / 36f);
        var movex = x / fps;
        var movey = y / fps;

        if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            sy = ly;
            do
            {
                sx += movex;
                StatusLight.transform.localPosition = new Vector3(sx, sz, sy);
                yield return null;
                fps = (1 / Time.deltaTime) * (eta / 36f);
                movex = x / fps;

                if (sw.ElapsedMilliseconds <= 5000) continue;
                sw.Stop();
                StartCoroutine(InstantlySolveModule("The Status Light Left the maze while moving from {0} to {1} - {2}", GetCoordinates(from), GetCoordinates(location), GetDirection(from, location)));
                yield break;
            } while (Mathf.Abs(sx - lx) > Mathf.Abs(movex));
            sx = lx;
        }
        else
        {
            sx = lx;
            do
            {
                sy += movey;
                StatusLight.transform.localPosition = new Vector3(sx, sz, sy);
                yield return null;
                fps = (1 / Time.deltaTime) * (eta / 36f);
                movey = y / fps;

                if (sw.ElapsedMilliseconds <= 5000) continue;
                sw.Stop();
                StartCoroutine(InstantlySolveModule("The Status Light Left the maze while moving from {0} to {1} - {2}", GetCoordinates(from), GetCoordinates(location), GetDirection(from, location)));
                yield break;
            } while (Mathf.Abs(sy - ly) > Mathf.Abs(movey));
            sy = ly;
        }
        StatusLight.transform.localPosition = new Vector3(sx, sz, sy);

        if (location == _destination)
        {
            StartCoroutine(MoveStatusLightToCorner());
        }
        yield return null;
        sw.Stop();
    }

    private List<MeshRenderer> _showingWalls = new List<MeshRenderer>();
    private IEnumerator ShowWall(MeshRenderer wall)
    {
        if(wall == null)
            yield break;
        _showingWalls.Add(wall);
        var color = wall.material.color;
        for (var j = 0; j < 3; j++)
        {
            for (var i = color.a; i > 0; i -= 0.05f)
            {
                color = new Color(color.r, color.g, color.b, i);
                wall.material.color = color;
                yield return null;
            }
            for (var i = 0f; i < 1.0f; i += 0.05f)
            {
                color = new Color(color.r, color.g, color.b, i);
                wall.material.color = color;
                yield return null;
            }
        }
        if (_edgeworkRules[_rule] == EdgeworkRules.Strikes && !_unicorn)
        {
            for (var i = color.a; i > 0; i -= 0.01f)
            {
                color = new Color(color.r, color.g, color.b, i);
                wall.material.color = color;
                yield return null;
            }
            color = new Color(color.r, color.g, color.b, 0);
            wall.material.color = color;
        }
        wall.material.color = color;
        _showingWalls.Remove(wall);
    }

    private bool _unicorn;

    private IEnumerator HideWall(MeshRenderer wall)
    {
        if (_showingWalls.Contains(wall))
            yield break;
        if (wall == null)
            yield break;

        /*if (_unicorn)
        {
            StartCoroutine(ShowWall(wall));
            yield break;
        }*/

        var color = wall.material.color;
        for (var i = color.a; i > 0; i -= 0.01f)
        {
            color = new Color(color.r, color.g, color.b, i);
            wall.material.color = color;
            yield return null;
        }
        color = new Color(color.r, color.g, color.b, 0);
        wall.material.color = color;
    }

    private bool ProcessMove(Transform wall, Transform newLocation)
    {
        if (wall != null)
        {
            _movements.AddToQueue(GiveStrike(wall, _currentLocation, newLocation));
            _strikePending = true;
            return false;
        }

        _movements.AddToQueue(MoveToLocation(newLocation, _currentLocation));
        _currentLocation = newLocation;
        _solved |= GetCoordinates(newLocation) == GetCoordinates(_destination);
        return true;
    }

    #region buttons
    private bool MoveLeft()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BombModule.transform);
        if (_solved || _strikePending) return true;
        const string x = "ABCDEF";
        var location = _currentLocation;
        if (location.parent.name == "A")
            return true;
        var destination = x.Substring(x.IndexOf(location.parent.name, StringComparison.Ordinal) - 1, 1);
        var wall = destination + location.parent.name;
        var loc = location.parent.parent.Find(destination).Find(location.name);

        return ProcessMove(CheckVerticalWalls(location.name, wall), loc);
    }

    private bool MoveRight()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BombModule.transform);
        if (_solved || _strikePending) return true;
        const string x = "ABCDEF";
        var location = _currentLocation;
        if (location.parent.name == "F")
            return true;
        var destination = x.Substring(x.IndexOf(location.parent.name, StringComparison.Ordinal) + 1, 1);
        var wall = location.parent.name + destination;
        var loc = location.parent.parent.Find(destination).Find(location.name);

        return ProcessMove(CheckVerticalWalls(location.name, wall), loc);
    }


    private bool MoveUp()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BombModule.transform);
        if (_solved || _strikePending)
            return true;
        var location = _currentLocation;
        if (location.name == "1")
            return true;
        var destination = (int.Parse(location.name) - 1).ToString();
        var wall = destination + location.name;
        var loc = location.parent.Find(destination);

        return ProcessMove(CheckHorizontalWall(location.parent.name, wall), loc);
    }

    private bool MoveDown()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BombModule.transform);
        if (_solved || _strikePending) return true;
        var location = _currentLocation;
        if (location.name == "6")
            return true;
        var destination = (int.Parse(location.name) + 1).ToString();
        var wall = location.name + destination;
        var loc = location.parent.Find(destination);

        return ProcessMove(CheckHorizontalWall(location.parent.name, wall), loc);
    }
    #endregion

    #region Activate()
    private void Activate()
    {
        BombModule.LogFormat("Bomb Serial Number = {0}", BombInfo.GetSerialNumber());
        Up.OnInteract += delegate { MoveUp(); return false; };
        Down.OnInteract += delegate { MoveDown(); return false; };
        Left.OnInteract += delegate { MoveLeft(); return false; };
        Right.OnInteract += delegate { MoveRight(); return false; };
 
        
        _rule = Random.Range(0, _morseCodeWords.Length);
        //if (BombModule.GetIDNumber() == 1)
        //    _rule = _edgeworkRules.ToList().IndexOf(EdgeworkRules.Strikes);

        _unicorn = BombInfo.IsIndicatorOff("BOB") && BombInfo.GetBatteryHolderCount(2) == 1 && BombInfo.GetBatteryHolderCount(1) == 2 && BombInfo.GetBatteryHolderCount() == 3;
        _souvenirQuestionWordPlaying = _morseCodeWords[_rule];

        StartCoroutine(PlayWordLocation(_souvenirQuestionWordPlaying));


        switch (_edgeworkRules[_rule])
        {
            case EdgeworkRules.Batteries:
                GetMazeSolution(BombInfo.GetBatteryCount());
                break;
            case EdgeworkRules.BatteryHolders:
                GetMazeSolution(BombInfo.GetBatteryHolderCount());
                break;
            case EdgeworkRules.LitIndicators:
                GetMazeSolution(BombInfo.GetOnIndicators().Count());
                break;
            case EdgeworkRules.UnlitIndicators:
                GetMazeSolution(BombInfo.GetOffIndicators().Count());
                break;
            case EdgeworkRules.TotalIndicators:
                GetMazeSolution(BombInfo.GetIndicators().Count());
                break;
            case EdgeworkRules.TotalPorts:
                GetMazeSolution(BombInfo.GetPortCount());
                break;
            case EdgeworkRules.UniquePorts:
                GetMazeSolution(BombInfo.CountUniquePorts());
                break;
            case EdgeworkRules.SerialLastDigit:
                GetMazeSolution(int.Parse(BombInfo.GetSerialNumber().Substring(5, 1)));
                break;
            case EdgeworkRules.SerialSum:
                GetMazeSolution(BombInfo.GetSerialNumberNumbers().Sum());
                break;
            case EdgeworkRules.PortPlates:
                GetMazeSolution(BombInfo.GetPortPlateCount());
                break;
            case EdgeworkRules.SolveCount:
                _lastSolved = BombInfo.GetSolvedModuleNames().Count;
                GetMazeSolution(_lastSolved);
                break;
            case EdgeworkRules.TwoFactor:
                SetTwoFactor();
                break;
            case EdgeworkRules.Strikes:
                _lastStrikes = BombInfo.GetStrikes();
                GetMazeSolution(_lastStrikes);
                break;
            case EdgeworkRules.DayOfWeek:
                GetMazeSolution((int) DateTime.Now.DayOfWeek);
                break;
            case EdgeworkRules.EmptyPortPlates:
                GetMazeSolution(BombInfo.GetPortPlates().Count(plate => plate.Length == 0));
                break;
            case EdgeworkRules.FirstSerialDigit:
                GetMazeSolution(BombInfo.GetSerialNumberNumbers().ToArray()[0]);
                break;
            case EdgeworkRules.SerialNumberLetter:
                GetMazeSolution("ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(BombInfo.GetSerialNumberLetters().ToArray()[0]));
                break;
            case EdgeworkRules.StartingTime:
                GetMazeSolution((int)(BombInfo.GetTime() / 60));
                break;
            // ReSharper disable once RedundantCaseLabel
            case EdgeworkRules.None:
            default:
                GetMazeSolution(_rule);
                break;
        }
    }
    #endregion

    #region Solution Generator
    private static int GetXYfromLocation(Transform location)
    {
        var ones = "ABCDEF".IndexOf(location.parent.name, StringComparison.Ordinal);
        var tens = "123456".IndexOf(location.name, StringComparison.Ordinal);
        return (tens * 10) + ones;
    }

    private readonly Stack<string> _mazeStack = new Stack<string>();
    private bool[] _explored;
    private bool GenerateMazeSolution(int maze, int startXY)
    {
        if (startXY == 77)
        {
            _explored = new bool[60];
            startXY = GetXYfromLocation(_currentLocation);
        }
        var endXY = GetXYfromLocation(_destination);


        var x = startXY % 10;
        var y = startXY / 10;

        if ((x > 5) || (y > 5) || (maze == -1) || (endXY == 66)) return false;
        //var directions = _mazes[maze, y, x];
        var cell = MazeRuleSet.Mazes[maze].GetCell(x, y);
        var directions = new[] { cell.WallUp, cell.WallDown, cell.WallLeft, cell.WallRight };
        if (startXY == endXY) return true;
        _explored[startXY] = true;

        var directionInt = new[] { -10, 10, -1, 1 };
        var directionReturn = new[] { "Up", "Down", "Left", "Right" };

        for (var i = 0; i < 4; i++)
        {
            if (directions[i]) continue;
            if (_explored[startXY + directionInt[i]]) continue;
            if (!GenerateMazeSolution(maze, startXY + directionInt[i])) continue;
            _mazeStack.Push(directionReturn[i]);
            return true;
        }

        return false;
    }


    private bool _firstGeneration = true;

    private bool IsLocationAlwaysThreeMovesTwoDirections()
    {
        var fromCoordinates = GetCoordinates(_currentLocation);
        var toCoordinates = GetCoordinates(_destination);
        var fromX = "ABCDEF".IndexOf(fromCoordinates.Substring(0, 1), StringComparison.Ordinal) + 1;
        var toX = "ABCDEF".IndexOf(toCoordinates.Substring(0, 1), StringComparison.Ordinal) + 1;
        var fromY = int.Parse(fromCoordinates.Substring(1));
        var toY = int.Parse(toCoordinates.Substring(1));
        if (fromX == toX)
            return false;
        if (fromY == toY)
            return false;
        return (Mathf.Abs(fromX - toX) + Mathf.Abs(fromY - toY)) >= 3;
    }

    private void GetMazeSolution(int maze)
    {
        maze %= 18;
        SetMaze(maze);


        var directions = new List<string>();
        StringBuilder sb = new StringBuilder();
        int moveLength = 0;
        bool success = false;

        //} while (_firstGeneration && (moveLength < 3 || directions.Count < 2) && (locationNumber < Locations.Length));


        var allMazes = _edgeworkRules[_rule] == EdgeworkRules.TwoFactor
                       || _edgeworkRules[_rule] == EdgeworkRules.SolveCount
                       || _edgeworkRules[_rule] == EdgeworkRules.Strikes;
        allMazes &= _firstGeneration;


        for (var i = 1; i < (_firstGeneration ? Locations.Length : 2); i++)
        {
            if (_firstGeneration)
            {
                _destination = Locations[i];
            }
            for (var j = maze; j < (allMazes ? maze + 18 + 1 : maze + 1); j++)
            {
                directions.Clear();
                sb = new StringBuilder();

                success = GenerateMazeSolution(j % 18, 77);
                moveLength = _mazeStack.Count;
                if (!success)
                {
                    BombModule.LogFormat(
                        "Failed to Generate the maze solution for going from {0} to {1} in maze {2}",
                        GetCoordinates(_currentLocation), GetCoordinates(_destination), maze + 1);
                    continue;
                }

                var move = _mazeStack.Pop();
                directions.Add(move);
                sb.Append(move);
                while (_mazeStack.Count > 0)
                {
                    move = _mazeStack.Pop();
                    if (!directions.Contains(move))
                        directions.Add(move);
                    sb.Append(", " + move);
                }
                if (IsLocationAlwaysThreeMovesTwoDirections() && j == maze) break;  //Always at least 3 moves and at least 2 Directions away in ALL mazes.
                if (moveLength < 3 || directions.Count < 2) break;  //No point in checking the rest of the mazes if the rules are not met.
                //The first maze will be run twice if ALL 18 mazes pass the rules.
            }
            if (IsLocationAlwaysThreeMovesTwoDirections()) break;   //And no need to check any more locations if this condition is met.
            if (moveLength >= 3 && directions.Count >= 2) break; //Likewise if this condition was met in ALL 18 mazes, being on same line or less than 3 manhatten distance away.
        }
        if (!success)
        {
            StartCoroutine(InstantlySolveModule("Failed to generate a maze solution."));
            return;
        }

        if (_firstGeneration)
        {
            _souvenirQuestionStartingLocation = GetCoordinates(_currentLocation);
            _souvenirQuestionEndingLocation = GetCoordinates(_destination);

            BombModule.LogFormat("Starting Location: {0} - Destination Location: {1}",
                _souvenirQuestionStartingLocation, _souvenirQuestionEndingLocation);
            BombModule.LogFormat("Rule used to Look up the Maze = {0}", GetRuleName());
            BombModule.LogFormat("Playing Morse code word: \"{0}\"", _morseCodeWords[_rule]);


            if (_unicorn)
            {
                BombModule.LogFormat("Bob will actively prevent you from getting any strikes.");
            }

            _firstGeneration = false;
        }
        else
        {
            BombModule.LogFormat("Updating the maze for rule {0}", _edgeworkRules[_rule]);
        }
        BombModule.LogFormat("Maze Solution from {0} to {1} in maze \"{2} - {3}\" is: {4}",
            GetCoordinates(_currentLocation),
            GetCoordinates(_destination), maze, _morseCodeWords[maze], sb);
    }

    private string GetRuleName()
    {
        if (_edgeworkRules[_rule] == EdgeworkRules.TwoFactor)
        {
            return BombInfo.IsTwoFactorPresent() ? "TwoFactor" : "UnsolvedModules";
        }
        return _edgeworkRules[_rule].ToString();
    }
    #endregion

    private int _lastStrikes = -1;
    private int _lastSolved = -1;
    private int _lastTwoFactorSum = -1;
    private void SetTwoFactor()
    {
        if (BombInfo.GetTwoFactorCounts() == 0)
        {
            _lastSolved = BombInfo.GetSolvedModuleNames().Count;
            GetMazeSolution(BombInfo.GetSolvableModuleNames().Count - _lastSolved);
        }
        else
        {
            foreach (var twofactor in BombInfo.GetTwoFactorCodes())
            {
                BombModule.LogFormat("Two Factor code: ",twofactor);
            }

            var sum = BombInfo.GetTwoFactorCodes().Select(twofactor => twofactor / 10).Select(code => code % 10).Sum();
            GetMazeSolution(sum);
            _lastTwoFactorSum = sum;
        }
    }

    private IEnumerator PlayWordLocation(string word)
    {
        while (!_solved)
        {
            var playword = FakeStatusLight.PlayWord(word);
            do
            {
                yield return playword.Current;
                if (_solved) yield break;
            } while (playword.MoveNext());
            yield return new WaitForSeconds(3f);
            if (_solved) yield break;
            playword = FakeStatusLight.PlayWord(_destination.parent.name + _destination.name);
            do
            {
                yield return playword.Current;
                if (_solved) yield break;
            } while (playword.MoveNext());
            yield return new WaitForSeconds(3f);
        }

    }

    #region TwitchPlays
#pragma warning disable 414
    // ReSharper disable InconsistentNaming
    private static bool TwitchPlaysDetected = false;
    private string TwitchManualCode = "Morse-A-Maze";
    private string TwitchHelpMessage = "!{0} move up down left right, !{0} move udlr [make a series of status light moves]";
    private string[] TwitchValidCommands = {"((UseDefaultColors|UseEasyColors|UseCruelColors|UseRedOnSolve|UseGreenOnSolve|UseOffOnSolve|UseRandomOnSolve)? ?)*(move .*)?"};
    // ReSharper restore InconsistentNaming
#pragma warning restore 414
    private IEnumerator ProcessTwitchCommand(string command)
    {
        TwitchPlaysDetected = true;
        var originalCommand = command;
        var commandUsed = false;
        if (command.Contains("UseDefaultColors"))
        {
            command = command.Replace("UseDefaultColors", "").Trim();
            FakeStatusLight.PassColor = StatusLightState.Off;
            FakeStatusLight.MorseTransmitColor = StatusLightState.Red;
            FakeStatusLight.OffColor = StatusLightState.Green;
            FakeStatusLight.FailColor = StatusLightState.Off;
            yield return new WaitForSeconds(0.1f);
            commandUsed = true;
        }
        else if (command.Contains("UseEasyColors"))
        {
            command = command.Replace("UseEasyColors", "").Trim();
            FakeStatusLight.PassColor = StatusLightState.Green;
            FakeStatusLight.MorseTransmitColor = StatusLightState.Green;
            FakeStatusLight.OffColor = StatusLightState.Off;
            FakeStatusLight.FailColor = StatusLightState.Red;
            yield return new WaitForSeconds(0.1f);
            commandUsed = true;
        }
        else if (command.Contains("UseCruelColors"))
        {
            command = command.Replace("UseCruelColors", "").Trim();
            FakeStatusLight.PassColor = StatusLightState.Random;
            FakeStatusLight.FailColor = StatusLightState.Random;
            FakeStatusLight.OffColor = StatusLightState.Random;
            FakeStatusLight.MorseTransmitColor = StatusLightState.Random;
            yield return new WaitForSeconds(0.1f);
            commandUsed = true;
        }

        if (command.Contains("UseRedOnSolve"))
        {
            command = command.Replace("UseRedOnSolve", "").Trim();
            FakeStatusLight.PassColor = StatusLightState.Red;
            yield return new WaitForSeconds(0.1f);
            commandUsed = true;
        }
        else if (command.Contains("UseGreenOnSolve"))
        {
            command = command.Replace("UseGreenOnSolve", "").Trim();
            FakeStatusLight.PassColor = StatusLightState.Green;
            yield return new WaitForSeconds(0.1f);
            commandUsed = true;
        }
        else if (command.Contains("UseOffOnSolve"))
        {
            command = command.Replace("UseOffOnSolve", "").Trim();
            FakeStatusLight.PassColor = StatusLightState.Off;
            yield return new WaitForSeconds(0.1f);
            commandUsed = true;
        }
        else if (command.Contains("UseRandomOnSolve"))
        {
            command = command.Replace("UseOffOnSolve", "").Trim();
            FakeStatusLight.PassColor = StatusLightState.Random;
            yield return new WaitForSeconds(0.1f);
            commandUsed = true;
        }

        /*
        if (command.Contains("ForceUnicorn"))
        {
            yield return null;
            commandUsed = true;
            _unicorn = true;

        }*/


        if (!command.StartsWith("move ", StringComparison.InvariantCultureIgnoreCase))
        {
            if (commandUsed)
            {
                yield return "sendtochat " + originalCommand + " processed successfully";
            }
            yield break;
        }

        if (_solved)
        {
            yield return "solve";
            yield break;
        }

        command = command.Substring(5);
        MatchCollection matches = Regex.Matches(command, @"[udlr]", RegexOptions.IgnoreCase);
        if (matches.Count == 0)
            yield break;

        yield return null;
        if (matches.Count > 35 || _movements.Processing)
        {
            yield return "elevator music";
        }

        while (_movements.Processing)
        {
            yield return "trycancel";
            yield return new WaitForSeconds(0.1f);
        }

        var moved = false;
        foreach (Match move in matches)
        {
            moved = true;
            bool safe;
            
            switch (move.Value.ToLowerInvariant())
            {
                case "u":
                    safe = MoveUp();
                    break;
                case "d":
                    safe = MoveDown();
                    break;
                case "l":
                    safe = MoveLeft();
                    break;
                case "r":
                    safe = MoveRight();
                    break;
                default:
                    continue;
            }
            if (!safe)
            {
                if (!TwitchPlays.Installed() && _unicorn)
                {
                    yield return "multiple strikes";
                    yield return "award strikes 0";
                }
                else
                {
                    yield return "strike";
                }
                yield break;
            }
            if (_solved)
            {
                yield return "solve";
                yield break;
            }
            yield return "trycancel";
            yield return new WaitForSeconds(0.1f);
        }
        if(moved) yield return "solve";
    }
    #endregion

    // ReSharper disable once UnusedMember.Local
    private void Update ()
    {
        if (_movements == null)
        {
            StartCoroutine(InstantlySolveModule("Module failed to Initialize"));
            _movements = new CoroutineQueue();
        }
        if (_movements.Processing) return;
        if (_solved) return;
        
	    // ReSharper disable once SwitchStatementMissingSomeCases
	    switch (_edgeworkRules[_rule])
	    {
	        case EdgeworkRules.SolveCount:
	            if (_lastSolved != BombInfo.GetSolvedModuleNames().Count)
	            {
	                _lastSolved = BombInfo.GetSolvedModuleNames().Count;
	                GetMazeSolution(_lastSolved);
	                BombModule.LogFormat("Maze updated for {0} modules Solved", _lastSolved);

	            }
	            break;
	        case EdgeworkRules.TwoFactor:
	            if (_lastSolved != -1 && _lastSolved != BombInfo.GetSolvedModuleNames().Count)
	            {
	                SetTwoFactor();
	                BombModule.LogFormat("Maze updated for {0} modules Unsolved", BombInfo.GetSolvableModuleNames().Count - _lastSolved);
	            }
	            if (_lastTwoFactorSum != -1 && _lastTwoFactorSum !=
	                BombInfo.GetTwoFactorCodes().Select(twofactor => twofactor / 10).Select(code => code % 10).Sum())
	            {
	                SetTwoFactor();
	                BombModule.LogFormat("Maze updated for Two Factor 2nd least significant digit sum of {0}", _lastTwoFactorSum);
	            }
	            break;
            case EdgeworkRules.Strikes:
                if (BombInfo.GetStrikes() != _lastStrikes)
                {
                    _lastStrikes = BombInfo.GetStrikes();
                    GetMazeSolution(_lastStrikes);
                    BombModule.LogFormat("Maze updated for {0} strikes", _lastStrikes);
                }
                break;
	    }
        

	}
}