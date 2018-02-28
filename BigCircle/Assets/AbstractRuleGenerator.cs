using System;
using System.Linq;
using System.Reflection;
using Random = System.Random;

namespace Assets.Scripts.RuleGenerator
{
    public abstract class AbstractRuleGenerator
    {
        public bool Initialized { get; private set; }
        public bool RulesGenerated { get; protected set; }
        public int Seed { get; protected set; }
        public delegate int RandomNext(int min=0, int max=int.MaxValue);
        public delegate double RandomNextDouble();
        public delegate void RandomNextBytes(byte[] buf);

        public RandomNext Next { get; private set; }
        public RandomNext NextMax { get; private set; }
        public RandomNext NextMinMax { get; private set; }
        public RandomNextDouble NextDouble { get; private set; }
        public RandomNextBytes NextBytes { get; private set; }

        private object _random;
        private Type _rngType = typeof(Random);
        public void InitializeRNG(int seed, Type rngType=null)
        {
            Seed = seed;
            if (rngType != null)
                _rngType = rngType;

            var nextMethod = _rngType.GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m => m.Name.Equals("Next") && m.GetParameters().Length == 0 && m.ReturnType == typeof(int));
            if (nextMethod == null) throw NotImplemented("int Next()");

            var nextmaxMethod = _rngType.GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m => m.Name.Equals("Next") && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(int) && m.ReturnType == typeof(int));
            if (nextmaxMethod == null) throw NotImplemented("int Next(int max)");

            var nextminmaxMethod = _rngType.GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m => m.Name.Equals("Next") && m.GetParameters().Length == 2 && m.GetParameters().All(x => x.ParameterType == typeof(int)) && m.ReturnType == typeof(int));
            if (nextminmaxMethod == null) throw NotImplemented("int Next(int min, int max)");

            var nextdoubleMethod = _rngType.GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m => m.Name.Equals("NextDouble") && m.GetParameters().Length == 0 && m.ReturnType == typeof(double));
            if (nextdoubleMethod == null) throw NotImplemented("double NextDouble()");

            var nextbytesMethod = _rngType.GetMethods(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(m => m.Name.Equals("NextBytes") && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(byte[]) && m.ReturnType == typeof(void));
            if (nextbytesMethod == null) throw NotImplemented("void NextBytes(byte[] buf)");

            object[] args = {seed};
            _random = Activator.CreateInstance(_rngType, args);

            Next = (min, max) => (int)nextMethod.Invoke(_random, new object[] { });
            NextMax = (min, max) => (int)nextmaxMethod.Invoke(_random, new object[] { min });
            NextMinMax = (min, max) => (int)nextminmaxMethod.Invoke(_random, new object[] { min, max });
            NextDouble = () => (double)nextdoubleMethod.Invoke(_random, new object[] {});
            NextBytes = bytes => nextdoubleMethod.Invoke(_random, new object[] { bytes });
            Initialized = true;
        }

        private NotImplementedException NotImplemented(string typeNotImplemented)
        {
            _rngType = typeof(Random);
            return new NotImplementedException(typeNotImplemented + " not implemented");
        }

        public abstract string GetHTMLManual(out string filename);
        public abstract void CreateRules();

        public virtual string[] GetTextFiles(out string[] textFilePaths)
        {
            textFilePaths = new string[0];
            return new string[0];
        }

        public virtual byte[][] GetBinaryFiles(out string[] binaryFilePaths)
        {
            binaryFilePaths = new string[0];
            return new byte[0][];
        }
    }
}