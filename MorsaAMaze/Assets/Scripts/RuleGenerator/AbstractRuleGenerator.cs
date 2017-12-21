using System;
using Random = System.Random;

namespace Assets.Scripts.RuleGenerator
{
    public abstract class AbstractRuleGenerator
    {
        public bool Initialized { get; private set; }
        public bool RulesGenerated { get; protected set; }
        public delegate int RandomNext(int min=0, int max=int.MaxValue);
        public delegate double RandomNextDouble();
        public delegate void RandomNextBytes(byte[] buf);

        public RandomNext Next { get; private set; }
        public RandomNext NextMax { get; private set; }
        public RandomNext NextMinMax { get; private set; }
        public RandomNextDouble NextDouble { get; private set; }
        public RandomNextBytes NextBytes { get; private set; }

        private Random _random;
        public void InitializeRNG(int seed)
        {
            _random = new Random(seed);
            Next = (min, max) => _random.Next();
            NextMax = (min, max) => _random.Next(min);
            NextMinMax = (min, max) => _random.Next(min, max);
            NextDouble = () => _random.NextDouble();
            NextBytes = bytes => _random.NextBytes(bytes);
            Initialized = true;
        }

        public void InitializeRNG(RandomNext next, RandomNext nextmax, RandomNext nextminmax, RandomNextDouble nextDouble, RandomNextBytes nextBytes)
        {
            if (next == null || nextmax == null || nextminmax == null || nextDouble == null || nextBytes == null)
                throw new NullReferenceException();
            Next = next;
            NextMax = nextmax;
            NextMinMax = nextminmax;
            NextDouble = nextDouble;
            NextBytes = nextBytes;
            Initialized = true;
        }

        public abstract string GetHTMLManual(int seed);
        public abstract void CreateRules(int seed);

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