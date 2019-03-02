using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Rules;

namespace VanillaRuleModifierAssembly.RuleSetGenerators
{
    public class WireSetRuleGenerator : AbstractRuleSetGenerator
    {
        public WireSequenceRuleSet GenerateWireSequenceRules(int seed)
        {
            RuleSet = (WireSequenceRuleSet)GenerateRuleSet(seed);
            return RuleSet;
        }

        protected override AbstractRuleSet CreateRules(bool useDefault)
        {
            var redWires = new IList<int>[WireSequenceRuleSetGenerator.NumWiresPerColour];
            var blueWires = new IList<int>[WireSequenceRuleSetGenerator.NumWiresPerColour];
            var blackWires = new IList<int>[WireSequenceRuleSetGenerator.NumWiresPerColour];
            if (useDefault)
            {
                PopulateEmptySolution(redWires);
                PopulateEmptySolution(blueWires);
                PopulateEmptySolution(blackWires);
                redWires[0].Add(2);
                redWires[1].Add(1);
                redWires[2].Add(0);
                redWires[3].Add(0);
                redWires[3].Add(2);
                redWires[4].Add(1);
                redWires[5].Add(0);
                redWires[5].Add(2);
                redWires[6].Add(0);
                redWires[6].Add(1);
                redWires[6].Add(2);
                redWires[7].Add(0);
                redWires[7].Add(1);
                redWires[8].Add(1);
                blueWires[0].Add(1);
                blueWires[1].Add(0);
                blueWires[1].Add(2);
                blueWires[2].Add(1);
                blueWires[3].Add(0);
                blueWires[4].Add(1);
                blueWires[5].Add(1);
                blueWires[5].Add(2);
                blueWires[6].Add(2);
                blueWires[7].Add(0);
                blueWires[7].Add(2);
                blueWires[8].Add(0);
                blackWires[0].Add(0);
                blackWires[0].Add(1);
                blackWires[0].Add(2);
                blackWires[1].Add(0);
                blackWires[1].Add(2);
                blackWires[2].Add(1);
                blackWires[3].Add(0);
                blackWires[3].Add(2);
                blackWires[4].Add(1);
                blackWires[5].Add(1);
                blackWires[5].Add(2);
                blackWires[6].Add(0);
                blackWires[6].Add(1);
                blackWires[7].Add(2);
                blackWires[8].Add(2);
            }

            else
            {
                PopulateSolution(redWires);
                PopulateSolution(blueWires);
                PopulateSolution(blackWires);
            }

            var ruleset = new WireSequenceRuleSet(redWires, blueWires, blackWires);
            for (var i = 0; i < WireSequenceRuleSetGenerator.NumWiresPerColour; i++)
            {
                if (ruleset.redWiresToSnip[i].Contains(3))
                {
                    ruleset.redWiresToSnip[i].Clear();
                    ruleset.RedTerms[i] = "";
                }
                if (ruleset.blueWiresToSnip[i].Contains(3))
                {
                    ruleset.blueWiresToSnip[i].Clear();
                    ruleset.BlueTerms[i] = "";
                }
                if (ruleset.blackWiresToSnip[i].Contains(3))
                {
                    ruleset.blackWiresToSnip[i].Clear();
                    ruleset.BlackTerms[i] = "";
                }
            }

            return ruleset;
        }

        protected void PopulateSolution(IList<int>[] wiresToSnip)
        {
            for (var i = 0; i < wiresToSnip.Length; i++)
            {
                wiresToSnip[i] = new List<int>(WireSequenceRuleSetGenerator.NUM_COLOURS);

                //while (wiresToSnip[i].Count == 0)
                //{
                    for (var j = 0; j < WireSequenceRuleSetGenerator.NUM_COLOURS; j++)
                    {
                        if (rand.NextDouble() > 0.55)
                        {
                            wiresToSnip[i].Add(j);
                        }
                    }

                if (wiresToSnip[i].Count == 0)
                {
                    wiresToSnip[i].Add(0);
                    wiresToSnip[i].Add(3);
                }

                //}
            }
        }

        protected void PopulateEmptySolution(IList<int>[] wiresToSnip)
        {
            for (var i = 0; i < wiresToSnip.Length; i++)
            {
                wiresToSnip[i] = new List<int>(WireSequenceRuleSetGenerator.NumWiresPerColour);
            }
        }

        protected WireSequenceRuleSet RuleSet;
    }
}