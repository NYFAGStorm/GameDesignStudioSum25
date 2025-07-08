// REVIEW: necessary namespaces

public static class PlayerSystem
{
    /// <summary>
    /// Creates a new player and profile
    /// </summary>
    /// <param name="name">in-game player name</param>
    /// <param name="profID">profile id</param>
    /// <returns>initialized player data</returns>
    public static PlayerData InitializePlayer( string name, string profID )
    {
        PlayerData retPlayer = new PlayerData();

        // initialize
        retPlayer.playerName = name;
        retPlayer.stats = new PlayerStats();
        retPlayer.profileID = profID;
        retPlayer.farm = FarmSystem.InitializeFarm();
        retPlayer.gold = 50; // starting gold is 50
        retPlayer.island.w = 7f;
        retPlayer.camera.x = 0f;
        retPlayer.camera.y = 2.5f;
        retPlayer.camera.z = -5f;
        retPlayer.camSaved = retPlayer.camera;
        retPlayer.camMode = CameraManager.CameraMode.Follow;
        retPlayer.inventory = InventorySystem.InitializeInventory(5); // players have 5
        retPlayer.magic = MagicSystem.IntializeMagic();
        retPlayer.effects = new PlayerEffects[0];

        return retPlayer;
    }

    public static UnityEngine.Color GetPlayerSkinColor( PlayerSkinColor tone )
    {
        UnityEngine.Color retColor = new UnityEngine.Color();

        switch (tone)
        {
            case PlayerSkinColor.Default:
                retColor.r = 1f;
                retColor.g = 1f;
                retColor.b = 1f;
                break;
            case PlayerSkinColor.ToneA:
                retColor.r = 0.3f;
                retColor.g = 0.25f;
                retColor.b = 0.175f;
                break;
            case PlayerSkinColor.ToneB:
                retColor.r = 0.45f;
                retColor.g = 0.381f;
                retColor.b = 0.27f;
                break;
            case PlayerSkinColor.ToneC:
                retColor.r = 0.55f;
                retColor.g = 0.45f;
                retColor.b = 0.333f;
                break;
            case PlayerSkinColor.ToneD:
                retColor.r = 0.67f;
                retColor.g = 0.575f;
                retColor.b = 0.45f;
                break;
            case PlayerSkinColor.ToneE:
                retColor.r = 0.7f;
                retColor.g = 0.6f;
                retColor.b = 0.5f;
                break;
            case PlayerSkinColor.ToneF:
                retColor.r = 0.725f;
                retColor.g = 0.67f;
                retColor.b = 0.55f;
                break;
            case PlayerSkinColor.ToneG:
                retColor.r = 0.75f;
                retColor.g = 0.7f;
                retColor.b = 0.618f;
                break;
        }
        retColor.a = 1f;

        return retColor;
    }

    public static UnityEngine.Color GetPlayerColor( PlayerColor tone )
    {
        UnityEngine.Color retColor = new UnityEngine.Color();

        switch (tone)
        {
            case PlayerColor.Default:
                retColor.r = 1f;
                retColor.g = 1f;
                retColor.b = 1f;
                break;
            case PlayerColor.ColorA:
                retColor.r = 0.8f;
                retColor.g = 0.8f;
                retColor.b = 0.8f;
                break;
            case PlayerColor.ColorB:
                retColor.r = 0.618f;
                retColor.g = 0.618f;
                retColor.b = 0.618f;
                break;
            case PlayerColor.ColorC:
                retColor.r = 0.8f;
                retColor.g = 0.3f;
                retColor.b = 0.3f;
                break;
            case PlayerColor.ColorD:
                retColor.r = 0.55f;
                retColor.g = 0.5f;
                retColor.b = 0.8f;
                break;
            case PlayerColor.ColorE:
                retColor.r = 0.3f;
                retColor.g = 0.7f;
                retColor.b = 0.8f;
                break;
            case PlayerColor.ColorF:
                retColor.r = 0.125f;
                retColor.g = 0.618f;
                retColor.b = 0.3f;
                break;
            case PlayerColor.ColorG:
                retColor.r = 0.8f;
                retColor.g = 0.618f;
                retColor.b = 0.381f;
                break;
            case PlayerColor.ColorH:
                retColor.r = 0.381f;
                retColor.g = 0.8f;
                retColor.b = 0.1f;
                break;
            case PlayerColor.ColorI:
                retColor.r = 0.1f;
                retColor.g = 0.381f;
                retColor.b = 0.8f;
                break;
            case PlayerColor.ColorJ:
                retColor.r = 0.618f;
                retColor.g = 0.1f;
                retColor.b = 0.381f;
                break;
            case PlayerColor.ColorK:
                retColor.r = 0.8f;
                retColor.g = 0.381f;
                retColor.b = 0.1f;
                break;
            case PlayerColor.ColorL:
                retColor.r = 0.381f;
                retColor.g = 0.1f;
                retColor.b = 0.8f;
                break;
            case PlayerColor.ColorM:
                retColor.r = 0.381f;
                retColor.g = 0.8f;
                retColor.b = 0.381f;
                break;
            case PlayerColor.ColorN:
                retColor.r = 0.8f;
                retColor.g = 0.8f;
                retColor.b = 0.1f;
                break;
            case PlayerColor.ColorO:
                retColor.r = 0.8f;
                retColor.g = 0.618f;
                retColor.b = 0.618f;
                break;
        }
        retColor.a = 1f;

        return retColor;
    }

