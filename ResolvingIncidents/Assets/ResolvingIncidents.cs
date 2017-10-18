using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rnd = UnityEngine.Random;

[Serializable]
public class Character
{
    public string Name;
    public bool Heroine;
    public int DistanceBonus;
    public int RangeBonus;
    public Incidents ForbiddenIncident = Incidents.None;
}

[Serializable]
public class Inventory
{
    public string Item;
    public int Distance;
    public int Range;
}


public class ResolvingIncidents : MonoBehaviour
{

    #region Public Variables
    public GameObject OnLED;
    public GameObject OffLED;
    public KMSelectable InventoryLeft;
    public KMSelectable InventoryRight;
    public KMSelectable WinButton;
    public KMSelectable LoseButton;
    public KMSelectable DrawButton;

    public TextMesh CharacterText;
    public TextMesh IncidentText;
    public TextMesh InventoryText;

    public KMBombModule BombModule;
    public KMAudio Audio;
    public KMBombInfo BombInfo;
    #endregion

    #region Prviate Variables
    private bool _solved = false;
    private bool _activated = false;

    private const int WINSTOSOLVE = 1;
    private const int LOSSESTOSOLVE = 2;
    private int _wins = 0;
    private int _losses = 0;

    private IncidentResult _correctIncidentResultEvenStrike;
    private IncidentResult _correctIncidentResultOddStrike;
    

    

    private const int MAXINVENTORYITEMS = 5;
    private List<Inventory> _inventory;
    private int _currentInventoryItem;
    private List<Character> _characters;
    private Incidents _incident;
    

    private Character _character;
    private CharacterSeasons _season;
    private bool _spellBonus;



    private Color[] _colorValues = Ext.NewArray(
            "3030F3",       //Winter
            "00FF21",       //Spring
            "C9CC21",       //Summer
            "CC0000"        //Fall
        )
        .Select(c => new Color(Convert.ToInt32(c.Substring(0, 2), 16) / 255f, Convert.ToInt32(c.Substring(2, 2), 16) / 255f, Convert.ToInt32(c.Substring(4, 2), 16) / 255f))
        .ToArray();


    private CharacterSeasons[] _seasons = (CharacterSeasons[]) Enum.GetValues(typeof(CharacterSeasons));
    private Incidents[] _incidents = (Incidents[])(Enum.GetValues(typeof(Incidents)));



    #endregion

    // Use this for initialization
    void Start ()
    {
        BombModule.GenerateLogFriendlyName();
        BombModule.OnActivate += OnActivate;
        
        _incident = _incidents[Rnd.Range(1, _incidents.Length)];

        _characters = new List<Character>
        {
            new Character {Name = "Crow Tengu",         DistanceBonus = 3, RangeBonus = 2, Heroine = true },
            new Character {Name = "Elegant Maid",       DistanceBonus = 3, RangeBonus = 3, Heroine = true },
            new Character {Name = "Gatekeeper",         DistanceBonus = 2, RangeBonus = 1, Heroine = false },
            new Character {Name = "Great Librarian",    DistanceBonus = 2, RangeBonus = 3, Heroine = false },
            new Character {Name = "Great Magician",     DistanceBonus = 4, RangeBonus = 5, Heroine = false },

            new Character {Name = "Green Shaman",       DistanceBonus = 2, RangeBonus = 3, Heroine = true, ForbiddenIncident = Incidents.EndlessParty },
            new Character {Name = "Heavenly Swordgirl", DistanceBonus = 5, RangeBonus = 5, Heroine = false, ForbiddenIncident = Incidents.CosmicWeather },
            new Character {Name = "Ice Fairy",          DistanceBonus = 1, RangeBonus = 2, Heroine = true, ForbiddenIncident = Incidents.FairyWars },
            new Character {Name = "Kappa",              DistanceBonus = 1, RangeBonus = 2, Heroine = false },
            new Character {Name = "Legendary Student",  DistanceBonus = 4, RangeBonus = 4, Heroine = false, ForbiddenIncident = Incidents.OccultInvasion },

            new Character {Name = "Lunar Doctor",       DistanceBonus = 1, RangeBonus = 3, Heroine = false, ForbiddenIncident = Incidents.OverdrivenNight },
            new Character {Name = "Moon Rabbit",        DistanceBonus = 2, RangeBonus = 2, Heroine = true },
            new Character {Name = "Ordinary Magician",  DistanceBonus = 2, RangeBonus = 4, Heroine = true },
            new Character {Name = "Phoenix",            DistanceBonus = 4, RangeBonus = 3, Heroine = false },
            new Character {Name = "Puppeteer",          DistanceBonus = 3, RangeBonus = 1, Heroine = false },

            new Character {Name = "Red Shaman",         DistanceBonus = 3, RangeBonus = 5, Heroine = true, ForbiddenIncident = Incidents.EndlessParty },
            new Character {Name = "Satori",             DistanceBonus = 1, RangeBonus = 2, Heroine = false },
            new Character {Name = "Scarlet Devil",      DistanceBonus = 2, RangeBonus = 1, Heroine = false, ForbiddenIncident = Incidents.ScarletMist },
            new Character {Name = "Scarlet Sister",     DistanceBonus = 3, RangeBonus = 1, Heroine = false, ForbiddenIncident = Incidents.ScarletMist },
            new Character {Name = "Unidentified Girl",  DistanceBonus = 5, RangeBonus = 4, Heroine = false, ForbiddenIncident = Incidents.UndefinedFantasticObject },
        };

        InventoryLeft.OnInteract += () => HandleInventory(true);
        InventoryRight.OnInteract += () => HandleInventory(false);
        WinButton.OnInteract += () => ResolveIncident(IncidentResult.Win);
        LoseButton.OnInteract += () => ResolveIncident(IncidentResult.Loss);
        DrawButton.OnInteract += () => ResolveIncident(IncidentResult.Draw);
        OffLED.SetActive(true);
        OnLED.SetActive(false);
        IncidentText.text = "";
        InventoryText.text = "";
        CharacterText.text = "";
    }

