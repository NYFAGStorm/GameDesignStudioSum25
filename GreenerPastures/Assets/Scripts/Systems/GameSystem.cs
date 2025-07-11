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
        retGame.casts = new CastData[0];

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
    /// Sets the now playing flag on the given player in the given game
    /// </summary>
    /// <param name="game">game data</param>
    /// <param name="player">player data</param>
    /// <param name="isPlaying">is this player now playing?</param>
    /// <returns>game data with changed player now playing flag, if existed in game</returns>
    public static GameData SetPlayerNowPlaying( GameData game, PlayerData player, bool isPlaying )
    {
        GameData retGame = game;

        bool found = false;
        // validate (in game)
        for (int i = 0; i < retGame.players.Length; i++)
        {
            if (retGame.players[i] == player)
            {
                found = true;
                break;
            }
        }
        if (!found)
            return retGame;
        int nowPlaying = 0;
        for (int i = 0; i < retGame.players.Length; i++)
        {
            if (retGame.players[i] == player)
            {
                retGame.players[i].nowPlaying = isPlaying;
                break;
            }
            if (retGame.players[i].nowPlaying)
                nowPlaying++;
        }
        // update game data for playersOnline
        retGame.playersOnline = nowPlaying; 

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

    /// <summary>
    /// Returns the player data in the given game matching the given profile
    /// </summary>
    /// <param name="game">game data</param>
    /// <param name="profileID">profile ID</param>
    /// <returns>player data with matching profile ID, or empty player data if not found</returns>
    public static PlayerData GetProfilePlayer(GameData game, string profileID)
    {
        PlayerData retPlayer = new PlayerData();

        // validate
        if (game == null || game.players == null || game.players.Length == 0)
        {
            UnityEngine.Debug.LogError("--- GameSystem [GetProfilePlayer] : invalid game or player data. will return empty player data.");
            return retPlayer;
        }

        for (int i = 0; i < game.players.Length; i++)
        {
            if (game.players[i].profileID == profileID)
            {
                retPlayer = game.players[i];
                break;
            }
        }

        return retPlayer;
    }

    /// <summary>
    /// Returns a unity engine vector3 structure from given position data
    /// </summary>
    /// <param name="pos">position data</param>
    /// <returns>vector3</returns>
    public static UnityEngine.Vector3 GetVector( PositionData pos )
    {
        UnityEngine.Vector3 retVec = UnityEngine.Vector3.zero;

        retVec.x = pos.x;
        retVec.y = pos.y;
        retVec.z = pos.z;

        return retVec;
    }

    /// <summary>
    /// Returns position data structure from given unity engine vector3
    /// </summary>
    /// <param name="vector">vector3</param>
    /// <returns>position data</returns>
    public static PositionData GetPositionData( UnityEngine.Vector3 vector )
    {
        PositionData retPos = new PositionData();

        retPos.x = vector.x;
        retPos.y = vector.y;
        retPos.z = vector.z;

        return retPos;
    }

    /// <summary>
    /// Returns the distance between given position data points
    /// </summary>
    /// <param name="a">position data point A</param>
    /// <param name="b">position data point B</param>
    /// <returns>distance between position points</returns>
    public static float PositionDistance( PositionData a, PositionData b )
    {
        float retFloat = 0f;

        float dA, dB, dC;
        dA = UnityEngine.Mathf.Pow(UnityEngine.Mathf.Abs(a.x - b.x), 2f);
        dB = UnityEngine.Mathf.Pow(UnityEngine.Mathf.Abs(a.y - b.y), 2f);
        dC = UnityEngine.Mathf.Pow(UnityEngine.Mathf.Abs(a.z - b.z), 2f);
        retFloat = UnityEngine.Mathf.Sqrt( dA + dB + dC );

        return retFloat;
    }

    /// <summary>
    /// Returns result position of linearly interpolating between two given position data points
    /// </summary>
    /// <param name="a">position data point A</param>
    /// <param name="b">position data point B</param>
    /// <param name="progress">normalized progress amount (0-1)</param>
    /// <returns>point between position data points, based on proress amount</returns>
    public static PositionData Lerp(PositionData a, PositionData b, float progress)
    {
        PositionData retPos = new PositionData();

        retPos.x = UnityEngine.Mathf.Lerp(a.x, b.x, progress);
        retPos.y = UnityEngine.Mathf.Lerp(a.y, b.y, progress);
        retPos.z = UnityEngine.Mathf.Lerp(a.z, b.z, progress);

        return retPos;
    }

    /// <summary>
    /// Returns true if two given position data are the same (including w)
    /// </summary>
    /// <param name="a">position data a</param>
    /// <param name="b">position data b</param>
    /// <returns>true if all values match, false if not</returns>
    public static bool Equals(PositionData a, PositionData b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
    }

    /// <summary>
    /// Returns true if the given position data equals zero in all values (including w)
    /// </summary>
    /// <param name="pos">position data</param>
    /// <returns>true if all values are zero, false if not</returns>
    public static bool IsZero(PositionData pos)
    {
        return pos.x == 0f && pos.y == 0f && pos.z == 0f && pos.w == 0f;
    }

    /// <summary>
    /// Returns a position data structure with all values zero (including w)
    /// </summary>
    /// <returns>position data of zero</returns>
    public static PositionData Zero()
    {
        PositionData retPos = new PositionData();

        retPos.x = 0f;
        retPos.y = 0f;
        retPos.z = 0f;
        retPos.w = 0f;

        return retPos;
    }
}
