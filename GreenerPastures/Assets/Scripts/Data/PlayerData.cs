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
}

// REVIEW:
public enum PlayerLocation
{
    Default,
    Home,
    Farm,
    Shop,
    Lab,
    Visiting
}

// REVIEW:
public enum PlayerAction
{
    Default,
    Idle,
    Walking,
    Tilling,
    Planting,
    Watering,
    Harvesting,
    Casting,
    Teleporting
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
    public ProfileData profile;
    public FarmData farm;
    public int gold;
    public int arcana;
    public PlayerLocation location;
    public PlayerAction action;
    public InventoryData inventory;
    public MagicData magic;
    public PlayerEffects[] effects;
}
