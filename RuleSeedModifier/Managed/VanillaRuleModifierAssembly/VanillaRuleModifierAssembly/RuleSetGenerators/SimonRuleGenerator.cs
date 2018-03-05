using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Rules;
using BombGame;

namespace VanillaRuleModifierAssembly.RuleSetGenerators
{
    public class SimonRuleGenerator : AbstractRuleSetGenerator
    {
        protected override AbstractRuleSet CreateRules(bool useDefault)
        {
            var simonRuleSet = new SimonRuleSet
            {
                RuleList = new Dictionary<string, List<SimonColor[]>>()
            };
            if (useDefault)
            {
                AddDefaultRules(simonRuleSet);
            }
            else
            {
                AddRules(simonRuleSet, "HASVOWEL", MaxStrikes);
                AddRules(simonRuleSet, "OTHERWISE", MaxStrikes);
            }
            return simonRuleSet;
        }

        public SimonRuleSet GenerateSimonRuleSet(int seed)
        {
            return (SimonRuleSet)GenerateRuleSet(seed);
        }

        protected void AddRules(SimonRuleSet ruleSet, string key, int num)
        {
            ruleSet.RuleList.Add(key, new List<SimonColor[]>());
            for (var i = 0; i <= num; i++)
            {
                
                var array = new SimonColor[Enum.GetNames(typeof(SimonColor)).Length];
                for (var j = 0; j < Enum.GetNames(typeof(SimonColor)).Length; j++)
                {
                    array[j] = (SimonColor)rand.Next(Enum.GetNames(typeof(SimonColor)).Length);
                }
                if (CommonReflectedTypeInfo.IsModdedSeed)
                {
                    while (array.Distinct().Count() == 1)
                    {
                        array[rand.Next(Enum.GetNames(typeof(SimonColor)).Length)] = (SimonColor) rand.Next(Enum.GetNames(typeof(SimonColor)).Length);
                    }
                }
                ruleSet.RuleList[key].Add(array);
            }
        }

        protected void AddDefaultRules(SimonRuleSet ruleSet)
        {
            ruleSet.RuleList.Add("HASVOWEL", new List<SimonColor[]>());
            ruleSet.RuleList["HASVOWEL"].Add(new[]
            {
                SimonColor.Blue,
                SimonColor.Red,
                SimonColor.Yellow,
                SimonColor.Green
            });
            ruleSet.RuleList["HASVOWEL"].Add(new[]
            {
                SimonColor.Yellow,
                SimonColor.Green,
                SimonColor.Blue,
                SimonColor.Red
            });
            ruleSet.RuleList["HASVOWEL"].Add(new[]
            {
                SimonColor.Green,
                SimonColor.Red,
                SimonColor.Yellow,
                SimonColor.Blue
            });
            ruleSet.RuleList.Add("OTHERWISE", new List<SimonColor[]>());
            ruleSet.RuleList["OTHERWISE"].Add(new[]
            {
                SimonColor.Blue,
                SimonColor.Yellow,
                SimonColor.Green,
                SimonColor.Red
            });
            ruleSet.RuleList["OTHERWISE"].Add(new[]
            {
                SimonColor.Red,
                SimonColor.Blue,
                SimonColor.Yellow,
                SimonColor.Green
            });
            ruleSet.RuleList["OTHERWISE"].Add(new[]
            {
                SimonColor.Yellow,
                SimonColor.Green,
                SimonColor.Blue,
                SimonColor.Red
            });
        }
        private const int MaxStrikes = 2;
    }
}