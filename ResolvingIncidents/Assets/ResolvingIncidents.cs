using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rnd = UnityEngine.Random;


public class ResolvingIncidents : MonoBehaviour
{
    #region Public Variables
    public GameObject OnLED;
    public GameObject OffLED;
    public KMSelectable InventoryLeft;
    public KMSelectable InventoryRight;
    public KMSelectable WinButton;
    public KMSelectable LoseButton;

    public TextMesh CharacterText;
    public TextMesh IncidentText;
    public TextMesh InventoryText;
    public TextMesh StageNumberText;

    public KMBombModule BombModule;
    public KMAudio Audio;
    public KMBombInfo BombInfo;

    #endregion

    #region Prviate Variables
    private ModSettings _modSettings;

    private bool _solved = false;
    private bool _activated = false;
    private bool _spinning = false;

    private const int WINSTOSOLVE = 1;
    private const int LOSSESTOSOLVE = 3;
    private int _wins = 0;
    private int _losses = 0;

    private IncidentResult _correctIncidentResultEvenStrike;
    private IncidentResult _correctIncidentResultOddStrike;




    private const int MAXINVENTORYITEMS = 5;
    private List<Inventory> _inventoryItems;
    private List<Character> _characters;
    private List<IncidentSet> _incidedentSets;

    private List<Inventory> _inventory;
    private int _currentInventoryItem;
    private Incidents _incident;
    private Character _character;
    private int _characterBaseDistance;
    private int _characterBaseRange;
    private CharacterSeasons _season;
    private bool _spellBonus;

    private int _twofactorsum;




    private Color[] _colorValues = Ext.NewArray(
            "3030F3", //Winter
            "00FF21", //Spring
            "C9CC21", //Summer
            "CC0000" //Fall
        )
        .Select(c => new Color(Convert.ToInt32(c.Substring(0, 2), 16) / 255f, Convert.ToInt32(c.Substring(2, 2), 16) / 255f, Convert.ToInt32(c.Substring(4, 2), 16) / 255f))
        .ToArray();


    private CharacterSeasons[] _seasons = (CharacterSeasons[]) Enum.GetValues(typeof(CharacterSeasons));
    private Incidents[] _incidents = (Incidents[]) (Enum.GetValues(typeof(Incidents)));


    private List<DisplayScreens> IdleScreens;
    private List<DisplayScreens> SolvedScreens;

    #endregion

    private void NoSimulationLog(string message, params object[] args)
    {
        if (_NO_SIMULATION_RUNNING)
            BombModule.LogFormat(message, args);
    }


