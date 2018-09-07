using UnityEngine;

public class RuleSeedWidget : MonoBehaviour
{
    public TextMesh NumberText;
	public KMRuleSeedable RuleSeed;
	public Transform[] Screws;

    void Awake()
    {
	    NumberText.text = RuleSeed.GetRNG().Seed.ToString();
	    foreach (var t in Screws)
	    {
		    t.localEulerAngles = new Vector3(0, Random.Range(0F, 360F), 0);
	    }
		//GetComponent<KMWidget>().OnWidgetActivate += Activate;
	}

    //This happens when the bomb turns on, don't turn on any lights or unlit shaders until activate
    public void Activate()
    {
        
    }
}
