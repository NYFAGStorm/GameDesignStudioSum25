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

public enum PlayerHairColor
{
    Default,
    ShadeA,
    ShadeB,
    ShadeC,
    ShadeD,
    ShadeE,
    ShadeF,
    ShadeG
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
    public PlayerHairColor hairColor;
    public PlayerSkinColor skinColor;
    public PlayerColor mainColor;
    public PlayerColor secondaryColor;
    public PlayerColor accentColor;
}

[System.Serializable]
public struct AlmanacDiscovery
{
    public bool[] revealed;
}

[System.Serializable]
public class PlayerMessage
{
    public string recipientProfileID;
    public float messageID;
    public string sender;
    public string recipient;
    public string message;
}

// if there are discrete effects that can be applied to a player character
// each effect can then apply separate rules in a modular way
public enum PlayerEffect
{
    Default,
    EdenLetterOne,
    EdenLetterTwo,
    EdenLetterThree,
    EdenLetterFour,
    EdenLetterFive,
    EdenLetterSix,
    EdenLetterSeven,
    SpellMirrorMirror,
    SpellGildedWordsI,
    SpellColorTrailI,
    SpellColorTrailII,
    SpellColorTrailIII,
    SpellSwiftness,
    SpellLightWork,
    SpellGildedWordsII,
    SpellRabbitHole,
    SkillCoverCrop,
    SkillFocusFlow,
    SkillDarkBiomage,
    SkillSeedFairy,
    SkillPlantDoctor,
    SkillFriendsChicken,
    SkillFriendsMerchant,
    SkillFriendsGoldFairy,
    SkillFriendsSalesman,
    SkillMidasBiomancer,
    SkillWasteManagement,
    SkillCleanUp,
    SkillLightenUp,
    SkillCashIn,
    SkillTakeMeHome,
    SkillGroceryList,
    SkillSoCrafty,
    SkillCoolCat,
    SkillMysticForager,
    SkillArchmage,
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
    public PlayerEffect[] effects;

    // XP AWARD VALUES
    public const int XP_USETELEPORTER = 0; // abuse potential too high
    public const int XP_PICKUPITEM = 0;
    public const int XP_DROPITEM = 0;
    public const int XP_WORKTHEPLOT = 3;
    public const int XP_PLANTASEED = 7;
    public const int XP_COMPLETETUTORIAL = 20;
    public const int XP_HARVESTPLANT = 12;
    public const int XP_SELLTOSHOP = 5;
    public const int XP_BUYFROMSHOP = 2;
    public const int XP_WATERTHEPLOT = 1;
    public const int XP_DIGAHOLE = 3;
    public const int XP_FERTILIZEPLOT = 4;
    public const int XP_PAYRENT = 50; // not implemented
    public const int XP_CRAFTMAGIC = 25;
    public const int XP_CASTMAGIC = 15;
    public const int XP_GRAFTPLANT = 15;
    public const int XP_BROADCASTPLANT = 8;
    public const int XP_TRANSPLANT = 9;
    public const int XP_FINDCLICKABLE = 10;
    public const int XP_CATCHFIREFLY = 10; // not implemented
    public const int XP_HOLIDAYBONUS = 100; // not implemented
}