using System;
using System.Collections.Generic;
using Assets.Scripts.Rules;
using VanillaRuleGenerator;

namespace VanillaRuleModifierAssembly.RuleSetGenerators
{
	public class ModRuleSetGenerator
	{
		public static ModRuleSetGenerator Instance => _instance ?? (_instance = new ModRuleSetGenerator());
		private static ModRuleSetGenerator _instance;
		private HashSet<Type> _ruleGeneratorTypes = new HashSet<Type>();
		private List<ModRuleGenerator> _modRuleGenerators = new List<ModRuleGenerator>();
		private int _seed;

		private void GetRuleGenerators()
		{
			Type[] types = ReflectionHelper.FindTypes("AbstractRuleGenerator");
			foreach (Type type in types)
			{
				if (_ruleGeneratorTypes.Add(type))
				{
					try
					{
						_modRuleGenerators.Add(ModRuleGenerator.GetRuleGenerator(type.Assembly));
					}
					catch (Exception ex)
					{
						UnityEngine.Debug.LogFormat("Could not generate rules using the provided AbstractRuleGenerator due to an Exception:\n{0}\n{1}",ex.Message,ex.StackTrace);
					}
				}
			}
		}

		public void CreateRules(int seed)
		{
			_seed = seed;
			GetRuleGenerators();
			foreach (ModRuleGenerator generator in _modRuleGenerators)
			{
				generator.InitializeRules(seed);
			}
		}

		public void WriteManuals(string path)
		{
			GetRuleGenerators();
			foreach (ModRuleGenerator generator in _modRuleGenerators)
			{
				generator.WriteAllFiles(path);
			}
		}
	}
}