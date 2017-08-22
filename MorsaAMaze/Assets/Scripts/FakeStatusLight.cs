using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FakeStatusLight : MonoBehaviour
{

    public GameObject InactiveLight;
    public GameObject StrikeLight;
    public GameObject PassLight;

    public KMBombModule Module;

    private bool _off = false;
    private bool _pass = false;
    private bool _fail = false;

    public void HandlePass()
    {
        if (Module == null) return;
        Module.HandlePass();
        SetPass();
    }

    public void HandleStrike()
    {
        if (Module == null) return;
        Module.HandleStrike();
        FlashStrike();
    }


    void Update()
    {
        if (InactiveLight != null)
            InactiveLight.SetActive(_off);
        if (PassLight != null)
            PassLight.SetActive(_pass);
        if (StrikeLight != null)
            StrikeLight.SetActive(_fail);
    }

    public void SetPass()
    {
        _pass = true;
        _fail = _off = false;
    }

    public void SetInActive()
    {
        _pass = _fail = false;
        _off = true;
    }

    public void FlashStrike()
    {
        if (_pass) return;
        _off = true;
        _fail = false;
        if (gameObject.activeInHierarchy)
            StartCoroutine(StrikeFlash(1f));
    }

    public void SetStrike()
    {
        _pass = _off = false;
        _fail = true;
    }

    protected IEnumerator StrikeFlash(float blinkTime)
    {
        _off = false;
        _fail = true;
        yield return new WaitForSeconds(blinkTime);
        _off = true;
        _fail = false;
    }

    private static int[] Morsify(string text)
    {
        char[] values = text.ToUpperInvariant().ToCharArray();
        List<int> data = new List<int>();
        for (int a = 0; a < values.Length; a++)
        {
            if (a > 0) data.Add(-1);
            char c = values[a];
            switch (c)
            {
                /*case ' ':
                    data.Add(-1);
                    break;*/
                case 'A':
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'B':
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'C':
                    data.Add(1);
                    data.Add(0);
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'D':
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'E':
                    data.Add(0);
                    break;
                case 'F':
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'G':
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'H':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'I':
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'J':
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    break;
                case 'K':
                    data.Add(1);
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'L':
                    data.Add(0);
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'M':
                    data.Add(1);
                    data.Add(1);
                    break;
                case 'N':
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'O':
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    break;
                case 'P':
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'Q':
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'R':
                    data.Add(0);
                    data.Add(1);
                    data.Add(0);
                    break;
                case 'S':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case 'T':
                    data.Add(1);
                    break;
                case 'U':
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'V':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'W':
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    break;
                case 'X':
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    break;
                case 'Y':
                    data.Add(1);
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    break;
                case 'Z':
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    break;
                case '1':
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    break;
                case '2':
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    break;
                case '3':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    data.Add(1);
                    break;
                case '4':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(1);
                    break;
                case '5':
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case '6':
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case '7':
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    data.Add(0);
                    break;
                case '8':
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    data.Add(0);
                    break;
                case '9':
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(0);
                    break;
                case '0':
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    data.Add(1);
                    break;
            }
        }
        return data.ToArray();
    }

    public IEnumerator PlayWord(string word)
    {
        SetInActive();
        foreach (var d in Morsify(word))
        {
            if (d == -1)
            {
                yield return new WaitForSeconds(0.75f);
            }
            else if (d == 0)
            {
                SetStrike();
                yield return new WaitForSeconds(0.25f);
                SetInActive();
                yield return new WaitForSeconds(0.25f);
            }
            else
            {
                SetStrike();
                yield return new WaitForSeconds(0.75f);
                SetInActive();
                yield return new WaitForSeconds(0.25f);
            }
        }
    }

}
