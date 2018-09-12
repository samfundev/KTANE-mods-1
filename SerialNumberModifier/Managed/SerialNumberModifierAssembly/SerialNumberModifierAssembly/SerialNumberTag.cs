using System.Collections.Generic;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class SerialNumberTag : MonoBehaviour
{
    //public Transform Label;
    //public Transform Cube;
    //public TextMesh Description;
    public string ForcedLetterNumberExclusions;
    public Object SerialNumber;
    public TextMesh[] TextMeshes;
    public Transform[] Screws;
    public float UnscrewOffset;
}

public class SerialNumberModWidget : MonoBehaviour
{
    public List<SerialNumberTag> Tags = new List<SerialNumberTag>();
}