using System;
using UnityEngine;

[Serializable]
public class DisplayScreens
{
    public readonly int MinChance=5;
    public readonly int MaxChance=100;
    public readonly string[] Screens;

    public DisplayScreens(string screen1, string screen2, string screen3)
    {
        Screens = new[] { screen1, screen2, screen3 };
        
    }

    public DisplayScreens(string screen1, string screen2, string screen3, int minchance, int maxchance)
    {
        Screens = new[] {screen1, screen2, screen3};
        MinChance = minchance;
        MaxChance = maxchance;
        
    }

    public void UpdateScreens(ref TextMesh screen1, ref TextMesh screen2, ref TextMesh screen3)
    {
        screen1.color = Color.white;
        screen1.text = Screens[0];
        screen2.text = Screens[1];
        screen3.text = Screens[2];
    }
}