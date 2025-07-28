using System;
using System.Collections.Generic;
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
    private string[] playerNames;

    private void Start()
    {
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_SendMessage([RpcTarget] PlayerRef targ, string message)
    {
        Debug.Log(message);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_RequestGameData([RpcTarget] PlayerRef targ)
    {
        RPC_TransferGameData(PlayerRef.None, FindFirstObjectByType<SaveLoadManager>().GetCurrentGameData());
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All, HostMode = RpcHostMode.SourceIsServer)]
    public void RPC_TransferGameData([RpcTarget] PlayerRef targ, GameData data)
    {
        if (Runner.IsServer)
        {
            RPC_SendMessage(host, "--- FusionManager [SendMessage] : Server has successfully received game data.");
            currentGame = data;
        }
        else
        {
            FindFirstObjectByType<SaveLoadManager>().SetCurrentGameData(data);
        }
    }

    // Use GameMode.Host and GameMode.Client to determine join type
    public async void StartMultiplayerGame(GameMode mode, string code)
    {
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
            RPC_SendMessage(player, "--- FusionManager [SendMessage] : You have successfully connected as " + player + ".");
            
            playerCount++;

            if (playerCount > 1)
            {
                RPC_SendMessage(player, "--- FusionManager [SendMessage] : You are a client. Getting save data from server...");
                
                RPC_TransferGameData(player, currentGame);
            }
            else
            {
                RPC_SendMessage(player, "--- FusionManager [SendMessage] : You are the host. Sending your save data to server...");
                
                host = player;
                RPC_RequestGameData(host);
            }
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (Runner.IsServer)
        {
            RPC_SendMessage(player, "--- FusionManager [SendMessage] : " + player + " has left.");
            
            playerCount--;
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // Receive client-sided input here

        //input.Set(playerInput);
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason exit) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
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
