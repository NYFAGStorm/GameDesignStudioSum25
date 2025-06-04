// REVIEW: necessary namespaces

public enum WorldMonth
{
    Jan,
    Feb,
    Mar, // start spring
    Apr,
    May,
    Jun, // start summer
    Jul,
    Aug,
    Sep, // start fall
    Oct,
    Nov,
    Dec // start winter
}

public enum WorldSeason
{
    Spring,
    Summer,
    Fall,
    Winter
}

[System.Serializable]
public class WorldData
{
    public float worldTimeOfDay; // 24 hours in each day cycle
    public int worldDayOfMonth; // 30 days in each month cycle
    public WorldMonth worldMonth;
    public WorldSeason worldSeason;
    public float annualProgress; // percentage of year cycle (0-1)
    public float baseTemperature;
    public float dawnTime;
    public float duskTime;
}

