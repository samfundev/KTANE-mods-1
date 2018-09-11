using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = System.Random;

// ReSharper disable once CheckNamespace
public class SerialNumberTag : MonoBehaviour
{
    //public Transform Label;
    //public Transform Cube;
    //public TextMesh Description;
    public string ForcedLetterNumberExclusions;
    public UnityEngine.Object SerialNumber;
    public TextMesh[] TextMeshes;
    public Transform[] Screws;
    public float UnscrewOffset;
}

public class SerialNumberModWidget : MonoBehaviour
{
    public List<SerialNumberTag> Tags = new List<SerialNumberTag>();
}