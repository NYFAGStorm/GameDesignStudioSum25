// REVIEW: necessary namespaces

public enum StructureType
{
    Default, // REVIEW:
    WizardTower,
    MarketShop
}

public enum StructureEffect
{
    Default,
    EffectA,
    EffectB,
    EffectC,
    EffectD
}

[System.Serializable]
public class StructureData
{
    public string name;
    public StructureType type;
    public PositionData location; // local space to island parent
    public StructureEffect[] effects;
}

// if there are discrete effects that can be applied to an island
// each effect can then apply separate rules in a modular way
public enum IslandEffect
{
    Default,
    EffectA,
    EffectB,
    EffectC,
    EffectD
}

[System.Serializable]
public class IslandData
{
    public string name;
    public PositionData location;
    public PositionData[] tportNodes; // local space to island parent
    public string[] tportTags;
    public StructureData[] structures;
    public IslandEffect[] effects;
}
