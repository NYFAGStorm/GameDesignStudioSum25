// REVIEW: necessary namespaces

// if there are classes of plants that follow separate rules
public enum PlantType
{
    Default,
    TypeA,
    TypeB,
    TypeC,
    TypeD
}

// if there are discrete effects that can be applied to a plant
// each effect can then apply separate rules in a modular way
public enum PlantEffect
{
    Default,
    EffectA,
    EffectB,
    EffectC,
    EffectD
}

// generic plant data
[System.Serializable]
public class PlantData
{
    public string plantName; // REVIEW: necessary?
    public PlantType plantType;
    public float plantGrowth;
    public float plantHealth;
    public float plantLight;
    public float plantWater;
    public float plantQuality;
    public PlantEffect[] plantEffects;
}
