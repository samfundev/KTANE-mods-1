using System;
using UnityEngine;

namespace VanillaRuleModifierAssembly
{
    public class RuleSeedModifierProperties : PropertiesBehaviour
    {
		public RuleSeedModifierProperties()
        {
            AddProperty("RuleSeed", new Property(RuleSeed_Get, RuleSeed_Set));
            AddProperty("IsSeedVanilla", new Property(IsSeedVanilla_Get, null));
            AddProperty("IsSeedModded", new Property(IsSeedModded_Get, null));
            AddProperty("GetRuleManual", new Property(RuleManaul_Get, null));
            AddProperty("AddSupportedModule", new Property(null, SupportedModules_Set));
            AddProperty("RandomRuleSeed", new Property(RandomRuleSeed_Get, RandomRuleSeed_Set));
        }

        private void RandomRuleSeed_Set(object value)
        {
            const string invalidArguments = "Arguments need to be either (bool randomseed) or (object[2] {bool randomseed, bool save}";
            if (VanillaRuleModifer.CurrentState != KMGameInfo.State.Setup && VanillaRuleModifer.CurrentState != KMGameInfo.State.PostGame)
                throw new Exception("Setting of Random seed is only allowed during Setup or Post game.");

            bool saveSettings = false;
            bool randomseed;
            switch (value)
            {
                case object[] objects when objects.Length == 2 && objects[0] is bool randomSeed && objects[1] is bool save:
                    randomseed = randomSeed;
                    saveSettings = save;
                    break;
                case bool randomSeed:
                    randomseed = randomSeed;
                    break;
                default:
                    throw new ArgumentException(invalidArguments);
            }

            VanillaRuleModifer.SetRandomRuleSeed(randomseed, saveSettings);
        }

        private object RandomRuleSeed_Get()
        {
            return VanillaRuleModifer.CurrentRandomSeed;
        }

        public static void AddSupportedModule(string moduleType)
	    {
		    if (!VanillaRuleModifier.ModsThatSupportRuleSeedModifier.Contains(moduleType))
			    VanillaRuleModifier.ModsThatSupportRuleSeedModifier.Add(moduleType);
			foreach (RuleSeedModifierProperties properties in VanillaRuleModifier.PublicProperties)
			{
				if (!properties.ContainsKey($"RuleSeedModifier_{moduleType}"))
					properties.AddProperty($"RuleSeedModifier_{moduleType}",new Property(properties.RuleSeed_Get,null));
			    
		    }
	    }

        private static void SupportedModules_Set(object moduleName)
        {
            if (moduleName is string mn)
                AddSupportedModule(mn);
        }

	    private object RuleSeed_Get()
	    {
	        return VanillaRuleModifer.CurrentSeed;
	    }

        private void RuleSeed_Set(object seed)
        {
            const string invalidArguments = "Arguments need to be either (int seed) or (object[2] {int seed, bool save}";
            if (VanillaRuleModifer.CurrentState != KMGameInfo.State.Setup && VanillaRuleModifer.CurrentState != KMGameInfo.State.PostGame)
                throw new Exception("Setting of Rule seed is only allowed during Setup or Post game.");

            bool saveSettings = false;
            int ruleseed;
            switch (seed)
            {
	            case object[] objects when objects.Length == 2 && objects[0] is int ruleSeed && objects[1] is bool save:
	                ruleseed = ruleSeed == int.MinValue ? 0 : Mathf.Abs(ruleSeed);
		            saveSettings = save;
		            break;
	            case int ruleSeed:
	                ruleseed = ruleSeed == int.MinValue ? 0 : Mathf.Abs(ruleSeed);
                    break;
	            default:
		            throw new ArgumentException(invalidArguments);
            }
	        VanillaRuleModifer.SetRuleSeed(ruleseed, saveSettings);

        }

        private static object IsSeedVanilla_Get()
        {
            return CommonReflectedTypeInfo.IsVanillaSeed;
        }

        private static object IsSeedModded_Get()
        {
            return CommonReflectedTypeInfo.IsModdedSeed;
        }

        private object RuleManaul_Get()
        {
            return VanillaRuleModifer.GenerateManual();
        }

        internal VanillaRuleModifier VanillaRuleModifer { get; set; }
    }
}