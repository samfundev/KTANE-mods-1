using System;

[Serializable]
public class Character
{
    public string Name;
    public bool Heroine;
    public int DistanceBonus;
    public int RangeBonus;
    public Incidents ForbiddenIncident;

    public Character(string name, int distance, int range, bool heroine = false)
    {
        Name = name;
        DistanceBonus = distance;
        RangeBonus = range;
        ForbiddenIncident = Incidents.None;
        Heroine = heroine;
    }

    public Character(string name, int distance, int range, Incidents forbidden)
    {
        Name = name;
        DistanceBonus = distance;
        RangeBonus = range;
        ForbiddenIncident = forbidden;
        Heroine = false;
    }

    public Character(string name, int distance, int range, bool heroine, Incidents forbidden)
    {
        Name = name;
        Heroine = heroine;
        DistanceBonus = distance;
        RangeBonus = range;
        ForbiddenIncident = forbidden;
    }
}