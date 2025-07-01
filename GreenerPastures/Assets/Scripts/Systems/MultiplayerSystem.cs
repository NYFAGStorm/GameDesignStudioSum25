// REVIEW: necessary namespaces

public static class MultiplayerSystem
{
    /// <summary>
    /// Creates multiplayer data for use in tracking multiplayer status
    /// </summary>
    /// <returns>initialized multiplayer data</returns>
    public static MultiplayerData InitializeMultiplayerData()
    {
        MultiplayerData retData = new MultiplayerData();

        retData.network = NetworkState.NotAvailable;
        retData.state = MultiplayerState.InGameHost;
        retData.profileID = "-none-";
        retData.actingAsHost = true;
        retData.gameKey = "-none-";
        retData.playerName = "-none-";

        return retData;
    }

    /// <summary>
    /// Configures given multiplayer data with given values, or skips if values are empty
    /// </summary>
    /// <param name="mData">multiplayer data</param>
    /// <param name="nState">network state</param>
    /// <param name="mState">multiplayer state</param>
    /// <param name="profID">local current profile ID</param>
    /// <param name="isHost">should this machine act as host?</param>
    /// <param name="gKey">game key for the current running game</param>
    /// <param name="pName">player name for this profile in this game</param>
    /// <returns>configured multiplayer data with given values, if not empty</returns>
    public static MultiplayerData ConfigureMultiplayer(MultiplayerData mData, 
        NetworkState nState, MultiplayerState mState, string profID, bool isHost,
        string gKey, string pName)
    {
        MultiplayerData retData = mData;

        if (nState != NetworkState.Default)
            mData.network = nState;
        if (mState != MultiplayerState.Default)
            mData.state = mState;
        if (profID != "")
            mData.profileID = profID;
        mData.actingAsHost = isHost;
        if (gKey != "")
            mData.gameKey = gKey;
        if (pName != "")
            mData.playerName = pName;

        return retData;
    }

    /// <summary>
    /// Forms a host ping structure from given game data
    /// </summary>
    /// <param name="gData">game data</param>
    /// <returns>formed multiplayer host ping structure</returns>
    public static MultiplayerHostPing FormHostPing( GameData gData )
    {
        MultiplayerHostPing retPing = new MultiplayerHostPing();

        // validate
        if (gData == null || gData.options == null || gData.players == null)
        {
            UnityEngine.Debug.LogError("--- MultiplayerSystem [FormHostPing] : invalid game data or missing options or player data. returning empty ping structure.");
            return retPing;
        }

        retPing.gameKey = gData.gameKey;
        retPing.availablePlayerSlots = (gData.options.maxPlayers - gData.players.Length);
        retPing.profiles = new string[gData.players.Length];
        retPing.playerNames = new string[gData.players.Length];
        for (int i = 0; i < retPing.profiles.Length; i++)
        {
            retPing.profiles[i] = gData.players[i].profileID;
            retPing.playerNames[i] = gData.players[i].playerName;
        }

        return retPing;
    }

    /// <summary>
    /// Forms a remote ping from given profile data
    /// </summary>
    /// <param name="pData">profile data</param>
    /// <returns>formed multiplayer remote ping structure</returns>
    public static MultiplayerRemotePing FormRemotePing( ProfileData pData )
    {
        MultiplayerRemotePing retPing = new MultiplayerRemotePing();

        // validate
        if (pData == null)
        {
            UnityEngine.Debug.LogError("--- MultiplayerSystem [FormRemotePing] : invalid profile data. returning empty ping structure.");
            return retPing;
        }

        retPing.profileID = pData.profileID;
        retPing.gameKeys = pData.gameKeys;

        return retPing;
    }

    /// <summary>
    /// Forms a signal to join from remote client to host, given profile data and player name
    /// </summary>
    /// <param name="pData">profile data</param>
    /// <param name="pName">player name</param>
    /// <returns>formed multiplayer remove join signal structure</returns>
    public static MultiplayerRemoteJoin FormRemoteJoin( ProfileData pData, string pName )
    {
        MultiplayerRemoteJoin retJoin = new MultiplayerRemoteJoin();

        // validate
        if (pData == null || pName == "")
        {
            UnityEngine.Debug.LogError("--- MultiplayerSystem [FormRemoteJoin] : invalid profile data or empty name. returning empty join structure.");
            return retJoin;
        }

        retJoin.profileID = pData.profileID;
        retJoin.playerName = pName;

        return retJoin;
    }
}
