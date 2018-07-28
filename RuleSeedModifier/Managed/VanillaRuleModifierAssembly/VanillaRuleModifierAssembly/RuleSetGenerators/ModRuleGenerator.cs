using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using VanillaRuleModifierAssembly;

namespace VanillaRuleGenerator
{
	public class ModRuleGenerator
	{
		private Type _ruleGeneratorType;
		private MethodInfo _initRNG;
		private MethodInfo _createRules;
		private MethodInfo _getHTMLManual;
		private MethodInfo _getTextFiles;
		private MethodInfo _getBinaryFiles;
		private MethodInfo _getInstance;
		private MethodInfo _getModuleType;
		private string _htmlFileName;

		private int _seed;
		private object _ruleGenerator;


		public void InitializeRules(int seed)
		{
			if (_ruleGenerator == null)
			{
				_ruleGenerator = _getInstance.Invoke(null, new object[] {_ruleGeneratorType});
				_seed = seed;
				_initRNG.Invoke(_ruleGenerator, new object[] { _seed, null });
				_createRules.Invoke(_ruleGenerator, null);
				RuleSeedModifierProperties.AddSupportedModule(GetModuleType());
			}
			else if (_seed != seed)
			{
				_seed = seed;
				_initRNG.Invoke(_ruleGenerator, new object[] { _seed, null });
				_createRules.Invoke(_ruleGenerator, null);
			}
		}

		public string GetHTML(int seed)
		{
			return GetHTML(seed, out string _);
		}

		public string GetHTML(int seed, out string filename)
		{
			InitializeRules(seed);
			var htmlFilename = "";
			object[] args = { htmlFilename };
			var html = (string)_getHTMLManual.Invoke(_ruleGenerator, args);
			filename = (string)args[0];
			_htmlFileName = filename;
			return html;
		}

		public string GetHTMLFileName()
		{
			if (_htmlFileName == null)
				GetHTML(_seed, out _htmlFileName);
			return _htmlFileName;
		}

		public string[] GetTextFiles(out string[] filenames)
		{
			var textFileNames = new string[0];
			var textFileArgs = new object[] { textFileNames };
			var textFiles = (string[])_getTextFiles.Invoke(_ruleGenerator, textFileArgs);
			filenames = (string[])textFileArgs[0];
			return textFiles;
		}

		public byte[][] GetBinaryFiles(out string[] filenames)
		{
			var binaryFileNames = new string[0];
			var binaryFileArgs = new object[] { binaryFileNames };
			var binaryFiles = (byte[][])_getBinaryFiles.Invoke(_ruleGenerator, binaryFileArgs);
			filenames = (string[])binaryFileArgs[0];
			return binaryFiles;
		}

		public void WriteHTMLFile(int seed, string path)
		{
			var html = GetHTML(seed, out string htmlFileName);
			File.WriteAllText(Path.Combine(path, htmlFileName), html);
		}

		public void WriteTextFiles(string path)
		{
			var textFiles = GetTextFiles(out string[] textFileNames);
			for (var i = 0; i < textFileNames.Length; i++)
			{
				var filePath = Path.GetDirectoryName(textFileNames[i]);
				if (!string.IsNullOrEmpty(filePath) && !Directory.Exists(Path.Combine(path, filePath)))
					Directory.CreateDirectory(Path.Combine(path, filePath));
				File.WriteAllText(Path.Combine(path, textFileNames[i]), textFiles[i]);
			}
		}

		public void WriteBinaryFiles(string path)
		{
			var binaryFiles = GetBinaryFiles(out string[] binaryFileNames);
			for (var i = 0; i < binaryFileNames.Length; i++)
			{
				var filePath = Path.GetDirectoryName(binaryFileNames[i]);
				if (!string.IsNullOrEmpty(filePath) && !Directory.Exists(Path.Combine(path, filePath)))
					Directory.CreateDirectory(Path.Combine(path, filePath));
				File.WriteAllBytes(Path.Combine(path, binaryFileNames[i]), binaryFiles[i]);
			}
		}

		public void WriteAllFiles(string path)
		{
			WriteHTMLFile(_seed, path);
			WriteTextFiles(path);
			WriteBinaryFiles(path);
		}

		public string GetModuleType()
		{
			InitializeRules(_seed);
			return _getModuleType.Invoke(_ruleGenerator, null) as string;
		}

