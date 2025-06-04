// REVIEW: necessary namespaces

public enum ItemType
{
    Default,
    ItemA,
    ItemB,
    ItemC,
    ItemD
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
    public string name; // REVIEW: necessary?
    public ItemType type;
    public float durability; // item 'health'
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
