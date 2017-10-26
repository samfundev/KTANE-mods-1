using System;

[Serializable]
public class Inventory
{
    public string Item;
    public int Distance;
    public int Range;

    public Inventory(string name, int distance, int range)
    {
        Item = name;
        Distance = distance;
        Range = range;
    }
}