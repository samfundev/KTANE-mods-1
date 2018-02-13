using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    class TwitchPlays
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
            for (var i = 0; i < 4 && _gameObject == null; i++)
            {
                _gameObject = GameObject.Find("TwitchPlays_Info");
                yield return null;
            }
        }

        public static bool Installed()
        {
            return _gameObject != null;
        }

        public static bool TimeMode()
        {
            return Properties != null && Properties.ContainsKey("TimeMode") && ((bool) Properties["TimeMode"]);
        }

        public static int TimeModeTimeLimit(int time=300)
        {
            return TimeMode() && Properties.ContainsKey("TimeModeTimeLimit")
                ? ((int) Properties["TimeModeTimeLimit"]) * 60
                : time;
        }

        public static void SetReward(int reward)
        {
            if (Properties != null && Properties.ContainsKey("Reward"))
            {
                Properties["Reward"] = reward;
            }
        }

        public static void SendMessage(string s)
        {
            if (Properties != null && Properties.ContainsKey("ircConnectionSendMessage"))
                Properties["ircConnectionSendMessage"] = s;
        }

        public static void CauseFakeStrike(object module)
        {
            if (Properties != null && Properties.ContainsKey("CauseFakeStrike"))
                Properties["CauseFakeStrike"] = module;
        }
    }
}
