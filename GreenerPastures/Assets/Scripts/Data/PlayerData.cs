// REVIEW: necessary namespaces

// as we develop, continue to add statistics to keep for the player
[System.Serializable]
public class PlayerStats
{
    public float totalGameTime;
    public int totalPlanted;
    public int totalHarvested;
    public int totalGoldEarned;
    public int totalArcanaEarned;
    public int totalXPEarned;
}

// if there are discrete effects that can be applied to a player character
// each effect can then apply separate rules in a modular way
public enum PlayerEffects
{
    Default,
    EffectA,
    EffectB,
    EffectC,
    EffectD
}

[System.Serializable]
public class PlayerData
{
    public string playerName;
    public PlayerStats stats;
    public string profileID;
    public FarmData farm;
    public int gold;
    public int arcana;
    public int xp;
    public int level;
    public bool freeFly; // can walk off island edges
    public float islandCenterX;
    public float islandCenterY;
    public float islandCenterZ;
    public float islandRange;
    public InventoryData inventory;
    public MagicData magic;
    public PlayerEffects[] effects;
}
