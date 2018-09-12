using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class MultipleBombs
    {
        private static GameObject _gameObject;

        private static IDictionary<string, object> Properties => _gameObject == null
            ? null
            : _gameObject.GetComponent<IDictionary<string, object>>();

        //Call this in KMGameState.Setup
        public static IEnumerator Refresh()
        {
            _gameObject = null;
            for (var i = 0; i < 4 && _gameObject == null; i++)
            {
                _gameObject = GameObject.Find("MultipleBombs_Info");
                yield return null;
            }
        }

        public static bool Installed()
        {
            return Properties != null;
        }

        public static int GetMaximumBombCount()
        {
            try
            {
                return (int?) Properties?[MaxBombCount] ?? 1;
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
                return (int?) Properties?[BombCount] ?? 1;
            }
            catch
            {
                return 1;
            }
        }

        private const string MaxBombCount = "CurrentMaximumBombCount";
        private const string BombCount = "CurrentFreePlayBombCount";
    }