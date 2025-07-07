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
using System.Diagnostics;

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

    private PlayerRef[] players = new PlayerRef[8];
    private NetworkRunner Runner;
    private int plrCount = 0;
    private Camera cam;
    private bool inGame;
    private MultiplayerServer multiplayerServer;

    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode actionAKey = KeyCode.E;
    public KeyCode actionBKey = KeyCode.F;
    public KeyCode actionCKey = KeyCode.C;
    public KeyCode actionDKey = KeyCode.V;
    public KeyCode lBumpKey = KeyCode.LeftBracket;
    public KeyCode rBumpKey = KeyCode.RightBracket;
    private PlayerInput inp = new PlayerInput();

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
            multiplayerServer.RPC_AddPlayer(player);
            players[plrCount] = player;
            plrCount++;
        }
    }
    
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (Runner.IsServer)
        {
            multiplayerServer.RPC_RemovePlayer(player);
            players[plrCount] = PlayerRef.None;
            plrCount--;
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        inp.up = Input.GetKey(upKey);
        inp.down = Input.GetKey(downKey);
        inp.left = Input.GetKey(leftKey);
        inp.right = Input.GetKey(rightKey);

        inp.actionA = Input.GetKey(actionAKey);
        inp.actionB = Input.GetKey(actionBKey);
        inp.actionC = Input.GetKey(actionCKey);
        inp.actionD = Input.GetKey(actionDKey);
        inp.lBump = Input.GetKey(lBumpKey);
        inp.rBump = Input.GetKey(rBumpKey);

        inp.actionADown = Input.GetKeyDown(actionAKey);
        inp.actionBDown = Input.GetKeyDown(actionBKey);
        inp.actionCDown = Input.GetKeyDown(actionCKey);
        inp.actionDDown = Input.GetKeyDown(actionDKey);
        inp.lBumpDown = Input.GetKeyDown(lBumpKey);
        inp.rBumpDown = Input.GetKeyDown(rBumpKey);

        input.Set(inp);
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        OpenMenu(-1);

        if (SceneManager.GetActiveScene().buildIndex == 8)
        {
            multiplayerServer = UnityEngine.Object.FindFirstObjectByType<MultiplayerServer>();
        }
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
