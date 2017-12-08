List<GameObject> goList = new List<GameObject>();
    private void PrintObjectNames(Transform t, List<GameObject> list, int depth = 0)
    {
        list.Add(t.gameObject);
        StringBuilder sb = new StringBuilder();
        for (var i = 0; i < depth; i++)
            sb.Append("\t");
        sb.Append(string.Format("{0} - {1}", t.name, (t.gameObject.activeInHierarchy && t.gameObject.activeSelf)));
        if ((t.gameObject.activeInHierarchy && t.gameObject.activeSelf) || depth == 0)
            Debug.Log(sb.ToString());

        for (var i = 0; i < t.childCount; i++)
            PrintObjectNames(t.GetChild(i), list, depth + 1);
    }
    
    
    //Get ALL objects
    GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        Debug.Log("GameObject Name - Active");
        foreach (var go in allObjects)
        {
            if (goList.Contains(go))
                continue;
            GameObject GO = go;
            while (GO.transform.parent != null)
                GO = GO.transform.parent.gameObject;

            try
            {
                //CommonReflectedTypeInfo.DebugLog("GameObject: {0}, ActiveInHierarchy: {1}, ActiveSelf: {2}", go.name, go.activeInHierarchy, go.activeSelf);
                PrintObjectNames(GO.transform, goList);
            }
            catch
            {
                CommonReflectedTypeInfo.DebugLog("Could Not list this object");
            }
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);