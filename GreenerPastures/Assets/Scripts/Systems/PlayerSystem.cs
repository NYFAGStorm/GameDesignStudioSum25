// REVIEW: necessary namespaces

public static class PlayerSystem
{
    /// <summary>
    /// Creates a new player and profile
    /// </summary>
    /// <param name="name">in-game player name</param>
    /// <param name="user">profile user name</param>
    /// <param name="pass">profile password</param>
    /// <returns>initialized player data</returns>
    public static PlayerData InitializePlayer( string name, string user, string pass )
    {
        PlayerData retPlayer = new PlayerData();

        // initialize
        retPlayer.playerName = name;
        retPlayer.stats = new PlayerStats();
        retPlayer.profile = ProfileSystem.InitializeProfile(user, pass);
        retPlayer.farm = FarmSystem.InitializeFarm();
        retPlayer.inventory = new InventoryData();
        retPlayer.magic = MagicSystem.IntializeMagic();
        retPlayer.effects = new PlayerEffects[0];

        return retPlayer;
    }
}
