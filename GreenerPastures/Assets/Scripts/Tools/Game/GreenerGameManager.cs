using UnityEngine;

public class GreenerGameManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the highest level of game data during game scenes

    public GameData game;

    private bool noisyLogging = false;

    // TODO: indicate who is the server (owner of game data) [match profile ID]
    // TODO: indicate the local player (client player, has main camera control)

    private SaveLoadManager saveMgr;
    private ArtLibraryManager alm;

    private bool gameDataDistributed;
    private bool shutdownDataCollected;
    private bool firstRunDetected;


    void Awake()
    {
        saveMgr = GameObject.FindFirstObjectByType<SaveLoadManager>();
        if (saveMgr != null)
        {
            game = saveMgr.GetCurrentGameData();
            if (noisyLogging)
                Debug.Log("--- GreenerGameManager [Awake] : game data loaded for '" + game.gameName + "'");
            if (game == null)
            {
                Debug.LogError("--- GreenerGameManager [Awake] : no game data. aborting.");
                enabled = false;
                return;
            }
            if (game.state == GameState.Initializing)
            {
                if (noisyLogging)
                    Debug.Log("--- GreenerGameManager [Awake] : game data 'first-run' detected. establishing data init.");
                firstRunDetected = true;
            }
        }
    }

    void OnApplicationQuit()
    {
        DoShutDownGameDataCollection();
    }

    void Start()
    {
        // validate
        if (saveMgr == null)
        {
            Debug.LogError("--- GreenerGameManager [Start] : no save load manager found in scene. aborting.");
            enabled = false;
        }
        alm = GameObject.FindFirstObjectByType<ArtLibraryManager>();
        if (alm == null)
        {
            Debug.LogError("--- GreenerGameManager [Start] : no art library manager found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {

        }
    }

    void Update()
    {
        if (!gameDataDistributed)
        {
            // first-run establish data
            if (firstRunDetected)
            {
                FirstWorldData();
                FirstIslandData();
                FirstLooseItemData();
                FirstCastData();
                FirstPlayerData();
                game.state = GameState.Established;
                if (noisyLogging)
                    Debug.Log("--- GreenerGameManager [Update] : new game '" + game.gameName + "' established.");
                firstRunDetected = false;
            }

            gameDataDistributed = DoGameDataDistribution();

            if (!gameDataDistributed)
                Debug.LogWarning("--- GreenerGameManager [Update] : DoGameDataDistribution routine attempt failed. will ignore.");
            else
            {
                SignalToFastForwardFeatures();
                // game configured for startup
                // profile connected to game
                saveMgr.GetCurrentProfile().state = ProfileState.Playing;
                game = GameSystem.SetPlayerNowPlaying(game, GameSystem.GetProfilePlayer(game, saveMgr.GetCurrentProfile()), true);
            }
        }
    }

    void SignalToFastForwardFeatures()
    {
        // time mananger to signal to other features with 'daysAhead' amount
        GameObject.FindFirstObjectByType<TimeManager>().FastForwardFeatures();
    }

    bool DoGameDataDistribution()
    {
        bool retBool = true;

        // world
        if (!DistributeWorldData())
            retBool = false;
        else if (noisyLogging)
            Debug.Log("--- GreenerGameManager [DoGameDataDistribution] : world data distributed.");
        // islands
        if (!DistributeIslandData())
            retBool = false;
        else if (noisyLogging)
            Debug.Log("--- GreenerGameManager [DoGameDataDistribution] : island data distributed.");
        // loose items
        if (!DistributeLooseItems())
            retBool = false;
        else if (noisyLogging)
            Debug.Log("--- GreenerGameManager [DoGameDataDistribution] : loose items distributed.");
        // casts
        if (!DistributeCastData())
            retBool = false;
        else if (noisyLogging)
            Debug.Log("--- GreenerGameManager [DoGameDataDistribution] : cast data distributed.");
        // player
        if (!DistributePlayerData())
            retBool = false;
        else if (noisyLogging)
            Debug.Log("--- GreenerGameManager [DoGameDataDistribution] : player data distributed.");

        // validation notice
        if (retBool)
        {
            if (noisyLogging)
                Debug.Log("--- GreenerGameManager [DoGameDataDistribution] : game data distribution routine succeeded.");
        }
        else
            Debug.LogWarning("--- GreenerGameManager [DoGameDataDistribution] : game data distribution routine failed. will ignore.");

        return retBool;
    }

    public void DoShutDownGameDataCollection()
    {
        if (shutdownDataCollected)
            return;

        bool validShutdown = true;

        // world
        if (!CollectWorldData())
            validShutdown = false;
        else if (noisyLogging)
            Debug.Log("--- GreenerGameManager [DoShutDownGameDataCollection] : world data collected.");
        // islands
        if (!CollectIslandData())
            validShutdown = false;
        else if (noisyLogging)
            Debug.Log("--- GreenerGameManager [DoShutDownGameDataCollection] : island data collected.");
        // loose items
        if (!CollectLooseItemData())
            validShutdown = false;
        else if (noisyLogging && game.looseItems != null)
            Debug.Log("--- GreenerGameManager [DoShutDownGameDataCollection] : " + game.looseItems.Length + " loose items collected.");
        // casts
        if (!CollectCastData())
            validShutdown = false;
        else if (noisyLogging && game.casts != null)
            Debug.Log("--- GreenerGameManager [DoShutDownGameDataCollection] : " + game.casts.Length + " casts collected.");
        // player
        if (!CollectPlayerData())
            validShutdown = false;
        else if (noisyLogging)
            Debug.Log("--- GreenerGameManager [DoShutDownGameDataCollection] : player data collected.");

        // validation notice
        if (validShutdown)
        {
            if (noisyLogging)
                Debug.Log("--- GreenerGameManager [DoShutDownGameDataCollection] : game data collection routine complete. shut down success.");
        }
        else
            Debug.LogWarning("--- GreenerGameManager [DoShutDownGameDataCollection] : game data collection routine invalid. will ignore.");

        shutdownDataCollected = true;

        // profile disconnecting from game
        saveMgr.GetCurrentProfile().state = ProfileState.Disconnecting;
        game = GameSystem.SetPlayerNowPlaying(game, GameSystem.GetProfilePlayer(game, saveMgr.GetCurrentProfile()), false);
    }

    bool CollectWorldData()
    {
        bool retBool = false;

        TimeManager tim = GameObject.FindFirstObjectByType<TimeManager>();
        if (tim != null)
        {
            game.world = tim.GetWorldData();
            retBool = true;
        }

        return retBool;
    }

    bool CollectIslandData()
    {
        bool retBool = false;

        IslandManager im = GameObject.FindFirstObjectByType<IslandManager>();
        if (im == null)
            return retBool;

        game.islands = im.GetIslandData();
        retBool = true;

        return retBool;
    }

    bool CollectLooseItemData()
    {
        bool retBool = false;
        
        LooseItemManager[] lItems = GameObject.FindObjectsByType<LooseItemManager>(FindObjectsSortMode.None);
        if (lItems.Length == 0)
            return true; // no loose items, yet still valid

        game.looseItems = new LooseItemData[lItems.Length];
        for (int i = 0; i < lItems.Length; i++)
        {
            lItems[i].looseItem.location.x = lItems[i].transform.position.x;
            lItems[i].looseItem.location.y = lItems[i].transform.position.y;
            lItems[i].looseItem.location.z = lItems[i].transform.position.z;
            game.looseItems[i] = lItems[i].looseItem;
        }
        retBool = true;

        return retBool;
    }

    bool CollectCastData()
    {
        bool retBool = false;

        CastManager cm = GameObject.FindFirstObjectByType<CastManager>();
        if (cm != null)
        {
            game.casts = cm.GetCastData();
            retBool = true;
        }
        
        return retBool;
    }

    bool CollectPlayerData()
    {
        bool retBool = false;

        // REVIEW: hold off on collecting remote player character data until multiplayer

        // REVIEW: have not filled in player data for this profile?
        /*
        string clientProfile = saveMgr.GetCurrentProfile().profileID;
        PlayerData clientPlayerData = null;
        PlayerControlManager[] pcms = GameObject.FindObjectsByType<PlayerControlManager>(FindObjectsSortMode.None);
        for (int i=0; i < pcms.Length; i++)
        {
            if (pcms[i].playerData.profileID == clientProfile)
            {
                clientPlayerData = pcms[i].playerData;
                break;
            }
        }

        if (clientPlayerData == null)
            return retBool;

        for (int i = 0; i < game.players.Length; i++)
        {
            if (game.players[i].profileID == clientProfile)
            {
                game.players[i] = clientPlayerData;
                retBool = true;
                break;
            }
        }
        */

        PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        if (pcm != null && game != null && game.players != null &&
            game.players.Length > 0)
        {
            // temp (change for multiplayer)
            game.players[0] = pcm.GetPlayerData(); // REVIEW: [0] always data-owning player?
            retBool = true;
        }

        return retBool;
    }

    bool DistributeWorldData()
    {
        bool retBool = false;

        TimeManager tim = GameObject.FindFirstObjectByType<TimeManager>();
        if (tim != null)
        {
            tim.SetWorldData(game.world);
            tim.SetGameSeedTime(game.stats.gameInitTime);
            retBool = true;
        }

        return retBool;
    }

    bool DistributeIslandData()
    {
        bool retBool = false;

        IslandManager im = GameObject.FindFirstObjectByType<IslandManager>();
        if (im == null) // REVIEW: still needed?
        {
            Debug.LogError("--- GreenerGameManager [DistributeIslandData] : no island manager found in scene. aborting.");
            return retBool;
        }

        im.SetIslandData(game.islands);
        retBool = true;

        return retBool;
    }

    bool DistributeLooseItems()
    {
        bool retBool = false;

        if (alm == null)
        {
            Debug.LogError("GreenerGameManager [DistributeLooseItems] : no art library manager found in scene. aborting.");
            return retBool;
        }

        if (game == null)
        {
            Debug.LogError("GreenerGameManager [DistributeLooseItems] : no game data found. aborting.");
            return retBool;
        }

        if ( game.looseItems != null )
        {
            for (int i = 0; i < game.looseItems.Length; i++)
            {
                GameObject lItem = GameObject.Instantiate((GameObject)Resources.Load("Loose Item"));
                LooseItemManager lim = lItem.GetComponent<LooseItemManager>();
                if (lim != null)
                {
                    Vector3 pos = Vector3.zero;
                    lim.looseItem = game.looseItems[i];
                    pos.x = lim.looseItem.location.x;
                    pos.y = lim.looseItem.location.y;
                    pos.z = lim.looseItem.location.z;
                    lim.transform.position = pos;
                    // parent within Environment/Items
                    GameObject itemsObjFolder = GameObject.Find("Items");
                    if (itemsObjFolder != null)
                        lim.transform.parent = itemsObjFolder.transform;
                    // name appropriately
                    lim.gameObject.name = "Loose Item " + lim.looseItem.inv.items[0].name;
                    // get art (first try from plant type)
                    ArtData aData = new ArtData();
                    lim.frames = new Texture2D[1];
                    if (lim.looseItem.inv.items[0].plant != PlantType.Default)
                    {
                        aData = alm.GetArtData(lim.looseItem.inv.items[0].type, lim.looseItem.inv.items[0].plant);
                    }
                    if (aData.type == ItemType.Default)
                        aData = alm.GetArtData(lim.looseItem.inv.items[0].type);
                    lim.frames[0] = alm.itemImages[aData.artIndexBase];
                }
            }
            //if (noisyLogging)
            //    Debug.Log("--- GreenerGameManager [DistributeLooseItems] : " + game.looseItems.Length + " loose items distributed.");
            game.looseItems = null;
        }

        retBool = true;

        return retBool;
    }

    bool DistributeCastData()
    {
        bool retBool = false;

        CastManager cm = GameObject.FindFirstObjectByType<CastManager>();
        if (cm != null)
        {
            cm.SetCastData(game.casts);
            retBool = true;
        }

        return retBool;
    }

    bool DistributePlayerData()
    {
        bool retBool = false;

        if (game.players == null || game.players.Length == 0)
        {
            Debug.LogError("--- GreenerGameManager [DistributePlayerData] : no players found in data. aborting.");
            return retBool;
        }

        PlayerControlManager[] pcms = GameObject.FindObjectsByType<PlayerControlManager>(FindObjectsSortMode.None);
        if (pcms.Length != 0)
        {
            print("pcm already in scene, aborting");
            return retBool;
        }

        // REVIEW: we need to use a 'RemotePlayerManager' class for all non-client players?

        // establish player character in scene
        GameObject pc = GameObject.Instantiate((GameObject)Resources.Load("Player Character"));
        pc.name = "Player Character '" + game.players[0].playerName + "'";
        Vector3 pos = Vector3.zero;
        pos.x = game.players[0].location.x;
        pos.y = game.players[0].location.y;
        pos.z = game.players[0].location.z;
        bool playerArtFlipped = game.players[0].location.w < 0f;
        pc.transform.GetChild(0).GetComponent<PlayerAnimManager>().imageFlipped = playerArtFlipped;
        pc.transform.parent = GameObject.Find("Character").transform;
        PlayerControlManager pcm = pc.GetComponent<PlayerControlManager>();

        if (Camera.main != null)
        {
            CameraManager cam = Camera.main.gameObject.AddComponent<CameraManager>();
            cam.gameObject.GetComponent<AudioListener>().enabled = false; // REVIEW: remove?
            pc.AddComponent<AudioListener>();
        }

        if (pcm != null)
        {
            pcm.SetPlayerData();
            retBool = true;
        }

        return retBool;
    }

    void FirstWorldData()
    {
        if (game.world != null)
            return;

        game.world = WorldSystem.InitializeWorld();

        if (noisyLogging)
            Debug.Log("--- GreenerGameManager [FirstWorldData] : first world data established.");
    }

    void FirstIslandData()
    {
        if (game.islands != null && game.islands.Length > 0)
            return;

        PositionData pos = new PositionData();

        game.islands = new IslandData[2];

        pos.x = 0f;
        pos.y = 0f;
        pos.z = 0f;
        pos.w = 1f; // scale of 1,1,1
        game.islands[0] = IslandSystem.InitializeIsland("Alpha", pos);
        pos.x = 20f;
        pos.y = 0f;
        pos.z = -20f;
        pos.w = 1f; // scale of 1,1,1
        game.islands[1] = IslandSystem.InitializeIsland("Beta", pos);

        game.islands[0].tports = new TPortNodeConfig[3];
        pos.x = 1.3f;
        pos.y = 0f;
        pos.z = 0.75f;
        pos.w = 0f;
        game.islands[0].tports[0] = IslandSystem.InitializeTeleportNode("tower", 0, pos);
        game.islands[0].tports[0].cameraMode = CameraManager.CameraMode.Follow;
        pos.x = 1f;
        pos.y = -3.67f;
        pos.z = -2.5f;
        pos.w = 0f;
        game.islands[0].tports[1] = IslandSystem.InitializeTeleportNode("tower", 1, pos);
        game.islands[0].tports[1].cameraMode = CameraManager.CameraMode.PanFollow;
        pos.x = 1f;
        pos.y = -1.17f;
        pos.z = -3f;
        pos.w = 0f;
        game.islands[0].tports[1].cameraPosition = pos;
        pos.x = 4f;
        pos.y = 0f;
        pos.z = -4f;
        pos.w = 0f;
        game.islands[0].tports[2] = IslandSystem.InitializeTeleportNode("testTPort", 0, pos);
        game.islands[0].structures = new StructureData[3];
        pos.x = 1f;
        pos.y = 1f;
        pos.z = 2f;
        pos.w = 0f;
        game.islands[0].structures[0] = IslandSystem.InitialzieStructure("wiz tower", StructureType.WizardTower, pos);
        pos.x = 0f;
        pos.y = -2f;
        pos.z = 0f;
        pos.w = 0f;
        game.islands[0].structures[1] = IslandSystem.InitialzieStructure("tower interior", StructureType.WizardInterior, pos);
        pos.x = -3f;
        pos.y = 0f;
        pos.z = -2f;
        pos.w = 0f;
        game.islands[0].structures[2] = IslandSystem.InitialzieStructure("compost bin", StructureType.CompostBin, pos);

        game.islands[1].tports = new TPortNodeConfig[1];
        pos.x = -4f;
        pos.y = 0f;
        pos.z = 4f;
        pos.w = 0f;
        game.islands[1].tports[0] = IslandSystem.InitializeTeleportNode("testTPort", 1, pos);
        game.islands[1].structures = new StructureData[1];
        pos.x = -.75f;
        pos.y = 0.5f;
        pos.z = -2f;
        pos.w = 0f;
        game.islands[1].structures[0] = IslandSystem.InitialzieStructure("market", StructureType.MarketShop, pos);

        if (noisyLogging)
            Debug.Log("--- GreenerGameManager [FirstIslandData] : first island data established.");
    }

    void FirstLooseItemData()
    {
        if (game.looseItems != null && game.looseItems.Length > 0)
            return;

        // REVIEW: necessary?
        game.looseItems = new LooseItemData[0];

        if (noisyLogging)
            Debug.Log("--- GreenerGameManager [FirstLooseItemData] : first loose item data established.");
    }

    void FirstCastData()
    {
        if (game.casts != null && game.casts.Length > 0)
            return;

        // REVIEW: necesary?
        game.casts = new CastData[0];

        if (noisyLogging)
            Debug.Log("--- GreenerGameManager [FirstCastData] : first cast data established.");
    }

    void FirstPlayerData()
    {
        if (game.players == null || game.players.Length == 0)
        {
            Debug.LogError("--- GreenerGameManager [FirstPlayerData] : no owning player. aborting.");
            enabled = false;
        }

        // init farm data
        PositionData pos = new PositionData();

        game.players[0].farm = FarmSystem.InitializeFarm();
        game.players[0].farm.plots = new PlotData[10];

        game.players[0].farm.plots[0] = FarmSystem.InitializePlot();
        pos.x = -1f;
        pos.y = 0f;
        pos.z = -1f;
        game.players[0].farm.plots[0].location = pos;
        game.players[0].farm.plots[1] = FarmSystem.InitializePlot();
        pos.x = 0f;
        pos.y = 0f;
        pos.z = -1f;
        game.players[0].farm.plots[1].location = pos;
        game.players[0].farm.plots[2] = FarmSystem.InitializePlot();
        pos.x = 1f;
        pos.y = 0f;
        pos.z = -1f;
        game.players[0].farm.plots[2].location = pos;
        game.players[0].farm.plots[3] = FarmSystem.InitializePlot();
        pos.x = 2f;
        pos.y = 0f;
        pos.z = -1f;
        game.players[0].farm.plots[3].location = pos;
        game.players[0].farm.plots[4] = FarmSystem.InitializePlot();
        pos.x = -1f;
        pos.y = 0f;
        pos.z = -2f;
        game.players[0].farm.plots[4].location = pos;
        game.players[0].farm.plots[5] = FarmSystem.InitializePlot();
        pos.x = 0f;
        pos.y = 0f;
        pos.z = -2f;
        game.players[0].farm.plots[5].location = pos;
        game.players[0].farm.plots[6] = FarmSystem.InitializePlot();
        pos.x = 1f;
        pos.y = 0f;
        pos.z = -2f;
        game.players[0].farm.plots[6].location = pos;
        game.players[0].farm.plots[7] = FarmSystem.InitializePlot();
        pos.x = 2f;
        pos.y = 0f;
        pos.z = -2f;
        game.players[0].farm.plots[7].location = pos;
        game.players[0].farm.plots[8] = FarmSystem.InitializePlot();
        pos.x = 0f;
        pos.y = 0f;
        pos.z = -3f;
        game.players[0].farm.plots[8].location = pos;
        game.players[0].farm.plots[9] = FarmSystem.InitializePlot();
        pos.x = 1f;
        pos.y = 0f;
        pos.z = -3f;
        game.players[0].farm.plots[9].location = pos;

        if (noisyLogging)
            Debug.Log("--- GreenerGameManager [FirstPlayerData] : first player data established.");
    }
}