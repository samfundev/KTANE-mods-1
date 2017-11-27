using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Rules;

namespace VanillaRuleModifierAssembly
{
    public class ButtonRuleGenerator
    {

        public static ButtonRuleGenerator Instance => instance ?? (instance = new ButtonRuleGenerator());

        public AbstractRuleSet GenerateRules(int seed)
        {
            var generator = new ButtonRuleSetGenerator();
            var solutionWeightsField = generator.GetType().GetField("solutionWeights", BindingFlags.NonPublic | BindingFlags.Instance);
            var queryPropertyWeightsField = generator.GetType().GetField("queryPropertyWeights", BindingFlags.NonPublic | BindingFlags.Instance);
            var createRulesMethod = generator.GetType().GetMethod("CreateRules", BindingFlags.NonPublic | BindingFlags.Instance);

            ruleSet = (ButtonRuleSet) generator.GenerateRuleSet(seed);
            if (seed == 1 || seed == 2 || solutionWeightsField == null || queryPropertyWeightsField == null || createRulesMethod == null)
                return ruleSet; //I think the ltHummus hack has some rules that would be overwritten by this generator if allowed to continue.
            while (!DoesRuleSetHaveSevenRules())
            {
                ((Dictionary<Solution, float>)solutionWeightsField.GetValue(generator))?.Clear();
                ((Dictionary<QueryableProperty, float>)queryPropertyWeightsField.GetValue(generator))?.Clear();
                ruleSet = (ButtonRuleSet) createRulesMethod.Invoke(generator, new object[] {false});
            }
            return ruleSet;
        }

        public bool DoesRuleSetHaveSevenRules()
        {

            var lastrule = ruleSet.RuleList.Last();
            var remainder = ruleSet.RuleList.Take(ruleSet.RuleList.Count - 1).ToList();

            if (lastrule.GetSolutionString().Equals(remainder.Last().GetSolutionString()))
                return false;

            var twobattery = -1;
            var onebattery = -1;
            var solution = string.Empty;
            var samesolution = true;

            for (var i = remainder.Count - 1; i >= 0; i--)
            {
                if (remainder[i].GetQueryString().Contains("more than 2 batteries"))
                {
                    
                    twobattery = i;
                    if (onebattery > -1)
                    {
                        samesolution &= remainder[i].GetSolutionString().Equals(solution);
                        break;
                    }
                    solution = remainder[i].GetSolutionString();
                }
                else if (remainder[i].GetQueryString().Contains("more than 1 battery"))
                {
                    onebattery = i;
                    if (twobattery > -1)
                    {
                        samesolution &= remainder[i].GetSolutionString().Equals(solution);
                        break;
                    }
                    solution = remainder[i].GetSolutionString();
                }
                if (solution == string.Empty)
                    continue;
                samesolution &= remainder[i].GetSolutionString().Equals(solution);
            }
            if (onebattery < twobattery && remainder[onebattery].Queries.Count == 1)
                return false;
            if (remainder[onebattery].Queries.Count == 1 && remainder[twobattery].Queries.Count == 1 && samesolution)
                return false;

            return true;
        }

        protected static ButtonRuleGenerator instance;
        
        public ButtonRuleSet ruleSet;
    }
}