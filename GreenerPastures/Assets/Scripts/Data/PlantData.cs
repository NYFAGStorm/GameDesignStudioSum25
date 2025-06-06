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

public enum PlantSegment
{
    Default,
    Fruit,
    Stalk
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
    public PlantType type;
    public PlantSegment segment;
    public float growth;
    public float vitality;
    public float health;
    public float quality;
    public PlantEffect[] plantEffects;
}