    // Use this for initialization
    void Start()
    {
        _modSettings = new ModSettings(BombModule);
        _modSettings.ReadSettings();

        BombModule.GenerateLogFriendlyName();
        BombModule.OnActivate += OnActivate;

        _incident = _modSettings.Settings.DebugMode
            ? _incidents[Rnd.Range(1, _incidents.Length)] 
            : _incidents[((BombModule.GetIDNumber() - 1) % (_incidents.Length - 1)) + 1];

        _characters = new List<Character>
        {
            //             Name                  Di  Ra  Heroine   Forbidden Incident
            new Character ("Crow Tengu",         3,  2,   true                                       ),
            new Character ("Elegant Maid",       3,  3,   true                                       ),
            new Character ("Gatekeeper",         2,  1                                               ),
            new Character ("Great Librarian",    2,  3                                               ),
            new Character ("Great Magician",     5,  5                                               ),
            //             Name                  Di  Ra  Heroine   Forbidden Incident
            new Character ("Green Shaman",       2,  3,   true,    Incidents.EndlessParty            ),
            new Character ("Heavenly Swordgirl", 4,  5,            Incidents.CosmicWeather           ),
            new Character ("Ice Fairy",          1,  2,   true,    Incidents.FairyWars               ),
            new Character ("Kappa",              1,  2                                               ),
            new Character ("Legendary Student",  4,  4,            Incidents.OccultInvasion          ),
            //             Name                  Di  Ra  Heroine   Forbidden Incident
            new Character ("Lunar Doctor",       1,  3,            Incidents.OverdrivenNight         ),
            new Character ("Moon Rabbit",        2,  2,   true,    Incidents.OverdrivenNight         ),
            new Character ("Ordinary Magician",  2,  4,   true                                       ),
            new Character ("Phoenix",            4,  3                                               ),
            new Character ("Puppeteer",          3,  1                                               ),
            //             Name                  Di  Ra  Heroine   Forbidden Incident
            new Character ("Red Shaman",         3,  5,   true,    Incidents.EndlessParty            ),
            new Character ("Satori",             1,  2                                               ),
            new Character ("Scarlet Devil",      2,  1,            Incidents.ScarletMist             ),
            new Character ("Scarlet Sister",     3,  1,            Incidents.ScarletMist             ),
            new Character ("Unidentified Girl",  5,  4,            Incidents.UndefinedFantasticObject),
        };

        _incidedentSets = new List<IncidentSet>
        {
            new IncidentSet(),
            //                Incident Name                (Boss #1   Boss Name             BASE     Bonus    Widget)      (Boss #2  Boss name           Base     Bonus    Widget)  Bonus Reason              //
            //                                                                             Di  Ra    Di  Ra   Di  Ra                                    Di  Ra    Di  Ra   Di  Ra)
            new IncidentSet ("Cosmic Weather",             new Boss ("Heavenly Swordgirl", 12, 14,   1,  1,   3,  3  ),                                                             "season"                  ),
            new IncidentSet ("Endless Party",              new Boss ("Green Shaman",       6,  14,   0,  0,   2,  1  ),   new Boss ("Red Shaman",       7,  13,   0,  0,   0,  1 ), ""                        ),
            new IncidentSet ("Fairy Wars",                 new Boss ("Ice Fairy",          7,  8,    1,  1,   1,  1  ),   new Boss ("Sunflower Fairy",  8,  9,    1,  2,   1,  1 ), "season"                  ),
            new IncidentSet ("Lily Black and White",       new Boss ("Lily Black",         13, 12,   1,  1,   2,  1  ),   new Boss ("Lily White",       12, 13,   2,  0,   1,  2 ), "season"                  ),
            new IncidentSet ("Lunar War",                  new Boss ("Lunar King",         11, 12,   1,  1,   3,  2  ),   new Boss ("Lunar Queen",      12, 11,   1,  1,   2,  3 ), "serial number digit sum" ),

            //                Incident Name                (Boss #1   Boss Name             BASE     Bonus    Widget)      (Boss #2  Boss name           Base     Bonus    Widget)  Bonus Reason              //
            //                                                                             Di  Ra    Di  Ra   Di  Ra                                    Di  Ra    Di  Ra   Di  Ra)
            new IncidentSet ("Occult Invasion",            new Boss ("Legendary Student",  11, 12,   1,  2,   3,  3  ),                                                             "season"                  ),
            new IncidentSet ("Overdriven Night",           new Boss ("Lunar Doctor",       14, 12,   0,  0,   2,  3  ),   new Boss ("Moon Rabbit",      12, 13,   0,  0,   2,  3 ), ""                        ),
            new IncidentSet ("Undefined Fantastic Object", new Boss ("Undefined Girl",     12, 12,   0,  0,   3,  4  ),   new Boss ("Fantastic Girl",   14, 13,   0,  0,   1,  2 ), ""                        ),
            new IncidentSet ("Scarlet Mist",               new Boss ("Scarlet Devil",      14, 13,   0,  0,   3,  3  ),   new Boss ("Scarlet Sister",   12, 14,   0,  0,   3,  3 ), ""                        ),
            new IncidentSet ("Worldly Desires",            new Boss ("Wise Hermit",        12, 15,   0,  0,   3,  1  ),   new Boss ("Wicked Hermit",    13, 16,   0,  0,   4,  2 ), ""                        )
        };

        _inventoryItems = new List<Inventory>
        {   //              Name              Di  Ra
            new Inventory ("Flower",         -2,  2  ),
            new Inventory ("Glove",           2,  2  ),
            new Inventory ("Gohei",           4,  5  ),
            new Inventory ("Grimoire",       -2,  4  ),
            new Inventory ("Hisou Sword",     2,  3  ),
            //              Name              Di  Ra
            new Inventory ("Ice",             3,  3  ),
            new Inventory ("Jeweled Pagoda",  5,  6  ),
            new Inventory ("Keystone",        5,  5  ),
            new Inventory ("Lunar Cape",      4,  4  ),
            new Inventory ("Mini-Hakkero",   -2,  8  ),
            //              Name              Di  Ra
            new Inventory ("Miracle Mallet",  3,  4  ),
            new Inventory ("Nimble Cloth",    8,  1  ),
            new Inventory ("Rock",            1,  3  ),
            new Inventory ("Occult Orb",      4,  7  ),
            new Inventory ("Seal",            6,  5  ),
            //              Name              Di  Ra
            new Inventory ("Soul Torch",      3,  1  ),
            new Inventory ("Sunflower",       1,  2  ),
            new Inventory ("Trident",         2,  3  ),
            new Inventory ("UFO",             5,  5  ),
            new Inventory ("Yin-Yang Orb",    3,  6  ),
        };

        IdleScreens = new List<DisplayScreens>
        {
            new DisplayScreens("It is Time","To Resolve","Some Incidents!"),
            new DisplayScreens("That Character","Did Not Spill","A Drop Of Sake!"),
            new DisplayScreens("Did You","hear An Incident","occurred Nearby?"),
            new DisplayScreens("Have you","ever wonder what a","youkai is like?"),
            new DisplayScreens("Sorry, Characters.","It is that", "time again..."),
            new DisplayScreens("Extra! Extra!","Nothing Really Happened!","Still an Extra!"),
            new DisplayScreens("Three Characters?","This must be","an incident!"),
            new DisplayScreens("This Must Be","Some Kind","Of Incident"),
            new DisplayScreens("This Must Be","The Youkai's","Doing Again."),
            new DisplayScreens("That's Sweet!","I only get","Weekends Off, Though..."),
            new DisplayScreens("I'll Tell You","My Weakness...","It's Nothing."),
            new DisplayScreens("Your surrounding","are full of","unnatural naturalness."),
            new DisplayScreens("Oh No.","Toshi!","Where Are You??",0,10),
            new DisplayScreens("Resolving Incidents","By Toshi","and CaitSith2",0,5),
            new DisplayScreens("Thank You","For Resolving Incidents","With Us!",0,1)
        };

        SolvedScreens = new List<DisplayScreens>
        {
            new DisplayScreens("Well Done!","You Are Victorious","Heroine!"),
            new DisplayScreens("Congratulations!","The Incident Is Now","Resolved!"),
            new DisplayScreens("Congratulations!","You Have Well Played","This Round!"),
            new DisplayScreens("20,000,000 points!","Spell Card Bonus Get!","20,000,000 points"),
            new DisplayScreens("Extra! Extra!","The Heroine Defeats","The Incidents's Boss"),
            new DisplayScreens("Extra! Extra!","The Heroine Resolves","The Incident!"),
            new DisplayScreens("Extra! Extra!","The Boss is Defeated","Incident Resolved!"),
            new DisplayScreens("!     !     ! ","This  Incident is Resolved","!     !     ! ")
        };

        InventoryLeft.OnInteract += () => HandleInventory(true);
        InventoryRight.OnInteract += () => HandleInventory(false);
        WinButton.OnInteract += () => ResolveIncident(IncidentResult.Win);
        LoseButton.OnInteract += () => ResolveIncident(IncidentResult.Loss);
        OffLED.SetActive(true);
        OnLED.SetActive(false);

        int screenChance = Rnd.Range(0, 100);
        IdleScreens.OrderBy(x => Rnd.value).First(y => screenChance >= y.MinChance && screenChance < y.MaxChance).UpdateScreens(ref CharacterText, ref IncidentText, ref InventoryText);
    }

