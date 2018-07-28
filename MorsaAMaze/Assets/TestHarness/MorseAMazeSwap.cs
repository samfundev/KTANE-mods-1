using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Assets.Scripts;
using Assets.Scripts.RuleGenerator;
using Random = UnityEngine.Random;

// ReSharper disable once CheckNamespace
[RummageNoRename]
public class MorseAMazeSwap : MonoBehaviour
{
    public FakeStatusLight FakeStatusLight;

    public Transform StatusLight;
    public Transform StatusLightCorner;
    public Transform[] Locations;

    public MeshRenderer[] BorderWalls;
    public MeshRenderer[] Floors;

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

    public Color[] WallColors = {Color.cyan, Color.magenta};

    public bool MorseASwap = true;

    private Transform _currentLocation;
    private Transform _destination;

    private CoroutineQueue _movements;
    private bool _swapped;
    private bool _solved;
    private bool _strikePending;
    private readonly int[] _mazes = {0, 0};
    private readonly List<List<MeshRenderer>> _shownWalls = new List<List<MeshRenderer>> {new List<MeshRenderer>(), new List<MeshRenderer>()};

    private bool Swapped
    {
        get { return MorseASwap && _swapped; }
        set { _swapped = MorseASwap && value; }
    }
    

    private string _souvenirQuestionStartingLocation;
    private string _souvenirQuestionEndingLocation;
    private string[] _souvenirQuestionWordsPlaying = {null, null};

    private ModSettings _modSettings;

    private static MorseAMazeSwapRuleGenerator MazeRuleSetSwap
    {
        get { return MorseAMazeSwapRuleGenerator.Instance; }
    }

    private static MorseAMazeRuleGenerator MazeRuleSetNormal
    {
        get { return MorseAMazeRuleGenerator.Instance; }
    }

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

    private readonly int[] _rules = {-1, -1};
    // ReSharper disable InconsistentNaming
    private static string[] MorseCodeWords;    //Generated randomly in a fixed way for challenge purposes.
    private static Maze[] Mazes;
    // ReSharper restore InconsistentNaming

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

    private IEnumerator ChangeBorderWallColors()
    {
        float currentTime = Time.time;
        while (Time.time < (currentTime + 0.5f))
        {
            float lerp = (Time.time - currentTime) / 0.5f;
            Color color = Color.Lerp(Swapped ? WallColors[0] : WallColors[1], Swapped ? WallColors[1] : WallColors[0], lerp);
            foreach (MeshRenderer r in BorderWalls)
            {
                r.material.color = color;
            }
            yield return null;
        }

        foreach (MeshRenderer r in BorderWalls)
        {
            r.material.color = Swapped ? WallColors[1] : WallColors[0];
        }
    }

    // Use this for initialization
    // ReSharper disable once UnusedMember.Local
    [RummageNoRename]
    [RummageNoMarkPublic]
    private void Awake()
    {
        _modSettings = new ModSettings("MorseAMaze");
        BombModule.GenerateLogFriendlyName();

        if (MorseASwap)
        {
            MorseCodeWords = MazeRuleSetSwap.Words.ToArray();
            Mazes = MazeRuleSetSwap.Mazes.ToArray();
            
        }
        else
        {
            MorseCodeWords = MazeRuleSetNormal.Words.ToArray();
            Mazes = MazeRuleSetNormal.Mazes.ToArray();
            WallColors[0] = Color.red;
            WallColors[1] = Color.red;
            foreach (MeshRenderer mr in Floors)
            {
                const float gray = 133 / 255f;
                mr.material.color = new Color(gray, gray, gray);
            }
        }
    }

    // Use this for initialization
    // ReSharper disable once UnusedMember.Local
    [RummageNoRename]
    [RummageNoMarkPublic]
    private void Start()
    {
        /*if (Application.isEditor)
        {
            UnityEngine.Debug.LogFormat("The Morse code words in order are: {0}", string.Join(", ", MorseCodeWords).ToUpperInvariant());
            for (int i = 0; i < 18; i++)
            {
                File.WriteAllText(string.Format(@"H:\KTANE-Mods\MorsaAMaze\Manual\img\Morse-A-Maze-Swap\maze{0}.svg", i), Mazes[i].ToSVG());
            }
        }*/

        StartCoroutine(TwitchPlays.Refresh());
        _modSettings.ReadSettings();
        _movements = gameObject.AddComponent<CoroutineQueue>();
        
        
	    Locations.Shuffle();
        SetMaze(new [] {0,0}, Swapped); //Hide the walls now.

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

        Swapped = Random.Range(0, 100) >= 50;
        StartCoroutine(ChangeBorderWallColors());
    }

