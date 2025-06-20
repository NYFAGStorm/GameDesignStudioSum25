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
        retGame.gameKey = "[" + System.DateTime.Now.Millisecond + "]" + name;
        retGame.stats = new GameStats();
        retGame.stats.gameInitTime = System.DateTime.Now.ToFileTimeUtc();
        retGame.state = GameState.Initializing;
        retGame.players = new PlayerData[0];
        retGame.options = new GameOptionsData();

        return retGame;
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
}