    void OnActivate()
    {
        BombModule.LogFormat("Bomb Serial Number = {0}", BombInfo.GetSerialNumber());
        int totalwidgets = BombInfo.GetBatteryHolderCount();
        totalwidgets += BombInfo.GetIndicators().Count();
        totalwidgets += BombInfo.GetPortPlateCount();
        totalwidgets += BombInfo.GetTwoFactorCounts();
        _widgetbonus = totalwidgets >= WIDGETSFORBONUS;

        if (_modSettings.Settings.DebugMode)
        {
            StartCoroutine(SimulatateBattles(0, 0));
        }
        else
        {
            SetEdgeworkBonus();
            StartCoroutine(SetNextCharacter(false));
        }
        _activated = true;
    }

    void Update()
    {
        if (_incident != Incidents.ScarletMist || !_activated || !_NO_SIMULATION_RUNNING)
            return;
        if (BombInfo.IsTwoFactorPresent() && BombInfo.GetTwoFactorCodes().Sum(twofactor => twofactor % 10) != _twofactorsum)
        {
            SetEdgeworkBonus();
            if (!_spinning)
            {
                Initialize(true);
            }
        }
    }

    private bool _widgetbonus = false;
    private bool _edgeworkDefinedBoss = false;
    private const int WIDGETSFORBONUS = 8;
    private void SetEdgeworkBonus()
    {
        _characterBaseDistance = BombInfo.GetBatteryCount() == 0 ? 3 : BombInfo.GetBatteryCount();
        _characterBaseRange = BombInfo.GetBatteryHolderCount() == 0 ? 3 : BombInfo.GetBatteryHolderCount();

        if (!_widgetbonus)
        {
            while (_characterBaseDistance > 5)
                _characterBaseDistance -= 5;
            while (_characterBaseRange > 5)
                _characterBaseRange -= 5;
        }
        else
        {
            while (_characterBaseDistance < 5)
                _characterBaseDistance += 5;
            while (_characterBaseRange < 5)
                _characterBaseRange += 5;
            while (_characterBaseDistance > 10)
                _characterBaseDistance -= 5;
            while (_characterBaseRange > 10)
                _characterBaseRange -= 5;
        }

        IncidentSet incident = new IncidentSet { Name = "Error" };
        if ((int)_incident < _incidedentSets.Count && (int)_incident >= 0)
            incident = _incidedentSets[(int)_incident];
        switch (_incident)
        {
            case Incidents.CosmicWeather:
                bool heavenlyNoEmptyPlatesBonus = BombInfo.GetPortPlates().All(x => x.Length != 0) && (BombInfo.GetPortCount() % 2) == 1;
                int heavenlyBonus = (BombInfo.GetPortCount() / 2);
                if (heavenlyNoEmptyPlatesBonus)
                    heavenlyBonus++;
                incident.Boss1.EdgeworkBonusDistance = heavenlyBonus;
                incident.EdgeworkBonusReason = string.Format("port counts divided by two{0}", heavenlyNoEmptyPlatesBonus ? " rounded up" : " rounded down");
                break;
            case Incidents.EndlessParty:
                _edgeworkDefinedBoss = BombInfo.CountUniquePorts() < 3;
                int shamanBonus = BombInfo.GetSerialNumberNumbers().Min();
                if (shamanBonus == 0)
                    shamanBonus = 10;
                incident.EdgeworkBonusReason = "the last serial number digit";
                incident.Boss1.EdgeworkBonusDistance = shamanBonus;
                incident.Boss2.EdgeworkBonusRange = shamanBonus;
                break;
            case Incidents.FairyWars:
                int FairyBonus = BombInfo.CountUniquePorts();
                incident.Boss1.EdgeworkBonusDistance = FairyBonus;
                incident.Boss1.EdgeworkBonusRange = FairyBonus;
                incident.Boss2.EdgeworkBonusDistance = FairyBonus;
                incident.Boss2.EdgeworkBonusRange = FairyBonus;
                incident.EdgeworkBonusReason = "unique ports divided by two";
                break;
            case Incidents.LilyBlackandWhite:
                break;
            case Incidents.LunarWar:
                _edgeworkDefinedBoss = BombInfo.CountDuplicatePorts() != 1;
                if (BombInfo.GetSerialNumberNumbers().Sum() <= 11)
                {
                    incident.Boss1.EdgeworkBonusDistance = incident.Boss1.BonusDistance;
                    incident.Boss1.EdgeworkBonusRange = incident.Boss1.BonusRange;
                    incident.Boss2.EdgeworkBonusDistance = incident.Boss2.BonusDistance;
                    incident.Boss2.EdgeworkBonusRange = incident.Boss2.BonusRange;
                    incident.EdgeworkBonusReason = incident.BonusReason;
                }
                break;
            case Incidents.OccultInvasion:
                int LegendaryBonus = BombInfo.GetModuleNames().Count % 5;
                bool LegendaryIndicator = BombInfo.GetOnIndicators().Count(x => "LEGEND".Contains(x.ToUpperInvariant().ToCharArray().Last())) == 2;
                if (LegendaryBonus == 0 && LegendaryIndicator)
                    LegendaryBonus = 5;
                incident.Boss1.EdgeworkBonusDistance = LegendaryBonus;
                incident.EdgeworkBonusReason = LegendaryBonus == 5 ? "Two indicators ending in a letter contained in \"LEGEND\"" : "Module count modulo 5";
                break;
            case Incidents.OverdrivenNight:
                incident.Boss1.EdgeworkBonusRange = BombInfo.GetOffIndicators().Count();
                incident.Boss2.EdgeworkBonusRange = BombInfo.GetOffIndicators().Count();
                incident.EdgeworkBonusReason = "the number of unlit Indicators";
                break;
            case Incidents.UndefinedFantasticObject:
                _edgeworkDefinedBoss = BombInfo.GetOffIndicators().Count() > BombInfo.GetOnIndicators().Count();
                int undefineduniqueports = BombInfo.CountUniquePorts();
                incident.Boss1.EdgeworkBonusDistance = undefineduniqueports;
                incident.Boss1.EdgeworkBonusRange = undefineduniqueports;
                incident.Boss2.EdgeworkBonusRange = undefineduniqueports;
                incident.EdgeworkBonusReason = "the number of unique port types";
                break;
            case Incidents.ScarletMist:
                _edgeworkDefinedBoss = BombInfo.GetSerialNumberLetters().Count() > BombInfo.GetSerialNumberNumbers().Count();
                int scarletdisadvantage = BombInfo.GetOnIndicators().Count();
                if (_NO_SIMULATION_RUNNING)
                {
                    _twofactorsum = BombInfo.IsTwoFactorPresent()
                        ? BombInfo.GetTwoFactorCodes().Sum(twofactor => twofactor % 10)
                        : BombInfo.GetSerialNumberNumbers().Last();
                    foreach (int twofactor in BombInfo.GetTwoFactorCodes())
                    {
                        BombModule.LogFormat("Two Factor code changed: {0}", twofactor);
                    }
                }
                else
                    _twofactorsum = BombInfo.IsTwoFactorPresent()
                        ? 0
                        : BombInfo.GetSerialNumberNumbers().Last();
                scarletdisadvantage += _twofactorsum;
                incident.Boss1.EdgeworkBonusDistance = -scarletdisadvantage;
                incident.Boss1.EdgeworkBonusRange = -scarletdisadvantage;
                incident.Boss2.EdgeworkBonusDistance = -scarletdisadvantage;
                incident.Boss2.EdgeworkBonusRange = -scarletdisadvantage;
                incident.EdgeworkBonusReason = string.Format("based on the number of lit indicators and {0}", BombInfo.IsTwoFactorPresent() ? "two factor least significant digit sum" : "last serial number digit");
                break;
            case Incidents.WorldlyDesires:
                int hermitpenalty = -BombInfo.GetPortCount();
                incident.Boss1.EdgeworkBonusRange = hermitpenalty;
                incident.Boss2.EdgeworkBonusRange = hermitpenalty;
                incident.EdgeworkBonusReason = "the number of ports";
                break;
            case Incidents.None:
                ForcePass("No idea why Incident.None was picked. That should not have happened.");
                return;
            default:
                ForcePass("Unknown Incident: {0}", _incident);
                return;
        }
    }



