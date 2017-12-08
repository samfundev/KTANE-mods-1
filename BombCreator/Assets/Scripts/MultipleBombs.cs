using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class MultipleBombs
    {
        private static GameObject _gameObject;

        private static IDictionary<string, object> Properties
        {
            get
            {
                return _gameObject == null
                    ? null
                    : _gameObject.GetComponent<IDictionary<string, object>>();
            }
        }

        //Call this in KMGameState.Setup
        public static IEnumerator Refresh()
        {
            for (var i = 0; i < 120 && _gameObject == null; i++)
            {
                _gameObject = GameObject.Find("MultipleBombs_Info");
                yield return null;
            }
        }

        public static bool Installed()
        {
            return _gameObject != null;
        }

        public static int GetMaximumBombCount()
        {
            try
            {
                return Properties != null
                    ? (int) Properties[MaxBombCount]
                    : 1;
            }
            catch
            {
                return 1;
            }
        }

        public static int GetBombCount()
        {
            try
            {
                return Properties != null
                    ? (int) Properties[BombCount]
                    : 1;
            }
            catch
            {
                return 1;
            }
        }

        private const string MaxBombCount = "CurrentMaximumBombCount";
        private const string BombCount = "CurrentBombCount";
    }
}