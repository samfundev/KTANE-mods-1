using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class SerialNumberTag : MonoBehaviour
{
    //public Transform Label;
    //public Transform Cube;
    //public TextMesh Description;
    public UnityEngine.Object SerialNumber;
    public Transform[] Screws;
}

// ReSharper disable once CheckNamespace
public class SerialNumberModWidget : MonoBehaviour
{
    public List<SerialNumberTag> Tags = new List<SerialNumberTag>();
}