    private void PrintBonus(int Distance, int Range, string Reason)
    {

        if (Distance > 0 && Range > 0)
        {
            BombModule.LogFormat("Boss has a Distance/Range advantage of {0}/{1} based on {2}", Distance, Range, Reason);
        }
        else if (Distance < 0 && Range < 0)
        {
            BombModule.LogFormat("Boss has a Distance/Range penalty of {0}/{1} based on {2}", Distance, Range, Reason);
        }
        else
        {

            if (Distance > 0)
            {
                BombModule.LogFormat("Boss has a Distance advantage of {0} based on {1}", Distance, Reason);
            }
            else if (Distance < 0)
            {
                BombModule.LogFormat("Boss has a Distance penalty of {0} based on {1}", Mathf.Abs(Distance), Reason);
            }

            if (Range > 0)
            {
                BombModule.LogFormat("Boss has a Range advantage of {0} based on {1}", Range, Reason);
            }
            else if (Range < 0)
            {
                BombModule.LogFormat("Boss has a Range penalty of {0} based on {1}", Mathf.Abs(Range), Reason);
            }
        }
    }

    private void PrintIncident(IncidentSet incident, bool boss1, bool bossbonus, out int distance, out int range)
    {
        if (incident == null)
        {
            distance = 0;
            range = 0;
            return;
        }
        Boss boss = boss1 ? incident.Boss1 : incident.Boss2;
        if (boss == null)
        {
            distance = 0;
            range = 0;
            return;
        }
        distance = boss.BaseDistance + boss.EdgeworkBonusDistance;
        range = boss.BaseRange + boss.EdgeworkBonusRange;
        if (bossbonus)
        {
            distance += boss.BonusDistance;
            range += boss.BonusRange;
        }

        if (_widgetbonus)
        {
            distance += boss.WidgetBonusDistance;
            range += boss.WidgetBonusRange;
        }

        if (!_NO_SIMULATION_RUNNING) return;

        BombModule.LogFormat("Fighting {0} - Distance = {1}, Range = {2}", boss.Name, boss.BaseDistance, boss.BaseRange);
        if (bossbonus) PrintBonus(boss.BonusDistance, boss.BonusRange, incident.BonusReason);
        PrintBonus(boss.EdgeworkBonusDistance, boss.EdgeworkBonusRange, incident.EdgeworkBonusReason);
        if(_widgetbonus) PrintBonus(boss.WidgetBonusDistance, boss.WidgetBonusRange, incident.WidgetBonusReason);
    }

