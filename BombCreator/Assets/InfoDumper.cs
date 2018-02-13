using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class InfoDumper : MonoBehaviour
{

    public KMSelectable StartButton;
    public KMAudio Audio;

    public static void DebugLog(string message, params object[] args)
    {
        var debugstring = string.Format("[BombCreator] {0}", message);
        Debug.LogFormat(debugstring, args);
    }
    // Use this for initialization
    void Start ()
    {
        StartButton.OnInteract += OnInteract;
    }

    List<GameObject> goList = new List<GameObject>();
    private void PrintObjectNames(Transform t, List<GameObject> list, StringBuilder sb, int depth = 0)
    {
        list.Add(t.gameObject);
        for (var i = 0; i < depth; i++)
            sb.Append("\t");
        sb.Append(string.Format("{0} - {1}", t.name, (t.gameObject.activeInHierarchy && t.gameObject.activeSelf)));
        //if ((t.gameObject.activeInHierarchy && t.gameObject.activeSelf) || depth == 0)
        //    Debug.Log(sb.ToString());
        sb.Append("\n");

        for (var i = 0; i < t.childCount; i++)
            PrintObjectNames(t.GetChild(i), list, sb, depth + 1);
    }

    IEnumerator DumpInfo()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("GameObject Name - Active");
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        
        for(var i = 0; i < allObjects.Length; i++)
        {
            if (i % 30 == 0)
            {
                yield return null;
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.FastestTimerBeep, transform);
            }
            GameObject go = allObjects[i];
            if (goList.Contains(go))
                continue;
            GameObject GO = go;
            while (GO.transform.parent != null)
                GO = GO.transform.parent.gameObject;

            try
            {
                PrintObjectNames(GO.transform, goList, sb);
            }
            catch
            {
                continue;
            }
            
            
        }
        DebugLog("{0}", sb.ToString());

        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BombExplode, transform);
    }

    bool OnInteract()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        //StartCoroutine(DumpInfo());
        return false;
    }

    private string TwitchHelpMessage = "Realized it was a bad idea to not check for strikes on custom holdables.";
    private IEnumerator ProcessTwitchCommand(string command)
    {
        int strikeCount;
        if (!int.TryParse(command, out strikeCount) || strikeCount < 0)
        {
            DebugLog(command);
            foreach (string str in command.Split(new [] {":"}, StringSplitOptions.RemoveEmptyEntries))
            {
                DebugLog(str);
                if (str.ToLowerInvariant().Equals("strike"))
                    GetComponent<KMGameCommands>().CauseStrike(string.Format("Strike {0}", strikeCount));
                else
                    yield return str.Trim();
            }
        }
        else
        {
            yield return ProcessTwitchCommand((strikeCount - 1).ToString());
            GetComponent<KMGameCommands>().CauseStrike(string.Format("Strike {0}", strikeCount));
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
