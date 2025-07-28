using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FusionManager : MonoBehaviour, INetworkRunnerCallbacks
{
    // Author: Gustavo Rojas Flores
    // Handles all things connected to network management

    private NetworkRunner Runner;
    private int playerCount = 0;
    private PlayerRef host;
    private GameData currentGame;
    private PlayerRef[] players;
    private RemotePlayerManager[] remotePlayers;
    private SaveLoadManager slm;
    private int nextFreePlayerSlot = 0;
    private PlayerRef localPlayer;

    public GameObject remotePlayerSpawnable;

    private int GetPlayerNumber(PlayerRef player)
    {
        for (int plr = 0; plr < 8; plr++)
        {
            if (players[plr] == player)
            {
                return plr;
            }
        }
        return -1;
    }

    private void UpdateNextPlayerJoinSlot()
    {
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

    private void Start()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        players = new PlayerRef[8];
        remotePlayers = new RemotePlayerManager[8];
        slm = FindFirstObjectByType<SaveLoadManager>();
    }
    
    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SendMessage([RpcTarget] PlayerRef targ, string message)
    {
        Debug.Log(message);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_InitializeLocalPlayer([RpcTarget] PlayerRef targ, PlayerRef p)
    {
        localPlayer = p;
        Debug.Log("--- FusionManager [InitializeLocalPlayer] : You have successfully connected as " + p + ".");
    }

    // Request host's game data as server
    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RequestGameData([RpcTarget] PlayerRef targ)
    {
        RPC_TransferGameData(PlayerRef.None, slm.GetCurrentGameData());
    }

    //// Request client's profile data as server
    //[Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    //public void RPC_RequestProfile([RpcTarget] PlayerRef targ)
    //{
    //    RPC_SendNameToServer(slm.GetCurrentGameData(), localPlayer);
    //}

    //// Transfer profile data to server as client
    //[Rpc(sources: RpcSources.All, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    //public void RPC_SendProfileToServer(PlayerData profile, PlayerRef sender)
    //{
    //    //playerProfiles[GetPlayerNumber(sender)] = profile;
    //}

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SpawnPlayer([RpcTarget] PlayerRef targ, PlayerRef matchingPlayer)
    {
        
    }

    // Transfer game data across network
    [Rpc(sources: RpcSources.All, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_TransferGameData([RpcTarget] PlayerRef targ, GameData data)
    {
        if (Runner.IsServer)
        {
            RPC_SendMessage(host, "--- FusionManager [TransferGameData] : Server has successfully received game data: " + data.gameName);
            currentGame = data;
        }
        else
        {
            Debug.Log("--- FusionManager [TransferGameData] : Loading game data: " + data.gameName);
            slm.SetCurrentGameData(data);
        }
    }

    // Use GameMode.Host and GameMode.Client to determine join type
    public async void StartMultiplayerGame(GameMode mode, string code)
    {
        Debug.Log("--- FusionManager [StartMultiplayerGame] : Starting multiplayer session...");

        Runner = gameObject.AddComponent<NetworkRunner>();
        Runner.ProvideInput = true;

        //var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        //var sceneInfo = new NetworkSceneInfo();
        //if (scene.IsValid) sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        await Runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = code,
            PlayerCount = 8,
            IsOpen = true,
            IsVisible = false
        }
        );
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (Runner.IsServer)
        {
            playerCount++;
            players[nextFreePlayerSlot] = player;
            UpdateNextPlayerJoinSlot();
            RPC_InitializeLocalPlayer(player, player);

            if (playerCount > 1)
            {
                RPC_SendMessage(player, "--- FusionManager [OnPlayerJoined] : You are a client. Getting save data from server...");
                RPC_SendMessage(host, "--- FusionManager [OnPlayerJoined] : " + player + " has connected.");

                RPC_TransferGameData(player, currentGame);
            }
            else
            {
                RPC_SendMessage(player, "--- FusionManager [OnPlayerJoined] : You are the host. Sending your save data to server...");
                
                host = player;
                RPC_RequestGameData(host);
            }
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (Runner.IsServer)
        {
            RPC_SendMessage(host, "--- FusionManager [OnPlayerLeft] : " + player + " has left.");
            
            playerCount--;
            players[GetPlayerNumber(player)] = PlayerRef.None;
            UpdateNextPlayerJoinSlot();
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // Receive client-sided input here

        //input.Set(playerInput);
    }

    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason exit) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
