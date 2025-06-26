// REVIEW: necessary namespaces

[System.Serializable]
public struct TPortNodeConfig
{
    public string tag;
    public int tPortIndex; // 0 or 1 (the associated tports are indexed)
    public PositionData location;
    public CameraManager.CameraMode cameraMode;
    public PositionData cameraPosition;
}

public enum StructureType  // REVIEW:
{
    Default,
    WizardTower,
    WizardInterior,
    MarketShop,
    MarketShopInterior
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
    public PositionData location; // w = scale of island
    public TPortNodeConfig[] tports;
    public StructureData[] structures;
    public IslandEffect[] effects;
}
