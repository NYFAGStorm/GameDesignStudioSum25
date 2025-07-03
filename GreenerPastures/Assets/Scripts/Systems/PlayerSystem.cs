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
                retColor.r = 0.25f;
                retColor.g = 0.2f;
                retColor.b = 0.125f;
                break;
            case PlayerSkinColor.ToneB:
                retColor.r = 0.3f;
                retColor.g = 0.25f;
                retColor.b = 0.175f;
                break;
            case PlayerSkinColor.ToneC:
                retColor.r = 0.45f;
                retColor.g = 0.381f;
                retColor.b = 0.27f;
                break;
            case PlayerSkinColor.ToneD:
                retColor.r = 0.55f;
                retColor.g = 0.45f;
                retColor.b = 0.333f;
                break;
            case PlayerSkinColor.ToneE:
                retColor.r = 0.67f;
                retColor.g = 0.575f;
                retColor.b = 0.45f;
                break;
            case PlayerSkinColor.ToneF:
                retColor.r = 0.7f;
                retColor.g = 0.6f;
                retColor.b = 0.5f;
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
        }
        retColor.a = 1f;

        return retColor;
    }
}
