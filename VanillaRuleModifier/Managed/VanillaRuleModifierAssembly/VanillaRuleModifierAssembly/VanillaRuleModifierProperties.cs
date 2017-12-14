using System;

namespace VanillaRuleModifierAssembly
{
    public class VanillaRuleModifierProperties : PropertiesBehaviour
    {
        public VanillaRuleModifierProperties()
        {
            AddProperty("RuleSeed", new Property(RuleSeed_Get, RuleSeed_Set));
            AddProperty("IsSeedVanilla", new Property(IsSeedVanilla_Get, null));
            AddProperty("IsSeedModded", new Property(IsSeedModded_Get, null));
            AddProperty("GetRuleManual",new Property(RuleManaul_Get, null));
        }

        private object RuleSeed_Get()
        {
            return VanillaRuleModifer._modSettings.Settings.RuleSeed;
        }

        private void RuleSeed_Set(object seed)
        {
            const string invalidArguments = "Arguments need to be either (int seed) or (object[2] {int seed, bool save}";
            if (VanillaRuleModifer.CurrentState != KMGameInfo.State.Setup && VanillaRuleModifer.CurrentState != KMGameInfo.State.PostGame)
                throw new Exception("Setting of Rule seed is only allowed duing Setup or Post game.");

            var saveSettings = false;
            int ruleseed;
            if (seed == null)
                throw new ArgumentException(invalidArguments);
            var objects = seed as object[];
            if (objects != null)
            {
                var array = objects;
                if (array.Length != 2 || !(array[0] is int) || !(array[1] is bool))
                    throw new ArgumentException(invalidArguments);
                ruleseed = (int) array[0];
                saveSettings = (bool) array[1];
            }
            else if (seed is int)
            {
                ruleseed = (int) seed;
            }
            else
            {
                throw new ArgumentException(invalidArguments);
            }
            VanillaRuleModifer.SetRuleSeed(ruleseed, saveSettings);

        }

        private object IsSeedVanilla_Get()
        {
            return VanillaRuleModifer.IsSeedVanilla();
        }

        private object IsSeedModded_Get()
        {
            return !VanillaRuleModifer.IsSeedVanilla();
        }

        private object RuleManaul_Get()
        {
            if (VanillaRuleModifer.CurrentState != KMGameInfo.State.Setup && VanillaRuleModifer.CurrentState != KMGameInfo.State.PostGame)
                throw new Exception("Getting the manual can only be done duing setup or postgame.");
            return VanillaRuleModifer.GenerateManual();
        }

        internal VanillaRuleModifier VanillaRuleModifer { get; set; }
    }
}