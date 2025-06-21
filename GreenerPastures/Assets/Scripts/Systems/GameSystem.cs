// REVIEW: necessary namespaces

public static class GameSystem
{
    /// <summary>
    /// Creates a new game with a given name
    /// </summary>
    /// <returns>initialized game data</returns>
    public static GameData InitializeGame( string name )
    {
        GameData retGame = new GameData();

        // initialize
        retGame.gameName = name;
        retGame.gameKey = "[" + System.DateTime.Now.Millisecond + "]-" + name;
        retGame.stats = new GameStats();
        retGame.stats.gameInitTime = System.DateTime.Now.ToFileTimeUtc();
        retGame.state = GameState.Initializing;
        retGame.players = new PlayerData[0];
        retGame.options = InitializeGameOptions();

        return retGame;
    }

    public static GameOptionsData InitializeGameOptions()
    {
        GameOptionsData retOptions = new GameOptionsData();

        retOptions.maxPlayers = 8;
        retOptions.allowCheats = false;
        retOptions.allowHazards = false;
        retOptions.allowCurses = true;

        return retOptions;
    }

    /// <summary>
    /// Adds a player to a given game
    /// </summary>
    /// <param name="game">game data</param>
    /// <param name="player">player data</param>
    /// <returns>game data, with added player if not already in</returns>
    public static GameData AddPlayer( GameData game, PlayerData player )
    {
        GameData retGame = game;

        bool found = false;
        // validate (not already in game)
        for ( int i=0; i<retGame.players.Length; i++)
        {
            if (retGame.players[i] == player)
            {
                found = true;
                break;
            }
        }
        if (found)
            return retGame;
        // add player
        PlayerData[] tmp = new PlayerData[retGame.players.Length + 1];
        for (int i = 0; i < retGame.players.Length; i++)
        {
            tmp[i] = retGame.players[i];
        }
        tmp[retGame.players.Length] = player;
        retGame.players = tmp;

        return retGame;
    }

    /// <summary>
    /// Removes a player from a given game
    /// </summary>
    /// <param name="game">game data</param>
    /// <param name="player">player data</param>
    /// <returns>game data without player, if was in game</returns>
    public static GameData RemovePlayer( GameData game, PlayerData player )
    {
        GameData retGame = game;

        bool found = false;
        // validate (player in game)
        for (int i = 0; i < retGame.players.Length; i++)
        {
            if ( retGame.players[i] == player )
            {
                found = true;
                break;
            }
        }
        if (!found)
            return retGame;
        // remove player
        int count = 0;
        PlayerData[] tmp = new PlayerData[retGame.players.Length - 1];
        for (int i = 0; i < retGame.players.Length; i++)
        {
            if (retGame.players[i] != player)
            {
                tmp[count] = retGame.players[i];
                count++;
            }
        }
        retGame.players = tmp;

        return retGame;
    }

    /// <summary>
    /// Returns the player data in the given game matching the given profile
    /// </summary>
    /// <param name="game">game data</param>
    /// <param name="profile">profile data</param>
    /// <returns>player data with matching profile ID, or empty player data if not found</returns>
    public static PlayerData GetProfilePlayer( GameData game, ProfileData profile )
    {
        PlayerData retPlayer = new PlayerData();

        // validate
        if (game == null || game.players == null || game.players.Length == 0)
        {
            UnityEngine.Debug.LogError("--- GameSystem [GetProfilePlayer] : invalid game or player data. will return empty player data.");
            return retPlayer;
        }

        for ( int i = 0; i < game.players.Length; i++ )
        {
            if (game.players[i].profileID == profile.profileID)
            {
                retPlayer = game.players[i];
                break;
            }
        }

        return retPlayer;
    }
}