    private static int _sets = 0;
    private static Dictionary<int, Inventory[]> _itemSets = new Dictionary<int, Inventory[]>();

    private void InitializeItemSets()
    {
        for (int i = 0; i < (_inventoryItems.Count - 4); i++)
        {
            for (int j = (i + 1); j < (_inventoryItems.Count - 3); j++)
            {
                for (int k = (j + 1); k < (_inventoryItems.Count - 2); k++)
                {
                    for (int l = (k + 1); l < (_inventoryItems.Count - 1); l++)
                    {
                        for (int m = (l + 1); m < _inventoryItems.Count; m++)
                        {
                            //_inventory.OrderByDescending(x => x.Distance).ThenBy(y => y.Range).ToList()[0];
                            //_inventory.OrderByDescending(x => x.Range).ThenBy(y => y.Distance).Where(z => z != FindBestDistanceAdvantage).ToList()[0];
                            var inventory = new [] { _inventoryItems[i], _inventoryItems[j], _inventoryItems[k], _inventoryItems[l], _inventoryItems[m]};
                            var bestdist = inventory.OrderByDescending(x => x.Distance).ThenBy(y => y.Range).ToList()[0];
                            var bestrange = inventory.OrderByDescending(x => x.Range).ThenBy(y => y.Distance).Where(z => z != bestdist).ToList()[0];
                            if ((from w in _itemSets.Values let wdist = w.OrderByDescending(x => x.Distance).ThenBy(y => y.Range).ToList()[0] let wrange = w.OrderByDescending(x => x.Range).ThenBy(y => y.Distance).Where(z => z != wdist).ToList()[0] where bestdist == wdist && bestrange == wrange select wdist).Any())
                                continue;
                            _itemSets[_sets++] = inventory;
                        }
                    }
                }
            }
        }
        BombModule.LogFormat("Found {0} Unique Items sets", _sets);
    }

    private void RandomizeCharacter()
    {
        _spellBonus = Rnd.value > 0.5f;
        _character = _characters.OrderBy(x => Rnd.value).First(y => y.ForbiddenIncident != _incident);
        _season = _seasons[Rnd.Range(0, _seasons.Length)];
    }

    private void RandomizeInventory()
    {
        _inventory = new List<Inventory>(_inventoryItems).OrderBy(x => Rnd.value).Take(MAXINVENTORYITEMS).ToList();
    }

    private void UpdateDisplay(bool updateLED=true, bool UpdateCharacter=true, bool UpdateInventory=true, bool forceUpdate=false)
    {
        if (!_NO_SIMULATION_RUNNING && !forceUpdate)
            return;
        if (UpdateCharacter)
        {
            CharacterText.text = _character.Name;
            CharacterText.color = _colorValues[(int) _season];
            if (updateLED)
            {
                OffLED.SetActive(!_spellBonus);
                OnLED.SetActive(_spellBonus);
            }
        }
        if (UpdateInventory)
        {
            InventoryText.text = _inventory[_currentInventoryItem].Item;
        }
        StageNumberText.text = string.Format("{0}", _losses + 1);

        IncidentSet incident = new IncidentSet();
        if ((int)_incident < _incidedentSets.Count && (int)_incident >= 0)
            incident = _incidedentSets[(int)_incident];
        IncidentText.text = incident.Name;
        
        
    }

    private void Initialize(bool TwoFactorUpdate = false)
    {
        Initialize(0, TwoFactorUpdate);
    }

