using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.RuleGenerator
{
    public static class BigCircleRuleGenerator
    {
		//Organized in such a way that seed 1 rules will not break.
	    private static readonly List<WedgeColors[]> PossibleColorSets = new List<WedgeColors[]>
	    {
		    new[] {WedgeColors.Green, WedgeColors.Orange, WedgeColors.White},
		    new[] {WedgeColors.Red, WedgeColors.Orange, WedgeColors.Green},
		    new[] {WedgeColors.Red, WedgeColors.Orange, WedgeColors.Blue},
		    new[] {WedgeColors.Red, WedgeColors.Orange, WedgeColors.Magenta},
		    new[] {WedgeColors.Red, WedgeColors.Orange, WedgeColors.White},
		    new[] {WedgeColors.Red, WedgeColors.Orange, WedgeColors.Black},
		    new[] {WedgeColors.Red, WedgeColors.Yellow, WedgeColors.Orange},
		    new[] {WedgeColors.Red, WedgeColors.Yellow, WedgeColors.Green},
		    new[] {WedgeColors.Red, WedgeColors.White, WedgeColors.Blue},
		    new[] {WedgeColors.Red, WedgeColors.Yellow, WedgeColors.Magenta},
		    new[] {WedgeColors.Red, WedgeColors.Yellow, WedgeColors.White},
		    new[] {WedgeColors.Red, WedgeColors.Yellow, WedgeColors.Black},
		    new[] {WedgeColors.Red, WedgeColors.Green, WedgeColors.Orange},
		    new[] {WedgeColors.Red, WedgeColors.Green, WedgeColors.Yellow},
		    new[] {WedgeColors.Red, WedgeColors.Green, WedgeColors.Blue},
		    new[] {WedgeColors.Red, WedgeColors.Green, WedgeColors.Magenta},
		    new[] {WedgeColors.Red, WedgeColors.Green, WedgeColors.White},
		    new[] {WedgeColors.Red, WedgeColors.Green, WedgeColors.Black},
		    new[] {WedgeColors.Red, WedgeColors.Blue, WedgeColors.Orange},
		    new[] {WedgeColors.Red, WedgeColors.Blue, WedgeColors.Yellow},
		    new[] {WedgeColors.Red, WedgeColors.Blue, WedgeColors.Green},
		    new[] {WedgeColors.Red, WedgeColors.Blue, WedgeColors.Magenta},
		    new[] {WedgeColors.Red, WedgeColors.Blue, WedgeColors.White},
		    new[] {WedgeColors.Red, WedgeColors.Blue, WedgeColors.Black},
		    new[] {WedgeColors.Red, WedgeColors.Magenta, WedgeColors.Orange},
		    new[] {WedgeColors.Red, WedgeColors.Magenta, WedgeColors.Yellow},
		    new[] {WedgeColors.Red, WedgeColors.Magenta, WedgeColors.Green},
		    new[] {WedgeColors.Red, WedgeColors.Magenta, WedgeColors.Blue},
		    new[] {WedgeColors.Red, WedgeColors.Magenta, WedgeColors.White},
		    new[] {WedgeColors.Red, WedgeColors.Magenta, WedgeColors.Black},
		    new[] {WedgeColors.Red, WedgeColors.White, WedgeColors.Orange},
		    new[] {WedgeColors.Red, WedgeColors.White, WedgeColors.Yellow},
		    new[] {WedgeColors.Red, WedgeColors.White, WedgeColors.Green},
		    new[] {WedgeColors.Red, WedgeColors.Yellow, WedgeColors.Blue},
		    new[] {WedgeColors.Red, WedgeColors.White, WedgeColors.Magenta},
		    new[] {WedgeColors.Red, WedgeColors.White, WedgeColors.Black},
		    new[] {WedgeColors.Red, WedgeColors.Black, WedgeColors.Orange},
		    new[] {WedgeColors.Red, WedgeColors.Black, WedgeColors.Yellow},
		    new[] {WedgeColors.Red, WedgeColors.Black, WedgeColors.Green},
		    new[] {WedgeColors.Red, WedgeColors.Black, WedgeColors.Blue},
		    new[] {WedgeColors.Red, WedgeColors.Black, WedgeColors.Magenta},
		    new[] {WedgeColors.Red, WedgeColors.Black, WedgeColors.White},
		    new[] {WedgeColors.Orange, WedgeColors.Red, WedgeColors.Yellow},
		    new[] {WedgeColors.Orange, WedgeColors.Red, WedgeColors.Green},
		    new[] {WedgeColors.Orange, WedgeColors.Red, WedgeColors.Blue},
		    new[] {WedgeColors.Orange, WedgeColors.Red, WedgeColors.Magenta},
		    new[] {WedgeColors.Orange, WedgeColors.Red, WedgeColors.White},
		    new[] {WedgeColors.Orange, WedgeColors.Red, WedgeColors.Black},
		    new[] {WedgeColors.Orange, WedgeColors.Yellow, WedgeColors.Red},
		    new[] {WedgeColors.Orange, WedgeColors.Yellow, WedgeColors.Green},
		    new[] {WedgeColors.Orange, WedgeColors.Yellow, WedgeColors.Blue},
		    new[] {WedgeColors.Orange, WedgeColors.Yellow, WedgeColors.Magenta},
		    new[] {WedgeColors.Orange, WedgeColors.Yellow, WedgeColors.White},
		    new[] {WedgeColors.Orange, WedgeColors.Yellow, WedgeColors.Black},
		    new[] {WedgeColors.Orange, WedgeColors.Green, WedgeColors.Red},
		    new[] {WedgeColors.Orange, WedgeColors.Green, WedgeColors.Yellow},
		    new[] {WedgeColors.Orange, WedgeColors.Green, WedgeColors.Blue},
		    new[] {WedgeColors.Magenta, WedgeColors.Red, WedgeColors.White},
		    new[] {WedgeColors.Orange, WedgeColors.Green, WedgeColors.White},
		    new[] {WedgeColors.Orange, WedgeColors.Green, WedgeColors.Black},
		    new[] {WedgeColors.Orange, WedgeColors.Blue, WedgeColors.Red},
		    new[] {WedgeColors.Orange, WedgeColors.Blue, WedgeColors.Yellow},
		    new[] {WedgeColors.Orange, WedgeColors.Blue, WedgeColors.Green},
		    new[] {WedgeColors.Orange, WedgeColors.Blue, WedgeColors.Magenta},
		    new[] {WedgeColors.Orange, WedgeColors.Blue, WedgeColors.White},
		    new[] {WedgeColors.Blue, WedgeColors.Orange, WedgeColors.Magenta},
		    new[] {WedgeColors.Orange, WedgeColors.Magenta, WedgeColors.Red},
		    new[] {WedgeColors.Orange, WedgeColors.Magenta, WedgeColors.Yellow},
		    new[] {WedgeColors.Orange, WedgeColors.Magenta, WedgeColors.Green},
		    new[] {WedgeColors.Orange, WedgeColors.Magenta, WedgeColors.Blue},
		    new[] {WedgeColors.Orange, WedgeColors.Magenta, WedgeColors.White},
		    new[] {WedgeColors.Orange, WedgeColors.Magenta, WedgeColors.Black},
		    new[] {WedgeColors.Orange, WedgeColors.White, WedgeColors.Red},
		    new[] {WedgeColors.Orange, WedgeColors.White, WedgeColors.Yellow},
		    new[] {WedgeColors.Green, WedgeColors.Red, WedgeColors.White},
		    new[] {WedgeColors.Orange, WedgeColors.White, WedgeColors.Blue},
		    new[] {WedgeColors.Orange, WedgeColors.White, WedgeColors.Magenta},
		    new[] {WedgeColors.Orange, WedgeColors.White, WedgeColors.Black},
		    new[] {WedgeColors.Orange, WedgeColors.Black, WedgeColors.Red},
		    new[] {WedgeColors.Orange, WedgeColors.Black, WedgeColors.Yellow},
		    new[] {WedgeColors.Blue, WedgeColors.Black, WedgeColors.Red},
		    new[] {WedgeColors.Orange, WedgeColors.Black, WedgeColors.Blue},
		    new[] {WedgeColors.Orange, WedgeColors.Black, WedgeColors.Magenta},
		    new[] {WedgeColors.Orange, WedgeColors.Black, WedgeColors.White},
		    new[] {WedgeColors.Yellow, WedgeColors.Red, WedgeColors.Orange},
		    new[] {WedgeColors.Yellow, WedgeColors.Red, WedgeColors.Green},
		    new[] {WedgeColors.Yellow, WedgeColors.Red, WedgeColors.Blue},
		    new[] {WedgeColors.Yellow, WedgeColors.Red, WedgeColors.Magenta},
		    new[] {WedgeColors.Yellow, WedgeColors.Red, WedgeColors.White},
		    new[] {WedgeColors.Yellow, WedgeColors.Red, WedgeColors.Black},
		    new[] {WedgeColors.Yellow, WedgeColors.Orange, WedgeColors.Red},
		    new[] {WedgeColors.Yellow, WedgeColors.Orange, WedgeColors.Green},
		    new[] {WedgeColors.Yellow, WedgeColors.Orange, WedgeColors.Blue},
		    new[] {WedgeColors.Yellow, WedgeColors.Orange, WedgeColors.Magenta},
		    new[] {WedgeColors.Yellow, WedgeColors.Orange, WedgeColors.White},
		    new[] {WedgeColors.Yellow, WedgeColors.Orange, WedgeColors.Black},
		    new[] {WedgeColors.Yellow, WedgeColors.Green, WedgeColors.Red},
		    new[] {WedgeColors.Yellow, WedgeColors.Green, WedgeColors.Orange},
		    new[] {WedgeColors.White, WedgeColors.Blue, WedgeColors.Green},
		    new[] {WedgeColors.Yellow, WedgeColors.Green, WedgeColors.Magenta},
		    new[] {WedgeColors.Yellow, WedgeColors.Green, WedgeColors.White},
		    new[] {WedgeColors.Yellow, WedgeColors.Green, WedgeColors.Black},
		    new[] {WedgeColors.Yellow, WedgeColors.Blue, WedgeColors.Red},
		    new[] {WedgeColors.Yellow, WedgeColors.Blue, WedgeColors.Orange},
		    new[] {WedgeColors.Yellow, WedgeColors.Blue, WedgeColors.Green},
		    new[] {WedgeColors.Yellow, WedgeColors.Blue, WedgeColors.Magenta},
		    new[] {WedgeColors.Yellow, WedgeColors.Blue, WedgeColors.White},
		    new[] {WedgeColors.Yellow, WedgeColors.Blue, WedgeColors.Black},
		    new[] {WedgeColors.Yellow, WedgeColors.Magenta, WedgeColors.Red},
		    new[] {WedgeColors.Yellow, WedgeColors.Magenta, WedgeColors.Orange},
		    new[] {WedgeColors.Yellow, WedgeColors.Magenta, WedgeColors.Green},
		    new[] {WedgeColors.Yellow, WedgeColors.Magenta, WedgeColors.Blue},
		    new[] {WedgeColors.Yellow, WedgeColors.Magenta, WedgeColors.White},
		    new[] {WedgeColors.Yellow, WedgeColors.Magenta, WedgeColors.Black},
		    new[] {WedgeColors.Yellow, WedgeColors.White, WedgeColors.Red},
		    new[] {WedgeColors.Yellow, WedgeColors.White, WedgeColors.Orange},
		    new[] {WedgeColors.Black, WedgeColors.White, WedgeColors.Green},
		    new[] {WedgeColors.Yellow, WedgeColors.White, WedgeColors.Blue},
		    new[] {WedgeColors.Yellow, WedgeColors.White, WedgeColors.Magenta},
		    new[] {WedgeColors.Yellow, WedgeColors.White, WedgeColors.Black},
		    new[] {WedgeColors.Yellow, WedgeColors.Black, WedgeColors.Red},
		    new[] {WedgeColors.Yellow, WedgeColors.Black, WedgeColors.Orange},
		    new[] {WedgeColors.Yellow, WedgeColors.Black, WedgeColors.Green},
		    new[] {WedgeColors.Yellow, WedgeColors.Black, WedgeColors.Blue},
		    new[] {WedgeColors.Yellow, WedgeColors.Black, WedgeColors.Magenta},
		    new[] {WedgeColors.Yellow, WedgeColors.Black, WedgeColors.White},
		    new[] {WedgeColors.Green, WedgeColors.Red, WedgeColors.Orange},
		    new[] {WedgeColors.Green, WedgeColors.Red, WedgeColors.Yellow},
		    new[] {WedgeColors.Green, WedgeColors.Red, WedgeColors.Blue},
		    new[] {WedgeColors.Green, WedgeColors.Red, WedgeColors.Magenta},
		    new[] {WedgeColors.Orange, WedgeColors.White, WedgeColors.Green},
		    new[] {WedgeColors.Green, WedgeColors.Red, WedgeColors.Black},
		    new[] {WedgeColors.Green, WedgeColors.Orange, WedgeColors.Red},
		    new[] {WedgeColors.Green, WedgeColors.Orange, WedgeColors.Yellow},
		    new[] {WedgeColors.Magenta, WedgeColors.White, WedgeColors.Orange},
		    new[] {WedgeColors.Green, WedgeColors.Orange, WedgeColors.Magenta},
		    new[] {WedgeColors.Red, WedgeColors.Orange, WedgeColors.Yellow},
		    new[] {WedgeColors.Green, WedgeColors.Orange, WedgeColors.Black},
		    new[] {WedgeColors.Green, WedgeColors.Yellow, WedgeColors.Red},
		    new[] {WedgeColors.Green, WedgeColors.Yellow, WedgeColors.Orange},
		    new[] {WedgeColors.Green, WedgeColors.Yellow, WedgeColors.Blue},
		    new[] {WedgeColors.Green, WedgeColors.Yellow, WedgeColors.Magenta},
		    new[] {WedgeColors.Green, WedgeColors.Yellow, WedgeColors.White},
		    new[] {WedgeColors.Green, WedgeColors.Yellow, WedgeColors.Black},
		    new[] {WedgeColors.Green, WedgeColors.Blue, WedgeColors.Red},
		    new[] {WedgeColors.Green, WedgeColors.Blue, WedgeColors.Orange},
		    new[] {WedgeColors.Green, WedgeColors.Blue, WedgeColors.Yellow},
		    new[] {WedgeColors.Green, WedgeColors.Blue, WedgeColors.Magenta},
		    new[] {WedgeColors.Green, WedgeColors.Blue, WedgeColors.White},
		    new[] {WedgeColors.Green, WedgeColors.Blue, WedgeColors.Black},
		    new[] {WedgeColors.Green, WedgeColors.Magenta, WedgeColors.Red},
		    new[] {WedgeColors.Green, WedgeColors.Magenta, WedgeColors.Orange},
		    new[] {WedgeColors.Green, WedgeColors.Magenta, WedgeColors.Yellow},
		    new[] {WedgeColors.Green, WedgeColors.Magenta, WedgeColors.Blue},
		    new[] {WedgeColors.Green, WedgeColors.Magenta, WedgeColors.White},
		    new[] {WedgeColors.Green, WedgeColors.Magenta, WedgeColors.Black},
		    new[] {WedgeColors.Green, WedgeColors.White, WedgeColors.Red},
		    new[] {WedgeColors.Green, WedgeColors.White, WedgeColors.Orange},
		    new[] {WedgeColors.Green, WedgeColors.White, WedgeColors.Yellow},
		    new[] {WedgeColors.Green, WedgeColors.White, WedgeColors.Blue},
		    new[] {WedgeColors.Green, WedgeColors.White, WedgeColors.Magenta},
		    new[] {WedgeColors.Green, WedgeColors.White, WedgeColors.Black},
		    new[] {WedgeColors.Green, WedgeColors.Black, WedgeColors.Red},
		    new[] {WedgeColors.Green, WedgeColors.Black, WedgeColors.Orange},
		    new[] {WedgeColors.Green, WedgeColors.Black, WedgeColors.Yellow},
		    new[] {WedgeColors.Green, WedgeColors.Black, WedgeColors.Blue},
		    new[] {WedgeColors.Green, WedgeColors.Black, WedgeColors.Magenta},
		    new[] {WedgeColors.Green, WedgeColors.Black, WedgeColors.White},
		    new[] {WedgeColors.Blue, WedgeColors.Red, WedgeColors.Orange},
		    new[] {WedgeColors.Blue, WedgeColors.Red, WedgeColors.Yellow},
		    new[] {WedgeColors.Blue, WedgeColors.Red, WedgeColors.Green},
		    new[] {WedgeColors.Blue, WedgeColors.Red, WedgeColors.Magenta},
		    new[] {WedgeColors.Blue, WedgeColors.Red, WedgeColors.White},
		    new[] {WedgeColors.Blue, WedgeColors.Red, WedgeColors.Black},
		    new[] {WedgeColors.Blue, WedgeColors.Orange, WedgeColors.Red},
		    new[] {WedgeColors.Blue, WedgeColors.Orange, WedgeColors.Yellow},
		    new[] {WedgeColors.Blue, WedgeColors.Orange, WedgeColors.Green},
		    new[] {WedgeColors.Orange, WedgeColors.Blue, WedgeColors.Black},
		    new[] {WedgeColors.Blue, WedgeColors.Orange, WedgeColors.White},
		    new[] {WedgeColors.Blue, WedgeColors.Orange, WedgeColors.Black},
		    new[] {WedgeColors.Blue, WedgeColors.Yellow, WedgeColors.Red},
		    new[] {WedgeColors.Blue, WedgeColors.Yellow, WedgeColors.Orange},
		    new[] {WedgeColors.Blue, WedgeColors.Yellow, WedgeColors.Green},
		    new[] {WedgeColors.Blue, WedgeColors.Yellow, WedgeColors.Magenta},
		    new[] {WedgeColors.Blue, WedgeColors.Yellow, WedgeColors.White},
		    new[] {WedgeColors.Blue, WedgeColors.Yellow, WedgeColors.Black},
		    new[] {WedgeColors.White, WedgeColors.Yellow, WedgeColors.Blue},
		    new[] {WedgeColors.Blue, WedgeColors.Green, WedgeColors.Orange},
		    new[] {WedgeColors.Blue, WedgeColors.Green, WedgeColors.Yellow},
		    new[] {WedgeColors.Blue, WedgeColors.Green, WedgeColors.Magenta},
		    new[] {WedgeColors.Blue, WedgeColors.Green, WedgeColors.White},
		    new[] {WedgeColors.Blue, WedgeColors.Green, WedgeColors.Black},
		    new[] {WedgeColors.Black, WedgeColors.Yellow, WedgeColors.Blue},
		    new[] {WedgeColors.Blue, WedgeColors.Magenta, WedgeColors.Orange},
		    new[] {WedgeColors.Blue, WedgeColors.Magenta, WedgeColors.Yellow},
		    new[] {WedgeColors.Blue, WedgeColors.Magenta, WedgeColors.Green},
		    new[] {WedgeColors.Blue, WedgeColors.Magenta, WedgeColors.White},
		    new[] {WedgeColors.Blue, WedgeColors.Magenta, WedgeColors.Black},
		    new[] {WedgeColors.Blue, WedgeColors.White, WedgeColors.Red},
		    new[] {WedgeColors.Blue, WedgeColors.White, WedgeColors.Orange},
		    new[] {WedgeColors.Blue, WedgeColors.White, WedgeColors.Yellow},
		    new[] {WedgeColors.Blue, WedgeColors.White, WedgeColors.Green},
		    new[] {WedgeColors.Blue, WedgeColors.White, WedgeColors.Magenta},
		    new[] {WedgeColors.Blue, WedgeColors.White, WedgeColors.Black},
		    new[] {WedgeColors.Orange, WedgeColors.Black, WedgeColors.Green},
		    new[] {WedgeColors.Blue, WedgeColors.Black, WedgeColors.Orange},
		    new[] {WedgeColors.Blue, WedgeColors.Black, WedgeColors.Yellow},
		    new[] {WedgeColors.Magenta, WedgeColors.Yellow, WedgeColors.Black},
		    new[] {WedgeColors.Blue, WedgeColors.Black, WedgeColors.Magenta},
		    new[] {WedgeColors.Blue, WedgeColors.Black, WedgeColors.White},
		    new[] {WedgeColors.Magenta, WedgeColors.Red, WedgeColors.Orange},
		    new[] {WedgeColors.Magenta, WedgeColors.Red, WedgeColors.Yellow},
		    new[] {WedgeColors.Magenta, WedgeColors.Red, WedgeColors.Green},
		    new[] {WedgeColors.Magenta, WedgeColors.Red, WedgeColors.Blue},
		    new[] {WedgeColors.Orange, WedgeColors.Green, WedgeColors.Magenta},
		    new[] {WedgeColors.Magenta, WedgeColors.Red, WedgeColors.Black},
		    new[] {WedgeColors.Magenta, WedgeColors.Orange, WedgeColors.Red},
		    new[] {WedgeColors.Magenta, WedgeColors.Orange, WedgeColors.Yellow},
		    new[] {WedgeColors.Magenta, WedgeColors.Orange, WedgeColors.Green},
		    new[] {WedgeColors.Magenta, WedgeColors.Orange, WedgeColors.Blue},
		    new[] {WedgeColors.Magenta, WedgeColors.Orange, WedgeColors.White},
		    new[] {WedgeColors.Magenta, WedgeColors.Orange, WedgeColors.Black},
		    new[] {WedgeColors.Magenta, WedgeColors.Yellow, WedgeColors.Red},
		    new[] {WedgeColors.Magenta, WedgeColors.Yellow, WedgeColors.Orange},
		    new[] {WedgeColors.Magenta, WedgeColors.Yellow, WedgeColors.Green},
		    new[] {WedgeColors.Magenta, WedgeColors.Yellow, WedgeColors.Blue},
		    new[] {WedgeColors.Magenta, WedgeColors.Yellow, WedgeColors.White},
		    new[] {WedgeColors.Blue, WedgeColors.Black, WedgeColors.Green},
		    new[] {WedgeColors.Magenta, WedgeColors.Green, WedgeColors.Red},
		    new[] {WedgeColors.Magenta, WedgeColors.Green, WedgeColors.Orange},
		    new[] {WedgeColors.Magenta, WedgeColors.Green, WedgeColors.Yellow},
		    new[] {WedgeColors.Magenta, WedgeColors.Green, WedgeColors.Blue},
		    new[] {WedgeColors.Magenta, WedgeColors.Green, WedgeColors.White},
		    new[] {WedgeColors.Magenta, WedgeColors.Green, WedgeColors.Black},
		    new[] {WedgeColors.Magenta, WedgeColors.Blue, WedgeColors.Red},
		    new[] {WedgeColors.Magenta, WedgeColors.Blue, WedgeColors.Orange},
		    new[] {WedgeColors.Magenta, WedgeColors.Blue, WedgeColors.Yellow},
		    new[] {WedgeColors.Magenta, WedgeColors.Blue, WedgeColors.Green},
		    new[] {WedgeColors.Magenta, WedgeColors.Blue, WedgeColors.White},
		    new[] {WedgeColors.Magenta, WedgeColors.Blue, WedgeColors.Black},
		    new[] {WedgeColors.Magenta, WedgeColors.White, WedgeColors.Red},
		    new[] {WedgeColors.Green, WedgeColors.Orange, WedgeColors.Blue},
		    new[] {WedgeColors.Magenta, WedgeColors.White, WedgeColors.Yellow},
		    new[] {WedgeColors.Magenta, WedgeColors.White, WedgeColors.Green},
		    new[] {WedgeColors.Magenta, WedgeColors.White, WedgeColors.Blue},
		    new[] {WedgeColors.Magenta, WedgeColors.White, WedgeColors.Black},
		    new[] {WedgeColors.Magenta, WedgeColors.Black, WedgeColors.Red},
		    new[] {WedgeColors.Magenta, WedgeColors.Black, WedgeColors.Orange},
		    new[] {WedgeColors.Magenta, WedgeColors.Black, WedgeColors.Yellow},
		    new[] {WedgeColors.Magenta, WedgeColors.Black, WedgeColors.Green},
		    new[] {WedgeColors.Magenta, WedgeColors.Black, WedgeColors.Blue},
		    new[] {WedgeColors.Magenta, WedgeColors.Black, WedgeColors.White},
		    new[] {WedgeColors.White, WedgeColors.Red, WedgeColors.Orange},
		    new[] {WedgeColors.White, WedgeColors.Red, WedgeColors.Yellow},
		    new[] {WedgeColors.White, WedgeColors.Red, WedgeColors.Green},
		    new[] {WedgeColors.White, WedgeColors.Red, WedgeColors.Blue},
		    new[] {WedgeColors.White, WedgeColors.Red, WedgeColors.Magenta},
		    new[] {WedgeColors.White, WedgeColors.Red, WedgeColors.Black},
		    new[] {WedgeColors.White, WedgeColors.Orange, WedgeColors.Red},
		    new[] {WedgeColors.White, WedgeColors.Orange, WedgeColors.Yellow},
		    new[] {WedgeColors.White, WedgeColors.Orange, WedgeColors.Green},
		    new[] {WedgeColors.White, WedgeColors.Orange, WedgeColors.Blue},
		    new[] {WedgeColors.White, WedgeColors.Orange, WedgeColors.Magenta},
		    new[] {WedgeColors.White, WedgeColors.Orange, WedgeColors.Black},
		    new[] {WedgeColors.White, WedgeColors.Yellow, WedgeColors.Red},
		    new[] {WedgeColors.White, WedgeColors.Yellow, WedgeColors.Orange},
		    new[] {WedgeColors.White, WedgeColors.Yellow, WedgeColors.Green},
		    new[] {WedgeColors.Blue, WedgeColors.Green, WedgeColors.Red},
		    new[] {WedgeColors.White, WedgeColors.Yellow, WedgeColors.Magenta},
		    new[] {WedgeColors.White, WedgeColors.Yellow, WedgeColors.Black},
		    new[] {WedgeColors.White, WedgeColors.Green, WedgeColors.Red},
		    new[] {WedgeColors.White, WedgeColors.Green, WedgeColors.Orange},
		    new[] {WedgeColors.White, WedgeColors.Green, WedgeColors.Yellow},
		    new[] {WedgeColors.White, WedgeColors.Green, WedgeColors.Blue},
		    new[] {WedgeColors.White, WedgeColors.Green, WedgeColors.Magenta},
		    new[] {WedgeColors.White, WedgeColors.Green, WedgeColors.Black},
		    new[] {WedgeColors.White, WedgeColors.Blue, WedgeColors.Red},
		    new[] {WedgeColors.White, WedgeColors.Blue, WedgeColors.Orange},
		    new[] {WedgeColors.White, WedgeColors.Blue, WedgeColors.Yellow},
		    new[] {WedgeColors.Yellow, WedgeColors.Green, WedgeColors.Blue},
		    new[] {WedgeColors.White, WedgeColors.Blue, WedgeColors.Magenta},
		    new[] {WedgeColors.White, WedgeColors.Blue, WedgeColors.Black},
		    new[] {WedgeColors.White, WedgeColors.Magenta, WedgeColors.Red},
		    new[] {WedgeColors.White, WedgeColors.Magenta, WedgeColors.Orange},
		    new[] {WedgeColors.White, WedgeColors.Magenta, WedgeColors.Yellow},
		    new[] {WedgeColors.White, WedgeColors.Magenta, WedgeColors.Green},
		    new[] {WedgeColors.White, WedgeColors.Magenta, WedgeColors.Blue},
		    new[] {WedgeColors.White, WedgeColors.Magenta, WedgeColors.Black},
		    new[] {WedgeColors.White, WedgeColors.Black, WedgeColors.Red},
		    new[] {WedgeColors.White, WedgeColors.Black, WedgeColors.Orange},
		    new[] {WedgeColors.White, WedgeColors.Black, WedgeColors.Yellow},
		    new[] {WedgeColors.White, WedgeColors.Black, WedgeColors.Green},
		    new[] {WedgeColors.White, WedgeColors.Black, WedgeColors.Blue},
		    new[] {WedgeColors.White, WedgeColors.Black, WedgeColors.Magenta},
		    new[] {WedgeColors.Black, WedgeColors.Red, WedgeColors.Orange},
		    new[] {WedgeColors.Black, WedgeColors.Red, WedgeColors.Yellow},
		    new[] {WedgeColors.Black, WedgeColors.Red, WedgeColors.Green},
		    new[] {WedgeColors.Black, WedgeColors.Red, WedgeColors.Blue},
		    new[] {WedgeColors.Black, WedgeColors.Red, WedgeColors.Magenta},
		    new[] {WedgeColors.Black, WedgeColors.Red, WedgeColors.White},
		    new[] {WedgeColors.Black, WedgeColors.Orange, WedgeColors.Red},
		    new[] {WedgeColors.Black, WedgeColors.Orange, WedgeColors.Yellow},
		    new[] {WedgeColors.Black, WedgeColors.Orange, WedgeColors.Green},
		    new[] {WedgeColors.Black, WedgeColors.Orange, WedgeColors.Blue},
		    new[] {WedgeColors.Black, WedgeColors.Orange, WedgeColors.Magenta},
		    new[] {WedgeColors.Black, WedgeColors.Orange, WedgeColors.White},
		    new[] {WedgeColors.Black, WedgeColors.Yellow, WedgeColors.Red},
		    new[] {WedgeColors.Black, WedgeColors.Yellow, WedgeColors.Orange},
		    new[] {WedgeColors.Black, WedgeColors.Yellow, WedgeColors.Green},
		    new[] {WedgeColors.Blue, WedgeColors.Magenta, WedgeColors.Red},
		    new[] {WedgeColors.Black, WedgeColors.Yellow, WedgeColors.Magenta},
		    new[] {WedgeColors.Black, WedgeColors.Yellow, WedgeColors.White},
		    new[] {WedgeColors.Black, WedgeColors.Green, WedgeColors.Red},
		    new[] {WedgeColors.Black, WedgeColors.Green, WedgeColors.Orange},
		    new[] {WedgeColors.Black, WedgeColors.Green, WedgeColors.Yellow},
		    new[] {WedgeColors.Black, WedgeColors.Green, WedgeColors.Blue},
		    new[] {WedgeColors.Black, WedgeColors.Green, WedgeColors.Magenta},
		    new[] {WedgeColors.Black, WedgeColors.Green, WedgeColors.White},
		    new[] {WedgeColors.Black, WedgeColors.Blue, WedgeColors.Red},
		    new[] {WedgeColors.Black, WedgeColors.Blue, WedgeColors.Orange},
		    new[] {WedgeColors.Black, WedgeColors.Blue, WedgeColors.Yellow},
		    new[] {WedgeColors.Black, WedgeColors.Blue, WedgeColors.Green},
		    new[] {WedgeColors.Black, WedgeColors.Blue, WedgeColors.Magenta},
		    new[] {WedgeColors.Black, WedgeColors.Blue, WedgeColors.White},
		    new[] {WedgeColors.Black, WedgeColors.Magenta, WedgeColors.Red},
		    new[] {WedgeColors.Black, WedgeColors.Magenta, WedgeColors.Orange},
		    new[] {WedgeColors.Black, WedgeColors.Magenta, WedgeColors.Yellow},
		    new[] {WedgeColors.Black, WedgeColors.Magenta, WedgeColors.Green},
		    new[] {WedgeColors.Black, WedgeColors.Magenta, WedgeColors.Blue},
		    new[] {WedgeColors.Black, WedgeColors.Magenta, WedgeColors.White},
		    new[] {WedgeColors.Black, WedgeColors.White, WedgeColors.Red},
		    new[] {WedgeColors.Black, WedgeColors.White, WedgeColors.Orange},
		    new[] {WedgeColors.Black, WedgeColors.White, WedgeColors.Yellow},
		    new[] {WedgeColors.Yellow, WedgeColors.White, WedgeColors.Green},
		    new[] {WedgeColors.Black, WedgeColors.White, WedgeColors.Blue},
		    new[] {WedgeColors.Black, WedgeColors.White, WedgeColors.Magenta},
	    };

	    private static readonly string[] Indicators =
	    {
		      "SIG", "BOB", "TRN", "CAR", "CLR", "NSA", "MSA", "SND", "IND", "FRQ", "FRK"
		    //"CAR", "NSA", "SND", "FRK", "MSA", "SIG", "CLR", "TRN", "IND", "FRQ", "BOB"
	    };

	    private static readonly KMBombInfoExtensions.KnownPortType[] PortSet1 =
	    {
		    KMBombInfoExtensions.KnownPortType.Parallel, KMBombInfoExtensions.KnownPortType.Serial
	    };

	    private static readonly KMBombInfoExtensions.KnownPortType[] PortSet2 =
	    {
		    KMBombInfoExtensions.KnownPortType.DVI, KMBombInfoExtensions.KnownPortType.StereoRCA, KMBombInfoExtensions.KnownPortType.PS2, KMBombInfoExtensions.KnownPortType.RJ45
	    };

	    private static MonoRandom _rng;

		public static void CreateRules(MonoRandom rng)
		{
			if (_rng != null && _rng.Seed == rng.Seed) return;
			_rng = rng;

			Rules = PossibleColorSets.OrderBy(x => rng.NextDouble()).Take(12).ToArray();
			IndicatorRules = Indicators.OrderBy(x => rng.NextDouble()).ToList();
			IndicatorLitPositive = new[] {rng.Next(0, 2) == 1, rng.Next(0, 2) == 1, rng.Next(0, 2) == 1};
			ScorePositive = rng.Next(0, 2) == 1;

			PortRules1 = PortSet1.OrderBy(x => rng.NextDouble()).ToArray();
			PortRules1Positive = rng.Next(0, 2) == 1;

			PortRules2 = PortSet2.OrderBy(x => rng.NextDouble()).ToArray();
			PortRules2Positive = rng.Next(0, 2) == 1;

			Rule4 = (Rule4) rng.Next(7);
			Rule4PositiveEven = rng.Next(0, 2) == 1;

			AddSpecialIndicators = rng.Next(0, 2) == 1;
			AddSpecialPorts = rng.Next(0, 2) == 1;

			TwoFactorDigit = (TwoFactorDigit) rng.Next(2);
			AddTwoFactor = rng.Next(0, 2) == 1;

			ReverseOrderIfCounterClockwise = rng.Next(0, 2) == 0;

			                                   //"XCHLFQR9EDPM15OGBYV8SN0643IJZTUKA27W"
			                                   //"MCXPODNYJ7WG1984F2QRV3BLEA56KTUIZ0HS"
			SerialNumberLookup = string.Join("", "MCXPODNYJ7WG1984F2QRV3BLEA56KTUIZ0HS".OrderBy(x => rng.NextDouble()).Select(x => x.ToString()).ToArray());


			Debug.LogFormat("For each {0}, {1}, or {2} indicator: {3}1 if lit, {4}1 if unlit", IndicatorRules[0],
				IndicatorRules[1], IndicatorRules[2], IndicatorLitPositive[0] ? "+" : "-", IndicatorLitPositive[0] ? "-" : "+");
			Debug.LogFormat("For each {0}, {1}, {2}, or {3} indicator: {4}2 if lit, {5}2 if unlit", IndicatorRules[3],
				IndicatorRules[4], IndicatorRules[6], IndicatorRules[7], IndicatorLitPositive[1] ? "+" : "-", IndicatorLitPositive[1] ? "-" : "+");
			Debug.LogFormat("For each {0}, {1}, or {2} indicator: {3}3 if lit, {4}3 if unlit", IndicatorRules[8],
				IndicatorRules[9], IndicatorRules[10], IndicatorLitPositive[2] ? "+" : "-", IndicatorLitPositive[2] ? "-" : "+");


			Debug.LogFormat("For each Solved module: {0}", ScorePositive ? "+3" : "-3");

			Debug.LogFormat("{0}: {1}4 for odd, {2}4 for even", Rule4, Rule4PositiveEven ? "-" : "+", Rule4PositiveEven ? "+" : "-");

			Debug.LogFormat("There are port plates with {0}: {1}5 each,  {2}4 if paired with {3}", PortRules1[0], PortRules1Positive ? "+" : "-", PortRules1Positive ? "-" : "+", PortRules1[1]);
			Debug.LogFormat("There are port plates with {0}: {1}5 each,  {2}4 if paired with {3}", PortRules2[0], PortRules2Positive ? "+" : "-", PortRules2Positive ? "-" : "+", PortRules2[1]);

			Debug.LogFormat("for each special indicator: {0}6 each.", AddSpecialIndicators ? "+" : "-");
			Debug.LogFormat("for each special port: {0}6 each.", AddSpecialPorts ? "+" : "-");

			Debug.LogFormat("For each Two Factor code: {0} the {1}", AddTwoFactor ? "Add" : "Subtract", TwoFactorDigit);

			for (var i = 0; i < 12; i++)
			{
				Debug.LogFormat("{0}, {1}, {2}: {3}, {4}, {5}", SerialNumberLookup[(i * 3) + 0], SerialNumberLookup[(i * 3) + 1], SerialNumberLookup[(i * 3) + 2], Rules[i][0], Rules[i][1],
					Rules[i][2]);
			}

			Debug.LogFormat("If circle is spinning {0}, reverse order of button presses.", ReverseOrderIfCounterClockwise ? "counter-clockwise" : "clockwise");

		}

        public static WedgeColors[][] Rules { get; private set; }
		public static string SerialNumberLookup { get; private set; }
		public static List<string> IndicatorRules { get; private set; }
		public static bool[] IndicatorLitPositive { get; private set; }
		public static bool ScorePositive { get; private set; }
		public static KMBombInfoExtensions.KnownPortType[] PortRules1 { get; private set; }
		public static bool PortRules1Positive { get; private set; }
		public static KMBombInfoExtensions.KnownPortType[] PortRules2 { get; private set; }
		public static bool PortRules2Positive { get; private set; }
		public static Rule4 Rule4 { get; private set; }
		public static bool Rule4PositiveEven { get; private set; }

		public static bool AddSpecialIndicators { get; private set; }
		public static bool AddSpecialPorts { get; private set; }

		public static TwoFactorDigit TwoFactorDigit { get; private set; }
		public static bool AddTwoFactor { get; private set; }

		public static bool ReverseOrderIfCounterClockwise { get; private set; }
    }

	public enum Rule4
	{
		BatteryHolders,
		PortPlates,
		UniquePorts,
		Batteries,
		TotalPorts,
		FirstSerialDigit,
		LastSerialDigit,
	}

	public enum TwoFactorDigit
	{
		MostSignicant,
		LeastSignificant,
	}
}