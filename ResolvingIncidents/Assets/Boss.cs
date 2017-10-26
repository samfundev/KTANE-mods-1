using System;

[Serializable]
public class Boss
{
    public string Name;
    public int BaseDistance;
    public int BaseRange;
    public int BonusDistance = 0;
    public int BonusRange = 0;
    public int EdgeworkBonusDistance = 0;
    public int EdgeworkBonusRange = 0;
    public int WidgetBonusDistance = 0;
    public int WidgetBonusRange = 0;

    public Boss(){}

    public Boss(string name, int baseDistance, int baseRange, int bonusDistance, int bonusRange, int widgetDistance, int widgetRange)
    {
        Name = name;
        BaseDistance = baseDistance;
        BaseRange = baseRange;
        BonusDistance = bonusDistance;
        BonusRange = bonusRange;
        WidgetBonusDistance = widgetDistance;
        WidgetBonusRange = widgetRange;
    }
}