    private void Initialize(int BossBuff, bool TwoFactorUpdate)
    {
        if (_solved)
            return;

        int BossDistance;
        int BossRange;
        IncidentSet incident = new IncidentSet { Name = "Error" };
        if ((int)_incident < _incidedentSets.Count && (int)_incident >= 0)
            incident = _incidedentSets[(int)_incident];
        bool boss1 = true;
        bool bossbonus = false;

        if (!TwoFactorUpdate)
        {
            RandomizeCharacter();
            RandomizeInventory();
            if (_NO_SIMULATION_RUNNING)
            {
                UpdateDisplay();
                BombModule.LogFormat("Resolving Incident: {0}", incident.Name);
            }
        }
        else
        {
            if (_NO_SIMULATION_RUNNING)
            {
                UpdateDisplay();
                BombModule.LogFormat("Updating Incident {0} results based on a change in the Two Factor sum", incident.Name);
            }
        }

        bool Strikes = false;
        OddStrikes:
        if (_incident == Incidents.WorldlyDesires)
        {
            NoSimulationLog(!Strikes ? "Calculating the results for Even number of Strikes" : "Calculating the results for Odd number of Strikes");
        }

        int CharacterDistance = _characterBaseDistance;
        int CharacterRange = _characterBaseRange;

        NoSimulationLog("Chararcter: {0} - Distance = {1}, Range = {2}, Season = {3}", _character.Name, CharacterDistance, CharacterRange, _season);

        if (_character.Heroine)
        {
            CharacterDistance++;
            CharacterRange++;
            NoSimulationLog("Character is a Heroine, Gets a bonus of 1/1 to Distance/Range");
        }

        if (_spellBonus)
        {
            CharacterDistance += _character.DistanceBonus;
            CharacterRange += _character.RangeBonus;
            NoSimulationLog("Spell Bonus is Active. Character gets a bonus of {0} to Distance and {1} to Range", _character.DistanceBonus, _character.RangeBonus);
        }

        switch (_incident)
        {
            case Incidents.CosmicWeather:
                bossbonus = _season == CharacterSeasons.Summer;
                break;
            case Incidents.EndlessParty:
                boss1 = _edgeworkDefinedBoss;
                break;
            case Incidents.FairyWars:
                boss1 = _season == CharacterSeasons.Fall || _season == CharacterSeasons.Winter;
                bossbonus = _season == CharacterSeasons.Winter || _season == CharacterSeasons.Summer;
                break;
            case Incidents.LilyBlackandWhite:
                boss1 = _season == CharacterSeasons.Summer || _season == CharacterSeasons.Fall;
                bossbonus = _season == CharacterSeasons.Spring || _season == CharacterSeasons.Summer;
                break;
            case Incidents.LunarWar:
                boss1 = _edgeworkDefinedBoss;
                break;
            case Incidents.OccultInvasion:
                bossbonus = _season == CharacterSeasons.Spring || _season == CharacterSeasons.Summer;
                break;
            case Incidents.OverdrivenNight:
                boss1 = _season == CharacterSeasons.Fall || _season == CharacterSeasons.Winter;
                break;
            case Incidents.UndefinedFantasticObject:
                boss1 = _edgeworkDefinedBoss;
                break;
            case Incidents.ScarletMist:
                boss1 = _edgeworkDefinedBoss;
                break;
            case Incidents.WorldlyDesires:
                boss1 = Strikes;
                break;
            case Incidents.None:
                ForcePass("No idea why Incident.None was picked. That should not have happened.");
                return;
            default:
                ForcePass("Unknown Incident: {0}", _incident);
                return;
        }
        PrintIncident(incident, boss1, bossbonus, out BossDistance, out BossRange);

        foreach (Inventory item in _inventory.OrderBy(x => x.Item))
        {
            string footer = "";
            if (item == FindBestDistanceAdvantage)
            {
                int i = item.Distance;
                footer = " <--- Best Distance advantage";
                if (_inventory.Count(x => x.Distance == i) > 1)
                    footer += " and worst Range disadvantage";
            }
            else if (item == FindBestRangeAdvantage)
            {
                int i = item.Range;
                footer = " <--- Best Range advantage";
                if (_inventory.Count(x => x.Range == i) > 1)
                    footer += " and worst Distance disadvantage";
            }
            NoSimulationLog("Item: {0}, Distance: {1}, Range: {2}{3}", item.Item, item.Distance, item.Range, footer);
        }
        CharacterDistance += FindBestDistanceAdvantage.Distance;
        CharacterRange += FindBestDistanceAdvantage.Range;

        CharacterDistance += FindBestRangeAdvantage.Distance;
        CharacterRange += FindBestRangeAdvantage.Range;

        NoSimulationLog("Final character results - Distance = {0}, Range = {1}", CharacterDistance, CharacterRange);
        NoSimulationLog("Final Boss results - Distance = {0}, Range = {1}", BossDistance, BossRange);

        BossRange += BossBuff;
        BossDistance += BossBuff;

        bool BossWins = BossRange >= CharacterDistance;
        bool CharacterWins = CharacterRange >= BossDistance;

        if (CharacterWins) NoSimulationLog(BossWins ? "Although the Boss was defeated, the character died in the process." : "The Boss was defeated and the character returned victorious");
        else NoSimulationLog(BossWins ? "The character was killed by the boss" : "The Battle ended in a stale-mate");

        IncidentResult correctResult;
        if (CharacterWins && BossWins)
        {
            correctResult = _losses < 2 ? IncidentResult.Loss : IncidentResult.Win;
        }
        else
        {
            correctResult = CharacterWins ? IncidentResult.Win : IncidentResult.Loss;
        }
        NoSimulationLog("Expecting {0} to be pressed.", correctResult);

        if (!Strikes)
            _correctIncidentResultEvenStrike = correctResult;
        else
            _correctIncidentResultOddStrike = correctResult;

        if (boss1)
        {
            if (CharacterWins && BossWins)
            {
                _BOTHDEFEATEDLOSS = !_BOTHDEFEATEDLOSS;
                if(_BOTHDEFEATEDLOSS)
                    _WINSBOSS1++;
                else
                    _LOSSESBOSS1++;
            }
            else if (CharacterWins)
                _WINSBOSS1++;
            else
                _LOSSESBOSS1++;
        }
        else
        {
            if (CharacterWins && BossWins)
            {
                _BOTHDEFEATEDLOSS = !_BOTHDEFEATEDLOSS;
                if (_BOTHDEFEATEDLOSS)
                    _WINSBOSS2++;
                else
                    _LOSSESBOSS2++;
            }
            else if(CharacterWins)
                _WINSBOSS2++;
            else
                _LOSSESBOSS2++;
        }


        if (_incident == Incidents.WorldlyDesires)
        {
            NoSimulationLog(!Strikes ? "Done calculating for Even strikes" : "Done calculating for Odd strikes");
            if (!Strikes)
            {
                Strikes = true;
                goto OddStrikes;
            }
        }
        else
        {
            _correctIncidentResultOddStrike = _correctIncidentResultEvenStrike;
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

    public void ForcePass(string reason, params object[] args)
    {
        BombModule.LogFormat("Forcing a pass because of reason: {0}", string.Format(reason, args));

        //Clear the things that souvenir may eventually read, to tell souvenir to abort readout of this module instance.
        StopAllCoroutines();
        _incident = Incidents.None;
        _character = null;
        _solved = true;
        IncidentText.text = "Error";
        InventoryText.text = "";
        CharacterText.text = "";
        OnLED.SetActive(false);
        OffLED.SetActive(true);

        BombModule.HandlePass();
    }

    void PlayRandomSound()
    {
        List<KMSoundOverride.SoundEffect> effects = ((KMSoundOverride.SoundEffect[])Enum.GetValues(typeof(KMSoundOverride.SoundEffect))).ToList();
        effects.Remove(KMSoundOverride.SoundEffect.AlarmClockBeep);
        effects.Remove(KMSoundOverride.SoundEffect.BombExplode);
        effects.Remove(KMSoundOverride.SoundEffect.GameOverFanfare);
        effects.Remove(KMSoundOverride.SoundEffect.NeedyWarning);
        Audio.PlayGameSoundAtTransform(effects.OrderBy(x => Rnd.value).First(), transform);
    }

    bool HandleInventory(bool left)
    {
        if (_modSettings.Settings.DebugMode)
        {
            PlayRandomSound();
            return false;
        }
        Audio.HandlePlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (_solved || _spinning)
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
        if (_modSettings.Settings.DebugMode)
        {
            PlayRandomSound();
            return false;
        }
        Audio.HandlePlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (_solved || _spinning)
            return false;
        if (!_activated)
        {
            BombModule.HandleStrike();
            BombModule.LogFormat("Pressed {0} button before module was ready.", result);
            return false;
        }
        bool EvenStrikes = (BombInfo.GetStrikes() % 2) == 0;
        IncidentResult strikeResult = EvenStrikes ? _correctIncidentResultEvenStrike : _correctIncidentResultOddStrike;
        if (_incident == Incidents.WorldlyDesires)
            BombModule.LogFormat(EvenStrikes ? "Even number of strikes present - Fighting Wicked Hermit" : "Odd number of Strikes present - Fighting Wise Hermit");

        if (result == strikeResult)
        {
            BombModule.LogFormat("Pressed {0}, Expected {1} - Correct", result, _correctIncidentResultEvenStrike);
            switch (result)
            {
                case IncidentResult.Win:
                    _wins++;
                    break;
                case IncidentResult.Loss:
                    _losses++;
                    break;
                default:
                    ForcePass("Unknown Incident result was pressed.");
                    return false;
            }
            StartCoroutine(SetNextCharacter(false));
        }
        else
        {
            BombModule.LogFormat("Pressed {0}, Expected {1} - Incorrect", result, _correctIncidentResultEvenStrike);
            _losses++;
            StartCoroutine(SetNextCharacter(true));
        }
        return false;
    }

    private bool _twitchPlayStrike;
    IEnumerator SetNextCharacter(bool giveStrike)
    {
        _spinning = true;
        _twitchPlayStrike = giveStrike;
        if (_wins >= WINSTOSOLVE || _losses >= LOSSESTOSOLVE)
        {
            _solved = true;
        }

        for (int i = 0; i < 60; i++)
        {
            RandomizeCharacter();
            RandomizeInventory();
            UpdateDisplay();
            StageNumberText.text = string.Format("{0}", (_losses + 1 + i) % 10);
            yield return null;
        }
        if (_solved) CharacterText.text = "";
        OnLED.SetActive(giveStrike);
        OffLED.SetActive(!giveStrike);

        for (int i = 0; i < 60; i++)
        {
            RandomizeInventory();
            UpdateDisplay(false, !_solved);
            StageNumberText.text = string.Format("{0}", (_losses + 1 + i) % 10);
            yield return null;
        }
        if (_solved)
        {
            InventoryText.text = "";
            OnLED.SetActive(false);
            OffLED.SetActive(true);
        }
        else
        {
            UpdateDisplay();
        }
        

        for (int i = 0; i < 60; i++)
        {
            StageNumberText.text = string.Format("{0}", (_losses + 1 + i) % 10);
            yield return null;
        }

        if (giveStrike)
            BombModule.HandleStrike();

        if (_solved)
        {
            SolvedScreens.OrderBy(x => Rnd.value).First().UpdateScreens(ref CharacterText, ref IncidentText, ref InventoryText);
            StageNumberText.text = "!";
            OnLED.SetActive(false);
            OffLED.SetActive(true);
            BombModule.Log("Passed");
            BombModule.HandlePass();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        }
        else
        {
            Initialize(true);
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.TitleMenuPressed, transform);
        }
        _spinning = false;
    }

    private string TwitchHelpMessage = "Cycle the inventory with !{0} cycle. Resolve the Incident with !{0} win, or !{0} loss";

    IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        _twitchPlayStrike = false;
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
        if (_twitchPlayStrike)
        {
            yield return "strike";
        }
        if (_solved)
        {
            yield return "solve";
        }
    }

