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
    // REVIEW: this is messy with just necessary plant data, need another way?
    public string name; // REVIEW: necessary?
    public ItemType type;
    public int plantIndex; // if plant or seed is plant type, else is ?
    public float size; // if plant is growth, else is item size? item amount? (default 1f)
    public float health; // if plant is health, else is item durability (default 1f)
    public float quality; // if plant is quality, else is item quality? (default 1f)
    public ItemEffects[] effects;
}

[System.Serializable]
public class LooseItemData
{
    public InventoryData inv; // an inventory of size 1, the item
    // REVIEW: conform location data, etc to game world properties
    public int parentIsland;
    public float posX;
    public float posY;
    public float posZ;
    public int spriteFrame; // index to sprite art (if static, remains type sprite)
    public bool flipped; // facing opposite horizontal direction
    public bool deleteMe; // item has been stored, not here, destroy game object
}

[System.Serializable]
public class InventoryData
{
    public int maxSlots;
    public ItemData[] items;
}
