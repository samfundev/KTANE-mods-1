using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class RuleSeedWidget : MonoBehaviour
{
    public TextMesh NumberText;
	public KMRuleSeedable RuleSeed;
	public Transform[] Screws;

    void Awake()
    {
	    
	    foreach (var t in Screws)
	    {
		    t.localEulerAngles = new Vector3(0, Random.Range(0F, 360F), 0);
	    }
		//GetComponent<KMWidget>().OnWidgetActivate += Activate;
		GetComponent<KMWidget>().OnQueryRequest += OnQueryRequest;
	    StartCoroutine(ShowRuleSeed());
    }

	private IEnumerator ShowRuleSeed()
	{
		yield return null;
		yield return null;
		NumberText.text = RuleSeed.GetRNG().Seed.ToString();
		Debug.Log(string.Format("[Rule Seed Modifier] The Seed is {0}", NumberText.text));
	}

	private string OnQueryRequest(string querykey, string queryinfo)
	{
		if (querykey == "RuleSeedModifier")
		{
			return JsonConvert.SerializeObject(new Dictionary<string, string>
			{
				{"seed",RuleSeed.GetRNG().Seed.ToString()}
			});
		}

		return null;
	}

	//This happens when the bomb turns on, don't turn on any lights or unlit shaders until activate
    public void Activate()
    {
        
    }
}
