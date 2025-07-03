using UnityEngine;

public class PlayerControlManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the local player controls for their character

    public PlayerData playerData;
    public float characterSpeed = 2.7f;

    public enum PlayerControlType
    {
        Default,
        Up,
        Down,
        Left,
        Right,
        ActionA,
        ActionB,
        ActionC,
        ActionD
    }

    public struct PlayerActions
    {
        public bool actionA;
        public bool actionADown; // 'first press' frame signal only (must un-press)
        public bool actionB;
        public bool actionBDown;
        public bool actionC;
        public bool actionCDown;
        public bool actionD;
        public bool actionDDown;
        public bool lBump;
        public bool lBumpDown;
        public bool rBump;
        public bool rBumpDown;
        public bool castMagic;
        public bool graftPlant;
    }

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
    public KeyCode castKey = KeyCode.Q;
    public KeyCode graftKey = KeyCode.G;

    public bool characterFrozen; // prevent movement controls
    public bool freezeCharacterActions; // prevent use of action controls
    public bool hidePlayerHUD; // prevent normal player HUD display

    public bool hidePlayerNameTag; // prevent player name HUD display
    private string playerName;

    private Vector3 characterMove;
    private LooseItemManager activeItem;
    private PlotManager activePlot;
    private PlayerActions characterActions;

    private InventoryData playerInventory; // reference to player data inventory
    private int currentInventorySelection;

    private MultiGamepad padMgr;

    private CameraManager cam;
    private PlayerAnimManager pam;
    private MagicManager mm;
    private ArtLibraryManager alm;
    private TimeManager tim;
    private SaveLoadManager saveMgr;

    const float PROXIMITYRANGE = 0.381f;
    const float ISLANDTETHERSTRENGTH = 1f;
    const bool ALLOWPLAYERDATALOAD = true; // set false for testing only


    // REFACTOR: the entire validation and intialization happens when game manager called SetPlayerData()
    void Start()
    {
        // validate
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        // TODO: change this to error and abort if no gamepad manager found (allow no pad for testing)
        // (then clean up below checks for padMgr existing)
        if (padMgr == null )
            Debug.LogWarning("--- PlayerControlManager [Start] : " + gameObject.name + " no pad manager. will ignore.");
        cam = GameObject.FindFirstObjectByType<CameraManager>();
        if ( cam == null )
        {
            Debug.LogError("--- PlayerControlManager [Start] : " + gameObject.name + " no camera manager found in scene. aborting.");
            enabled = false;
        }
        pam = gameObject.transform.GetComponentInChildren<PlayerAnimManager>();
        if ( pam == null )
        {
            Debug.LogError("--- PlayerControlManager [Start] : "+gameObject.name+" no player anim manager found in children. aborting.");
            enabled = false;
        }
        mm = gameObject.GetComponent<MagicManager>();
        if ( mm == null )
        {
            Debug.LogError("--- PlayerControlManager [Start] : " + gameObject.name + " no magic manager found on player object. aborting.");
            enabled = false;
        }
        alm = GameObject.FindFirstObjectByType<ArtLibraryManager>();
        if (alm == null)
        {
            Debug.LogError("--- PlayerControlManager [Start] : "+gameObject.name+" no art library manager found in scene. aborting.");
            enabled = false;
        }
        tim = GameObject.FindAnyObjectByType<TimeManager>();
        if (tim == null)
        {
            Debug.LogError("--- PlayerControlManager [Start] : " + gameObject.name + " no time manager found in scene. aborting.");
            enabled = false;
        }
        saveMgr = GameObject.FindAnyObjectByType<SaveLoadManager>();
        if ( saveMgr == null )
        {
            Debug.LogWarning("--- PlayerControlManager [Start] : " + gameObject.name + " no save load manager found in scene. aborting.");
            //enabled = false; // temp - keep enabled for prototype testing
        }
        // initialize
        if (enabled)
        {
            // TODO: fix in prep for multiplayer
            cam.SetPlayer(this);
            GameObject.FindFirstObjectByType<InGameControls>().SetPlayerControlManager(this);
            GameObject.FindFirstObjectByType<InGameAlmanac>().SetPlayerControlManager(this);

            if (saveMgr == null || !ALLOWPLAYERDATALOAD)
            {
                // temp - fill player inventory for testing
                playerInventory = InventorySystem.InitializeInventory(5);
                currentInventorySelection = 2;

                playerInventory.items = new ItemData[3];
                playerInventory.items[0] = InventorySystem.InitializeItem(ItemType.Fertilizer);
                playerInventory.items[1] = InventorySystem.InitializeItem(ItemType.Seed);
                playerInventory.items[1].name += " (Carrot)";
                playerInventory.items[1].plant = PlantType.Carrot;
                playerInventory.items[2] = InventorySystem.InitializeItem(ItemType.Seed);
                playerInventory.items[2].name += " (Tomato)";
                playerInventory.items[2].plant = PlantType.Tomato;

                // temp - player data (mainly for gold)
                ProfileData tempProfile = ProfileSystem.InitializeProfile("user", "pass");
                playerData = PlayerSystem.InitializePlayer("Player", tempProfile.profileID);
                playerData.inventory = playerInventory;

                // temp - player magic
                playerData.magic = MagicSystem.IntializeMagic();
                playerData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.FastGrowI, playerData.magic.library);
                playerData.magic.library = MagicSystem.AddSpellToGrimoire(SpellType.SummonWaterI, playerData.magic.library);
                playerData.magic.library.grimiore[0].name = "Fast Grow I";
                playerData.magic.library.grimiore[0].type = SpellType.FastGrowI; // REVIEW: why this not already in?
                playerData.magic.library.grimiore[0].description = "Plants grow faster for one day. (5%)";
                playerData.magic.library.grimiore[0].ingredients = new ItemType[2];
                playerData.magic.library.grimiore[0].ingredients[0] = ItemType.Fertilizer;
                playerData.magic.library.grimiore[0].ingredients[1] = ItemType.Stalk;
                playerData.magic.library.grimiore[1].name = "Summon Water I";
                playerData.magic.library.grimiore[1].description = "Waters a 2x2 area that stays hydrated for one day.";
                playerData.magic.library.grimiore[1].ingredients = new ItemType[2];
                playerData.magic.library.grimiore[1].ingredients[0] = ItemType.Seed;
                playerData.magic.library.grimiore[1].ingredients[1] = ItemType.Fruit;

                playerName = "Test Player";
            }
        }
    }
    
    void Update()
    {
        if (playerData == null)
            return;

        // PLAYER STATS:
        playerData.stats.totalGameTime += Time.deltaTime;

        if (!freezeCharacterActions)
        {
            // check action input
            ReadActionInput();

            // detect inventory selection input
            DetectInventorySelectionInput();
            // check inventory selection drop
            CheckInventorySelectionDrop();
        }

        if (characterFrozen)
            return;

        // read move input
        ReadMoveInput();
        // move
        DoCharacterMove();

        // clear active loose item if moving
        ClearActiveItem();
        // disallow item pick up if player inventory is full
        if (playerInventory != null && playerInventory.items != null && 
            playerInventory.items.Length < playerInventory.maxSlots)
        {
            // check near loose item
            CheckNearItem();
        }
        // check action input (pickup)
        if ( activeItem != null )
        {
            // uses 'first press' control signal
            if (characterActions.actionADown)
            {
                // validate
                if (activeItem.looseItem.inv.items.Length == 0)
                {
                    Debug.LogWarning("--- PlayerControlManager [Update] : trying to take empty loose item. will ignore.");
                    return;
                }
                // pick up loose item, transfer to inventory
                playerInventory = InventorySystem.TakeItem(activeItem.looseItem, out activeItem.looseItem, playerInventory);
                activeItem = null;
            }
            return;
        }
        // NOTE: if loose item active, skip plot activity altogether

        // clear active plot if moving
        ClearActivePlot();
        // check near plot
        CheckNearPlot();

        // handle player tag display
        hidePlayerNameTag = activePlot != null;

        // temp (a = work land, b = water plot , c = harvest plant, d = uproot plot)
        // temp (hold-type control detection)
        if (activePlot != null)
        {
            if (characterActions.actionA)
                activePlot.WorkLand();
            if (characterActions.actionB)
                activePlot.WaterPlot();
            if (characterActions.actionC)
                activePlot.HarvestPlant();
            if (characterActions.actionD)
                activePlot.UprootPlot();
            if (characterActions.graftPlant)
                activePlot.GraftPlant();
            // if all controls un-pressed, signal plot action clear
            if (!characterActions.actionA && !characterActions.actionB &&
                !characterActions.actionC && !characterActions.actionD &&
                !characterActions.graftPlant)
                activePlot.ActionClear();
        }

        // REVIEW: cast magic is last considered?
        if (characterActions.castMagic)
        {
            if (!mm.EnterSpellCastMode())
                Debug.LogWarning("--- PlayerControlManager [Update] : unable to enter spell cast mode without spell charges. will ignore.");
        }
    }

    public void ConfigureAppearance( PlayerOptions options )
    {
        Renderer r = transform.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            if (options.model == PlayerModelType.Male)
            {
                // line (_LineArt)
                r.material.SetTexture("_LineArt", (Texture2D)Resources.Load("ProtoWizard_LineArt"));
                // skin (_AccentFill,_AccentCol)
                r.material.SetTexture("_AccentFill", (Texture2D)Resources.Load("ProtoWizard_FillSkin"));
                r.material.SetColor("_AccentCol", PlayerSystem.GetPlayerSkinColor(options.skinColor));
                // accent (_AltFill, _AltCol)
                r.material.SetTexture("_AltFill", (Texture2D)Resources.Load("ProtoWizard_FillAccent"));
                r.material.SetColor("_AltCol", PlayerSystem.GetPlayerColor(options.accentColor));
                // fill (_MainTex, _Color)
                r.material.SetTexture("_MainTex", (Texture2D)Resources.Load("ProtoWizard_FillMain"));
                r.material.SetColor("_Color", PlayerSystem.GetPlayerColor(options.mainColor));
            }
            else if (options.model == PlayerModelType.Female)
            {
                // line (_LineArt)
                r.material.SetTexture("_LineArt", (Texture2D)Resources.Load("ProtoWizardF_LineArt"));
                // skin (_AccentFill,_AccentCol)
                r.material.SetTexture("_AccentFill", (Texture2D)Resources.Load("ProtoWizardF_FillSkin"));
                r.material.SetColor("_AccentCol", PlayerSystem.GetPlayerSkinColor(options.skinColor));
                // accent (_AltFill, _AltCol)
                r.material.SetTexture("_AltFill", (Texture2D)Resources.Load("ProtoWizardF_FillAccent"));
                r.material.SetColor("_AltCol", PlayerSystem.GetPlayerColor(options.accentColor));
                // fill (_MainTex, _Color)
                r.material.SetTexture("_MainTex", (Texture2D)Resources.Load("ProtoWizardF_FillMain"));
                r.material.SetColor("_Color", PlayerSystem.GetPlayerColor(options.mainColor));
            }
        }
    }
    
    /// <summary>
    /// Gets the player data and camera data for the 'game-owning player character'
    /// </summary>
    /// <returns>player data</returns>
    public PlayerData GetPlayerData()
    {
        // update player location data
        if (pam.imageFlipped)
            playerData.location.w = -1f;
        else
            playerData.location.w = 1f;
        playerData.location.x = gameObject.transform.position.x;
        playerData.location.y = gameObject.transform.position.y;
        playerData.location.z = gameObject.transform.position.z;
        // handle camera data collection
        playerData.camera.x = cam.transform.position.x;
        playerData.camera.y = cam.transform.position.y;
        playerData.camera.z = cam.transform.position.z;
        playerData.camMode = cam.mode;
        if (cam.mode == CameraManager.CameraMode.PanFollow)
        {
            Vector3 cSaved = cam.GetSavedPosition();
            playerData.camSaved.x = cSaved.x;
            playerData.camSaved.y = cSaved.y;
            playerData.camSaved.z = cSaved.z;
        }
        // farm data collection
        if (playerData.farm != null && playerData.farm.plots != null &&
            playerData.farm.plots.Length > 0)
        {
            for (int i = 0; i < playerData.farm.plots.Length; i++)
            {
                PlotData pData = GetPlotData(playerData.farm.plots[i].location);
                playerData.farm.plots[i] = pData;
            }
            // REVIEW: farm effects on the player data already?
        }
        return playerData;
    }

    PlotData GetPlotData( PositionData pos )
    {
        PlotData retPlot = new PlotData();

        PlotManager[] pms = GameObject.FindObjectsByType<PlotManager>(FindObjectsSortMode.None);
        bool found = false;
        for (int i = 0; i < pms.Length; i++)
        {
            if (pms[i].data.location.x == pos.x && pms[i].data.location.y == pos.y &&
                pms[i].data.location.z == pos.z)
            {
                retPlot = pms[i].data;
                retPlot.plant = pms[i].data.plant;
                found = true;
            }
        }
        if (!found) // REVIEW: still needed?
        {
            Debug.LogWarning("--- PlayerControlManager [GetPlotData] : no plot found at " + pos.x + ", " + pos.y + ", " + pos.z + ". will ignore.");
            return null;
        }

        return retPlot;
    }

    /// <summary>
    /// Sets player data and camera data on this player character
    /// </summary>
    public void SetPlayerData()
    {
        // initialize player character

        // validate and initialize 
        if (saveMgr == null)
            Start(); // REFACTOR: migrate bc this needs to happen every time

        ProfileData profData = saveMgr.GetCurrentProfile();
        if (profData == null)
        {
            Debug.LogError("--- PlayerControlManager [SetPlayerData] : no current profile data. aborting.");
            return;
        }
        GameData gameData = saveMgr.GetCurrentGameData();
        if (gameData == null)
        {
            Debug.LogError("--- PlayerControlManager [SetPlayerData] : no current game data. aborting.");
            return;
        }
        playerData = GameSystem.GetProfilePlayer(gameData, profData);
        if (playerData == null)
        {
            Debug.LogError("--- PlayerControlManager [SetPlayerData] : no profile player data. aborting.");
            return;
        }
        // configure appearance (model and colors)
        ConfigureAppearance(playerData.options);
        // connecting property to data _as a reference_
        playerInventory = playerData.inventory;
        playerName = playerData.playerName;
        // place player character in location
        pam.imageFlipped = playerData.location.w < 0f;
        Vector3 pos = new Vector3(playerData.location.x, playerData.location.y, playerData.location.z);
        gameObject.transform.position = pos;
        // restore cam location and mode
        pos.x = playerData.camera.x;
        pos.y = playerData.camera.y;
        pos.z = playerData.camera.z;
        cam.gameObject.transform.position = pos;
        cam.mode = playerData.camMode;
        pos.x = playerData.camSaved.x;
        pos.y = playerData.camSaved.y;
        pos.z = playerData.camSaved.z;
        if (cam.mode == CameraManager.CameraMode.PanFollow)
            cam.SetCameraPanMode(pos);
        // island data already distributed
        IslandData island = gameData.islands[playerData.playerIsland];
        GameObject islandObj = GameObject.Find("Island " + gameData.islands[playerData.playerIsland].name);
        if (islandObj != null)
        {
            if (island == null)
                Debug.LogWarning("--- PlayerControlManager [SetPlayerData] : no associated island data for this player. will ignore.");
            else if (!ConfigurePlayerFarm(island, islandObj))
                Debug.LogWarning("--- PlayerControlManager [SetPlayerData] : ConfigurePlayerFarm failed. will ignore.");
        }
        else
            Debug.LogWarning("--- PlayerControlManager [SetPlayerData] : unable to find player island object 'Island " + gameData.islands[playerData.playerIsland].name + "'. will ignore.");
    }

    bool ConfigurePlayerFarm( IslandData iData, GameObject islandObj )
    {
        bool retBool = false;

        if (playerData == null || playerData.farm == null ||
            playerData.farm.plots == null)
            return retBool;

        // farm location at island center, establish plot array
        for (int i = 0; i < playerData.farm.plots.Length; i++)
        {
            PlotData pData = playerData.farm.plots[i];
            if (pData == null)
            {
                Debug.LogError("--- PlayerControlManager [ConfigurePlayerFarm] : player farm plot data missing. will ignore.");
                continue;
            }
            else
                pData.location = playerData.farm.plots[i].location;
            GameObject plot = GameObject.Instantiate((GameObject)Resources.Load("Plot"));
            plot.name = "Plot";
            PlotManager pm = plot.GetComponent<PlotManager>();
            if (pm == null)
            {
                Debug.LogError("--- PlayerControlManager [ConfigurePlayerFarm] : plot manager not available on plot prefab. aborting.");
                return false;
            }    
            pm.data = pData;
            Vector3 pos = Vector3.zero;
            pos.x = pData.location.x;
            pos.y = pData.location.y;
            pos.z = pData.location.z;
            plot.transform.position = pos + islandObj.transform.position;
            // ensure plot manager location is set
            pm.data.location = pData.location;
            if (pData.condition > PlotCondition.Wild)
            {
                // remove wild grasses
                GameObject grasses = plot.transform.Find("Plot Wild Grasses").gameObject;
                if (grasses == null)
                    Debug.LogWarning("--- PlayerControlManager [ConfigurePlayerFarm] : no wild grasses on plot to remove. will ignore.");
                else
                    Destroy(grasses);
            }
            // establish plant
            if (pData.plant.type != PlantType.Default)
            {
                if (pData.plant == null)
                {
                    Debug.LogError("--- PlayerControlManager [ConfigurePlayerFarm] : plant data missing for plant type '" +pData.plant.type+"'. aborting.");
                    return false;
                }
                pm.plant = GameObject.Instantiate((GameObject)Resources.Load("Plant"));
                pm.plant.transform.position = plot.transform.position;
                pm.plant.transform.parent = plot.transform;
                pm.data.plant = pData.plant;
                // set plant image now
                pm.plant.GetComponent<PlantManager>().ForceGrowthImage(pData.plant);
            }
            // set ground texture based on condition
            Renderer r = plot.transform.Find("Ground").gameObject.GetComponent<Renderer>();
            if (r == null)
            {
                Debug.LogError("--- PlayerControlManager [ConfigurePlayerFarm] : plot missing 'ground' renderer. aborting.");
                return false;
            }
            switch (pData.condition)
            {
                case PlotCondition.Default:
                    // we should never be here
                    break;
                case PlotCondition.Wild:
                    // default
                    break;
                case PlotCondition.Dirt:
                    r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Dirt");
                    break;
                case PlotCondition.Tilled:
                    r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Tilled");
                    break;
                case PlotCondition.Growing:
                    r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Tilled");
                    break;
                case PlotCondition.Uprooted:
                    r.material.mainTexture = (Texture2D)Resources.Load("ProtoPlot_Uprooted");
                    break;
                default:
                    break;
            }
            plot.transform.parent = islandObj.transform;
            // REVIEW: local position instead?
        }
        retBool = true;

        return retBool;
    }

    /// <summary>
    /// Gets item data for current selection of player inventory
    /// </summary>
    /// <returns>item data or null if inventory selection slot is empty</returns>
    public ItemData GetPlayerCurrentItemSelection()
    {
        if ( currentInventorySelection >= playerInventory.items.Length )
            return null;
        return playerInventory.items[currentInventorySelection];
    }

    /// <summary>
    /// Removes the current item selection from player inventory ( *poof* )
    /// </summary>
    public void DeleteCurrentItemSelection()
    {
        if (currentInventorySelection >= playerInventory.items.Length)
            return;
        playerInventory = InventorySystem.RemoveItemFromInventory(playerInventory, playerInventory.items[currentInventorySelection]);
    }

    /// <summary>
    /// Get current player character actions
    /// </summary>
    /// <returns>player actions struct data for this frame</returns>
    public PlayerActions GetPlayerActions()
    {
        return characterActions;
    }

    void HandleIslandTether()
    {
        if (playerData.freeFly)
            return;

        Vector3 center = Vector3.zero;
        center.x = playerData.island.x;
        center.y = playerData.island.y;
        center.z = playerData.island.z;
        float radius = playerData.island.w;
        float dist = Vector3.Distance(gameObject.transform.position, center);
        if (dist > radius )
        {
            Vector3 pushBack = (center - gameObject.transform.position);
            pushBack *= ISLANDTETHERSTRENGTH * Time.deltaTime;

            Vector3 pos = gameObject.transform.position;
            pos += pushBack;
            gameObject.transform.position = pos;
        }
    }

    void ReadMoveInput()
    {
        // player held (tethered) to center of current island
        HandleIslandTether();

        // reset character move
        characterMove = Vector3.zero;

        // reset gamepad input
        float upPad = 0f;
        float downPad = 0f;
        float leftPad = 0f;
        float rightPad = 0f;

        // check gamepad move input (override if gamepad active)
        if ( padMgr != null && padMgr.gamepads[0].isActive )
        {
            float padX = padMgr.gamepads[0].XaxisL;
            float padY = padMgr.gamepads[0].YaxisL;
            upPad = Mathf.Clamp01(padY);
            downPad = Mathf.Clamp01(-padY);
            leftPad = Mathf.Clamp01(-padX);
            rightPad = Mathf.Clamp01(padX);
        }

        // in each direction, test physics collision first, apply move if clear      
        if (Input.GetKey(upKey) || upPad > 0f)
        {
            Vector3 check = gameObject.transform.position + (Vector3.up * 0.25f);
            check += Vector3.forward * characterSpeed * Time.deltaTime;
            if (!Physics.CheckCapsule(check, check + (Vector3.up * 0.5f), 0.25f))
                characterMove += Vector3.forward * characterSpeed * Time.deltaTime;
        }
        if (Input.GetKey(downKey) || downPad > 0f)
        {
            Vector3 check = gameObject.transform.position + (Vector3.up * 0.25f);
            check += Vector3.back * characterSpeed * Time.deltaTime;
            if (!Physics.CheckCapsule(check, check + (Vector3.up * 0.5f), 0.25f))
                characterMove += Vector3.back * characterSpeed * Time.deltaTime;
        }
        if (Input.GetKey(leftKey) || leftPad > 0f)
        {
            Vector3 check = gameObject.transform.position + (Vector3.up * 0.25f);
            check += Vector3.left * characterSpeed * Time.deltaTime;
            if (!Physics.CheckCapsule(check, check + (Vector3.up * 0.5f), 0.25f))
                characterMove += Vector3.left * characterSpeed * Time.deltaTime;
        }
        if (Input.GetKey(rightKey) || rightPad > 0f)
        {
            Vector3 check = gameObject.transform.position + (Vector3.up * 0.25f);
            check += Vector3.right * characterSpeed * Time.deltaTime;
            if (!Physics.CheckCapsule(check, check + (Vector3.up * 0.5f), 0.25f))
                characterMove += Vector3.right * characterSpeed * Time.deltaTime;
        }
    }

    void DoCharacterMove()
    {
        Vector3 pos = gameObject.transform.position;
        pos += characterMove;
        // handle character art flip
        if (characterMove.x < 0f)
            pam.imageFlipped = true;
        if (characterMove.x > 0f)
            pam.imageFlipped = false;
        gameObject.transform.position = pos;
    }

    void CheckNearItem()
    {
        if (activeItem != null)
            return;

        LooseItemManager[] items = GameObject.FindObjectsByType<LooseItemManager>(FindObjectsSortMode.None);
        for (int i=0; i<items.Length; i++)
        {
            if (Vector3.Distance(items[i].gameObject.transform.position, gameObject.transform.position) < PROXIMITYRANGE)
            {
                activeItem = items[i];
                items[i].SetItemPulse(true);
                break;
            }
        }
    }

    void CheckNearPlot()
    {
        if (activePlot != null)
            return;

        PlotManager[] plots = GameObject.FindObjectsByType<PlotManager>(FindObjectsSortMode.None);
        for (int i=0; i<plots.Length; i++)
        {
            if (Vector3.Distance(plots[i].gameObject.transform.position,gameObject.transform.position) < PROXIMITYRANGE)
            {
                activePlot = plots[i];
                activePlot.SetCurrentPlayer(this);
                plots[i].SetCursorPulse(true);
                break;
            }
        }
    }

    void ReadActionInput()
    {
        characterActions = new PlayerActions();

        characterActions.actionA = Input.GetKey(actionAKey);
        characterActions.actionADown = Input.GetKeyDown(actionAKey);
        characterActions.actionB = Input.GetKey(actionBKey);
        characterActions.actionBDown = Input.GetKeyDown(actionBKey);
        characterActions.actionC = Input.GetKey(actionCKey);
        characterActions.actionCDown = Input.GetKeyDown(actionCKey);
        characterActions.actionD = Input.GetKey(actionDKey);
        characterActions.actionDDown = Input.GetKeyDown(actionDKey);
        characterActions.lBump = Input.GetKey(lBumpKey);
        characterActions.lBumpDown = Input.GetKeyDown(lBumpKey);
        characterActions.rBump = Input.GetKey(rBumpKey);
        characterActions.rBumpDown = Input.GetKeyDown(rBumpKey);
        // REVIEW: no need for hold control of cast magic
        characterActions.castMagic = Input.GetKeyDown(castKey);
        characterActions.graftPlant = Input.GetKey(graftKey);

        if (padMgr != null && padMgr.gamepads[0].isActive)
        {
            // use standard 'hold' signals from gamepad for these buttons
            characterActions.actionA = padMgr.gamepads[0].aButton;
            characterActions.actionADown = padMgr.gPadDown[0].aButton;
            characterActions.actionB = padMgr.gamepads[0].bButton;
            characterActions.actionBDown = padMgr.gPadDown[0].bButton;
            characterActions.actionC = padMgr.gamepads[0].xButton;
            characterActions.actionCDown = padMgr.gPadDown[0].xButton;
            characterActions.actionD = padMgr.gamepads[0].yButton;
            characterActions.actionDDown = padMgr.gPadDown[0].yButton;
            characterActions.lBump = padMgr.gamepads[0].LBump;
            characterActions.lBumpDown = padMgr.gPadDown[0].LBump;
            characterActions.rBump = padMgr.gamepads[0].RBump;
            characterActions.rBumpDown = padMgr.gPadDown[0].RBump;
            // REVIEW: cast magic control on gamepad is pressing D pad down
            characterActions.castMagic = padMgr.gPadDown[0].DpadPress;
        }
    }

    void DetectInventorySelectionInput()
    {
        // uses 'first press' control signal
        if (characterActions.lBumpDown)
        {
            currentInventorySelection--;
            if (currentInventorySelection < 0)
                currentInventorySelection = playerInventory.maxSlots - 1;
        }
        if (characterActions.rBumpDown)
        {
            currentInventorySelection++;
            if (currentInventorySelection > playerInventory.maxSlots - 1)
                currentInventorySelection = 0;
        }
    }

    void CheckInventorySelectionDrop()
    {
        // handle drop item selected, uses 'first press' control signal
        if (characterActions.actionCDown && currentInventorySelection < playerInventory.items.Length)
        {
            // spawn loose item dropped from inventory
            ItemSpawnManager ism = GameObject.FindAnyObjectByType<ItemSpawnManager>();
            if (ism == null)
                Debug.LogWarning("--- PlayerControlManager [Update] : no item spawn manager found in scene. will ignore.");
            else
            {
                LooseItemData lid = InventorySystem.DropItem(playerInventory.items[currentInventorySelection], playerInventory, out playerInventory);
                Vector3 pos = gameObject.transform.position;
                if (pam.imageFlipped)
                    pos += Vector3.left * PROXIMITYRANGE;
                else
                    pos += Vector3.right * PROXIMITYRANGE;
                pos.x += (RandomSystem.GaussianRandom01() * PROXIMITYRANGE) - (PROXIMITYRANGE / 2f);
                ism.SpawnItem(lid, gameObject.transform.position, pos);
            }
        }
    }

    void ClearActiveItem()
    {
        if (characterMove != Vector3.zero && activeItem != null)
        {
            activeItem.SetItemPulse(false);
            activeItem = null;
        }
    }

    void ClearActivePlot()
    {
        if (characterMove != Vector3.zero && activePlot != null)
        {
            activePlot.SetCurrentPlayer(null);
            activePlot.SetCursorPulse(false);
            activePlot = null;
        }
    }

    string FormatTimeOfDay( float currentTime )
    {
        string retString = "";

        float minutes = (currentTime * 24f * 60f);
        int hour = Mathf.RoundToInt((minutes / 60f)-0.5f);
        minutes -= (hour * 60f);
        if (hour > 12)
            hour -= 12;
        if (hour == 0)
            hour = 12;
        int min = Mathf.RoundToInt(minutes-0.5f);

        retString = hour.ToString() + ":" + min.ToString("00");

        if (currentTime > 0.5f)
            retString += " PM";
        else
            retString += " AM";

        return retString;
    }

    void OnGUI()
    {
        if (hidePlayerHUD || playerInventory == null)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.white;

        r.x = 0.475f * w;
        r.y = 0.01f * h;
        r.width = 0.05f * w;
        r.height = r.width;

        r.x -= (0.05f * w) * ((playerInventory.maxSlots/2f) + 0.5f);
        for (int i=0; i<5; i++)
        {
            r.x += 0.05f * w;
            if (playerInventory.items != null && playerInventory.items.Length > i)
            {
                if (playerInventory.items[i].type != ItemType.Default)
                {
                    // adjust smaller
                    r.x += 0.005f * w;
                    r.y += (0.005f * w);
                    r.width -= (0.01f * w);
                    r.height -= (0.01f * w);
                    // draw inventory item
                    t = alm.itemImages[alm.GetArtData(playerInventory.items[i].type).artIndexBase];
                    GUI.DrawTexture(r, t);
                    // re-adjust larger again
                    r.x -= 0.005f * w;
                    r.y -= (0.005f * w);
                    r.width += (0.01f * w);
                    r.height += (0.01f * w);
                }
            }
            // draw inventory slot frame
            t = (Texture2D)Resources.Load("Plot_Cursor");
            c = Color.white;
            if (i == currentInventorySelection)
                c = Color.yellow;
            GUI.color = c;
            GUI.DrawTexture(r, t);
            GUI.color = Color.white;
        }

        // player stats display
        // arcana, gold, level, xp
        r.x = 0.6375f * w;
        r.y = 0.015f * h;
        r.width = 0.125f * w;
        r.height = 0.05f * h;
        GUIStyle g = new GUIStyle(GUI.skin.label);
        g.alignment = TextAnchor.MiddleLeft;
        g.fontSize = Mathf.RoundToInt(20f * (w / 1204f));
        g.fontStyle = FontStyle.Bold;
        string s = "ARCANA: ";
        s += playerData.arcana.ToString();

        r.x += 0.0006f * w;
        r.y += 0.001f * h;
        GUI.color = Color.black;
        GUI.Label(r, s, g);
        r.x -= 0.0012f * w;
        r.y -= 0.002f * h;
        GUI.color = Color.yellow;
        GUI.Label(r, s, g);
        GUI.color = Color.white;

        r.y += 0.04f * h;
        s = "GOLD: ";
        s += playerData.gold.ToString();

        r.x += 0.0006f * w;
        r.y += 0.001f * h;
        GUI.color = Color.black;
        GUI.Label(r, s, g);
        r.x -= 0.0012f * w;
        r.y -= 0.002f * h;
        GUI.color = Color.yellow;
        GUI.Label(r, s, g);
        GUI.color = Color.white;

        r.x += 0.125f * w;
        r.y = 0.015f * h;

        s = "LEVEL: ";
        s += playerData.level.ToString();

        r.x += 0.0006f * w;
        r.y += 0.001f * h;
        GUI.color = Color.black;
        GUI.Label(r, s, g);
        r.x -= 0.0012f * w;
        r.y -= 0.002f * h;
        GUI.color = Color.yellow;
        GUI.Label(r, s, g);
        GUI.color = Color.white;

        r.y += 0.04f * h;
        s = "XP: ";
        s += playerData.xp.ToString();

        r.x += 0.0006f * w;
        r.y += 0.001f * h;
        GUI.color = Color.black;
        GUI.Label(r, s, g);
        r.x -= 0.0012f * w;
        r.y -= 0.002f * h;
        GUI.color = Color.yellow;
        GUI.Label(r, s, g);
        GUI.color = Color.white;

        // world stats display
        // time, day, month, season, temperature
        r.x = 0.175f * w;
        r.y = 0.015f * h;
        r.width = 0.2f * w;
        r.height = 0.05f * h;

        s = "DAY: ";
        s += tim.monthOfYear.ToString() + " " + tim.dayOfMonth.ToString() + " " + tim.season.ToString();

        r.x += 0.0006f * w;
        r.y += 0.001f * h;
        GUI.color = Color.black;
        GUI.Label(r, s, g);
        r.x -= 0.0012f * w;
        r.y -= 0.002f * h;
        GUI.color = Color.yellow;
        GUI.Label(r, s, g);
        GUI.color = Color.white;

        r.y += 0.04f * h;
        s = "TIME: ";
        s += FormatTimeOfDay(tim.dayProgress);

        r.x += 0.0006f * w;
        r.y += 0.001f * h;
        GUI.color = Color.black;
        GUI.Label(r, s, g);
        r.x -= 0.0012f * w;
        r.y -= 0.002f * h;
        GUI.color = Color.yellow;
        GUI.Label(r, s, g);
        GUI.color = Color.white;

        r.y += 0.04f * h;
        s = "TEMP: ";
        s += tim.currentTempC.ToString("00.0")+ " C ("+ tim.currentTempF.ToString("00.0") +" F)";

        r.x += 0.0006f * w;
        r.y += 0.001f * h;
        GUI.color = Color.black;
        GUI.Label(r, s, g);
        r.x -= 0.0012f * w;
        r.y -= 0.002f * h;
        GUI.color = Color.yellow;
        GUI.Label(r, s, g);
        GUI.color = Color.white;

        // player name tag
        if (!hidePlayerNameTag && playerName != "")
        {
            float distToCam = Vector3.Distance(Camera.main.transform.position, gameObject.transform.position);
            float fadeName = Mathf.Clamp01( (distToCam - 2f) );

            Vector3 tagPos = Camera.main.WorldToViewportPoint(gameObject.transform.position + Vector3.up + (Vector3.up * 0.381f * fadeName));
            r.x = tagPos.x;
            r.y = 1f - tagPos.y;

            r.x -= 0.05f;

            r.x *= w;
            r.y *= h;
            r.width = 0.1f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.label);
            g.alignment = TextAnchor.MiddleCenter;
            g.fontSize = Mathf.RoundToInt(14f * (w / 1204f));
            g.fontStyle = FontStyle.Bold;
            s = playerName;
            
            r.x += 0.0006f * w;
            r.y += 0.001f * h;
            c = Color.black;
            if (fadeName < 1f)
                c.a = fadeName * 0.381f;
            GUI.color = c; 
            GUI.Label(r, s, g);
            r.x -= 0.0012f * w;
            r.y -= 0.002f * h;
            c = Color.white;
            if (fadeName < 1f)
                c.a = fadeName;
            GUI.color = c;
            GUI.Label(r, s, g);
        }

        if (currentInventorySelection >= playerInventory.items.Length)
            return;

        // bg and label for current item selected
        r.x = 0.375f * w;
        r.y = 0.1f * h;
        r.width = 0.25f * w;
        r.height = 0.05f * h;

        t = Texture2D.whiteTexture;
        c = Color.white;
        c.r = .1f;
        c.g = .1f;
        c.b = .1f;
        c.a = 0.25f;
        GUI.color = c;
        GUI.DrawTexture(r, t);
        GUI.color = Color.white;

        g.alignment = TextAnchor.MiddleCenter;
        g.fontSize = Mathf.RoundToInt( 22f * (w/1204f));
        g.fontStyle = FontStyle.Bold;
        s = playerInventory.items[currentInventorySelection].name;

        r.x += 0.0005f * w;
        r.y += 0.0008f * w;
        GUI.color = Color.black;
        GUI.Label(r, s, g);

        r.x -= 0.001f * w;
        r.y -= 0.0016f * w;
        GUI.color = Color.white;
        GUI.Label(r, s, g);
    }
}