    private static Dictionary<Incidents, int[]> IncidentResults = new Dictionary<Incidents, int[]>
    {
        {Incidents.None, new int[6]},
        {Incidents.CosmicWeather, new int[6]},
        {Incidents.EndlessParty, new int[6]},
        {Incidents.FairyWars, new int[6]},
        {Incidents.LilyBlackandWhite, new int[6]},
        {Incidents.LunarWar, new int[6]},
        {Incidents.OccultInvasion, new int[6]},
        {Incidents.OverdrivenNight, new int[6]},
        {Incidents.UndefinedFantasticObject, new int[6]},
        {Incidents.ScarletMist, new int[6]},
        {Incidents.WorldlyDesires, new int[6]}
    };

    void ReportWinResults(int BossBuff, bool printBuffInfo)
    {
        if (_WINSBOSS1 > 0 || _LOSSESBOSS1 > 0)
        {
            IncidentResults[_incident][0]++;
            IncidentResults[_incident][1] += _WINSBOSS1;
            IncidentResults[_incident][2] += _LOSSESBOSS1;
        }
        if (_WINSBOSS2 > 0 || _LOSSESBOSS2 > 0)
        {
            IncidentResults[_incident][3]++;
            IncidentResults[_incident][4] += _WINSBOSS2;
            IncidentResults[_incident][5] += _LOSSESBOSS2;
        }

        IncidentSet incident = _incidedentSets[(int) _incident];
        if(printBuffInfo)
            BombModule.LogFormat("results with {0}/{0} Dist/Range {1}:", BossBuff, (BossBuff < 0 ? "penalty" : (BossBuff > 0 ? "bonus" : "bonus/penalty")));
        if (_WINSBOSS1 > 0 || _LOSSESBOSS1 > 0)
            BombModule.LogFormat("Boss #1 ({0}): W/L: {1}/{2}", incident.Boss1.Name, _WINSBOSS1, _LOSSESBOSS1);

        if (_WINSBOSS2 > 0 || _LOSSESBOSS2 > 0)
            BombModule.LogFormat("Boss #2 ({0}): W/L: {1}/{2}", incident.Boss2.Name, _WINSBOSS2, _LOSSESBOSS2);



        _WINSBOSS1 = 0;
        _LOSSESBOSS1 = 0;
        _WINSBOSS2 = 0;
        _LOSSESBOSS2 = 0;
    }

