// REVIEW: necessary namespaces

public enum PlotCondition
{
    Default,
    Wild,
    Dirt,
    Tilled,
    Growing,
    Uprooted
}

// if there are discrete effects that can be applied to a plot
// each effect can then apply separate rules in a modular way
public enum PlotEffect
{
    Default,
    EffectA,
    EffectB,
    EffectC,
    EffectD
}

[System.Serializable]
public class PlotData
{
    public PlotCondition condition;
    public float sun;
    public float water;
    public PlantData plant;
    public PlotEffect[] plotEffects;
}

// if there are discrete effects that can be applied to a whole farm
// each effect can then apply separate rules in a modular way
public enum FarmEffects
{
    Default,
    EffectA,
    EffectB,
    EffectC,
    EffectD
}

// REVIEW:
// we may avoid complexity by asserting all farms have a 2D grid of plots
// rows and columns of plots to make up a rectangular farm field
// this allows us to define plots as a 2D array, columns first
// example: a 5x5 array of plot can be described as an array of 25

[System.Serializable]
public class FarmData
{
    public PlotData[] farmPlots;
    public FarmEffects[] farmEffects;
}
