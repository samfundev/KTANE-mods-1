using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Rules;

namespace VanillaRuleModifierAssembly.RuleSetGenerators
{
    public class PasswordRuleGenerator : AbstractRuleSetGenerator
    {
        protected override AbstractRuleSet CreateRules(bool useDefault)
        {
            return new PasswordRuleSet(Possibilities.GetRange(useDefault ? 0 : 35, 35).OrderBy(x => x).ToList());
        }

        public PasswordRuleSet GeneratePasswordRules(int seed)
        {
            var ruleset = (PasswordRuleSet) GenerateRuleSet(seed);
            if (seed > 2)
                ruleset.possibilities = Possibilities.Distinct().OrderBy(x => rand.Next()).Take(35).OrderBy(x => x).ToList();
            return ruleset;
        }

        

        public static List<string> Possibilities = new List<string>
        {
            "there","which","their","other","about","these","would",
            "write","could","first","water","sound","place","after",
            "thing","think","great","where","right","three","small",
            "large","again","spell","house","point","found","study",
            "still","learn","world","every","below","plant","never",

            "aloof","arena","bleat","boxed","butts","caley","crate",
            "feret","freak","humus","jewel","joule","joust","knobs",
            "local","pause","press","prime","rings","sails","snake",
            "space","splat","spoon","steel","tangy","texas","these",
            "those","toons","tunes","walks","weird","wodar","words",

            "quick","timwi","pingu","thief","cluck","rubik","ktane",
            "phone","decoy","debit","death","fails","flunk","flush",
            "games","gangs","goals","hotel","india","joker","lemon",
            "level","maker","mains","major","noble","noose","obese",
            "olive","paste","party","peace","quest","quack","radar",

            "react","ready","spawn","safer","scoop","ulcer","unban",
            "unite","vinyl","virus","wagon","wrong","xerox","yawns",
            "years","youth","zilch","zones"
        };
    }
}