		public static ModRuleGenerator GetRuleGenerator(Assembly assembly, List<Type> seenTypes)
		{
			var ruleGeneratorType = assembly.GetSafeTypes().FirstOrDefault(t => t.FullName != null && t.FullName.EndsWith("AbstractRuleGenerator"));
			if (ruleGeneratorType == null) throw new Exception($"Could not find AbstractRuleGenerator class in assembly {assembly}");
			var implementsType = assembly.GetSafeTypes().Where(p => ruleGeneratorType.IsAssignableFrom(p)).FirstOrDefault(t => t != ruleGeneratorType && !seenTypes.Contains(t)) ?? (seenTypes.Count == 0 ? ruleGeneratorType : null);
			if (implementsType == null) return null;
			seenTypes.Add(implementsType);

			var initRNGMethod = ruleGeneratorType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.FirstOrDefault(m => m.Name.Equals("InitializeRNG") &&
									 m.ReturnType == typeof(void) &&
									 m.GetParameters().Length == 2 &&
									 m.GetParameters()[0].ParameterType == typeof(int) &&
									 m.GetParameters()[1].ParameterType == typeof(Type));

			if (initRNGMethod == null) throw new Exception($"Could not find InitializeRNG method, or its return type/parameters are of the wrong types in assembly {assembly}");

			var getHTMLMethod = implementsType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.FirstOrDefault(m => m.Name.Equals("GetHTMLManual") &&
									 m.ReturnType == typeof(string) &&
									 m.GetParameters().Length == 1);

			if (getHTMLMethod == null) throw new Exception($"Could not find GetHTMLManual method, or its return type/parameters are of the wrong types in assembly {assembly}");

			var createRulesMethod = implementsType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.FirstOrDefault(m => m.Name.Equals("CreateRules") &&
									 m.ReturnType == typeof(void) &&
									 m.GetParameters().Length == 0);

			if (createRulesMethod == null) throw new Exception($"Could not find CreateRules method, or its return type/parameters are of the wrong types in assembly {assembly}");

			var getTextFilesMethod = implementsType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.FirstOrDefault(m => m.Name.Equals("GetTextFiles") &&
									 m.ReturnType == typeof(string[]) &&
									 m.GetParameters().Length == 1 &&
									 m.GetParameters()[0].IsOut);

			if (getTextFilesMethod == null) throw new Exception($"Could not find GetTextFiles method, or its return type/parameters are of the wrong types in assembly {assembly}");


			var getBinaryFilesMethod = implementsType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.FirstOrDefault(m => m.Name.Equals("GetBinaryFiles") &&
									 m.ReturnType == typeof(byte[][]) &&
									 m.GetParameters().Length == 1 &&
									 m.GetParameters()[0].IsOut);

			if (getBinaryFilesMethod == null) throw new Exception($"Could not find GetBinaryFiles method, or its return type/parameters are of the wrong types in assembly {assembly}");

			var getInstanceMethod = ruleGeneratorType.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.FirstOrDefault(m => m.Name.Equals("GetInstance") &&
				                     m.ReturnType == ruleGeneratorType &&
				                     m.GetParameters().Length == 1 &&
				                     m.GetParameters()[0].ParameterType == typeof(Type));

			if(getInstanceMethod == null) throw new Exception($"Could not find GetInstance method, or its return type/parameters are of the wrong types in assembly {assembly}");

			var getModuleTypeMethod = implementsType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
				.FirstOrDefault(m => m.Name.Equals("GetModuleType") &&
				                     m.ReturnType == typeof(string) &&
				                     m.GetParameters().Length == 0);

			if (getModuleTypeMethod == null) throw new Exception($"Could not find GetModuleType method, or its return type/parameters are of the wrong types in assembly {assembly}");

			var ruleGen = new ModRuleGenerator
			{
				_ruleGeneratorType = implementsType,
				_createRules = createRulesMethod,
				_initRNG = initRNGMethod,
				_getHTMLManual = getHTMLMethod,
				_getTextFiles = getTextFilesMethod,
				_getBinaryFiles = getBinaryFilesMethod,
				_getInstance = getInstanceMethod,
				_getModuleType = getModuleTypeMethod,
			};
			return ruleGen;
		}
	}
}