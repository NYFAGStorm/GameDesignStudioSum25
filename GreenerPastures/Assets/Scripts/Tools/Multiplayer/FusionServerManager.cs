using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class FusionServerManager : NetworkBehaviour
{
    // Author: Gustavo Rojas Flores
    // Handles server-side Fusion actions

    private NetworkRunner Runner;
    private FusionManager fm;
    private int playerCount = 0;
    private PlayerRef host;
    private GameData currentGame;
    private PlayerRef[] players;
    private RemotePlayerManager[] remotePlayers;
    private SaveLoadManager slm;
    private int nextFreePlayerSlot = 0;
    private PlayerRef localPlayer;

    [HideInInspector]
    public GameObject remotePlayerSpawnable;

    public override void Spawned()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        players = new PlayerRef[8];
        remotePlayers = new RemotePlayerManager[8];
        slm = FindFirstObjectByType<SaveLoadManager>();
        fm = FindFirstObjectByType<FusionManager>();
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_UpdatePlayerCount(int newPlayerCount)
    {
        playerCount = newPlayerCount;
        for (int plr = 0; plr < 8; plr++)
        {
            if (players[plr] == PlayerRef.None)
            {
                nextFreePlayerSlot = plr;
                return;
            }
        }

        return;
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SpawnPlayer([RpcTarget] PlayerRef targ, PlayerRef matchingPlayer)
    {

    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_BeginGameDataTransfer(PlayerRef target)
    {
        RPC_TransferGameDataToClient(target, JsonUtility.ToJson(currentGame));
    }

    // Transfer game data to client
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_TransferGameDataToClient([RpcTarget] PlayerRef client, string data)
    {
        GameData game = JsonUtility.FromJson<GameData>(data);
        Debug.Log("--- FusionManager [TransferGameData] : Loading game data: " + game.gameName);
        slm.SetCurrentGameData(game);
        fm.waitingForHostData = false;
    }

    // Transfer game data to server
    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_TransferGameDataToServer(string data)
    {
        GameData game = JsonUtility.FromJson<GameData>(data);
        RPC_SendMessage(host, "--- FusionManager [TransferGameData] : Server has successfully received game data: " + game.gameName);
        currentGame = game;
    }

    // Send a message to a client
    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, InvokeLocal = true, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SendMessage([RpcTarget] PlayerRef targ, string message)
    {
        Debug.Log(message);
    }

    // Initialize a client
    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, InvokeLocal = true, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_InitializeLocalPlayer([RpcTarget] PlayerRef targ)
    {
        localPlayer = targ;
        Debug.Log("--- FusionManager [InitializeLocalPlayer] : You have successfully connected as " + targ + ".");
    }

    // Request host's game data as server
    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, InvokeLocal = true, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RequestGameData([RpcTarget] PlayerRef targ)
    {
        RPC_TransferGameDataToServer(JsonUtility.ToJson(slm.GetCurrentGameData()));
    }
}
