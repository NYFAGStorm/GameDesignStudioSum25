using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using System.Diagnostics;

public class MultiplayerServer : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerObject;
    private PlayerRef[] playerRefs = new PlayerRef[8];
    private NetworkObject[] playerCharacters = new NetworkObject[8];
    private FarmData[] farms = new FarmData[8];
    private int playerCount = 0;

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_AddPlayer(PlayerRef plr)
    {
        if (!Runner.IsServer) return;

        NetworkObject newPlayer = Runner.Spawn(playerObject);
        playerRefs[playerCount] = plr;
        playerCharacters[playerCount] = newPlayer;
        newPlayer.AssignInputAuthority(plr);
        playerCount++;
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RemovePlayer(PlayerRef plr)
    {
        if (!Runner.IsServer) return;

        int plrSlot = GetPlayerNumber(plr);
        Runner.Despawn(playerCharacters[plrSlot]);
        playerCharacters[plrSlot] = null;
        playerRefs[plrSlot] = PlayerRef.None;
        playerCount--;
    }

    private int GetPlayerNumber(PlayerRef plr)
    {
        // Get player order number using PlayerRef
        for (int p = 0; p < 4; p++)
        {
            if (playerRefs[p] == plr) return p;
        }
        return -1;
    }
}
