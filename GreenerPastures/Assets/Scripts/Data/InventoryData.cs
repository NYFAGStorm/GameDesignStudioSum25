// REVIEW: necessary namespaces

// REVIEW: using 'seed', 'plant', 'stalk' and 'fruit'; relying on plant type property
public enum ItemType
{
    Default,
    Fertilizer,
    Seed,
    Plant,
    Stalk,
    Fruit,
    Rock // REVIEW:
}

// if there are discrete effects that can be applied to an item
// each effect can then apply separate rules in a modular way
public enum ItemEffects
{
    Default,
    EffectA,
    EffectB,
    EffectC,
    EffectD
}

[System.Serializable]
public class ItemData
{
    public string name;
    public ItemType type;
    public PlantType plant; // if plant, stalk, fruit or seed, else is default
    public float size; // if plant is growth, else is item size? item amount? (default 1f)
    public float health; // if plant is health, else is item durability (default 1f)
    public float quality; // if plant is quality, else is item quality? (default 1f)
    public ItemEffects[] effects;
}

[System.Serializable]
public class LooseItemData
{
    public InventoryData inv; // an inventory of size 1, the item
    public PositionData location;
    public int artFrame; // index to art (if static, remains type item)
    public bool flipped; // facing opposite horizontal direction
    public bool deleteMe; // item has been stored, not here, destroy game object
}

[System.Serializable]
public class InventoryData
{
    public int maxSlots;
    public ItemData[] items;
}
