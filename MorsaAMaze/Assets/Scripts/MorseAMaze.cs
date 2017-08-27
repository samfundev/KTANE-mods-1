using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

    private string _souvenirQuestionStartingLocation;
    private string _souvenirQuestionEndingLocation;
    private string _souvenirQuestionWordPlaying;

    private ModSettings _modSettings;

    private readonly string[,,] _mazes =
    {
        {{"rd","lr","ld","rd","lr","l"},{"ud","rd","ul","ur","lr","ld"},{"ud","ur","ld","rd","lr","uld"},{"ud","r","ulr","lu","r","uld"},{"urd","lr","ld","rd","l","ud"},{"ur","l","ur","ul","r","ul"}},
        {{"r","lrd","l","rd","lrd","l"},{"rd","ul","rd","ul","ur","ld"},{"ud","rd","ul","rd","lr","uld"},{"urd","ul","rd","ul","d","ud"},{"ud","d","ud","rd","ul","ud"},{"u","ur","ul","ur","lr","lu"}},
        {{"dr","lr","ld","d","dr","dl"},{"u","d","ud","ur","lu","ud"},{"dr","uld","ud","rd","ld","ud"},{"ud","ud","ud","ud","ud","ud"},{"ud","ur","ul","ud","ud","ud"},{"ur","lr","lr","ul","ur","ul"}},
        {{"rd","ld","r","lr","lr","ld"},{"ud","ud","dr","lr","lr","uld"},{"ud","ur","lu","rd","l","ud"},{"ud","r","lr","lru","lr","lud"},{"udr","lr","lr","lr","ld","ud"},{"ur","lr","l","r","ul","u"}},
        {{"r","lr","lr","lr","lrd","ld"},{"rd","lr","lr","lrd","lu","u"},{"udr","ld","r","ul","rd","ld"},{"ud","ur","lr","ld","u","ud"},{"ud","rd","lr","ulr","l","ud"},{"u","ur","lr","lr","lr","lu"}},
        {{"d","dr","ld","r","ldr","ld"},{"ud","ud","ud","rd","ul","ud"},{"udr","ul","u","ud","rd","ul"},{"ur","ld","dr","udl","ud","d"},{"rd","ul","u","ud","ur","uld"},{"ur","lr","lr","ul","r","ul"}},
        {{"dr","lr","lr","ld","dr","ld"},{"ud","rd","l","ur","lu","ud"},{"ur","ul","rd","l","rd","ul"},{"dr","ld","udr","lr","ul","d"},{"ud","u","ur","lr","ld","ud"},{"ur","lr","lr","lr","ulr","ul"}},
        {{"d","dr","lr","ld","dr","ld"},{"udr","ulr","l","ur","ul","ud"},{"ud","dr","lr","lr","ld","ud"},{"ud","ur","ld","r","ulr","ul"},{"ud","d","ur","lr","lr","l"},{"ur","ulr","lr","lr","lr","l"}},
        {{"d","dr","lr","lr","ldr","ld"},{"ud","ud","rd","l","ud","ud"},{"udr","ulr","ul","rd","ul","ud"},{"ud","d","dr","ul","r","uld"},{"ud","ud","ud","dr","dl","u"},{"ur","ul","ur","ul","ur","l"}},
        {{"d","rd","ld","r","ldr","l"},{"ud","u","ur","ld","ur","ld"},{"urd","lr","l","ud","rd","uld"},{"ud","rd","ld","ud","ud","ud"},{"ud","ud","ud","ur","ul","ud"},{"ur","ul","ur","lr","lr","ul"}},
        {{"rd","lr","ld","d","r","ld"},{"ur","ld","ud","ur","lr","uld"},{"rd","ul","ur","lr","l","ud"},{"ud","rd","ld","rd","lr","ul"},{"urd","ul","ud","ur","ld","d"},{"ur","l","ur","lr","ulr","ul"}},
        {{"rd","rl","rl","rl","rl","ld"},{"ur","rl","rld","ld","r","uld"},{"rd","ld","u","ur","ld","u"},{"ud","ur","lr","lrd","lu","d"},{"ud","r","lr","lu","rd","lu"},{"ur","lr","lr","lr","ulr","l"}},
        {{"rd","lr","lr","lrd","lr","ld"},{"ur","lr","ld","ud","d","ud"},{"r","ld","ud","ud","ud","ud"},{"d","ud","u","ud","ud","ud"},{"ud","ud","rd","lu","urd","lu"},{"ur","lur","lu","r","lur","l"}},
        {{"rd","lr","lr","lr","lr","ld"},{"ur","lr","ld","r","ld","ud"},{"d","rd","lur","lr","ul","ud"},{"ur","ul","rd","lr","lr","uld"},{"rd","ld","ud","rd","lr","ul"},{"u","ur","ul","ur","lr","l"}},
        {{"rd","lr","ldr","lr","ld","d"},{"ur","ld","ud","r","lur","uld"},{"d","ud","ur","lr","ld","ud"},{"urd","lur","l","rd","ul","ud"},{"ur","ld","rd","ul","rd","ul"},{"r","ul","u","r","lur","l"}},
        {{"rd","lr","ld","r","lrd","ld"},{"ud","r","lur","lr","lu","ud"},{"urd","lr","ld","rd","lr","ul"},{"ur","ld","ud","u","rd","ld"},{"rd","lu","ud","r","lu","ud"},{"ur","l","ur","lr","lr","lu"}},
        {{"rd","ld","rd","lrd","ld","d"},{"ud","ur","lu","ud","ud","ud"},{"ur","lr","ld","ud","ur","uld"},{"d","rd","lu","ur","ld","u"},{"ud","ud","rd","ld","ud","d"},{"ur","lur","lu","u","ur","lu"}},
        {{"rd","lr","lr","ldr","lr","ld"},{"u","rd","lr","lu","rd","lu"},{"d","ur","lr","ld","ud","d"},{"ur","ld","rd","lu","ur","uld"},{"rd","lu","ud","d","rd","lu"},{"ur","lr","lu","ur","lur","l"}}
    };

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




    // Use this for initialization
    // ReSharper disable once UnusedMember.Local
    private void Start ()
    {
        _modSettings = new ModSettings(BombModule);
        _modSettings.ReadSettings();
        _movements = gameObject.AddComponent<CoroutineQueue>();
        BombModule.GenerateLogFriendlyName();
	    Locations.Shuffle();
        SetMaze(0); //Hide the walls now.
	    
        _currentLocation = Locations[0];
	    _destination = Locations[1];
        StartCoroutine(MoveStatusLightToStart());


        BombModule.OnActivate += Activate;
	    FakeStatusLight = Instantiate(FakeStatusLight);

        if (BombModule != null)
            FakeStatusLight.Module = BombModule;

        FakeStatusLight.PassColor = _modSettings.Settings.SolvedState;
        FakeStatusLight.FailColor = _modSettings.Settings.StrikeState;
        FakeStatusLight.OffColor = _modSettings.Settings.OffState;
        FakeStatusLight.MorseTransmitColor = _modSettings.Settings.MorseOn;
        FakeStatusLight.GetStatusLights(StatusLight);
        FakeStatusLight.SetInActive();



    }

    private Vector3 _originalStatusLightLocation;
    private float _statusLightMoveFrames;
    private IEnumerator MoveStatusLightToStart()
    {
        yield return null; yield return null; yield return null;

        _originalStatusLightLocation = StatusLight.localPosition;
        _statusLightMoveFrames = 1f / Time.deltaTime;
        var move = _originalStatusLightLocation.y / _statusLightMoveFrames;
        for (var i = 0; i < (_statusLightMoveFrames*2); i++)
        {
            StatusLight.localPosition = new Vector3(StatusLight.localPosition.x, StatusLight.localPosition.y - move,
                StatusLight.localPosition.z);
            yield return null;
        }

        StatusLight.localPosition = new Vector3(_currentLocation.parent.localPosition.x, StatusLight.localPosition.y,
            _currentLocation.localPosition.z);
        yield return null;

        for (var i = 0; i < (_statusLightMoveFrames * 2); i++)
        {
            StatusLight.localPosition = new Vector3(StatusLight.localPosition.x, StatusLight.localPosition.y + move,
                StatusLight.localPosition.z);
            yield return null;
        }
    }

    private IEnumerator MoveStatusLightToCorner()
    {
        var move = _originalStatusLightLocation.y / _statusLightMoveFrames;
        for (var i = 0; i < (_statusLightMoveFrames * 2); i++)
        {
            StatusLight.localPosition = new Vector3(StatusLight.localPosition.x, StatusLight.localPosition.y - move,
                StatusLight.localPosition.z);
            yield return null;
        }

        StatusLight.localPosition = new Vector3(StatusLightCorner.localPosition.x, StatusLight.localPosition.y,
            StatusLightCorner.localPosition.z);
        yield return null;

        for (var i = 0; i < (_statusLightMoveFrames * 2); i++)
        {
            StatusLight.localPosition = new Vector3(StatusLight.localPosition.x, StatusLight.localPosition.y + move,
                StatusLight.localPosition.z);
            yield return null;
        }
        
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
            var wall = VerticalWalls.FindChild(number).FindChild(letters);
            if (wall == null) return;
            wall.gameObject.SetActive(active);
            StartCoroutine(HideWall(wall.gameObject.GetComponent<MeshRenderer>()));
        }
        else
        {
            var wall = HorizontalWalls.FindChild(letter).FindChild(numbers);
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
                SetWall(x, y, false, !_mazes[maze, y, x].Contains("d"));
                SetWall(x, y, true, !_mazes[maze, y, x].Contains("r"));
            }
        }
    }


    private Transform CheckHorizontalWall(string x, string y)
    {
        var hwall = HorizontalWalls.FindChild(x);
        var wall = hwall.FindChild(y);
        
        return wall.gameObject.activeSelf ? wall : null;
    }

    private Transform CheckVerticalWalls(string x, string y)
    {
        var hwall = VerticalWalls.FindChild(x);
        var wall = hwall.FindChild(y);

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
        FakeStatusLight.HandleStrike();
        Audio.HandlePlaySoundAtTransform(GlassBreak.name, transform);
        for (var i = 0; i < hitWall; i++)
        {
            StatusLight.localPosition -= movement;
            yield return null;
        }
        
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator MoveToLocation(Transform location, Transform from)
    {
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
            } while (Mathf.Abs(sy - ly) > Mathf.Abs(movey));
            sy = ly;
        }
        StatusLight.transform.localPosition = new Vector3(sx, sz, sy);

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

        if (_unicorn)
        {
            StartCoroutine(ShowWall(wall));
            yield break;
        }

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
        if (_solved) return true;
        const string x = "ABCDEF";
        var location = _currentLocation;
        if (location.parent.name == "A")
            return true;
        var destination = x.Substring(x.IndexOf(location.parent.name, StringComparison.Ordinal) - 1, 1);
        var wall = destination + location.parent.name;
        var loc = location.parent.parent.FindChild(destination).FindChild(location.name);

        return ProcessMove(CheckVerticalWalls(location.name, wall), loc);
    }

    private bool MoveRight()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BombModule.transform);
        if (_solved) return true;
        const string x = "ABCDEF";
        var location = _currentLocation;
        if (location.parent.name == "F")
            return true;
        var destination = x.Substring(x.IndexOf(location.parent.name, StringComparison.Ordinal) + 1, 1);
        var wall = location.parent.name + destination;
        var loc = location.parent.parent.FindChild(destination).FindChild(location.name);

        return ProcessMove(CheckVerticalWalls(location.name, wall), loc);
    }


    private bool MoveUp()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BombModule.transform);
        if (_solved)
            return true;
        var location = _currentLocation;
        if (location.name == "1")
            return true;
        var destination = (int.Parse(location.name) - 1).ToString();
        var wall = destination + location.name;
        var loc = location.parent.FindChild(destination);

        return ProcessMove(CheckHorizontalWall(location.parent.name, wall), loc);
    }

    private bool MoveDown()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, BombModule.transform);
        if (_solved) return true;
        var location = _currentLocation;
        if (location.name == "6")
            return true;
        var destination = (int.Parse(location.name) + 1).ToString();
        var wall = location.name + destination;
        var loc = location.parent.FindChild(destination);

        return ProcessMove(CheckHorizontalWall(location.parent.name, wall), loc);
    }
    #endregion

    #region Activate()
    private void Activate()
    {
        Up.OnInteract += delegate { MoveUp(); return false; };
        Down.OnInteract += delegate { MoveDown(); return false; };
        Left.OnInteract += delegate { MoveLeft(); return false; };
        Right.OnInteract += delegate { MoveRight(); return false; };
 
        _rule = Random.Range(0, _morseCodeWords.Length);
        _unicorn = BombInfo.IsIndicatorOff("BOB") && BombInfo.GetBatteryHolderCount(2) == 1 && BombInfo.GetBatteryHolderCount(1) == 2 && BombInfo.GetBatteryHolderCount() == 3;
        _souvenirQuestionWordPlaying = !_unicorn ? _morseCodeWords[_rule] : "Thank you BOB";

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
        var directions = _mazes[maze, y, x];
        if (startXY == endXY) return true;
        _explored[startXY] = true;


        var directionLetter = new[] { "u", "d", "l", "r" };
        var directionInt = new[] { -10, 10, -1, 1 };
        var directionReturn = new[] { "Up", "Down", "Left", "Right" };

        for (var i = 0; i < 4; i++)
        {
            if (!directions.Contains(directionLetter[i])) continue;
            if (_explored[startXY + directionInt[i]]) continue;
            if (!GenerateMazeSolution(maze, startXY + directionInt[i])) continue;
            _mazeStack.Push(directionReturn[i]);
            return true;
        }

        return false;
    }


    private bool _firstGeneration = true;
    private void GetMazeSolution(int maze)
    {
        maze %= 18;
        SetMaze(maze);
        

        var directions = new List<string>();
        StringBuilder sb;
        int moveLength;
        var locationNumber = 1;
        bool success;
        do
        {
            directions.Clear();
            sb = new StringBuilder();
            if (_firstGeneration)
            {
                _destination = Locations[locationNumber++];
            }
            success = GenerateMazeSolution(maze, 77);
            moveLength = _mazeStack.Count;
            if (!success)
            {
                BombModule.LogFormat("Failed to Generate the maze solution for going from {0} to {1} in maze {2}",
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
        } while (_firstGeneration && (moveLength < 3 || directions.Count < 2) && (locationNumber < Locations.Length));
        if (!success) return;

        if (_firstGeneration)
        {
            _souvenirQuestionStartingLocation = GetCoordinates(_currentLocation);
            _souvenirQuestionEndingLocation = GetCoordinates(_destination);

            BombModule.LogFormat("Starting Location: {0} - Destination Location: {1}",
                _souvenirQuestionStartingLocation, _souvenirQuestionEndingLocation);
            BombModule.LogFormat("Rule used to Look up the Maze = {0}", _edgeworkRules[_rule]);

            if (_unicorn)
            {
                BombModule.LogFormat("Playing Morse code word: \"Thank you BOB\"");
                BombModule.LogFormat("Bob came and painted all of the walls, making them visible.");
            }
            else
            {
                BombModule.LogFormat("Playing Morse code word: \"{0}\"", _morseCodeWords[_rule]);
            }

            _firstGeneration = false;
        }
        else
        {
            BombModule.LogFormat("Updating the maze for rule {0}",_edgeworkRules[_rule]);
        }
        BombModule.LogFormat("Maze Solution from {0} to {1} in maze \"{2} - {3}\" is: {4}", GetCoordinates(_currentLocation),
            GetCoordinates(_destination), maze, _morseCodeWords[maze], sb);
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
    public string TwitchManualCode = "Morse-A-Maze";
    public string TwitchHelpMessage = "!{0} move up down left right, !{0} move udlr [make a series of status light moves]";
    public string[] TwitchValidCommands = {"move .*"};
    public IEnumerator ProcessTwitchCommand(string command)
    {
        if (!command.StartsWith("move ", StringComparison.InvariantCultureIgnoreCase))
            yield break;

        yield return null;
        command = command.Substring(5);

        while (_movements.Processing)
        {
            yield return "trycancel";
            yield return new WaitForSeconds(0.1f);
        }

        foreach (Match move in Regex.Matches(command, @"[udlr]", RegexOptions.IgnoreCase))
        {
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
                yield return "strike";
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
    }
    #endregion

    // ReSharper disable once UnusedMember.Local
    private void Update ()
    {
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