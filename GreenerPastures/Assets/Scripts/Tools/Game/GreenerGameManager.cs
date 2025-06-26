using UnityEngine;

public class GreenerGameManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the highest level of game data during game scenes

    public GameData game;
    public bool noisyLogging = true;

    // TODO: indicate who is the server (owner of game data) [match profile ID]
    // TODO: indicate the local player (client player, has main camera control)
    // TODO: integrate player character collection and distribution

    private SaveLoadManager saveMgr;
    private ArtLibraryManager alm;

    private bool gameDataDistributed;
    private bool shutdownDataCollected;


    void Awake()
    {
        saveMgr = GameObject.FindFirstObjectByType<SaveLoadManager>();
        if (saveMgr != null)
        {
            game = saveMgr.GetCurrentGameData();
            Debug.Log("--- GreenerGameManager [Awake] : game data loaded for '" + game.gameName + "'");
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
            gameDataDistributed = DoGameDataDistribution();
    }

    bool DoGameDataDistribution()
    {
        bool retBool = true;

        // player
        if (!DistributePlayerData())
            retBool = false;
        else if (noisyLogging)
            Debug.Log("--- GreenerGameManager [DoGameDataDistribution] : player data distributed.");
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

        // validation notice
        if (retBool)
            Debug.Log("--- GreenerGameManager [DoGameDataDistribution] : game data distribution routine succeeded.");
        else
            Debug.LogWarning("--- GreenerGameManager [DoGameDataDistribution] : game data distribution routine failed. will ignore.");

        return retBool;
    }

    public void DoShutDownGameDataCollection()
    {
        if (shutdownDataCollected)
            return;

        bool validShutdown = true;

        // player
        if (!CollectPlayerData())
            validShutdown = false;
        else if (noisyLogging)
            Debug.Log("--- GreenerGameManager [DoShutDownGameDataCollection] : player data collected.");
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
        else if (noisyLogging)
            Debug.Log("--- GreenerGameManager [DoShutDownGameDataCollection] : " + game.looseItems.Length + " loose items collected.");
        // casts
        if (!CollectCastData())
            validShutdown = false;
        else if (noisyLogging)
            Debug.Log("--- GreenerGameManager [DoShutDownGameDataCollection] : " + game.casts.Length + " casts collected.");

        // validation notice
        if (validShutdown)
            Debug.Log("--- GreenerGameManager [DoShutDownGameDataCollection] : game data collection routine complete. shut down success.");
        else
            Debug.LogWarning("--- GreenerGameManager [DoShutDownGameDataCollection] : game data collection routine invalid. will ignore.");

        shutdownDataCollected = true;
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
        if (pcm != null)
        {
            // temp
            game.players[0] = pcm.GetPlayerData(); // REVIEW: [0] always data-owning player?
            retBool = true;
        }
        

        return retBool;
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

    bool DistributePlayerData()
    {
        bool retBool = false;

        // REVIEW: we need to use a 'RemotePlayerManager' class for all non-client players?
        PlayerControlManager pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        if (pcm != null)
        {
            pcm.SetPlayerData();
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
            // REVIEW: confirm this is all time manager needs to set gloabl time progress
            tim.SetGameSeedTime(game.stats.gameInitTime);
            retBool = true;
        }

        return retBool;
    }

    bool DistributeIslandData()
    {
        bool retBool = false;

        IslandManager im = GameObject.FindFirstObjectByType<IslandManager>();
        if (im == null)
            return retBool;

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
            if (noisyLogging)
                Debug.Log("--- GreenerGameManager [DistributeLooseItems] : " + game.looseItems.Length + " loose items distributed.");
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
}