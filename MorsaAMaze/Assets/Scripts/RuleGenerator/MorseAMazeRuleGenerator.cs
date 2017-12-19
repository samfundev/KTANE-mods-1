using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Assets.Scripts.RuleGenerator
{
    public class AbstractRuleGenerator
    {
        public bool Initialized { get; private set; }
        public int Seed { get; private set; }
        public delegate int RandomNext(int min, int max);
        public delegate double RandomNextDouble();
        public delegate void RandomNextBytes(byte[] buf);

        public RandomNext randomNext;
        public RandomNext randomNextMax;
        public RandomNext randomNextMinMax;
        public RandomNextDouble randomNextDouble;
        public RandomNextBytes randomNextBytes;

        private Random _random;

        public int GetRuleSeed()
        {
            GameObject vanillaRuleModifierAPIGameObject = GameObject.Find("VanillaRuleModifierProperties");
            if (vanillaRuleModifierAPIGameObject == null) //If the Vanilla Rule Modifer is not installed, return.
                return 1;
            IDictionary<string, object> vanillaRuleModifierAPI = vanillaRuleModifierAPIGameObject.GetComponent<IDictionary<string, object>>();
            object seed;
            if (vanillaRuleModifierAPI.TryGetValue("", out seed))
                return (int)seed;
            return 1;
        }

        public void InitializeRNG()
        {
            if (randomNext == null)
            {


                _random = new Random(1);    //Default seed is 1.
                randomNext = (min,max) => _random.Next();
                randomNextMax = (min, max) => _random.Next(max);
                randomNextMinMax = (min, max) => _random.Next(min, max);
                randomNextDouble = () => _random.NextDouble();
                randomNextBytes = (bytes) => _random.NextBytes(bytes);
            }
            Initialized = true;
        }

        public void InitializeRNG(RandomNext next)
        {
            randomNext = next;
            Initialized = true;
        }

        public virtual string GetHTMLManual()
        {
            return string.Empty;
        }

    }

    public class MorseAMazeRuleGenerator : AbstractRuleGenerator
    {
        
    }
}