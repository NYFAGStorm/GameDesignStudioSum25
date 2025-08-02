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
        retPlayer.almanac = new AlmanacDiscovery();
        retPlayer.almanac.revealed = new bool[0];
        retPlayer.effects = new PlayerEffect[0];

        return retPlayer;
    }

    /// <summary>
    /// Returns true if given player data includes given player effect type
    /// </summary>
    /// <param name="player">player data</param>
    /// <param name="effect">player effect type</param>
    /// <returns>true if given player data includes given player effect type, false if not</returns>
    public static bool PlayerHasEffect( PlayerData player, PlayerEffect effect )
    {
        bool retBool = false;

        for ( int i = 0; i < player.effects.Length; i++ )
        {
            if (player.effects[i] == effect)
            {
                retBool = true;
                break;
            }
        }

        return retBool;
    }

    /// <summary>
    /// Adds a given player effect type to the given player data
    /// </summary>
    /// <param name="player">player data</param>
    /// <param name="effect">player effect type</param>
    /// <returns>player data with effect added, if it didn't already exist</returns>
    public static PlayerData AddPlayerEffect( PlayerData player, PlayerEffect effect )
    {
        PlayerData retPlayer = player;

        // validate does not exist
        if (PlayerHasEffect(retPlayer, effect))
            return retPlayer;
        // add effect
        PlayerEffect[] tmp = new PlayerEffect[retPlayer.effects.Length + 1];
        for (int i = 0; i < retPlayer.effects.Length; i++)
        {
            tmp[i] = retPlayer.effects[i];
        }
        tmp[player.effects.Length] = effect;
        retPlayer.effects = tmp;

        return retPlayer;
    }

    /// <summary>
    /// Removes an player effect from a given player, if it existed
    /// </summary>
    /// <param name="island">player data</param>
    /// <param name="effect">effect type</param>
    /// <returns>player data with effect removed, if it existed</returns>
    public static PlayerData RemovePlayerEffect( PlayerData player, PlayerEffect effect )
    {
        PlayerData retPlayer = player;

        // validate does exist
        if (!PlayerHasEffect(retPlayer, effect))
            return retPlayer;
        // remove effect
        int count = 0;
        PlayerEffect[] tmp = new PlayerEffect[retPlayer.effects.Length - 1];
        for (int i = 0; i < retPlayer.effects.Length; i++)
        {
            if (retPlayer.effects[i] != effect)
            {
                tmp[count] = retPlayer.effects[i];
                count++;
            }
        }
        retPlayer.effects = tmp;

        return retPlayer;
    }

    public static UnityEngine.Color GetPlayerHairColor( PlayerHairColor shade )
    {
        UnityEngine.Color retColor = new UnityEngine.Color();

        switch (shade)
        {
            case PlayerHairColor.Default:
                retColor.r = 1f;
                retColor.g = 1f;
                retColor.b = 1f;
                break;
            case PlayerHairColor.ShadeA:
                retColor.r = .618f;
                retColor.g = .618f;
                retColor.b = .618f;
                break;
            case PlayerHairColor.ShadeB:
                retColor.r = .1f;
                retColor.g = .1f;
                retColor.b = .1f;
                break;
            case PlayerHairColor.ShadeC:
                retColor.r = .381f;
                retColor.g = .2f;
                retColor.b = .1f;
                break;
            case PlayerHairColor.ShadeD:
                retColor.r = .618f;
                retColor.g = .381f;
                retColor.b = .2f;
                break;
            case PlayerHairColor.ShadeE:
                retColor.r = .8f;
                retColor.g = .618f;
                retColor.b = .381f;
                break;
            case PlayerHairColor.ShadeF:
                retColor.r = .9f;
                retColor.g = .8f;
                retColor.b = .618f;
                break;
            case PlayerHairColor.ShadeG:
                retColor.r = .8f;
                retColor.g = .381f;
                retColor.b = .381f;
                break;
        }
        retColor.a = 1f;

        return retColor;
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
                retColor.r = 0.381f;
                retColor.g = 0.25f;
                retColor.b = 0.175f;
                break;
            case PlayerSkinColor.ToneB:
                retColor.r = 0.618f;
                retColor.g = 0.381f;
                retColor.b = 0.27f;
                break;
            case PlayerSkinColor.ToneC:
                retColor.r = 0.618f;
                retColor.g = 0.45f;
                retColor.b = 0.333f;
                break;
            case PlayerSkinColor.ToneD:
                retColor.r = 0.8f;
                retColor.g = 0.618f;
                retColor.b = 0.45f;
                break;
            case PlayerSkinColor.ToneE:
                retColor.r = 0.8f;
                retColor.g = 0.618f;
                retColor.b = 0.5f;
                break;
            case PlayerSkinColor.ToneF:
                retColor.r = 0.725f;
                retColor.g = 0.618f;
                retColor.b = 0.55f;
                break;
            case PlayerSkinColor.ToneG:
                retColor.r = 0.8f;
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
                retColor.r = 0.618f;
                retColor.g = 0.381f;
                retColor.b = 0.381f;
                break;
            case PlayerColor.ColorD:
                retColor.r = 0.8f;
                retColor.g = 0.3f;
                retColor.b = 0.3f;
                break;
            case PlayerColor.ColorE:
                retColor.r = 0.618f;
                retColor.g = 0.2f;
                retColor.b = 0.381f;
                break;
            case PlayerColor.ColorF:
                retColor.r = 0.381f;
                retColor.g = 0.2f;
                retColor.b = 0.618f;
                break;
            case PlayerColor.ColorG:
                retColor.r = 0.618f;
                retColor.g = 0.5f;
                retColor.b = 0.8f;
                break;
            case PlayerColor.ColorH:
                retColor.r = 0.381f;
                retColor.g = 0.618f;
                retColor.b = 0.8f;
                break;
            case PlayerColor.ColorI: // I
                retColor.r = 0.2f;
                retColor.g = 0.381f;
                retColor.b = 0.8f;
                break;
            case PlayerColor.ColorJ:
                retColor.r = 0.1f;
                retColor.g = 0.618f;
                retColor.b = 0.381f;
                break;
            case PlayerColor.ColorK:
                retColor.r = 0.381f;
                retColor.g = 0.618f;
                retColor.b = 0.381f;
                break;
            case PlayerColor.ColorL:
                retColor.r = 0.381f;
                retColor.g = 0.618f;
                retColor.b = 0.1f;
                break;
            case PlayerColor.ColorM:
                retColor.r = 0.8f;
                retColor.g = 0.8f;
                retColor.b = 0.1f;
                break;
            case PlayerColor.ColorN:
                retColor.r = 0.8f;
                retColor.g = 0.618f;
                retColor.b = 0.381f;
                break;
            case PlayerColor.ColorO:
                retColor.r = 0.8f;
                retColor.g = 0.381f;
                retColor.b = 0.2f;
                break;
        }
        retColor.a = 1f;

        return retColor;
    }

    /// <summary>
    /// Returns the additional xp needed to reach the next level given the current level
    /// </summary>
    /// <returns>xp amount of the level</returns>
    public static int GetXPLevelInterval( int currentLevel )
    {
        int xpToNextLevel = 600 + (UnityEngine.Mathf.Max(currentLevel,0) * 300);

        return xpToNextLevel;
    }

    /// <summary>
    /// Returns the level of player based on total xp amount
    /// </summary>
    /// <param name="xp">xp amount total</param>
    /// <returns>associated level</returns>
    public static int GetPlayerLevel( int xp )
    {
        int retInt = 0;
        int xpUsed = xp;

        while ( xpUsed >= 0 )
        {
            xpUsed -= GetXPLevelInterval(retInt);
            retInt++;
        }
        retInt--;

        if (retInt < 0)
            retInt = 0;
        
        return retInt;
    }

    /// <summary>
    /// Returns the amount of xp that would be needed for a player to reach the given level
    /// </summary>
    /// <param name="targetLevel">target level hypothetically reached</param>
    /// <returns>amount of xp needed</returns>
    public static int GetXPAmountToLevel( int targetLevel )
    {
        int retInt = 0;

        for (int i = 0; i < targetLevel; i++)
        {
           retInt += GetXPLevelInterval(i);
        }

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

        int prevLevel = currentLevel;
        int prevLevelXP = 0;
        for ( int i = 0; i < prevLevel; i++ )
        {
            prevLevelXP += GetXPLevelInterval(i);
        }
        int targetAmount = prevLevelXP + GetXPLevelInterval( currentLevel );
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
            retData.arcana++;
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
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.SoiledItI, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.MirrorMirror, retData.magic.library);
                break;
            case 2:
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.BlessI, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.DaylightI, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.GildedWordsI, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.SeedingEcho, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.ColorTrailI, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.ColorTrailII, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.ColorTrailIII, retData.magic.library);
                break;
            case 3:
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.FastGrowII, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.MalnutritionI, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.ProsperousI, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.TheGreatHarvest, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.Splaturn, retData.magic.library);
                break;
            case 4:
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.SummonWaterII, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.LesionI, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.TheReaper, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.Swiftness, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.LightWork, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.StarbloomBurst, retData.magic.library);
                break;
            case 5:
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.SoiledItII, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.GildedWordsII, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.EclipseI, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.GoldenThumbI, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.DullEarth, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.FogOfWar, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.BlessedSpring, retData.magic.library);
                break;
            case 6:
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.MalnutritionII, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.BlessII, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.ProsperousII, retData.magic.library);
                break;
            case 7:
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.DaylightII, retData.magic.library);
                retData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.RabbitHole, retData.magic.library);
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
                retNotifications = new string[7];
                retNotifications[0] = "You Leveled Up!\nYou reached LEVEL 1";
                retNotifications[1] = "You recieve\none ARCANA";
                retNotifications[2] = "Magic Crafting\nUNLOCKED";
                retNotifications[3] = "New spell in Grimoire:\nFast Grow I";
                retNotifications[4] = "New spell in Grimoire:\nSummon Water I";
                retNotifications[5] = "New spell in Grimoire:\nSoiled It I";
                retNotifications[6] = "New spell in Grimoire:\nMirror Mirror";
                break;
            case 2:
                retNotifications = new string[11];
                retNotifications[0] = "You Leveled Up!\nYou reached LEVEL 2";
                retNotifications[1] = "You recieve\none ARCANA";
                retNotifications[2] = "Plant Grafting\nUNLOCKED";
                retNotifications[3] = "UNCOMMON plants at market\nAVAILABLE";
                retNotifications[4] = "New spell in Grimoire:\nBless I";
                retNotifications[5] = "New spell in Grimoire:\nDaylight I";
                retNotifications[6] = "New spell in Grimoire:\nGilded Words I";
                retNotifications[7] = "New spell in Grimoire:\nSeeding Echo";
                retNotifications[8] = "New spell in Grimoire:\nColor Trail I";
                retNotifications[9] = "New spell in Grimoire:\nColor Trail II";
                retNotifications[10] = "New spell in Grimoire:\nColor Trail III";
                break;
            case 3:
                retNotifications = new string[8];
                retNotifications[0] = "You Leveled Up!\nYou reached LEVEL 3";
                retNotifications[1] = "You recieve\none ARCANA";
                retNotifications[2] = "Magic Crafting Cauldron\nUPGRADED";
                retNotifications[3] = "New spell in Grimoire:\nFast Grow II";
                retNotifications[4] = "New spell in Grimoire:\nMalnutrition I";
                retNotifications[5] = "New spell in Grimoire:\nProsperous I";
                retNotifications[6] = "New spell in Grimoire:\nThe Great HArvest";
                retNotifications[7] = "New spell in Grimoire:\nSplaturn";
                break;
            case 4:
                retNotifications = new string[8];
                retNotifications[0] = "You Leveled Up!\nYou reached LEVEL 4";
                retNotifications[1] = "You recieve\none ARCANA";
                retNotifications[2] = "New spell in Grimoire:\nSummon Water II";
                retNotifications[3] = "New spell in Grimoire:\nLesion I";
                retNotifications[4] = "New spell in Grimoire:\nThe Reaper";
                retNotifications[5] = "New spell in Grimoire:\nSwiftness";
                retNotifications[6] = "New spell in Grimoire:\nLight Work";
                retNotifications[7] = "New spell in Grimoire:\nStarbloom Burst";
                break;
            case 5:
                retNotifications = new string[11];
                retNotifications[0] = "You Leveled Up!\nYou reached LEVEL 5";
                retNotifications[1] = "You recieve\none ARCANA";
                retNotifications[2] = "RARE plants at market\nAVAILABLE";
                retNotifications[3] = "Magic Crafting Cauldron\nUPGRADED";
                retNotifications[4] = "New spell in Grimoire:\nSoiled IT II";
                retNotifications[5] = "New spell in Grimoire:\nGilded Words II";
                retNotifications[6] = "New spell in Grimoire:\nEclipse I";
                retNotifications[7] = "New spell in Grimoire:\nGolden Thumb I";
                retNotifications[8] = "New spell in Grimoire:\nDull Earth";
                retNotifications[9] = "New spell in Grimoire:\nFog Of War";
                retNotifications[10] = "New spell in Grimoire:\nBlessed Spring";
                break;
            case 6:
                retNotifications = new string[5];
                retNotifications[0] = "You Leveled Up!\nYou reached LEVEL 6";
                retNotifications[1] = "You recieve\none ARCANA";
                retNotifications[2] = "New spell in Grimoire:\nMalnutrition II";
                retNotifications[3] = "New spell in Grimoire:\nBless II";
                retNotifications[4] = "New spell in Grimoire:\nProsperous II";
                break;
            case 7:
                retNotifications = new string[5];
                retNotifications[0] = "You Leveled Up!\nYou reached LEVEL 7";
                retNotifications[1] = "You recieve\none ARCANA";
                retNotifications[2] = "Magic Crafting Cauldron\nUPGRADED";
                retNotifications[3] = "New spell in Grimoire:\nDaylight II";
                retNotifications[4] = "New spell in Grimoire:\nRabbit Hole I";
                break;
            case 8:
                retNotifications = new string[3];
                retNotifications[0] = "You Leveled Up!\nYou reached LEVEL 8";
                retNotifications[1] = "You recieve\none ARCANA";
                retNotifications[2] = "SPECIAL plants at market\nAVAILABLE";
                break;
            case 9:
                retNotifications = new string[2];
                retNotifications[0] = "You Leveled Up!\nYou reached LEVEL 9";
                retNotifications[1] = "You recieve\none ARCANA";
                break;
            case 10:
                retNotifications = new string[2];
                retNotifications[0] = "You Leveled Up!\nYou reached LEVEL 10";
                retNotifications[1] = "You recieve\none ARCANA";
                break;
            case 11:
                retNotifications = new string[2];
                retNotifications[0] = "You Leveled Up!\nYou reached LEVEL 11";
                retNotifications[1] = "You recieve\none ARCANA";
                break;
            case 12:
                retNotifications = new string[2];
                retNotifications[0] = "You Leveled Up!\nYou reached LEVEL 12";
                retNotifications[1] = "You recieve\none ARCANA";
                break;
            case 13:
                retNotifications = new string[2];
                retNotifications[0] = "You Leveled Up!\nYou reached LEVEL 13";
                retNotifications[1] = "You recieve\none ARCANA";
                break;
            case 14:
                retNotifications = new string[2];
                retNotifications[0] = "You Leveled Up!\nYou reached LEVEL 14";
                retNotifications[1] = "You recieve\none ARCANA";
                break;
            case 15:
                retNotifications = new string[2];
                retNotifications[0] = "You Leveled Up!\nYou reached LEVEL 15";
                retNotifications[1] = "You recieve\none ARCANA";
                break;
        }

        return retNotifications;
    }
}
