// REVIEW: necessary namespaces

// as we develop, continue to add statistics to keep for the game
[System.Serializable]
public class GameStats
{
    public long gameInitTime; // seed date/time when game created
    public float totalGameTime; // increment while any profile player is in game
}

public enum GameState
{
    Default,
    Initializing,
    Established,
    Destroying
}

[System.Serializable]
public struct PositionData
{
    public float w; // additional data (orientation or range)
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class GameData
{
    public string gameName;
    public string gameKey; // game ID, a unique key used on data file name
    public GameStats stats;
    public GameState state;
    public PlayerData[] players; // player characters and farms
    public int playersOnline;
    public GameOptionsData options;
    public WorldData world; // time and weather
    public IslandData[] islands; // floating islands and structures
    public LooseItemData[] looseItems; // loose items
    public CastData[] casts; // spell casts
}
