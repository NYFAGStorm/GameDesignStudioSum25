// REVIEW: necessary namespaces

public enum NetworkState
{
    Default,
    NotAvailable,
    Available,
    GameConnected // handshake has taken place, in game
}

public enum MultiplayerState
{
    Default,
    Scanning, // for games on network as remote
    Joining, // entering game remotely
    InGameRemote, // connected and playing as remote
    InGameHost, // playing as host, connected or not
    Leaving // sending last data if remote, saving if host
}

// a host running a game should provide this to scanning remote clients
[System.Serializable]
public struct MultiplayerHostPing
{
    public string gameKey; // this is a unique identifier for games (profiles know this)
    public int availablePlayerSlots; // game.options.maxPlayers - game.players.length
    public string[] profiles; // an array of profile IDs who have been in this game as players
    public string[] playerNames; // an array of player names matching the array of profile IDs
}

// a scanning remote client should provide this to hosts running a game
[System.Serializable]
public struct MultiplayerRemotePing
{
    public string profileID; // this is a unique identifier for profiles (games know this)
    public string[] gameKeys; // an array of game keys that this profile has been in as a player

    // (can match this ping to a player who has been in this game before)
    // either a player can join because a) they have been in the game before,
    // or, b) they can join because there is an availbale player slot
}

// a definitive handshake from remote to host confirming entrance to game, as player
[System.Serializable]
public struct MultiplayerRemoteJoin
{
    public string profileID;
    public string playerName;
}

// this represents what a local machine would want to know about our multiplayer system
// REVIEW: collecting all potential data now, will organize later
[System.Serializable]
public class MultiplayerData
{
    public NetworkState network;
    public MultiplayerState state;
    public string profileID; // this local profile
    public bool actingAsHost; // owner of game data
    public string gameKey; // current game
    public string playerName; // game player name
}