    /// <summary>
    /// Returns the regular interval a player can reach the next level
    /// </summary>
    /// <returns>xp amount of the level interval</returns>
    public static int GetXPLevelInterval()
    {
        const int XPLEVELINTERVAL = 300; // x10 spreadsheet numbers (we could x100)

        return XPLEVELINTERVAL;
    }

    /// <summary>
    /// Returns the level of player based on total xp amount
    /// </summary>
    /// <param name="xp">xp amount total</param>
    /// <returns>associated level</returns>
    public static int GetPlayerLevel( int xp )
    {
        int retInt = 0;

        // x2 interval needed to reach level 1
        retInt = (xp / GetXPLevelInterval()) - 1;

        if (retInt < 0)
            retInt = 0;
        
        return retInt;
    }

    /// <summary>
    /// Returns the amount of xp a player would need to reach next level, given current xp and level
    /// </summary>
    /// <param name="currentXP">total player xp</param>
    /// <param name="currentLevel">current player level</param>
    /// <returns>amount of xp to reach next level</returns>
    public static int GetXPAmountToNextLevel( int currentXP, int currentLevel )
    {
        int retInt = 0;

        int targetAmount = (currentLevel + 1) * GetXPLevelInterval();
        // x2 interval needed to reach level 1
        targetAmount += GetXPLevelInterval();
        retInt = targetAmount - currentXP;

        return retInt;
    }

    /// <summary>
    /// Returns true if given player data will level up if awarded given xp amount
    /// </summary>
    /// <param name="pData">player data</param>
    /// <param name="xpAmount">xp amount to be awarded</param>
    /// <returns>true if award will result in level up, false if not</returns>
    public static bool WillPlayerLevelUp( PlayerData pData, int xpAmount )
    {
        return GetXPAmountToNextLevel(pData.xp, pData.level) <= xpAmount;
    }

    /// <summary>
    /// Awards the given player data the given amount of xp
    /// </summary>
    /// <param name="pData">player data</param>
    /// <param name="amount">awarded xp amount</param>
    /// <returns>player data with xp added and level increased, if level up</returns>
    public static PlayerData AwardPlayerXP( PlayerData pData, int amount )
    {
        PlayerData retData = pData;

        if (amount <= 0)
            return retData;

        if (WillPlayerLevelUp(retData, amount))
        {
            retData.level++;
            retData = AwardPlayerForLevelUp(retData);
            // player control manager also calls GetLevelUpNotifications()
        }
        retData.xp += amount;

        return retData;
    }

    /// <summary>
    /// Applies awards to player for reaching their current level (at level-up)
    /// </summary>
    /// <param name="pData">player data</param>
    /// <returns>player data with awards configured</returns>
    public static PlayerData AwardPlayerForLevelUp( PlayerData pData )
    {
        PlayerData retData = pData;

        // NOTE: mirror this with the GetLevelUpNotifications() function below
        // the level this player just reached
        switch (retData.level)
        {
            case 0:
                // welcome to the game, no participation award
                break;
            case 1:
                retData.magic = MagicSystem.IntializeMagic();
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.FastGrowI, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.SummonWaterI, retData.magic.library);
                break;
            case 2:
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.BlessI, retData.magic.library);
                break;
            case 3:
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.MalnutritionI, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.ProsperousI, retData.magic.library);
                break;
            case 4:
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.LesionI, retData.magic.library);
                break;
            case 5:
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.EclipseI, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.GoldenThumbI, retData.magic.library);
                break;
            case 6:
                break;
            case 7:
                break;
            case 8:
                break;
            case 9:
                break;
            case 10:
                break;
            case 11:
                break;
            case 12:
                break;
            case 13:
                break;
            case 14:
                break;
            case 15:
                break;
        }

        return retData;
    }

    /// <summary>
    /// Returns a list of notifications of awards recieved by leveling up to given level
    /// </summary>
    /// <param name="level">new level reached</param>
    /// <returns>an array of string messages intended to use as player notifications</returns>
    public static string[] GetLevelUpNotifications( int level )
    {
        string[] retNotifications = new string[0];

        // the level this player just reached
        switch (level)
        {
            case 0:
                // welcome to the game
                break;
            case 1:
                retNotifications = new string[3];
                retNotifications[0] = "Magic Crafting\nUNLOCKED";
                retNotifications[1] = "New spell in Grimoire:\nFast Grow I";
                retNotifications[2] = "New spell in Grimoire:\nSummon Water I";
                break;
            case 2:
                retNotifications = new string[2];
                retNotifications[0] = "Plant Grafting\nUNLOCKED";
                retNotifications[1] = "New spell in Grimoire:\nBless I";
                break;
            case 3:
                retNotifications = new string[2];
                retNotifications[0] = "New spell in Grimoire:\nMalnutrition I";
                retNotifications[1] = "New spell in Grimoire:\nProsperous I";
                break;
            case 4:
                retNotifications = new string[1];
                retNotifications[0] = "New spell in Grimoire:\nLesion I";
                break;
            case 5:
                retNotifications = new string[2];
                retNotifications[0] = "New spell in Grimoire:\nEclipse I";
                retNotifications[1] = "New spell in Grimoire:\nGolden Thumb I";
                break;
            case 6:
                break;
            case 7:
                break;
            case 8:
                break;
            case 9:
                break;
            case 10:
                break;
            case 11:
                break;
            case 12:
                break;
            case 13:
                break;
            case 14:
                break;
            case 15:
                break;
        }

        return retNotifications;
    }
}
