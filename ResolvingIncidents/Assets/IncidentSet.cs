using System;

[Serializable]
public class IncidentSet
{
    public string Name;
    public Boss Boss1;
    public Boss Boss2;
    public string BonusReason = "";
    public string EdgeworkBonusReason = "";
    public string WidgetBonusReason = "number of widgets on the bomb";

    public IncidentSet()
    {
        Name = "Error";
    }

    public IncidentSet(string name, Boss boss1, string bonusReason)
    {
        Name = name;
        Boss1 = boss1;
        BonusReason = bonusReason;
    }

    public IncidentSet(string name, Boss boss1, Boss boss2, string bonusReason)
    {
        Name = name;
        Boss1 = boss1;
        Boss2 = boss2;
        BonusReason = bonusReason;
    }
}