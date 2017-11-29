using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Rules;

namespace VanillaRuleModifierAssembly.RuleSetGenerators
{
    public class MorseCodeRuleGenerator : AbstractRuleSetGenerator
    {
        public MorseCodeRuleGenerator()
        {
            PossibleFrequencies = PossibleFrequencies.Distinct().ToList();
            PossibleWords = PossibleWords.Distinct().ToList();
            if (PossibleFrequencies.Count < NumFrequenciesUsed || PossibleWords.Count < NumFrequenciesUsed)
            {
                throw new Exception("Not enough frequencies or words to satisfy desired rule set size!");
            }
        }

        // Token: 0x06003334 RID: 13108 RVA: 0x0011637C File Offset: 0x0011477C
        protected override AbstractRuleSet CreateRules(bool useDefault)
        {
            var dictionary = new Dictionary<int, string>();
            var list = new List<int>(PossibleFrequencies);
            var list2 = new List<string>(PossibleWords);
            if (Seed > 2)
            {
                list2.AddRange(PasswordRuleGenerator.Possibilities);
                list2.AddRange(ExtendedWords);
                list2 = list2.Distinct().ToList();
            }
            for (var i = 0; i < NumFrequenciesUsed; i++)
            {
                var index = rand.Next(0, list.Count);
                var key = list[index];
                list.RemoveAt(index);
                var index2 = rand.Next(0, list2.Count);
                var value = list2[index2];
                list2.RemoveAt(index2);
                dictionary.Add(key, value);

            }
            return new MorseCodeRuleSet(dictionary);
        }

        public MorseCodeRuleSet GenerateMorseCodeRuleSet(int seed)
        {
            Seed = seed;
            return (MorseCodeRuleSet) GenerateRuleSet(seed);
        }


        protected int Seed;
        protected static readonly int NumFrequenciesUsed = 16;
        protected List<int> PossibleFrequencies = new List<int>
        {
            502,505,512,515,
            522,525,532,535,
            542,545,552,555,
            562,565,572,575,
            582,585,592,595,
            600
        };

        protected List<string> PossibleWords = new List<string>
        {
            "trick","bravo","vector","brain",
            "boxes","alien","beats","bombs",
            "sting","steak","leaks","verse",
            "brick","break","hello","halls",
            "shell","bistro","strobe","slick",
            "flick"
        };

        protected List<string> ExtendedWords = new List<string>
        {
            "strike", "tango", "timer", "penguin", "elias", "manual", "zulu",
            "november", "kaboom", "unicorn", "quebec", "bashly", "slick", "victor",
            "timwi", "kitty", "bitstim", "market", "oxtrot", "foxtrot","hexicube",
            "lthummus","caitsith2","samfun","rexkix"
        };
    }
}