    private void Initialize()
    {
        int strikeCount = 0;
        if (_solved)
            return;
        if (_incident == Incidents.WorldlyDesires)
        {
            BombModule.LogFormat("Calculating the results for Even number of Strikes");
        }

        _inventory = new List<Inventory>()
        {
            new Inventory {Item = "Flower", Distance = -2, Range = 2},
            new Inventory {Item = "Glove", Distance = 2, Range = 2},
            new Inventory {Item = "Gohei", Distance = 4, Range = 5},
            new Inventory {Item = "Grimoire", Distance = -2, Range = 4},
            new Inventory {Item = "Hisou Sword", Distance = 2, Range = 3},
            new Inventory {Item = "Ice", Distance = 3, Range = 3},
            new Inventory {Item = "Jeweled Pagoda", Distance = 5, Range = 6},
            new Inventory {Item = "Keystone", Distance = 5, Range = 5},
            new Inventory {Item = "Lunar Cape", Distance = 4, Range = 4},
            new Inventory {Item = "Mini-Hakkero", Distance = -2, Range = 8},
            new Inventory {Item = "Miracle Mallet", Distance = 3, Range = 4},
            new Inventory {Item = "Nimble Cloth", Distance = 8, Range = 1},
            new Inventory {Item = "Rock", Distance = 1, Range = 3},
            new Inventory {Item = "Occult Orb", Distance = 4, Range = 7},
            new Inventory {Item = "Seal", Distance = 6, Range = 5},
            new Inventory {Item = "Soul Torch", Distance = 3, Range = 1},
            new Inventory {Item = "Sunflower", Distance = 1, Range = 2},
            new Inventory {Item = "Trident", Distance = 2, Range = 3},
            new Inventory {Item = "UFO", Distance = 5, Range = 5},
            new Inventory {Item = "Yin-Yang Orb", Distance = 3, Range = 6},
        }.OrderBy(x => Rnd.value).Take(MAXINVENTORYITEMS).ToList();

        _spellBonus = Rnd.value > 0.5f;
        _character = _characters.OrderBy(x => Rnd.value).First(y => y.ForbiddenIncident != _incident);
        _season = _seasons[Rnd.Range(0, _seasons.Length)];

        OffLED.SetActive(!_spellBonus);
        OnLED.SetActive(_spellBonus);

        CharacterText.text = _character.Name;
        CharacterText.color = _colorValues[(int) _season];
        InventoryText.text = _inventory[_currentInventoryItem].Item;

        

        OddStrikes:
        if (strikeCount == 1)
        {
            BombModule.LogFormat("Calculating the results for Odd number of Strikes");
        }

        int CharacterDistance = BombInfo.GetBatteryCount() == 0 ? 3 : (BombInfo.GetBatteryCount() % 5);
        int CharacterRange = BombInfo.GetBatteryHolderCount() == 0 ? 3 : (BombInfo.GetBatteryHolderCount() % 5);

        BombModule.LogFormat("Chararcter: {0} - Distance = {1}, Range = {2}, Season = {3}", _character.Name, CharacterDistance, CharacterRange, _season);

        if (_character.Heroine)
        {
            CharacterDistance++;
            CharacterRange++;
            BombModule.LogFormat("Character is a Heroine, Gets a bonus of 1/1 to Distance/Range");
        }

        if (_spellBonus)
        {
            CharacterDistance += _character.DistanceBonus;
            CharacterRange += _character.RangeBonus;
            BombModule.LogFormat("Spell Bonus is Active. Character gets a bonus of {0} to Distance and {1} to Range", _character.DistanceBonus, _character.RangeBonus);
        }

        int BossDistance;
        int BossRange;
 
        switch (_incident)
        {
            case Incidents.CosmicWeather:
                IncidentText.text = "Cosmic Weather";
                BossDistance = (_season == CharacterSeasons.Spring || _season == CharacterSeasons.Summer) ? 9 : 7;
                BossDistance += BombInfo.GetPortPlateCount();
                BossRange = (_season == CharacterSeasons.Spring || _season == CharacterSeasons.Summer) ? 10 : 9;
                BombModule.LogFormat("Fighting Heavenly Swordgirl - Distance = 7, Range = 9");
                if (_season == CharacterSeasons.Spring || _season == CharacterSeasons.Summer)
                {
                    BombModule.LogFormat("Boss has a Distance/Range advantage of 2/1 based on season");
                }
                if (BombInfo.GetPortPlateCount() > 0)
                    BombModule.LogFormat("Boss has a Distance advantage of {0} based on port plate count", BombInfo.GetPortPlateCount());
                break;
            case Incidents.EndlessParty:
                IncidentText.text = "Endless Party";
                int SakeCount = BombInfo.GetSerialNumberLetters().Count(c => "SAKE".Contains(c.ToString().ToUpperInvariant()));
                if (BombInfo.CountUniquePorts() >= 3)
                {
                    BombModule.LogFormat("Fighting Red Shaman - Distance = 8, Range = 7");
                    BossDistance = 8;
                    BossRange = 7 + SakeCount;
                    if (SakeCount > 0)
                        BombModule.LogFormat("Boss has a Range advantage of {0} based on serial number contain the letters of S, A, K, or E", SakeCount);
                }
                else
                {
                    BombModule.LogFormat("Fighting Red Shaman - Distance = 7, Range = 8");
                    BossDistance = 7 + SakeCount;
                    BossRange = 8;
                    if (SakeCount > 0)
                        BombModule.LogFormat("Boss has a Distance advantage of {0} based on serial number contain the letters of S, A, K, or E", SakeCount);
                }
                break;
            case Incidents.FairyWars:
                IncidentText.text = "Fairy Wars";
                int FairyBonus = BombInfo.CountUniquePorts() / 2;
                if (_season == CharacterSeasons.Fall || _season == CharacterSeasons.Winter)
                {
                    BombModule.LogFormat("Fighting Ice Fairy - Distance = 7, Range = 8");
                    BossDistance = 7;
                    BossRange = 8 + FairyBonus;
                    if (_season == CharacterSeasons.Winter)
                    {
                        BombModule.LogFormat("Boss has a Distance/Range advantage of 1/1 based on season");
                        BossDistance++;
                        BossRange++;
                    }
                }
                else
                {
                    BombModule.LogFormat("Fighting Sunflower Fairy - Distance = 8, Range = 9");
                    BossDistance = 8;
                    BossRange = 9 + FairyBonus;
                    if (_season == CharacterSeasons.Summer)
                    {
                        BombModule.LogFormat("Boss has a Distance advantage of 1 based on season");
                        BossDistance++;
                    }
                }
                if (FairyBonus > 0)
                    BombModule.LogFormat("Boss has a Range advantage of {0} based on unique ports divided by two", FairyBonus);
                break;
            case Incidents.LilyBlackandWhite:
                IncidentText.text = "Lily Black and White";
                if (_season == CharacterSeasons.Winter || _season == CharacterSeasons.Spring)
                {
                    BombModule.LogFormat("Fighting Lily White - Distance = 10, Range = 11");
                    BossDistance = 10;
                    BossRange = 11;
                }
                else
                {
                    BombModule.LogFormat("Fighting Lily Black - Distance = 10, Range = 9");
                    BossDistance = 10;
                    BossRange = 9;
                }
                if (_season == CharacterSeasons.Spring || _season == CharacterSeasons.Summer)
                {
                    BombModule.LogFormat("Boss has a Distance/Range advantage of 1/1 based on season");
                    BossDistance++;
                    BossRange++;
                }
                break;
            case Incidents.LunarWar:
                IncidentText.text = "Lunar War";
                if (BombInfo.CountDuplicatePorts() == 1)
                {
                    BombModule.LogFormat("Fighting Lunar Queen - Distance = 10, Range = 9");
                    BossDistance = 10;
                    BossRange = 9;
                }
                else
                {
                    BombModule.LogFormat("Fighting Lunar King - Distance = 9, Range = 10");
                    BossDistance = 9;
                    BossRange = 10;
                }
                if (BombInfo.GetSerialNumberNumbers().Sum() >= 9)
                {
                    BombModule.LogFormat("Boss has a Distance/Range advantage of 1/1 based on serial number digit sum");
                    BossDistance++;
                    BossRange++;
                }
                break;
            case Incidents.OccultInvasion:
                IncidentText.text = "Occult Invasion";
                int LegendaryBonus = BombInfo.GetIndicators().Count(x => x.ToCharArray().Any(y => "URBAN LEGEND".Contains(y.ToString().ToUpperInvariant()))) / 2;
                BombModule.LogFormat("Fighting Legendary Student - Distance = 9, Range = 10");
                BossDistance = 9 + LegendaryBonus;
                BossRange = 10;
                if (_season == CharacterSeasons.Spring || _season == CharacterSeasons.Summer)
                {
                    BombModule.LogFormat("Boss has a Distance/Range advantage of 1/1 based on season");
                    BossDistance++;
                    BossRange++;
                }
                if (LegendaryBonus > 0)
                {
                    BombModule.LogFormat("Boss has a Distance advantage of 1 based on every two indicators containing letters of \"URBAN LEGEND\"");
                }
                break;
            case Incidents.OverdrivenNight:
                IncidentText.text = "Overdriven Night";
                if (_season == CharacterSeasons.Fall || _season == CharacterSeasons.Winter)
                {
                    BombModule.LogFormat("Fighting Lunar Doctor - Distance = 11, Range = 8");
                    BossDistance = 11;
                    BossRange = 8;
                }
                else
                {
                    BombModule.LogFormat("Fighting Moon Rabbit - Distance = 8, Range = 9");
                    BossDistance = 8;
                    BossRange = 9;
                }
                if (BombInfo.GetOffIndicators().Any())
                {
                    BombModule.LogFormat("Boss has a Distance advantage of {0} based on the number of unlit Indicators", BombInfo.GetOffIndicators().Count());
                    BossRange += BombInfo.GetOffIndicators().Count();
                }
                break;
            case Incidents.UndefinedFantasticObject:
                IncidentText.text = "Undefined Fastastic Object";
                int undefineduniqueports = BombInfo.CountUniquePorts();
                if (BombInfo.GetOffIndicators().Count() > BombInfo.GetOnIndicators().Count())
                {
                    BombModule.LogFormat("Fighting Undefined Girl - Distance = 7, Range = 7");
                    BossDistance = 7;
                    BossRange = 7;
                    if (undefineduniqueports > 0)
                    {
                        BombModule.LogFormat("Boss has a Distance/Range advantage of {0}/{0} based on the number of unique port types", undefineduniqueports);
                        BossDistance += undefineduniqueports;
                        BossRange += undefineduniqueports;
                    }
                }
                else
                {
                    BombModule.LogFormat("Fighting Fantastic Girl - Distance = 9, Range = 8");
                    BossDistance = 9;
                    BossRange = 8;
                    if (undefineduniqueports > 0)
                    {
                        BombModule.LogFormat("Boss has a Range advantage of {0} based on the number of unique port types", undefineduniqueports);
                        BossDistance += undefineduniqueports;
                        BossRange += undefineduniqueports;
                    }
                }
                break;
            case Incidents.ScarletMist:
                IncidentText.text = "Scarlet Mist";
                int scarletdisadvantage = BombInfo.GetOnIndicators().Count();
                if (BombInfo.GetSerialNumberLetters().Count() > BombInfo.GetSerialNumberNumbers().Count())
                {
                    BombModule.LogFormat("Fighting Scarlet Devil - Distance = 12, Range = 11");
                    BossDistance = 12;
                    BossRange = 11;
                }
                else
                {
                    BombModule.LogFormat("Fighting Scarlet Sister - Distance = 13, Range = 12");
                    BossDistance = 13;
                    BossRange = 12;
                }
                if (scarletdisadvantage > 0)
                {
                    BombModule.LogFormat("Boss has a Distance/Range disadvantage of {0}/{0} based on the number of lit indicators", scarletdisadvantage);
                    BossDistance -= scarletdisadvantage;
                    BossRange -= scarletdisadvantage;
                }
                break;
            case Incidents.WorldlyDesires:
                IncidentText.text = "Worldly Desires";
                if (strikeCount == 1)
                {
                    BombModule.LogFormat("Fighting Wise Hermit - Distance = 10, Range = 11");
                    BossDistance = 10;
                    BossRange = 11;
                }
                else
                {
                    BombModule.LogFormat("Fighting Wicked Hermit - Distance = 11, Range = 12");
                    BossDistance = 11;
                    BossRange = 12;
                }
                if (BombInfo.GetPortPlateCount() > 0)
                {
                    BombModule.LogFormat("Boss has a Range disadvantage of {0} based on the number of port plates", BombInfo.GetPortPlateCount());
                    BossRange -= BombInfo.GetPortPlateCount();
                }
                break;
            case Incidents.None:
                ForcePass("No idea why Incident.None was picked. That should not have happened.");
                return;
            default:
                ForcePass("Unknown Incident: {0}", _incident);
                return;
        }

        BombModule.LogFormat("Item with Best Distance advantage and worst Range disadvantage is {0}, Distance = {1}, Range = {2}", FindBestDistanceAdvantage.Item, FindBestDistanceAdvantage.Distance, FindBestDistanceAdvantage.Range);
        CharacterDistance += FindBestDistanceAdvantage.Distance;
        CharacterRange += FindBestDistanceAdvantage.Range;

        BombModule.LogFormat("Item with Best Range advantage and worst Distance disadvantage is {0}, Distance = {1}, Range = {2}", FindBestRangeAdvantage.Item, FindBestRangeAdvantage.Distance, FindBestRangeAdvantage.Range);
        CharacterDistance += FindBestRangeAdvantage.Distance;
        CharacterRange += FindBestRangeAdvantage.Range;

        BombModule.LogFormat("Final character results - Distance = {0}, Range = {1}", CharacterDistance, CharacterRange);
        BombModule.LogFormat("Final Boss results - Distance = {0}, Range = {1}", BossDistance, BossRange);

        if (CharacterDistance > BossRange && CharacterRange > BossDistance)
        {
            BombModule.LogFormat("The Boss is Defeated");
            if (strikeCount == 0)
                _correctIncidentResultEvenStrike = IncidentResult.Win;
            else
                _correctIncidentResultOddStrike = IncidentResult.Win;
        }
        else if (BossDistance > CharacterRange && BossRange > CharacterDistance)
        {
            BombModule.LogFormat("The Character dies");
            if (strikeCount == 0)
                _correctIncidentResultEvenStrike = IncidentResult.Loss;
            else
                _correctIncidentResultOddStrike = IncidentResult.Loss;
        }
        else
        {
            BombModule.LogFormat("The Battle ends in a Draw");
            if (strikeCount == 0)
                _correctIncidentResultEvenStrike = IncidentResult.Draw;
            else
                _correctIncidentResultOddStrike = IncidentResult.Draw;
        }

        
        if (_incident == Incidents.WorldlyDesires)
        {
            strikeCount++;
            BombModule.LogFormat(strikeCount == 1 ? "Done calculating for Even strikes" : "Done calculating for Odd strikes");
            if (strikeCount == 1)
                goto OddStrikes;
        }
    }

