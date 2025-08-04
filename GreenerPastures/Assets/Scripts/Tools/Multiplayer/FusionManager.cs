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
    private FusionServerManager fsm;
    private int playerCount = 0;
    private PlayerRef host;
    private PlayerRef[] players;
    private RemotePlayerManager[] remotePlayers;
    private PlayerRef localPlayer;

    public NetworkObject server;

    [HideInInspector]
    public bool waitingForHostData = false;

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

    private void Start()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
        players = new PlayerRef[8];
        remotePlayers = new RemotePlayerManager[8];
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

    // Use GameMode.Host and GameMode.Client to determine join type
    public async void StartMultiplayerGame(GameMode mode, string code)
    {
        Debug.Log("--- FusionManager [StartMultiplayerGame] : " + (mode == GameMode.Host ? "Hosting" : "Joining") + " session with code: " + code.ToUpper() + ".");

        Runner = gameObject.AddComponent<NetworkRunner>();
        Runner.ProvideInput = true;

        waitingForHostData = mode == GameMode.Client;

        //var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        //var sceneInfo = new NetworkSceneInfo();
        //if (scene.IsValid) sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        await Runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = code.ToUpper(),
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

            if (playerCount == 1)
            {
                fsm = Runner.Spawn(server).GetComponent<FusionServerManager>();
            }

            fsm.RPC_UpdatePlayerCount(playerCount);
            fsm.RPC_InitializeLocalPlayer(player);

            if (playerCount > 1)
            {
                fsm.RPC_SendMessage(player, "--- FusionManager [OnPlayerJoined] : You are a client. Getting save data from server...");
                fsm.RPC_SendMessage(host, "--- FusionManager [OnPlayerJoined] : " + player + " has connected.");

                fsm.RPC_BeginGameDataTransfer(player);
            }
            else
            {
                fsm.RPC_SendMessage(player, "--- FusionManager [OnPlayerJoined] : You are the host. Sending your save data to server...");
                
                host = player;
                fsm.RPC_RequestGameData(host);
            }
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (Runner.IsServer)
        {
            playerCount--;
            fsm.RPC_SendMessage(host, "--- FusionManager [OnPlayerLeft] : " + player + " has left.");
            fsm.RPC_UpdatePlayerCount(playerCount);
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
