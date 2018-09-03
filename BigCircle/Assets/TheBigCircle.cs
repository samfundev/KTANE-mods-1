using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.RuleGenerator;
using Random = UnityEngine.Random;
using Rnd = UnityEngine.Random;

// ReSharper disable once UnusedMember.Global
// ReSharper disable once CheckNamespace
public class TheBigCircle : MonoBehaviour
{

	#region Public Variables

	public GameObject Circle;
	public KMSelectable[] Wedges;
	public MeshRenderer[] WedgeRenderers;
	public TextMesh[] WedgeTextMeshes;

	public KMBombModule BombModule;
	public KMAudio Audio;
	public KMBombInfo BombInfo;

	public KMRuleSeedable RuleSeed;

	#endregion

	#region Prviate Variables

	private bool _rotateCounterClockwise = false;
	private bool _solved = false;
	private bool _spinning = true;
	private bool _activated = false;
	private Coroutine _spinCircle = null;


	private readonly Color[] _wedgeColors = Ext.NewArray(
			"CC0000", //red
			"CC7308", //orange
			"C9CC21", //yellow
			"00FF21", //green
			"1911F3", //blue
			"CC15C6", //magenta
			"FFFFFF", //white
			"121212" //black
		)
		.Select(c => new Color(Convert.ToInt32(c.Substring(0, 2), 16) / 255f, Convert.ToInt32(c.Substring(2, 2), 16) / 255f,
			Convert.ToInt32(c.Substring(4, 2), 16) / 255f))
		.ToArray();

	private readonly string[] _colorNames =
	{
		"red",
		"orange",
		"yellow",
		"green",
		"blue",
		"magenta",
		"white",
		"black"
	};


	private WedgeColors[] _colors = (WedgeColors[]) Enum.GetValues(typeof(WedgeColors));
	private float _holdTime;
	private bool _colorblindMode;

	#endregion

	// Use this for initialization
	void Start()
	{
		BigCircleRuleGenerator.CreateRules(RuleSeed.GetRNG());
		_colorblindMode = GetComponent<KMColorblindMode>().ColorblindModeActive;
		colorLookup = BigCircleRuleGenerator.Rules;
		BombModule.GenerateLogFriendlyName();
		_rotateCounterClockwise = Rnd.value < 0.5;
		WedgeRenderers[0].material.color = Color.cyan;
		_colors.Shuffle();
		for (var i = 0; i < 8; i++)
		{
			WedgeRenderers[i].material.color = _wedgeColors[(int) _colors[i]];
			var j = _colors[i];
			var k = i;
			Wedges[i].OnInteract += delegate
			{
				Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Wedges[k].transform);
				_holdTime = 0;
				return false;
			};
			Wedges[i].OnInteractEnded += delegate { HandleWedge(j); };

			//Colorblind mode
			WedgeTextMeshes[i].gameObject.SetActive(_colorblindMode);
			WedgeTextMeshes[i].text = _colorNames[(int) _colors[i]];
			WedgeTextMeshes[i].color = _wedgeColors[(int) _colors[i]];
		}