    private Inventory FindBestDistanceAdvantage
    {
        get { return _inventory.OrderByDescending(x => x.Distance).ThenBy(y => y.Range).ToList()[0]; }
    }

    private Inventory FindBestRangeAdvantage
    {
        get { return _inventory.OrderByDescending(x => x.Range).ThenBy(y => y.Distance).Where(z => z != FindBestDistanceAdvantage).ToList()[0]; }
    }

    void OnActivate()
    {
        Initialize();
        _activated = true;
    }

    public void ForcePass(string reason, params object[] args)
    {
        BombModule.LogFormat("Forcing a pass because of reason: {0}", string.Format(reason,args));

        //Clear the things that souvenir may eventually read, to tell souvenir to abort readout of this module instance.
        StopAllCoroutines();
        _incident = Incidents.None;
        _character = null;
        _solved = true;

        BombModule.HandlePass();
    }

    bool HandleInventory(bool left)
    {
        Audio.HandlePlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (_solved)
            return false;
        if (!_activated)
        {
            BombModule.HandleStrike();
            BombModule.LogFormat("Pressed {0} button before module was ready.", left ? "Left" : "Right");
            return false;
        }
        _currentInventoryItem += left ? (_inventory.Count - 1) : 1;
        _currentInventoryItem %= _inventory.Count;
        InventoryText.text = _inventory[_currentInventoryItem].Item;
        return false;
    }

