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
    FastGrowI,
    SummonWaterI,
    BlessI,
    MalnutritionI,
    ProsperousI,
    LesionI,
    EclipseI,
    GoldenThumbI
}

[System.Serializable]
public class PlotData
{
    public PlotCondition condition;
    public float sun;
    public float water;
    public float soil;
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

[System.Serializable]
public class FarmData
{
    public PlotData[] plots;
    // REVIEW: island data (where should this live?)
    // island (position, type?, teleport node)
    public FarmEffects[] farmEffects;
}
