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

        public static void SetTimeMode(bool on)
        {
            if (Properties == null || !Properties.ContainsKey("TimeMode")) return;
            Properties["TimeMode"] = on;
        }

        public static bool ZenMode()
        {
            return Properties != null && Properties.ContainsKey("ZenMode") && ((bool)Properties["ZenMode"]);
        }

        public static void SetZenMode(bool on)
        {
            if (Properties == null || !Properties.ContainsKey("ZenMode")) return;
            Properties["ZenMode"] = on;
        }

        public static int TimeModeTimeLimit(int time = 300, bool force = false)
        {
            if (force && Properties.ContainsKey("TimeModeTimeLimit"))
                return ((int) Properties["TimeModeTimeLimit"]) * 60;

            return TimeMode() && Properties.ContainsKey("TimeModeTimeLimit")
                ? ((int) Properties["TimeModeTimeLimit"]) * 60
                : time;
        }

        public static void SetTimeModeTimeLimit(int time)
        {
            if (!TimeMode() || !Properties.ContainsKey("TimeModeTimeLimit")) return;
            Properties["TimeModeTimeLimit"] = (int)Math.Ceiling(time / 60f);
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
    }
}