    bool ResolveIncident(IncidentResult result)
    {
        Audio.HandlePlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (_solved)
            return false;
        if (!_activated)
        {
            BombModule.HandleStrike();
            BombModule.LogFormat("Pressed {0} button before module was ready.", result);
            return false;
        }
        IncidentResult strikeResult = _incident == Incidents.WorldlyDesires 
            ? ((BombInfo.GetStrikes() % 2) == 0 ? _correctIncidentResultEvenStrike : _correctIncidentResultOddStrike )
            : _correctIncidentResultEvenStrike;

        if (result == strikeResult)
        {
            BombModule.LogFormat("Pressed {0}, Expected {1} - Correct", result, _correctIncidentResultEvenStrike);
            switch (result)
            {
                case IncidentResult.Win:
                    _wins++;
                    break;
                case IncidentResult.Loss:
                case IncidentResult.Draw:
                    _losses++;
                    break;
                default:
                    ForcePass("Unknown Incident result was pressed.");
                    return false;
            }
            if (_wins >= WINSTOSOLVE || _losses >= LOSSESTOSOLVE)
            {
                BombModule.Log("Passed");
                BombModule.HandlePass();
                CharacterText.text = "";
                InventoryText.text = "";
                IncidentText.text = "Incident Resolved";
                _solved = true;
                return false;
            }
        }
        else
        {
            BombModule.LogFormat("Pressed {0}, Expected {1} - Incorrect", result, _correctIncidentResultEvenStrike);
            BombModule.HandleStrike();
        }
        Initialize();
        return false;
    }

