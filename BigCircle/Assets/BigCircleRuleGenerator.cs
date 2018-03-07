using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.RuleGenerator
{
    public partial class BigCircleRuleGenerator : AbstractRuleGenerator
    {
        public static BigCircleRuleGenerator Instance
        {
            get { return (BigCircleRuleGenerator) GetInstance<BigCircleRuleGenerator>(); }
        }

        public override string GetModuleType()
        {
            return "BigCircle";
        }

        public override void CreateRules()
        {
            List<WedgeColors> colors = new List<WedgeColors>((WedgeColors[])Enum.GetValues(typeof(WedgeColors)));
            if (!Initialized) throw new Exception("You must initialize the random number generator first");
            if (Seed == 1)
            {
                Rules = new [] {
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
            }
            else
            {
                List<WedgeColors[]> rules = new List<WedgeColors[]>();
                for (int i = 0; i < 12; i++)
                {
                    bool unique;
                    do
                    {
                        WedgeColors[] ruleToAdd = colors.OrderBy(x => NextDouble()).Take(3).ToArray();
                        unique = rules.All(x => x[0] != ruleToAdd[0] || x[1] != ruleToAdd[1] || x[2] != ruleToAdd[2]);
                        if (unique) rules.Add(ruleToAdd);
                    } while (!unique);
                }
                Rules = rules.OrderBy(x => NextDouble()).ToArray();
            }
        }

        public WedgeColors[][] Rules { get; private set; }
    }
}