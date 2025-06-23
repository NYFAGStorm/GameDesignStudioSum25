using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    // Author: Gustavo Rojas Flores
    // Manages Network creation, room joining and hosting

    [SerializeField] private Accounts accountInfo;
    [SerializeField] private TMP_Text roomCode;
    [SerializeField] private TMP_Text user;
    [SerializeField] private TMP_Text pass;
    [SerializeField] private TMP_Text networkStatus;
    [SerializeField] private TMP_Text userDisplay;
    [SerializeField] private int gameScene;
    [SerializeField] private GameObject loginMenu;
    [SerializeField] private GameObject multiplayerMenu;
    [SerializeField] private GameObject statusPanel;

    private PlayerRef[] players = new PlayerRef[4];
    private NetworkRunner Runner;
    private int plrCount = 0;
    private Camera cam;
    private bool inGame;

    private void OpenMenu(int menu)
    {
        loginMenu.SetActive(menu == 0);
        multiplayerMenu.SetActive(menu == 1);
        statusPanel.SetActive(menu == 2);
    }

    private void Start()
    {
        OpenMenu(0);
    }

    public void Login()
    {
        foreach (Account acc in accountInfo.accounts)
        {
            if (user.text.ToLower().Contains(acc.username.ToLower()) && pass.text.Contains(acc.password))
            {
                userDisplay.text = acc.username;
                OpenMenu(1);
                break;
            }
        }
    }

    public void Host()
    {
        StartGame(GameMode.Host);
    }

    public void Join() 
    {
        StartGame(GameMode.Client);
    }

    async void StartGame(GameMode mode)
    {
        OpenMenu(2);
        networkStatus.text = (mode == GameMode.Host ? "Hosting" : "Joining") + " room \"" + roomCode.text + "\"";

        Runner = gameObject.AddComponent<NetworkRunner>();
        Runner.ProvideInput = true;
        
        var scene = SceneRef.FromIndex(gameScene);
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
        OpenMenu(-1);
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
