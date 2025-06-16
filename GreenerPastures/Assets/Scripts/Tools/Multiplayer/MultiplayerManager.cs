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

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    // Author: Gustavo Rojas Flores
    // Manages Network creation, room joining and hosting

    [SerializeField] private TMP_Text roomCode;
    [SerializeField] private TMP_Text networkStatus;

    private PlayerRef[] players = new PlayerRef[4];
    private NetworkRunner Runner;
    private int plrCount = 0;
    private Camera cam;
    private bool inGame;
    public bool lookEnabled = true;

    private void Start()
    {
        
    }

    public void Host() 
    {
        StartGame(GameMode.Host);
    }

    public void Join() 
    {
        StartGame(GameMode.Client);
    }

    public void BeginGame()
    {
        
    }

    async void StartGame(GameMode mode)
    {
        networkStatus.text = (mode == GameMode.Host ? "Hosting" : "Joining") + " room \"" + roomCode.text + "\"";

        Runner = gameObject.AddComponent<NetworkRunner>();
        Runner.ProvideInput = true;
        
        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid) sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        await Runner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = roomCode.text,
                Scene = scene,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
            }
        );
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) 
    {
        if (Runner.IsServer)
        {
            players[plrCount] = player;
            plrCount++;
        }
    }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) 
    {
        if (Runner.IsServer)
        {
            players[plrCount] = PlayerRef.None;
            plrCount--;
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) 
    {

        // input.Set(null);
    }
    
    public void OnSceneLoadDone(NetworkRunner runner) 
    {

    }
    
    public void OnSceneLoadStart(NetworkRunner runner) {}
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
    public void OnShutdown(NetworkRunner runner, ShutdownReason exit) {}
    public void OnConnectedToServer(NetworkRunner runner) {}
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {}
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}
}
