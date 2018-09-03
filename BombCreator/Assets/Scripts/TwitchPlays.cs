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
	    private static GameObject _tweaksObject;

        private static IDictionary<string, object> Properties
        {
            get
            {
                return _gameObject == null
                    ? null
                    : _gameObject.GetComponent<IDictionary<string, object>>();
            }
        }

	    private static IDictionary<string, object> TweakProperties
	    {
		    get
		    {
			    return _tweaksObject == null
				    ? null
				    : _tweaksObject.GetComponent<IDictionary<string, object>>();
		    }
	    }

        //Call this in KMGameState.Setup
        public static IEnumerator Refresh()
        {
            for (var i = 0; i < 4 && _gameObject == null; i++)
            {
                _gameObject = GameObject.Find("TwitchPlays_Info");
	            _tweaksObject = GameObject.Find("Tweaks_Info");

	            yield return null;
            }
        }

        public static bool Installed()
        {
            return _gameObject ?? _tweaksObject != null;
        }

	    public static void SetGameMode(GameModes mode)
	    {
		    bool tweaks = Properties == null;
		    switch (mode)
		    {
				case GameModes.NormalMode:
					SetTimeMode(Properties, false);
					SetTimeMode(TweakProperties, false);
					SetZenMode(Properties, false);
					SetZenMode(TweakProperties, false);
					break;
				case GameModes.TimeMode:
					SetTimeMode(Properties, !tweaks);
					SetTimeMode(TweakProperties, tweaks);
					break;
				case GameModes.ZenMode:
					SetZenMode(Properties, !tweaks);
					SetZenMode(TweakProperties, tweaks);
					break;
		    }
	    }

	    public static GameModes GetGameMode()
	    {
		    bool tweaks = Properties == null;
		    var timeMode = TimeMode(Properties ?? TweakProperties);
		    var zenmode = ZenMode(Properties ?? TweakProperties);
		    if (timeMode && zenmode)
			    throw new Exception("Unsupported game mode");
		    if (timeMode)
			    return GameModes.TimeMode;
		    if (zenmode)
			    return GameModes.ZenMode;
		    return GameModes.NormalMode;
	    }

		private static bool TimeMode(IDictionary<string, object> properties)
        {
            return properties != null && properties.ContainsKey("TimeMode") && ((bool)properties["TimeMode"]);
        }
		

        private static void SetTimeMode(IDictionary<string, object> properties, bool on)
        {
            if (properties == null || !properties.ContainsKey("TimeMode")) return;
	        properties["TimeMode"] = on;
        }

        private static bool ZenMode(IDictionary<string, object> properties)
        {
            return properties != null && properties.ContainsKey("ZenMode") && ((bool)properties["ZenMode"]);
        }

	    private static void SetZenMode(IDictionary<string, object> properties, bool on)
        {
	        if (properties == null || !properties.ContainsKey("ZenMode"))
	        {
		        SetTimeMode(properties, false);
		        return;
	        };
	        properties["ZenMode"] = on;
        }

	    public static int TimeModeTimeLimit(int time = 300, bool force = false)
	    {
		    return TimeModeTimeLimit(Properties ?? TweakProperties, Properties == null, time, force);
	    }

        private static int TimeModeTimeLimit(IDictionary<string, object> properties, bool _tweaks, int time = 300, bool force = false)
        {
	        var key = _tweaks ? "TimeModeStartingTime" : "TimeModeTimeLimit";

	        if (properties == null || !properties.ContainsKey(key) || (!force && !TimeMode(properties)))
		        return time;

	        int timeint;
	        if (properties[key] is float)
	        {
		        var timefloat = ((float)properties[key]) * 60.0F;
		        timeint = (int) timefloat;
	        }
	        else if (properties[key] is int)
	        {
		        timeint = ((int)properties[key]) * 60;
	        }
	        else
	        {
		        timeint = time;
	        }

	        return timeint;

        }

	    public static void SetTimeModeTimeLimit(int time)
	    {
		    SetTimeModeTimeLimit(Properties ?? TweakProperties, Properties == null, time);
	    }


		private static void SetTimeModeTimeLimit(IDictionary<string, object> properties, bool _tweaks, int time)
        {
	        var key = _tweaks ? "TimeModeStartingTime" : "TimeModeTimeLimit";
			if (!TimeMode(properties) || properties == null || !properties.ContainsKey(key)) return;
	        if (_tweaks)
		        properties[key] = (float) Math.Ceiling(time / 60f);
			else
		        properties[key] = (int)Math.Ceiling(time / 60f);
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

	public enum GameModes
	{
		NormalMode,
		TimeMode,
		ZenMode
	}
}