    private Vector3 _originalStatusLightLocation;
    private float _statusLightMoveFrames;
    private const float MoveTime = 1f;
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

        var cornerSink = new Vector3(_originalStatusLightLocation.x, -_originalStatusLightLocation.y, _originalStatusLightLocation.z);
        var startSink = new Vector3(_currentLocation.parent.localPosition.x, -_originalStatusLightLocation.y, _currentLocation.localPosition.z);
        var startRise = new Vector3(startSink.x, -startSink.y, startSink.z);

        var currentTime = Time.time;
        while (Time.time < (currentTime + MoveTime))
        {
            var lerp = (Time.time - currentTime) / MoveTime;
            StatusLight.localPosition = Vector3.Lerp(_originalStatusLightLocation, cornerSink, lerp);
            yield return null;
        }

        StatusLight.localPosition = startSink;
        yield return null;

        currentTime = Time.time;
        while (Time.time < (currentTime + MoveTime))
        {
            var lerp = (Time.time - currentTime) / MoveTime;
            StatusLight.localPosition = Vector3.Lerp(startSink, startRise, lerp);
            yield return null;
        }

        StatusLight.localPosition = startRise;
        yield return null;
    }

    private IEnumerator MoveStatusLightToCorner()
    {
        var cornerSink = new Vector3(_originalStatusLightLocation.x, -_originalStatusLightLocation.y, _originalStatusLightLocation.z);
        var startSink = new Vector3(_currentLocation.parent.localPosition.x, -_originalStatusLightLocation.y, _currentLocation.localPosition.z);
        var startRise = new Vector3(startSink.x, -startSink.y, startSink.z);

        var currentTime = Time.time;
        while (Time.time < (currentTime + MoveTime))
        {
            var lerp = (Time.time - currentTime) / MoveTime;
            StatusLight.localPosition = Vector3.Lerp(startRise, startSink, lerp);
            yield return null;
        }

        StatusLight.localPosition = cornerSink;
        yield return null;

        currentTime = Time.time;
        while (Time.time < (currentTime + MoveTime))
        {
            var lerp = (Time.time - currentTime) / MoveTime;
            StatusLight.localPosition = Vector3.Lerp(cornerSink, _originalStatusLightLocation, lerp);
            yield return null;
        }

        StatusLight.localPosition = _originalStatusLightLocation;
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

    private void SetMaze(int[] mazes, bool swapped)
    {
        mazes[0] %= 18;
        mazes[1] %= 18;
        _mazes[0] = mazes[0];
        _mazes[1] = mazes[1];
        int maze = mazes[swapped ? 1 : 0];
        if (Swapped != swapped) return;
        for (var x = 0; x < 6; x++)
        {
            for (var y = 0; y < 6; y++)
            {
                /*SetWall(x, y, false, !_mazes[maze, y, x].Contains("d"));
                SetWall(x, y, true, !_mazes[maze, y, x].Contains("r"));*/
                SetWall(x, y, false, Mazes[maze].GetCell(x, y).WallDown);
                SetWall(x, y, true, Mazes[maze].GetCell(x, y).WallRight);
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
        _souvenirQuestionWordsPlaying = null;

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
        //yield return null;
        BombModule.LogFormat("Moving from {0} to {1} - {2}", GetCoordinates(from), GetCoordinates(location), GetDirection(from, location));

        var sp = StatusLight.localPosition;
        var to = new Vector3(location.parent.localPosition.x, sp.y, location.localPosition.z);

        var currentTime = Time.time;

        while(Time.time < (currentTime + 0.25f))
        {
            var lerp = (Time.time - currentTime) / 0.25f;  
            StatusLight.transform.localPosition = Vector3.Lerp(sp, to, lerp);
            yield return null;
        }
        StatusLight.transform.localPosition = to;

        if (Random.value < (1/3f))
        {
            int fromMaze = Swapped ? 2 : 1;
            Swapped = !Swapped;
            int toMaze = Swapped ? 2 : 1;

            SetMaze(_mazes, Swapped);
            StartCoroutine(ChangeBorderWallColors());
            foreach (var wall in _shownWalls[Swapped ? 1 : 0])
            {
                StartCoroutine(ShowWall(wall));
            }
            BombModule.LogFormat("Maze swapped from {0} to {1}", fromMaze, toMaze);
        }

        if (location == _destination)
        {
            StartCoroutine(MoveStatusLightToCorner());
        }
        yield return null;
    }

    private List<MeshRenderer> _showingWalls = new List<MeshRenderer>();
    private IEnumerator ShowWall(MeshRenderer wall)
    {
        if(wall == null)
            yield break;
        var swapped = Swapped;
        _showingWalls.Add(wall);
        if(!_shownWalls[swapped ? 1 : 0].Contains(wall))
            _shownWalls[swapped ? 1 : 0].Add(wall);
        var color = Swapped ? WallColors[1] : WallColors[0];
        var colorlerp = new Color(color.r, color.g, color.b, 0);
        for (var j = 0; j < 3; j++)
        {
            for (var i = color.a; i > 0; i -= 0.05f)
            {
                wall.material.color = Color.Lerp(colorlerp, color, i);
                yield return null;
            }
            for (var i = 0f; i < 1.0f; i += 0.05f)
            {
                wall.material.color = Color.Lerp(colorlerp, color, i);
                yield return null;
            }
        }
        if ((_edgeworkRules[_rules[swapped ? 1 : 0]] == EdgeworkRules.Strikes && !_unicorn) || (swapped != Swapped && !_shownWalls[Swapped ? 1 : 0].Contains(wall)))
        {
            if (swapped == Swapped)
                _shownWalls[swapped ? 1 : 0].Clear();
            for (var i = color.a; i > 0; i -= 0.01f)
            {
                wall.material.color = Color.Lerp(colorlerp, color, i);
                yield return null;
            }
            wall.material.color = colorlerp;
        }
        else
        {
            wall.material.color = color;
        }
        _showingWalls.Remove(wall);
    }

    private bool _unicorn;

    private IEnumerator HideWall(MeshRenderer wall)
    {
        if (wall == null)
            yield break;
        if (_showingWalls.Contains(wall))
            yield break;
        if (_shownWalls[Swapped ? 1 : 0].Contains(wall))
            yield break;
        

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

        _rules[0] = Random.Range(0, MorseCodeWords.Length);
        _rules[1] = Random.Range(0, MorseCodeWords.Length);
        //if (BombModule.GetIDNumber() == 1)
        //    _rule = _edgeworkRules.ToList().IndexOf(EdgeworkRules.Strikes);

        _unicorn = BombInfo.IsIndicatorOff("BOB") && BombInfo.GetBatteryHolderCount(2) == 1 && BombInfo.GetBatteryHolderCount(1) == 2 && BombInfo.GetBatteryHolderCount() == 3;
        _souvenirQuestionWordsPlaying[0] = MorseCodeWords[_rules[0]];
        _souvenirQuestionWordsPlaying[1] = MorseCodeWords[_rules[1]];

        var words = new[] {_souvenirQuestionWordsPlaying[0], _souvenirQuestionWordsPlaying[1], _destination.parent.name + _destination.name};
        StartCoroutine(PlayWordLocation(words));

        int[] mazes = {0, 0};
        for(int i = 0; i < 2; i++)
        {
            switch (_edgeworkRules[_rules[i]])
            {
                case EdgeworkRules.Batteries:
                    mazes[i] = BombInfo.GetBatteryCount();
                    break;
                case EdgeworkRules.BatteryHolders:
                    mazes[i] = BombInfo.GetBatteryHolderCount();
                    break;
                case EdgeworkRules.LitIndicators:
                    mazes[i] = BombInfo.GetOnIndicators().Count();
                    break;
                case EdgeworkRules.UnlitIndicators:
                    mazes[i] = BombInfo.GetOffIndicators().Count();
                    break;
                case EdgeworkRules.TotalIndicators:
                    mazes[i] = BombInfo.GetIndicators().Count();
                    break;
                case EdgeworkRules.TotalPorts:
                    mazes[i] = BombInfo.GetPortCount();
                    break;
                case EdgeworkRules.UniquePorts:
                    mazes[i] = BombInfo.CountUniquePorts();
                    break;
                case EdgeworkRules.SerialLastDigit:
                    mazes[i] = int.Parse(BombInfo.GetSerialNumber().Substring(5, 1));
                    break;
                case EdgeworkRules.SerialSum:
                    mazes[i] = BombInfo.GetSerialNumberNumbers().Sum();
                    break;
                case EdgeworkRules.PortPlates:
                    mazes[i] = BombInfo.GetPortPlateCount();
                    break;
                case EdgeworkRules.SolveCount:
                    _lastSolved = BombInfo.GetSolvedModuleNames().Count;
                    mazes[i] = _lastSolved;
                    break;
                case EdgeworkRules.TwoFactor:
                    SetTwoFactor();
                    break;
                case EdgeworkRules.Strikes:
                    _lastStrikes = BombInfo.GetStrikes();
                    mazes[i] = _lastStrikes;
                    break;
                case EdgeworkRules.DayOfWeek:
                    mazes[i] = (int) DateTime.Now.DayOfWeek;
                    break;
                case EdgeworkRules.EmptyPortPlates:
                    mazes[i] = BombInfo.GetPortPlates().Count(plate => plate.Length == 0);
                    break;
                case EdgeworkRules.FirstSerialDigit:
                    mazes[i] = BombInfo.GetSerialNumberNumbers().ToArray()[0];
                    break;
                case EdgeworkRules.SerialNumberLetter:
                    mazes[i] = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(BombInfo.GetSerialNumberLetters().ToArray()[0]);
                    break;
                case EdgeworkRules.StartingTime:
                    mazes[i] = (int) (BombInfo.GetTime() / 60);
                    break;
                // ReSharper disable once RedundantCaseLabel
                case EdgeworkRules.None:
                default:
                    mazes[i] = _rules[i];
                    break;
            }
        }
        GetMazeSolution(mazes, Swapped);
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
        var cell = Mazes[maze].GetCell(x, y);
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

    private string GetMazeSolution(int[] mazes, bool swapped)
    {
        mazes[0] %= 18;
        mazes[1] %= 18;
        SetMaze(mazes, swapped);

        var directions = new List<string>();
        StringBuilder sb = new StringBuilder();
        int moveLength = 0;
        bool success = false;

        for(var h = !_firstGeneration ? (swapped ? 1 : 0) : 0; h < (!_firstGeneration ? (swapped ? 2 : 1) : 2); h++)
        {
            var allMazes = _edgeworkRules[_rules[h]] == EdgeworkRules.TwoFactor
                           || _edgeworkRules[_rules[h]] == EdgeworkRules.SolveCount
                           || _edgeworkRules[_rules[h]] == EdgeworkRules.Strikes;
            allMazes &= _firstGeneration;

            for (var i = 1; i < (_firstGeneration ? Locations.Length : 2); i++)
            {
                if (_firstGeneration)
                {
                    _destination = Locations[i];
                }
                for (var j = mazes[h]; j < (allMazes ? mazes[h] + 18 + 1 : mazes[h] + 1); j++)
                {
                    directions.Clear();
                    sb = new StringBuilder();

                    success = GenerateMazeSolution(j % 18, 77);
                    moveLength = _mazeStack.Count;
                    if (!success)
                    {
                        BombModule.LogFormat(
                            "Failed to Generate the maze solution for going from {0} to {1} in maze {2}",
                            GetCoordinates(_currentLocation), GetCoordinates(_destination), mazes[h] + 1);
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
                    if (IsLocationAlwaysThreeMovesTwoDirections() && j == mazes[h]) break;  //Always at least 3 moves and at least 2 Directions away in ALL mazes.
                    if (moveLength < 3 || directions.Count < 2) break;  //No point in checking the rest of the mazes if the rules are not met.
                    //The first maze will be run twice if ALL 18 mazes pass the rules.
                }
                if (IsLocationAlwaysThreeMovesTwoDirections()) break;   //And no need to check any more locations if this condition is met.
                if (moveLength >= 3 && directions.Count >= 2) break; //Likewise if this condition was met in ALL 18 mazes, being on same line or less than 3 manhatten distance away.
            }

            if (!success)
            {
                StartCoroutine(InstantlySolveModule("Failed to generate a maze solution."));
                return null;
            }

            if (_firstGeneration)
            {
                _souvenirQuestionStartingLocation = GetCoordinates(_currentLocation);
                _souvenirQuestionEndingLocation = GetCoordinates(_destination);

                BombModule.LogFormat("Maze {2} - Starting Location: {0} - Destination Location: {1}",
                    _souvenirQuestionStartingLocation, _souvenirQuestionEndingLocation, h + 1);
                BombModule.LogFormat("Rule used to Look up the Maze = {0}", GetRuleName(h == 1));
                BombModule.LogFormat("Playing Morse code word: \"{0}\"", MorseCodeWords[_rules[(h == 1) ? 1 : 0]]);

                if (_unicorn)
                {
                    BombModule.LogFormat("Bob will actively prevent you from getting any strikes.");
                }

                _firstGeneration = (h == 0);
            }
            else
            {
                BombModule.LogFormat("Updating the maze for rule {0}", _edgeworkRules[_rules[(h == 1) ? 1 : 0]]);
            }
            if(!MorseASwap)
                BombModule.LogFormat("Maze Solution from {0} to {1} in maze \"{2} - {3}\" is: {4}",
                    GetCoordinates(_currentLocation),
                    GetCoordinates(_destination), mazes[h], MorseCodeWords[mazes[h]], sb);
            else
                BombModule.LogFormat("Maze {5} Solution from {0} to {1} in maze \"{2} - {3}\" is: {4}",
                    GetCoordinates(_currentLocation),
                    GetCoordinates(_destination), "Redacted", "Redacted", sb, h+1);
        }
        
        return sb.ToString();
    }

    private string GetRuleName(bool swapped)
    {
        if (_edgeworkRules[_rules[swapped ? 1 : 0]] == EdgeworkRules.TwoFactor)
        {
            return BombInfo.IsTwoFactorPresent() ? "TwoFactor" : "UnsolvedModules";
        }
        return _edgeworkRules[_rules[swapped ? 1 : 0]].ToString();
    }
#endregion

    private int _lastStrikes = -1;
    private int _lastSolved = -1;
    private int _lastTwoFactorSum = -1;
    private int SetTwoFactor()
    {
        if (BombInfo.GetTwoFactorCounts() == 0)
        {
            _lastSolved = BombInfo.GetSolvedModuleNames().Count;
            return BombInfo.GetSolvableModuleNames().Count - _lastSolved;
        }
        else
        {
            var sum = BombInfo.GetTwoFactorCodes().Select(twofactor => twofactor / 10).Select(code => code % 10).Sum();

            if (sum != _lastTwoFactorSum)
            {
                foreach (var twofactor in BombInfo.GetTwoFactorCodes())
                {
                    BombModule.LogFormat("Two Factor code: ", twofactor);
                }
                _lastTwoFactorSum = sum;
            }
            return sum;
        }
    }

    private IEnumerator PlayWordLocation(string[] words)
    {
        int i = 0;
        while (!_solved)
        {
            var playword = FakeStatusLight.PlayWord(words[i]);
            i = (i + 1) % words.Length;

            while (playword.MoveNext() && !_solved)
            {
                yield return playword.Current;
            }
        }
    }

#region TwitchPlays
    private string TwitchPlaysChangeColors(StatusLightState pass, StatusLightState strike, StatusLightState tx, StatusLightState off)
    {
        if (!_modSettings.Settings.AllowTwitchPlaysMorseCodeColorChange)
            return "sendtochaterror Sorry, changing of the morse code transmission colors has been globally disabled in the settings.";

        FakeStatusLight.PassColor = pass;
        FakeStatusLight.MorseTransmitColor = tx;
        FakeStatusLight.OffColor = off;
        FakeStatusLight.FailColor = strike;
        return string.Empty;
    }

    private string TwitchPlaysChangeColors(StatusLightState pass)
    {
        if (!_modSettings.Settings.AllowTwitchPlaysStatusLightColorChange)
            return "sendtochaterror Sorry, changing of the solved state color has been globally disabled in the settings.";

        FakeStatusLight.PassColor = pass;
        return  string.Empty;
    }

    private IEnumerator ForceSolve()
    {
        while (!_solved)
        {
            bool swapped = Swapped;
            int maze = _mazes[swapped ? 1 : 0];
            yield return null;
            IEnumerator runMaze = ProcessTwitchCommand(GetMazeSolution(_mazes, Swapped));
            while (Swapped == swapped && maze == _mazes[swapped ? 1 : 0] && !_solved && runMaze.MoveNext())
                yield return runMaze.Current;
        }
    }

    [RummageNoRename]
    private void TwitchHandleForcedSolve()
    {
        _souvenirQuestionEndingLocation = null;
        _souvenirQuestionStartingLocation = null;
        _souvenirQuestionWordsPlaying = new string [] {null, null};
        _unicorn = true;
        StartCoroutine(ForceSolve());
    }

#pragma warning disable 414
    [RummageNoRename]
    private string TwitchHelpMessage = "!{0} move up down left right, !{0} move udlr [make a series of status light moves]. Use !{0} colorcommands to see color changing commands. Movement will automatically stop if the maze changes part way through the command.";
    // ReSharper restore InconsistentNaming
#pragma warning restore 414

    [RummageNoRename]
    private IEnumerator ProcessTwitchCommand(string command)
    {
        var originalCommand = command;
        command = command.ToLowerInvariant();
        string commandUsed = null;
        int maze = _mazes[Swapped ? 1 : 0];

        if (Application.isEditor && command.Equals("solve"))
        {
            TwitchHandleForcedSolve();
            yield break;
        }

        if (command.Equals("colorcommands"))
        {
            yield return @"sendtochat You can change which colors I flash the message with using ""UseDefaultColors"", ""UseEasyColors"", or ""UseCruelColors"" anywhere in the command. You can also change how I present the solve state with ""UseGreenOnSolve"", ""UseRedOnSolve"", ""UseOffOnSolve"" or ""UseRandomOnSolve"".";
            yield break;
        }

        if (command.Contains("usedefaultcolors"))
        {
            command = command.Replace("usedefaultcolors", "").Trim();
            commandUsed = TwitchPlaysChangeColors(StatusLightState.Off, StatusLightState.Off, StatusLightState.Red, StatusLightState.Green);
        }
        else if (command.Contains("useeasycolors"))
        {
            command = command.Replace("useeasycolors", "").Trim();
            commandUsed = TwitchPlaysChangeColors(StatusLightState.Green, StatusLightState.Red, StatusLightState.Green, StatusLightState.Off);
        }
        else if (command.Contains("usecruelcolors"))
        {
            command = command.Replace("usecruelcolors", "").Trim();
            commandUsed = TwitchPlaysChangeColors(StatusLightState.Random, StatusLightState.Random, StatusLightState.Random, StatusLightState.Random);
        }

        if (!string.IsNullOrEmpty(commandUsed))
        {
            yield return commandUsed;
            yield break;
        }

        if (command.Contains("useredonsolve"))
        {
            command = command.Replace("useredonsolve", "").Trim();
            commandUsed = TwitchPlaysChangeColors(StatusLightState.Red);
        }
        else if (command.Contains("usegreenonsolve"))
        {
            command = command.Replace("usegreenonsolve", "").Trim();
            commandUsed = TwitchPlaysChangeColors(StatusLightState.Green);
        }
        else if (command.Contains("useoffonsolve"))
        {
            command = command.Replace("useoffonsolve", "").Trim();
            commandUsed = TwitchPlaysChangeColors(StatusLightState.Off);
        }
        else if (command.Contains("userandomonsolve"))
        {
            command = command.Replace("userandomonsolve", "").Trim();
            commandUsed = TwitchPlaysChangeColors(StatusLightState.Random);
        }

        if (!string.IsNullOrEmpty(commandUsed))
        {
            yield return commandUsed;
            yield break;
        }

        /*
        if (command.Contains("forceunicorn"))
        {
            command = command.Replace("forceunicorn", "").Trim();
            commandUsed = true;
            _unicorn = true;
        }*/


        if (command.StartsWith("move ", StringComparison.InvariantCultureIgnoreCase))
        {
            command = command.Substring(5);
        }

        command = command.Replace("north", " u ").Replace("south", " d ").Replace("west", " l ").Replace("east", " r ");
        command = command.Replace("up", " u ").Replace("down", " d ").Replace("left", " l ").Replace("right", " r ");
        

        if (_solved)
        {
            yield return "sendtochat I don't need any further directions on how to reach the exit.";
            yield break;
        }
        
        MatchCollection matches = Regex.Matches(command, @"[udlr]", RegexOptions.IgnoreCase);
        if (matches.Count == 0)
        {
            if (commandUsed != null)
            {
                yield return string.Format("sendtochat I have changed my colors as specified by {0} successfully.", originalCommand);
            }
            else
            {
                yield return "sendtochaterror please tell me where the exit is without running me into walls.";
            }
            yield break;
        }

        foreach (char c in command)
        {
            if (!"udlr ,;".Contains(c))
            {
                yield return string.Format("sendtochaterror I don't know how to move in the '{0}' direction.", c);
            }
        }

        yield return null;
        if (matches.Count > 35 || _movements.Processing)
        {
            yield return "elevator music";
        }

        while (_movements.Processing)
        {
            yield return "trywaitcancel 0.1";
        }
        if (matches.Count <= 35)
        {
            yield return "end elevator music";
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
                    if (!safe) yield return "strikemessage running me into a wall north of " + _currentLocation.parent.name + _currentLocation.name;
                    break;
                case "d":
                    safe = MoveDown();
                    if (!safe) yield return "strikemessage running me into a wall south of " + _currentLocation.parent.name + _currentLocation.name;
                    break;
                case "l":
                    safe = MoveLeft();
                    if (!safe) yield return "strikemessage running me into a wall west of " + _currentLocation.parent.name + _currentLocation.name;
                    break;
                case "r":
                    safe = MoveRight();
                    if (!safe) yield return "strikemessage running me into a wall east of " + _currentLocation.parent.name + _currentLocation.name;
                    break;
                default:
                    continue;
            }
            

            if (_solved)
            {
                yield return "solve";
                yield break;
            }
            yield return "trycancel";
            yield return new WaitUntil(() => _movements.Processing);
            yield return new WaitUntil(() => !_movements.Processing);

            if (maze != _mazes[Swapped ? 1 : 0])
            {
                yield break;
            }
            
        }
        if(moved) yield return "solve";
    }
    #endregion

    // ReSharper disable once UnusedMember.Local

    [RummageNoRename]
    [RummageNoMarkPublic]
    private void Update ()
    {
        if (_rules[0] == -1 || _rules[1] == -1) return;
        if (_movements == null)
        {
            StartCoroutine(InstantlySolveModule("Module failed to Initialize"));
            _movements = new CoroutineQueue();
        }
        if (_movements.Processing) return;
        if (_solved) return;
        
	    // ReSharper disable once SwitchStatementMissingSomeCases
        for(int i = 0; i < 2; i++)
        {
            switch (_edgeworkRules[_rules[i]])
            {
                case EdgeworkRules.SolveCount:
                    if (_lastSolved != BombInfo.GetSolvedModuleNames().Count)
                    {
                        _lastSolved = BombInfo.GetSolvedModuleNames().Count;
                        GetMazeSolution(_mazes, i == 1);
                        BombModule.LogFormat("Maze {i} updated for {0} modules Solved", _lastSolved, i + 1);

                    }
                    break;
                case EdgeworkRules.TwoFactor:
                    if (_lastSolved != -1 && _lastSolved != BombInfo.GetSolvedModuleNames().Count)
                    {
                        BombModule.LogFormat("Maze {1} updated for {0} modules Unsolved", BombInfo.GetSolvableModuleNames().Count - _lastSolved, i+1);
                        _mazes[i] = SetTwoFactor();
                        GetMazeSolution(_mazes, i == 1);
                    }
                    if (_lastTwoFactorSum != -1 && _lastTwoFactorSum !=
                        BombInfo.GetTwoFactorCodes().Select(twofactor => twofactor / 10).Select(code => code % 10).Sum())
                    {
                        BombModule.LogFormat("Maze {1} updated for Two Factor 2nd least significant digit sum of {0}", _lastTwoFactorSum, i+1);
                        _mazes[i] = SetTwoFactor();
                        GetMazeSolution(_mazes, i == 1);
                    }
                    break;
                case EdgeworkRules.Strikes:
                    if (BombInfo.GetStrikes() != _lastStrikes)
                    {
                        _lastStrikes = BombInfo.GetStrikes();
                        GetMazeSolution(_mazes, i == 1);
                        BombModule.LogFormat("Maze {1} updated for {0} strikes", _lastStrikes, i+1);
                    }
                    break;
            }
        }
    }
}