    private static int IsItMyTurnYet = 1;
    private static string BombSerialNumber = "";
    private static bool IsSimulationRunning = false;
    IEnumerator SimulatateBattles(int debuff, int buff)
    {
        if (IsSimulationRunning)
        {
            IsItMyTurnYet = BombModule.GetIDNumber();
            IsSimulationRunning = false;
        }
        yield return null;
        yield return null;

        _NO_SIMULATION_RUNNING = false;
        BombModule.LogFormat("DEBUGGING MODE ENABLED");
        while (IsItMyTurnYet < BombModule.GetIDNumber())
            yield return null;
        IsSimulationRunning = true;

        if (_sets == 0)
            InitializeItemSets();

        if (!BombSerialNumber.Equals(BombInfo.GetSerialNumber()))
        {
            BombSerialNumber = BombInfo.GetSerialNumber();
            BombModule.LogFormat("------------------");
            BombModule.LogFormat("Simulation Results");
            BombModule.LogFormat("------------------");
            BombModule.LogFormat("");
            foreach (Incidents incident in _incidents.Skip(1))
            {
                _incident = incident;
                int battles = _sets * 4 * 2 * _characters.Count(x => x.ForbiddenIncident != _incident);
                SetEdgeworkBonus();

                IncidentSet incidentSet = _incidedentSets[(int) _incident];
                BombModule.LogFormat("Simulating Incident: {0}", incidentSet.Name);
                IncidentText.text = _incident.ToString();
                InventoryText.characterSize = 0.7f;
                for (int j = debuff; j <= buff; j++)
                {
                    int i = 0;
                    foreach (Character c in _characters.Where(x => x.ForbiddenIncident != incident))
                    {
                        _character = c;
                        foreach (CharacterSeasons s in _seasons)
                        {
                            _season = s;
                            foreach (Inventory[] items in _itemSets.Values)
                            {
                                _inventory = items.ToList();
                                _spellBonus = false;
                                Initialize(j, true);
                                _spellBonus = true;
                                Initialize(j, true);
                                i += 2;
                                if ((i % 50) == 0)
                                {
                                    UpdateDisplay(true, true, false, true);
                                    InventoryText.text = string.Format("{0}/{1}\n{2}: {3}/{4}", i + 1, battles, j < 0 ? "penalty" : "bonus", j, buff);
                                    yield return null;
                                }
                            }
                        }
                    }
                    ReportWinResults(j, debuff != buff);
                }
            }
            BombModule.LogFormat("");
            BombModule.LogFormat("---------------");
            BombModule.LogFormat("Average Results");
            BombModule.LogFormat("---------------");
            BombModule.LogFormat("");
            foreach (Incidents incident in _incidents.Skip(1))
            {
                IncidentSet incidentSet = _incidedentSets[(int)incident];
                BombModule.LogFormat("Incident {0}:", incidentSet.Name);
                if (IncidentResults[incident][0] > 0)
                    BombModule.LogFormat("Boss #1 ({0}): W/L: {1:0.##}/{2:0.##} ({3} Bombs)", incidentSet.Boss1.Name, (float)IncidentResults[incident][1] / IncidentResults[incident][0], (float)IncidentResults[incident][2] / IncidentResults[incident][0], IncidentResults[incident][0]);

                if (IncidentResults[incident][3] > 0)
                    BombModule.LogFormat("Boss #2 ({0}): W/L: {1:0.##}/{2:0.##} ({3} Bombs)", incidentSet.Boss2.Name, (float)IncidentResults[incident][4] / IncidentResults[incident][3], (float)IncidentResults[incident][5] / IncidentResults[incident][3], IncidentResults[incident][3]);
            }
            BombModule.LogFormat("");
        }
        BombModule.LogFormat("--------------");
        BombModule.LogFormat("Battle Results");
        BombModule.LogFormat("--------------");
        BombModule.LogFormat("");
        _NO_SIMULATION_RUNNING = true;
        foreach (Incidents incident in _incidents.Skip(1))
        {
            BombModule.LogFormat("--------------");
            _incident = incident;
            Initialize();
            BombModule.LogFormat("--------------");

        }

        IsItMyTurnYet++;
        IsSimulationRunning = false;
        BombModule.HandlePass();
        if (IsItMyTurnYet <= KMBombModuleExtensions.HighestConsecutiveID)
            yield break;
        yield return new WaitForSeconds(1);
        BombModule.HandleStrike();
    }

    private bool _NO_SIMULATION_RUNNING = true;
    private bool _BOTHDEFEATEDLOSS = false;
    private int _WINSBOSS1 = 0;
    private int _LOSSESBOSS1 = 0;
    private int _WINSBOSS2 = 0;
    private int _LOSSESBOSS2 = 0;

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
    Loss
}