    private string TwitchHelpMessage = "Cycle the inventory with !{0} cycle. Resolve the Incident with !{0} win, !{0} loss, or !{0} draw.";
    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        if (command == "cycle")
        {
            yield return null;
            for (var i = 0; i < MAXINVENTORYITEMS; i++)
            {
                yield return new WaitForSeconds(3);
                InventoryRight.OnInteract();
            }
        }
        else if (command == "win")
        {
            yield return null;
            WinButton.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        else if (command == "loss")
        {
            yield return null;
            LoseButton.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        else if (command == "draw")
        {
            yield return null;
            DrawButton.OnInteract();
            yield return new WaitForSeconds(0.1f);
        }

        else if (command == "bruteforce win")
        {
            yield return null;
            yield return "solve";
            yield return "multiple strikes";
            int strikes = 0;
            while (!_solved)
            {
                yield return new WaitForSeconds(0.1f);
                ResolveIncident(IncidentResult.Win);
                if (!_solved)
                    strikes++;
            }
            yield return "award strikes " + strikes;

            yield return null;
        }
        else if (command == "bruteforce draw")
        {
            yield return null;
            yield return "solve";
            yield return "multiple strikes";
            int strikes = 0;
            while (!_solved)
            {
                yield return new WaitForSeconds(0.1f);
                ResolveIncident(IncidentResult.Draw);
                if (!_solved)
                    strikes++;
            }
            yield return "award strikes " + strikes;

            yield return null;
        }
        else if (command == "bruteforce loss")
        {
            yield return null;
            yield return "solve";
            yield return "multiple strikes";
            int strikes = 0;
            while (!_solved)
            {
                yield return new WaitForSeconds(0.1f);
                ResolveIncident(IncidentResult.Loss);
                if (!_solved)
                    strikes++;
            }
            yield return "award strikes " + strikes;

            yield return null;
        }
    }
   

}

public enum Incidents
{
    None = 0,
    CosmicWeather,
    EndlessParty,
    FairyWars,
    LilyBlackandWhite,
    LunarWar,
    OccultInvasion,
    OverdrivenNight,
    UndefinedFantasticObject,
    ScarletMist,
    WorldlyDesires,
}

public enum CharacterSeasons
{
    Winter,
    Spring,
    Summer,
    Fall,
}

public enum IncidentResult
{
    Win,
    Draw,
    Loss
}
