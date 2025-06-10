// REVIEW: necessary namespaces

// if there are classes of plants that follow separate rules
public enum PlantType
{
    Default,
    Corn,
    Tomato,
    Carrot,
    Poppy,
    Rose,
    Sunflower,
    Moonflower,
    Apple,
    Orange,
    Lemon
}

public enum PlantRarity
{
    Default,
    Common,
    Uncommon,
    Rare,
    Special,
    Unique
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
    public string plantName;
    public PlantType type;
    public PlantRarity rarity;
    public float growthRate; // 100% = base growth rate of all plant types
    public bool isDarkPlant;
    public bool canReFruit; // when stalk only, does growth reduce to 90% for re-fruiting?
    public bool isHarvested; // stalk only, can be grafted with plant fruit item
    public float growth;
    public float vitality;
    public float health;
    public float quality; // does not improve if isHarvested, so canReFruit growth reduction is okay
    public float springVitality;
    public float summerVitality;
    public float fallVitality;
    public float winterVitality;
    public int harvestAmount;
    public float seedPotential;
    public PlantEffect[] plantEffects;
}
