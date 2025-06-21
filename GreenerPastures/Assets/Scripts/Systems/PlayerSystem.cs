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
        retPlayer.islandRange = 7f;
        retPlayer.inventory = InventorySystem.InitializeInventory(5); // players have 5
        retPlayer.magic = MagicSystem.IntializeMagic();
        retPlayer.effects = new PlayerEffects[0];

        return retPlayer;
    }
}
