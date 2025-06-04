// REVIEW: necessary namespaces

// as we develop, continue to add statistics to keep for the game
[System.Serializable]
public class GameStats
{
    public long gameInitTime; // seed date/time when game created
    public float totalGameTime; // increment while any player online
}

public enum GameState
{
    Default,
    Initializing,
    Established,
    Destroying
}

[System.Serializable]
public class GameData
{
    public GameStats stats;
    public GameState state;
    public PlayerData[] players;
    public int playersOnline;
    public GameOptionsData options;
    public WorldData world;
}
