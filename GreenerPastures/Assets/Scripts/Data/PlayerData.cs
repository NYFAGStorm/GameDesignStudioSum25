// REVIEW: necessary namespaces

// as we develop, continue to add statistics to keep for the player
[System.Serializable]
public struct PlayerStats
{
    public float totalGameTime;
    public int totalPlanted;
    public int totalHarvested;
    public int totalGoldEarned;
    public int totalArcanaEarned;
    public int totalXPEarned;
    public int totalLevelsEarned;
}

public enum PlayerModelType
{
    Default,
    Male,
    Female
}

public enum PlayerSkinColor
{
    Default,
    ToneA,
    ToneB,
    ToneC,
    ToneD,
    ToneE,
    ToneF,
    ToneG
}

public enum PlayerColor
{
    Default,
    ColorA,
    ColorB,
    ColorC,
    ColorD,
    ColorE,
    ColorF,
    ColorG,
    ColorH,
    ColorI,
    ColorJ,
    ColorK,
    ColorL,
    ColorM,
    ColorN,
    ColorO
}

[System.Serializable]
public struct PlayerOptions
{
    public PlayerModelType model;
    public PlayerSkinColor skinColor;
    public PlayerColor mainColor;
    public PlayerColor accentColor;
}

[System.Serializable]
public struct AlmanacDiscovery
{
    public bool[] revealed;
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
    public bool nowPlaying;
    public PlayerOptions options;
    public PlayerStats stats;
    public string profileID;
    public FarmData farm;
    public int playerIsland; // index on game data
    public int gold;
    public int arcana;
    public int xp;
    public int level;
    public bool freeFly; // can walk off island edges
    public PositionData location;
    public PositionData island; // island tether data
    public PositionData camera;
    public PositionData camSaved;
    public CameraManager.CameraMode camMode;
    public InventoryData inventory;
    public MagicData magic;
    public AlmanacDiscovery almanac;
    public PlayerEffects[] effects;

    // XP AWARD VALUES
    public const int XP_USETELEPORTER = 0; // abuse potential too high
    public const int XP_PICKUPITEM = 1;
    public const int XP_DROPITEM = 1;
    public const int XP_WORKTHEPLOT = 2;
    public const int XP_PLANTASEED = 5;
    public const int XP_COMPLETETUTORIAL = 15;
    public const int XP_HARVESTPLANT = 10;
    public const int XP_SELLTOSHOP = 5;
    public const int XP_BUYFROMSHOP = 4;
    public const int XP_WATERTHEPLOT = 3;
    public const int XP_DIGAHOLE = 4;
    public const int XP_FERTILIZEPLOT = 3;
    public const int XP_PAYRENT = 50; // not implemented
    public const int XP_CRAFTMAGIC = 15;
    public const int XP_CASTMAGIC = 10;
    public const int XP_GRAFTPLANT = 9;
    public const int XP_BROADCASTPLANT = 6;
    public const int XP_TRANSPLANT = 7;
    public const int XP_FINDCLICKABLE = 3;
    public const int XP_CATCHFIREFLY = 3; // not implemented
    public const int XP_HOLIDAYBONUS = 100; // not implemented
}