		//BombModule.LogFormat("Colors in Clockwise order: {0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", _colors[0], _colors[1], _colors[2], _colors[3], _colors[4], _colors[5], _colors[6], _colors[7]);
		BombModule.OnActivate += delegate
		{
			_activated = true;
			_spinCircle = StartCoroutine(SpinCircle());
			StartCoroutine(UpdateSolution());
		};
	}



	bool IsBobPresent()
	{
		var bobPresent = BombInfo.GetBatteryCount() == 5 && BombInfo.GetBatteryHolderCount() == 3 &&
		                 BombInfo.IsIndicatorPresent(KMBombInfoExtensions.KnownIndicatorLabel.BOB);

		if (!bobPresent || _baseOffsetGenerated) return bobPresent;
		_baseOffsetGenerated = true;
		BombModule.LogFormat(
			"Bob, Our true lord and savior has come to save the Day.: Press any solution that would be valid for any characters present on the serial number at any time.");
		var serial = BombInfo.GetSerialNumber().ToUpperInvariant();

		BombModule.Log(_rotateCounterClockwise 
			? "Circle is spinning counter-clockwise." 
			: "Circle is spinning clockwise.");

		for (var i = 0; i < serial.Length; i++)
		{
			var index = serial.Substring(i, 1);
			var colorIndex = BigCircleRuleGenerator.SerialNumberLookup.IndexOf(index, StringComparison.Ordinal);
			if (colorIndex < 0)
			{
				BombModule.LogFormat("Unrecognized Serial number character: {0} - Passing the module now", index);
				StartCoroutine(FadeCircle(_wedgeColors[(int) WedgeColors.Red]));
				return false;
			}

			var lookup = new List<WedgeColors>(colorLookup[colorIndex / 3]);
			if (_rotateCounterClockwise == BigCircleRuleGenerator.ReverseOrderIfCounterClockwise)
				lookup.Reverse();

			if (ValidBOBColors.Any(x => x[0] == lookup[0] && x[1] == lookup[1] && x[2] == lookup[2])) continue;

			ValidBOBColors.Add(lookup.ToArray());
		}

		ValidBOBColors = ValidBOBColors.OrderBy(x => x[0].ToString()).ThenBy(x => x[1].ToString()).ToList();
		for (var i = 0; i < ValidBOBColors.Count; i++)
		{
			var lookup = ValidBOBColors[i];
			BombModule.LogFormat("Bob Solution #{3}: {0}, {1}, {2}", lookup[0], lookup[1], lookup[2], i + 1);
		}

		_currentSolution = new WedgeColors[3];
		return true;
	}

	private bool _baseOffsetGenerated;
	private int _baseOffset;

	int GetBaseOffset()
	{
		if (_baseOffsetGenerated)
			return _baseOffset;

		var total = 0;

		var customIndicatorRule = new List<string>();
		var litIndicators = BombInfo.GetOnIndicators().ToList();
		var indicators = BombInfo.GetIndicators().ToList();
		indicators.Sort();
		int score;


		foreach (var indicator in indicators)
		{
			var lit = litIndicators.Contains(indicator);
			if (lit)
			{
				litIndicators.Remove(indicator);
			}

			switch (BigCircleRuleGenerator.IndicatorRules.IndexOf(indicator))
			{
				case 5:
					score = 0;
					break;
				case 0:
				case 1:
				case 2:
					score = lit == BigCircleRuleGenerator.IndicatorLitPositive[0] ? 1 : -1;
					break;
				case 3:
				case 4:
				case 6:
				case 7:
					score = lit == BigCircleRuleGenerator.IndicatorLitPositive[1] ? 2 : -2;
					break;
				case 8:
				case 9:
				case 10:
					score = lit == BigCircleRuleGenerator.IndicatorLitPositive[2] ? 3 : -3;
					break;
				default:
					score = BigCircleRuleGenerator.AddSpecialIndicators ? 6 : -6;
					break;
			}

			if (score < 6)
				BombModule.LogFormat("{0} {1}: {2:+#;-#}", lit ? "Lit" : "Unlit", indicator, score);
			else
				customIndicatorRule.Add(string.Format("Custom indicator: {0} {1}: {2}6", lit ? "Lit" : "Unlit", indicator, BigCircleRuleGenerator.AddSpecialIndicators ? "+" : "-"));

			total += score;
		}

		var rule4Positive = BigCircleRuleGenerator.Rule4PositiveEven ? 1 : 0;
		var rule4Even = BigCircleRuleGenerator.Rule4PositiveEven ? 4 : -4;
		switch (BigCircleRuleGenerator.Rule4)
		{
			case Rule4.Batteries:
				score = BombInfo.GetBatteryCount() % 2 == rule4Positive ? -4 : 4;
				BombModule.LogFormat("{0} Batteries ({1}): {2}", BombInfo.GetBatteryCount(), score == rule4Even ? "Even" : "Odd", score);
				total += score;
				break;
			case Rule4.BatteryHolders:
				score = BombInfo.GetBatteryHolderCount() % 2 == rule4Positive ? -4 : 4;
				BombModule.LogFormat("{0} Battery Holders ({1}): {2}", BombInfo.GetBatteryHolderCount(), score == rule4Even ? "Even" : "Odd", score);
				total += score;
				break;
			case Rule4.FirstSerialDigit:
				score = BombInfo.GetSerialNumberNumbers().First() % 2 == rule4Positive ? -4 : 4;
				BombModule.LogFormat("First Serial Number Digit {0} ({1}): {2}", BombInfo.GetSerialNumberNumbers().First(), score == rule4Even ? "Even" : "Odd", score);
				total += score;
				break;
			case Rule4.LastSerialDigit:
				score = BombInfo.GetSerialNumberNumbers().Last() % 2 == rule4Positive ? -4 : 4;
				BombModule.LogFormat("Last Serial Number Digit {0} ({1}): {2}", BombInfo.GetSerialNumberNumbers().Last(), score == rule4Even ? "Even" : "Odd", score);
				total += score;
				break;
			case Rule4.PortPlates:
				score = BombInfo.GetPortPlateCount() % 2 == rule4Positive ? -4 : 4;
				BombModule.LogFormat("{0} Port Plates ({1}): {2}", BombInfo.GetPortPlateCount(), score == rule4Even ? "Even" : "Odd", score);
				total += score;
				break;
			case Rule4.TotalPorts:
				score = BombInfo.GetPortCount() % 2 == rule4Positive ? -4 : 4;
				BombModule.LogFormat("{0} Ports ({1}): {2}", BombInfo.GetPortCount(), score == rule4Even ? "Even" : "Odd", score);
				total += score;
				break;
			case Rule4.UniquePorts:
				score = BombInfo.CountUniquePorts() % 2 == rule4Positive ? -4 : 4;
				BombModule.LogFormat("{0} Unique Ports ({1}): {2}", BombInfo.CountUniquePorts(), score == rule4Even ? "Even" : "Odd", score);
				total += score;
				break;
			default:
				ForceSolve(string.Format("Unrecognized rule: {0}", BigCircleRuleGenerator.Rule4));
				return 0;
		}

		

		var dviRule = new List<string>();
		var customPorts = new List<string>();
		foreach (var plate in BombInfo.GetPortPlates())
		{
			foreach (var port in plate)
			{
				if (port == BigCircleRuleGenerator.PortRules1[0].ToString())
				{
					if (plate.Contains(BigCircleRuleGenerator.PortRules1[1].ToString()))
					{
						BombModule.Log(string.Format("Port plate with both {0} and {1}: {2}4", BigCircleRuleGenerator.PortRules1[0], BigCircleRuleGenerator.PortRules1[1], BigCircleRuleGenerator.PortRules1Positive ? "-" : "+"));
						total += BigCircleRuleGenerator.PortRules1Positive ? -4 : 4;
					}
					else
					{
						BombModule.LogFormat(string.Format("Port plate with {0}: {1}5", BigCircleRuleGenerator.PortRules1[0], BigCircleRuleGenerator.PortRules1Positive ? "+" : "-"));
						total += BigCircleRuleGenerator.PortRules1Positive ? 5 : -5;
					}

					continue;
				}

				if (port == BigCircleRuleGenerator.PortRules1[1].ToString()) continue;
				if (port == BigCircleRuleGenerator.PortRules2[0].ToString())
				{
					if (plate.Contains(BigCircleRuleGenerator.PortRules2[1].ToString()))
					{
						dviRule.Add(string.Format("Port plate with both {0} and {1}: {2}4", BigCircleRuleGenerator.PortRules2[0], BigCircleRuleGenerator.PortRules2[1], BigCircleRuleGenerator.PortRules2Positive ? "-" : "+"));
						total += BigCircleRuleGenerator.PortRules2Positive ? -4 : 4;
					}
					else
					{
						dviRule.Add(string.Format("Port plate with {0}: {1}5", BigCircleRuleGenerator.PortRules2[0], BigCircleRuleGenerator.PortRules2Positive ? "+" : "-"));
						total += BigCircleRuleGenerator.PortRules2Positive ? 5 : -5;
					}

					continue;
				}

				if (port == BigCircleRuleGenerator.PortRules2[1].ToString()) continue;
				if (port == BigCircleRuleGenerator.PortRules2[2].ToString()) continue;
				if (port == BigCircleRuleGenerator.PortRules2[3].ToString()) continue;

				customPorts.Add(string.Format("Special port {0}: {1}6", port, BigCircleRuleGenerator.AddSpecialPorts ? "+" : "-"));
				total += BigCircleRuleGenerator.AddSpecialPorts ? 6 : -6;
			}
		}

		foreach (var s in dviRule)
			BombModule.Log(s);
		foreach (var s in customIndicatorRule)
			BombModule.Log(s);
		foreach (var s in customPorts)
			BombModule.Log(s);

		_baseOffset = total;

		_baseOffsetGenerated = true;
		return _baseOffset;
	}

	WedgeColors[][] colorLookup =
	{
		new[] {WedgeColors.Red, WedgeColors.Yellow, WedgeColors.Blue},
		new[] {WedgeColors.Orange, WedgeColors.Green, WedgeColors.Magenta},
		new[] {WedgeColors.Blue, WedgeColors.Black, WedgeColors.Red},
		new[] {WedgeColors.Magenta, WedgeColors.White, WedgeColors.Orange},
		new[] {WedgeColors.Orange, WedgeColors.Blue, WedgeColors.Black},
		new[] {WedgeColors.Green, WedgeColors.Red, WedgeColors.White},
		new[] {WedgeColors.Magenta, WedgeColors.Yellow, WedgeColors.Black},
		new[] {WedgeColors.Red, WedgeColors.Orange, WedgeColors.Yellow},
		new[] {WedgeColors.Yellow, WedgeColors.Green, WedgeColors.Blue},
		new[] {WedgeColors.Blue, WedgeColors.Magenta, WedgeColors.Red},
		new[] {WedgeColors.Black, WedgeColors.White, WedgeColors.Green},
		new[] {WedgeColors.White, WedgeColors.Yellow, WedgeColors.Blue}
	};

	private List<WedgeColors[]> ValidBOBColors = new List<WedgeColors[]>();

	private void ForceSolve(string reason)
	{
		_currentState = -1;
		_currentSolution = null;
		BombModule.Log(reason);
		StartCoroutine(FadeCircle(_wedgeColors[(int) WedgeColors.Red]));
	}

	WedgeColors[] GetSolution(int solved, int twofactor)
	{

		var total = GetBaseOffset();
		if (IsBobPresent())
		{
			return null;
		}

		BombModule.LogFormat("Base total: {0}", total);


		int solvedMultiplier = BigCircleRuleGenerator.ScorePositive ? 3 : -3;
		total += solved * solvedMultiplier;
		if (solved > 0)
			BombModule.LogFormat("{0} solved modules: {2}{1}", solved, solved * solvedMultiplier, BigCircleRuleGenerator.ScorePositive ? "+" : "-");

		total += twofactor;
		foreach (var twoFA in BombInfo.GetTwoFactorCodes())
		{
			var sum = twoFA;
			while (sum >= 10 && BigCircleRuleGenerator.TwoFactorDigit == TwoFactorDigit.MostSignicant)
				sum /= 10;
			sum %= 10;
			BombModule.LogFormat("Two Factor {2} significant digit of {0}: {3}{1}", twoFA, sum, BigCircleRuleGenerator.TwoFactorDigit == TwoFactorDigit.MostSignicant ? "most" : "least", BigCircleRuleGenerator.AddTwoFactor ? "+" : "-");
		}

		BombModule.LogFormat("Total: {0}", total);

		if (total < 0)
		{
			total *= -1;
			BombModule.LogFormat("Total is Negative.  Multiplying by -1.  New number is {0}.", total);
		}

		var serial = BombInfo.GetSerialNumber().ToUpperInvariant();
		serial += serial.Substring(4, 1) + serial.Substring(3, 1) + serial.Substring(2, 1) + serial.Substring(1, 1);

		var index = serial.Substring(total % 10, 1);
		BombModule.LogFormat("Extended serial number is {0}.", serial);
		BombModule.LogFormat("Using Character {0}, which is {1}.", total % 10, index);

		var colorIndex = BigCircleRuleGenerator.SerialNumberLookup.IndexOf(index, StringComparison.Ordinal);

		if (colorIndex < 0)
		{
			ForceSolve(string.Format("Unrecognized Serial number character: {0} - Passing the module now", index));
			return null;
		}

		var lookup = new List<WedgeColors>(colorLookup[colorIndex / 3]);

		BombModule.Log(BigCircleRuleGenerator.ReverseOrderIfCounterClockwise
			? "Reverse order is spinning counter-clockwise"
			: "Reverse order if spinning clockwise.");

		if (_rotateCounterClockwise == BigCircleRuleGenerator.ReverseOrderIfCounterClockwise)
			lookup.Reverse();

		BombModule.Log(_rotateCounterClockwise 
			? "Circle is spinning counter-clockwise." 
			: "Circle is spinning clockwise.");

		BombModule.LogFormat("Solution: {0}, {1}, {2}", lookup[0], lookup[1], lookup[2]);

		return lookup.ToArray();
	}



	private int _currentState = 0;
	private WedgeColors[] _currentSolution;

	void HandleWedge(WedgeColors color)
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, BombModule.transform);

		if (_holdTime > 0.5f)
		{
			BombModule.LogFormat("Module Reset to zero buttons pressed.");
			TwitchShouldCancelCommand = true;
			StartCoroutine(RandomSpin(1));
			_currentState = 0;
			return;
		}

		try
		{
			if (!_activated)
			{
				BombModule.LogFormat("Pressed {0} before module has activated", color);
				BombModule.HandleStrike();
				return;
			}

			if (_solved)
				return;



			if (IsBobPresent())
			{
				bool result = false;
				string[] validColors;
				if (_currentState == 0)
				{
					validColors = ValidBOBColors.Select(x => x[0].ToString()).Distinct().ToArray();
					result = ValidBOBColors.Any(x => x[0] == color);
				}
				else if (_currentState == 1)
				{
					validColors = ValidBOBColors.Where(x => x[0] == _currentSolution[0]).Select(x => x[1].ToString()).Distinct()
						.ToArray();
					result = ValidBOBColors.Any(x => x[0] == _currentSolution[0] && x[1] == color);
				}
				else
				{
					validColors = ValidBOBColors.Where(x => x[0] == _currentSolution[0] && x[1] == _currentSolution[1])
						.Select(x => x[2].ToString()).Distinct().ToArray();
					result = ValidBOBColors.Any(x => x[0] == _currentSolution[0] && x[1] == _currentSolution[1] && x[2] == color);
				}

				BombModule.LogFormat("Stage {0}: Pressed {1}. Bob Expected {2}", _currentState + 1, color,
					string.Join(", ", validColors));
				if (result)
				{
					_currentSolution[_currentState] = color;
					BombModule.LogFormat("Stage {0} Correct.", _currentState + 1);
					_currentState++;
					if (_currentState != _currentSolution.Length) return;
					BombModule.LogFormat("Module Passed");
					var bobColors = ValidBOBColors.SelectMany(x => x).ToArray();
					StartCoroutine(FadeCircle(_wedgeColors[(int) bobColors[Rnd.Range(0, bobColors.Length)]]));
				}
				else
				{
					BombModule.LogFormat("Stage {0} Incorrect. Strike", _currentState + 1);
					_currentState = 0;

					BombModule.HandleStrike();
				}

				return;
			}

			if (_currentSolution == null)
				return;


			BombModule.LogFormat("Stage {0}: Pressed {1}. I Expected {2}", _currentState + 1, color,
				_currentSolution[_currentState]);
			if (color == _currentSolution[_currentState])
			{
				BombModule.LogFormat("Stage {0} Correct.", _currentState + 1);
				_currentState++;
				if (_currentState != _currentSolution.Length) return;
				BombModule.LogFormat("Module Passed");
				StartCoroutine(FadeCircle(new Color(160f / 255, 160f / 255, 160f / 255)));
			}
			else
			{
				BombModule.LogFormat("Stage {0} Incorrect. Strike", _currentState + 1);
				_currentState = 0;

				BombModule.HandleStrike();
			}
		}
		catch (Exception ex)
		{
			BombModule.LogFormat("Exception caused by {0}\n{1}", ex.Message, ex.StackTrace);
			ForceSolve("Module passed by exception");
		}
	}

	private IEnumerator FadeCircle(Color solvedColor, bool solved = true)
	{
		_solved = solved;

		float endTime = 4f;
		if (!solved)
			endTime = TwitchShouldCancelCommand ? 1.25f : 2.5f;
		var startTime = Time.time;

		var textColor = new Color(solvedColor.r, solvedColor.g, solvedColor.b, 0);

		while ((Time.time - startTime) < endTime)
		{
			var currentTime = (Time.time - startTime) / endTime;
			for (var i = 0; i < 8; i++)
			{
				WedgeRenderers[i].material.color = Color.Lerp(_wedgeColors[(int) _colors[i]], solvedColor, currentTime);
				WedgeTextMeshes[i].color = Color.Lerp(_wedgeColors[(int) _colors[i]], textColor, currentTime);
			}

			yield return null;
		}

		if (!solved) yield break;
		BombModule.HandlePass();
		_spinning = false;
	}

	private IEnumerator UnfadeCircle()
	{
		float endTime = TwitchShouldCancelCommand ? 1.25f : 2.5f;
		var startTime = Time.time;
		var solvedColor = WedgeRenderers[0].material.color;
		var textColor = new Color(solvedColor.r, solvedColor.g, solvedColor.b, 0);

		while ((Time.time - startTime) < endTime)
		{
			var currentTime = (Time.time - startTime) / endTime;
			for (var i = 0; i < 8; i++)
			{
				WedgeRenderers[i].material.color = Color.Lerp(solvedColor, _wedgeColors[(int) _colors[i]], currentTime);
				WedgeTextMeshes[i].color = Color.Lerp(textColor, _wedgeColors[(int) _colors[i]], currentTime);
			}

			yield return null;
		}
	}

	private int GetTwoFactorSum()
	{
			var sum = 0;
			foreach (var twofactor in BombInfo.GetTwoFactorCodes())
			{
				var tf = twofactor;
				while (tf >= 10 && BigCircleRuleGenerator.TwoFactorDigit == TwoFactorDigit.MostSignicant)
					tf /= 10;

				tf %= 10;

				sum += BigCircleRuleGenerator.AddTwoFactor ? tf : -tf;
			}

			return sum;
	}

	private IEnumerator UpdateSolution()
	{
		if (IsBobPresent())
		{
			while (!_solved)
			{
				_holdTime += Time.deltaTime;
				yield return null;
			}

			yield break;
		}

		var solved = BombInfo.GetSolvedModuleNames().Count;
		var twofactorsum = GetTwoFactorSum();
		BombModule.LogFormat("Getting solution for 0 modules solved and {1} Significant Two Factor digit sum of {0}", twofactorsum, BigCircleRuleGenerator.TwoFactorDigit == TwoFactorDigit.MostSignicant ? "Most" : "Least");
		_currentSolution = GetSolution(solved, twofactorsum);

		do
		{
			_holdTime += Time.deltaTime;
			yield return null;
			var solvedUpdate = BombInfo.GetSolvedModuleNames().Count;
			var twofactorUpdate = GetTwoFactorSum();
			if (_currentState > 0 || (solved == solvedUpdate && twofactorsum == twofactorUpdate))
				continue;
			if (solved != solvedUpdate)
				BombModule.LogFormat("Updating solution for {0} modules solved", solvedUpdate);
			if (twofactorsum != twofactorUpdate)
				BombModule.LogFormat("Updating solution for new {1} significant two factor digit sum of {0}", twofactorUpdate, BigCircleRuleGenerator.TwoFactorDigit == TwoFactorDigit.MostSignicant ? "most" : "least");
			solved = solvedUpdate;
			twofactorsum = twofactorUpdate;
			_currentSolution = GetSolution(solved, twofactorsum);
		} while (!_solved);
	}

	private IEnumerator SpinCircle()
	{
		do
		{
			var framerate = 1f / Time.deltaTime;
			var rotation = 6f / framerate; //6 degrees per second.
			if (_rotateCounterClockwise)
				rotation *= -1;

			var y = Circle.transform.localEulerAngles.y;
			y += rotation;
			Circle.transform.localEulerAngles = new Vector3(0, y, 0);

			yield return null;
		} while (_spinning);
	}

	public IEnumerator TwitchHandleForcedSolve()
	{
		BombModule.Log("Module passed by Forced Solve");
		return FadeCircle(_wedgeColors[(int) _colors[Rnd.Range(0, _colors.Length)]]);
	}

	private string TwitchHelpMessage =
		"Submit the correct response with !{0} press blue black red. Reset the module with !{0} reset.  (Valid colors are Red, Orange, Yellow, Green, Blue, Magenta, White, blacK). Enable colorblind mode with !{0} colorblind. You can also be silly, and cause the circle to spin at random speeds/direction and fade in and out with !{0} spin";

	private bool TwitchShouldCancelCommand;

	public IEnumerator ProcessTwitchCommand(string command)
	{
		var split = command.ToLowerInvariant().Split(new[] {" ", ","}, StringSplitOptions.RemoveEmptyEntries).ToArray();
		if (split.Length == 1 && new[] {"spin", "fade", "troll"}.Contains(split[0]))
		{
			return RandomSpin(Random.Range(3, _wedgeColors.Length + 1));
		}
		else
		{
			return ProcessCommands(split);
		}
	}

	public IEnumerator ProcessCommands(string[] split)
	{
		if (split[0].Equals("colorblind"))
		{
			yield return null;
			foreach (var tm in WedgeTextMeshes)
				tm.gameObject.SetActive(true);
			yield break;
		}

		if (split[0].Equals("reset"))
		{
			yield return null;
			Wedges[0].OnInteract();
			yield return new WaitForSeconds(1);
			Wedges[0].OnInteractEnded();
			yield break;
		}

		if (split[0].Equals("press") || split[0].Equals("submit"))
		{
			split = split.Skip(1).ToArray();
		}

		if (!split.Any())
		{
			yield return "sendtochaterror Please tell me which colors to press.";
			yield break;
		}

		var buttons = new Dictionary<string, WedgeColors>
		{
			{"red", WedgeColors.Red},
			{"r", WedgeColors.Red},
			{"orange", WedgeColors.Orange},
			{"o", WedgeColors.Orange},
			{"yellow", WedgeColors.Yellow},
			{"y", WedgeColors.Yellow},
			{"green", WedgeColors.Green},
			{"g", WedgeColors.Green},
			{"blue", WedgeColors.Blue},
			{"b", WedgeColors.Blue},
			{"magenta", WedgeColors.Magenta},
			{"m", WedgeColors.Magenta},
			{"purple", WedgeColors.Magenta},
			{"p", WedgeColors.Magenta},
			{"white", WedgeColors.White},
			{"w", WedgeColors.White},
			{"black", WedgeColors.Black},
			{"k", WedgeColors.Black}
		};

		var unknownColor = split.FirstOrDefault(x => !buttons.ContainsKey(x));
		if (!string.IsNullOrEmpty(unknownColor))
		{
			yield return string.Format(
				"sendtochaterror What did the color {0} look like again? The Only colors I know about are Red, Orange, Yellow, Green, Blue, Magenta, White, blacK",
				unknownColor);
			yield break;
		}

		yield return null;
		foreach (var c in split)
		{
			_holdTime = 0;
			Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
			yield return new WaitForSeconds(0.1f);
			HandleWedge(buttons[c]);

			if (!_solved) continue;
			yield return "solve";
			yield break;
		}
	}


	public IEnumerator RandomSpin(int timesToFade)
	{
		yield return "antitroll Sorry, I am not going to waste time spinning/fading/unfading the circle multiple times";
		if (_spinCircle != null)
			StopCoroutine(_spinCircle);

		var wedgeColors = _wedgeColors.ToList();
		wedgeColors.Add(new Color(160f / 255, 160f / 255, 160f / 255));
		wedgeColors.Shuffle();

		var spinRate = 6f;

		yield return "elevator music";
		for (var i = 0; i < timesToFade; i++)
		{
			var fade = FadeCircle(wedgeColors[i], false);

			while (fade.MoveNext())
			{
				var framerate = 1f / Time.deltaTime;
				var rotation = spinRate / framerate; //6 degrees per second.
				if (_rotateCounterClockwise)
					rotation *= -1;

				var y = Circle.transform.localEulerAngles.y;
				y += rotation;
				Circle.transform.localEulerAngles = new Vector3(0, y, 0);

				yield return fade.Current;
			}

			fade = UnfadeCircle();

			spinRate = Random.Range(6f, 120f);
			if (Random.value < 0.5f)
				spinRate *= -1;

			if (TwitchShouldCancelCommand)
				timesToFade = i - 1;

			if (i >= (timesToFade - 1))
				spinRate = 6f;

			while (fade.MoveNext())
			{
				var framerate = 1f / Time.deltaTime;
				var rotation = spinRate / framerate; //6 degrees per second.
				if (_rotateCounterClockwise)
					rotation *= -1;

				var y = Circle.transform.localEulerAngles.y;
				y += rotation;
				Circle.transform.localEulerAngles = new Vector3(0, y, 0);

				yield return fade.Current;
			}
		}

		_spinCircle = StartCoroutine(SpinCircle());
		if (!TwitchShouldCancelCommand) yield break;

		yield return "cancelled";
		TwitchShouldCancelCommand = false;
	}

	public void CreateColorArray()
	{
		var vanillarules = new[] {
			new[] {WedgeColors.Red, WedgeColors.Yellow, WedgeColors.Blue},
			new[] {WedgeColors.Orange, WedgeColors.Green, WedgeColors.Magenta},
			new[] {WedgeColors.Blue, WedgeColors.Black, WedgeColors.Red},
			new[] {WedgeColors.Magenta, WedgeColors.White, WedgeColors.Orange},
			new[] {WedgeColors.Orange, WedgeColors.Blue, WedgeColors.Black},
			new[] {WedgeColors.Green, WedgeColors.Red, WedgeColors.White},
			new[] {WedgeColors.Magenta, WedgeColors.Yellow, WedgeColors.Black},
			new[] {WedgeColors.Red, WedgeColors.Orange, WedgeColors.Yellow},
			new[] {WedgeColors.Yellow, WedgeColors.Green, WedgeColors.Blue},
			new[] {WedgeColors.Blue, WedgeColors.Magenta, WedgeColors.Red},
			new[] {WedgeColors.Black, WedgeColors.White, WedgeColors.Green},
			new[] {WedgeColors.White, WedgeColors.Yellow, WedgeColors.Blue}
		};
		var rng = new MonoRandom(1);

		var swaps = new[]
		{
			8, 33,
			57, 214,
			204, 80,
			241, 134,
			65, 177,
			130, 74,
			227, 207,
			0, 136,
			98, 279,
			192, 309,
			333, 116,
			267, 186,
		};

		var possibleColorSets = new List<WedgeColors[]>();
		for (var i = 0; i < 8; i++)
		{
			for (var j = 0; j < 8; j++)
			{
				if (j == i) continue;
				for (var k = 0; k < 8; k++)
				{
					if (k == i || k == j) continue; 
					possibleColorSets.Add(new [] {(WedgeColors)i, (WedgeColors)j, (WedgeColors)k});
				}
			}
		}

		for (var i = 0; i < 12; i++)
		{
			var temp = possibleColorSets[swaps[(i * 2) + 0]];
			possibleColorSets[swaps[(i * 2) + 0]] = possibleColorSets[swaps[(i * 2) + 1]];
			possibleColorSets[swaps[(i * 2) + 1]] = temp;
		}

		var initalarray = "var possibleColorSets = new[] {" + Environment.NewLine;
		for (var i = 0; i < 336; i++)
		{
			initalarray += "    new[] {WedgeColors." + possibleColorSets[i][0] + ", WedgeColors." + possibleColorSets[i][1] +
			               ", WedgeColors." + possibleColorSets[i][2] + "}," + Environment.NewLine;
		}
		initalarray += "};";

		Debug.Log(initalarray);

		


		var shuffled = possibleColorSets.OrderBy(x => rng.NextDouble()).ToList();

		for (var i = 0; i < vanillarules.Length; i++)
		{
			int pos = -1;
			int pos2 = -1;
			for (var j = 0; j < possibleColorSets.Count; j++)
			{
				if (possibleColorSets[j][0] == vanillarules[i][0] && possibleColorSets[j][1] == vanillarules[i][1] && possibleColorSets[j][2] == vanillarules[i][2])
					pos = j;
				if (possibleColorSets[j][0] == shuffled[i][0] && possibleColorSets[j][1] == shuffled[i][1] && possibleColorSets[j][2] == shuffled[i][2])
					pos2 = j;
			}


			//var pos = Enumerable.Range(0, possibleColorSets.Count).First(x => possibleColorSets[x][0] == vanillarules[i][0] && possibleColorSets[x][1] == vanillarules[i][1] && possibleColorSets[x][2] == vanillarules[i][2]);
			//var pos2 = Enumerable.Range(0, possibleColorSets.Count).First(x => possibleColorSets[x][0] == shuffled[i][0] && possibleColorSets[x][1] == shuffled[i][1] && possibleColorSets[x][2] == shuffled[i][2]);
			Debug.LogFormat("Set {0} is at position {1}, but needs to be at {2}", i, pos, pos2);
		}

	}
}