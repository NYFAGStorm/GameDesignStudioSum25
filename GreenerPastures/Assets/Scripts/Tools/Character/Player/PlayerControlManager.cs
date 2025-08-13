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
        public bool graftPlantDown;
    }
    // NOTE: not included are the number keys used to quick-select inventory slot

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

    private float XPDisplayTimer;
    private float levelUpDisplayTimer;

    private bool letterPopup;
    private bool letterPopsDown;
    private string letterMessage;
    private float letterPopupTimer;
    private AnimationCurve letterPopupCurve;
    private bool letterPopOKSelected;

    private MultiGamepad padMgr;

    private GreenerGameManager ggm;
    private CameraManager cam;
    private CharacterAnimManager pam;
    private MagicManager mm;
    private ArtLibraryManager alm;
    private TimeManager tim;
    private PostOfficeManager pom;
    private SaveLoadManager saveMgr;
    private AudioManager sfxAudio;

    private bool inARabbitHole;

    private float focusFlowTimer;
    private PlayerActions flowAction; // most recent action performed
    private string groceryList;
    private float groceryListTimer;
    

    const float PROXIMITYRANGE = 0.381f;
    const float ISLANDTETHERSTRENGTH = 1f;
    const bool ALLOWPLAYERDATALOAD = true; // set false for testing only
    const float XPDISPLAYTIME = 1f;
    const float LEVELUPDISPLAYTIME = 4f;
    const float LETTERPOPUPTIME = 0.618f;
    const float FOCUSFLOWTIME = 2f;
    const float GROCERYLISTTIME = 60f;


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
        pam = gameObject.transform.GetComponentInChildren<CharacterAnimManager>();
        if ( pam == null )
        {
            Debug.LogError("--- PlayerControlManager [Start] : "+gameObject.name+" no character anim manager found in children. aborting.");
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
        pom = GameObject.FindFirstObjectByType<PostOfficeManager>();
        if ( pom == null )
        {
            Debug.LogError("--- PlayerControlManager [Start] : " + gameObject.name + " no post office manager found in scene. aborting.");
            enabled = false;
        }
        saveMgr = GameObject.FindAnyObjectByType<SaveLoadManager>();
        if ( saveMgr == null )
        {
            Debug.LogWarning("--- PlayerControlManager [Start] : " + gameObject.name + " no save load manager found in scene. aborting.");
            //enabled = false; // temp - keep enabled for prototype testing
        }
        GameObject sfxObj = GameObject.Find("AudioMgr SFX");
        if (sfxObj != null)
            sfxAudio = sfxObj.GetComponent<AudioManager>();
        // initialize
        if (enabled)
        {
            // TODO: fix in prep for multiplayer
            cam.SetPlayer(this);
            GameObject.FindFirstObjectByType<InGameControls>().SetPlayerControlManager(this);
            GameObject.FindFirstObjectByType<InGameAlmanac>().SetPlayerControlManager(this);

            letterPopupCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

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
                playerData.magic.library.grimoire[0].name = "Fast Grow I";
                playerData.magic.library.grimoire[0].type = SpellType.FastGrowI; // REVIEW: why this not already in?
                playerData.magic.library.grimoire[0].description = "Plants grow faster for one day. (5%)";
                playerData.magic.library.grimoire[0].ingredients = new IngredientData[2];
                playerData.magic.library.grimoire[0].ingredients[0].item = ItemType.Fertilizer;
                playerData.magic.library.grimoire[0].ingredients[1].item = ItemType.Stalk;
                playerData.magic.library.grimoire[1].name = "Summon Water I";
                playerData.magic.library.grimoire[1].description = "Waters a 2x2 area that stays hydrated for one day.";
                playerData.magic.library.grimoire[1].ingredients = new IngredientData[2];
                playerData.magic.library.grimoire[1].ingredients[0].item = ItemType.Seed;
                playerData.magic.library.grimoire[1].ingredients[1].item = ItemType.Fruit;

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

        // run xp and levelup timers
        if (XPDisplayTimer > 0f)
        {
            XPDisplayTimer -= Time.deltaTime;
            if (XPDisplayTimer < 0f)
                XPDisplayTimer = 0f;
        }
        if (levelUpDisplayTimer > 0f)
        {
            levelUpDisplayTimer -= Time.deltaTime;
            if (levelUpDisplayTimer < 0f)
                levelUpDisplayTimer = 0f;
        }
        // run letter popup timer
        if (letterPopupTimer > 0f)
        {
            letterPopupTimer -= Time.deltaTime;
            if (letterPopupTimer < 0f)
            {
                letterPopupTimer = 0f;
                if (letterPopsDown)
                {
                    characterFrozen = false;
                    freezeCharacterActions = false;
                    letterPopsDown = false;
                    letterMessage = "";
                    letterPopup = false;
                    letterPopOKSelected = false;
                }
            }
        }
        // letter pop gamepad support
        if (letterPopup && !letterPopOKSelected &&
            padMgr != null && padMgr.gamepads[0].isActive &&
            padMgr.gPadDown[0].YaxisL != 0f)
            letterPopOKSelected = true;

        // run grocery list timer
        if (groceryListTimer > 0f)
        {
            groceryListTimer -= Time.deltaTime;
            if (groceryListTimer < 0f)
            {
                groceryListTimer = 0f;
                groceryList = "";
            }
        }
        // run focus flow timer
        if (focusFlowTimer > 0f)
        {
            focusFlowTimer -= Time.deltaTime;
            if (focusFlowTimer < 0f)
                focusFlowTimer = 0f;
        }

        // SPELL RABBIT HOLE
        if (inARabbitHole && !PlayerSystem.PlayerHasEffect(playerData, PlayerEffect.SpellRabbitHole))
            FallInRabbitHole(false);
        if (!inARabbitHole && PlayerSystem.PlayerHasEffect(playerData, PlayerEffect.SpellRabbitHole))
            FallInRabbitHole(true);

        if (!freezeCharacterActions)
        {
            // check action input
            ReadActionInput();

            // detect inventory selection input
            DetectInventorySelectionInput();
            // detect player is harveting plant and prioritize over item drop
            if (activePlot == null || (activePlot != null && (activePlot.plant == null ||
                 (activePlot.data.plant.isHarvested && !activePlot.data.plant.canReFruit))))
            {
                // check inventory selection drop
                CheckInventorySelectionDrop();
            }
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
                if (HandlePlayerTakeItem())
                    AwardXP(PlayerData.XP_PICKUPITEM);
                activeItem = null;
            }
            return;
        }
        // NOTE: if loose item active, skip plot activity altogether

        bool hadActivePlot = (activePlot != null);
        
        // clear active plot if moving
        ClearActivePlot();
        // check near plot
        CheckNearPlot();

        // handle player tag display
        if (!hidePlayerNameTag && activePlot != null)
            hidePlayerNameTag = true;
        else if (hadActivePlot && activePlot == null)
            hidePlayerNameTag = false;

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
            else
                flowAction = characterActions; // store latest action
        }

        // cast magic is last considered
        if (characterActions.castMagic)
        {
            if (!mm.EnterSpellCastMode())
                ggm.AddNotification("You have no spell charges in your spell book. Craft more charges to cast.");
        }
    }

    /// <summary>
    /// Returns true if player is within focus flow time window and performing same action
    /// </summary>
    /// <param name="action">plot action</param>
    /// <returns>true if within window and performing same farm work action</returns>
    public bool IsPlayerFocusFlowing( PlotManager.CurrentAction action )
    {
        // ARCANA SKILL : Focus Flow
        bool retBool = false;

        if (focusFlowTimer > 0f)
        {

            if ((flowAction.actionA || flowAction.actionADown) &&
                action == PlotManager.CurrentAction.Working ||
                action == PlotManager.CurrentAction.Planting)
                retBool = true;
            if ((flowAction.actionB || flowAction.actionBDown) &&
                action == PlotManager.CurrentAction.Watering)
                retBool = true;
            if ((flowAction.actionC || flowAction.actionCDown) &&
                action == PlotManager.CurrentAction.Harvesting)
                retBool = true;
            if ((flowAction.actionD || flowAction.actionDDown) &&
                action == PlotManager.CurrentAction.Uprooting)
                retBool = true;
            if ((flowAction.graftPlant) && action == PlotManager.CurrentAction.Grafting)
                retBool = true;
        }

        if (retBool)
            focusFlowTimer = FOCUSFLOWTIME; // reset flow timer

        return retBool;
    }

    public void FocusFlowActionComplete( PlotManager.CurrentAction action )
    {
        //flowPlotAction = action;
        focusFlowTimer = FOCUSFLOWTIME;
    }
    
    /// <summary>
    /// Fills HUD display of ingredient list for most recently viewed grimoire recipe
    /// </summary>
    /// <param name="recipe">grimoire data</param>
    public void MakeGroceryList( GrimoireData recipe )
    {
        // ARCANA SKILL : Grocery List
        // Spell Name : Ingredient, Ingredient, Ingredient, Ingredient, Ingredient
        groceryList = recipe.name + " : ";
        for (int i = 0; i < recipe.ingredients.Length; i++)
        {
            groceryList += recipe.ingredients[i].name;
            if (i < recipe.ingredients.Length - 1)
                groceryList += ", ";
        }
        groceryListTimer = GROCERYLISTTIME;
    }

    void FallInRabbitHole( bool inHole )
    {
        inARabbitHole = inHole;
        characterFrozen = inHole;
        freezeCharacterActions = inHole;
        hidePlayerHUD = inHole;
        hidePlayerNameTag = inHole;
        pam.GetComponent<Renderer>().enabled = !inHole;
    }

    public void ConfigureAppearance( PlayerOptions options )
    {
        if (pam != null)
            pam.ConfigureAppearance(options);
        else
            Debug.LogWarning("--- PlayerControlManager [ConfigureAppearance] : no character animation manager referenced. will ignore.");
    }
    
    /// <summary>
    /// Gets the player data and camera data for the 'game-owning player character'
    /// </summary>
    /// <returns>player data</returns>
    public PlayerData GetPlayerData()
    {
        // update player location data
        playerData.location.w = pam.characterMoveVector.x;
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

        // TODO: return confirm bool, this is where crashes occur in startup

        // validate and initialize
        ggm = GameObject.FindFirstObjectByType<GreenerGameManager>();
        if (ggm == null)
        {
            Debug.LogError("--- PlayerControlManager [SetPlayerData] : no greener game manager found in scene. aborting.");
            return;
        }
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
        pam = gameObject.transform.GetComponentInChildren<CharacterAnimManager>();
        if (pam == null)
        {
            Debug.LogError("--- PlayerControlManager [SetPlayerData] : no character anim manager found in children. aborting.");
            enabled = false;
        }
        // configure appearance (model and colors)
        pam.ConfigureAppearance(playerData.options);
        // connecting property to data _as a reference_
        playerInventory = playerData.inventory;
        playerName = playerData.playerName;
        // place player character in location
        Vector3 moveVec = Vector3.zero;
        moveVec.x = playerData.location.w;
        pam.characterMoveVector = moveVec;
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
        if (cam != null)
        {
            if (cam.mode == CameraManager.CameraMode.PanFollow)
                cam.SetCameraPanMode(pos);
        }
        else
        {
            Debug.LogError("--- PlayerControlManger [SetPlayerData] : cam reference lost during setup. aborting.");
            enabled = false;
        }
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
                    r.material.mainTexture = (Texture2D)Resources.Load("Plot_Dirt");
                    break;
                case PlotCondition.Tilled:
                    r.material.mainTexture = (Texture2D)Resources.Load("Plot_Tilled");
                    break;
                case PlotCondition.Growing:
                    r.material.mainTexture = (Texture2D)Resources.Load("Plot_Tilled");
                    break;
                case PlotCondition.Uprooted:
                    r.material.mainTexture = (Texture2D)Resources.Load("Plot_Uprooted");
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
    /// Awards this player a given amount of xp
    /// </summary>
    /// <param name="xpAmount">xp amount</param>
    /// <returns>true if result is player level up, false if not</returns>
    public bool AwardXP( int xpAmount )
    {
        bool retBool = false;

        // validate
        if (xpAmount <= 0)
            return retBool;

        retBool = PlayerSystem.WillPlayerLevelUp(playerData, xpAmount);
        // trigger eden arcana visit if player has not yet been visited
        if (retBool && PlayerSystem.PlayerHasEffect(playerData,PlayerEffect.EdenArcanaVisit))
        {
            // eden visit will clear this effect once visit is complete
            GameObject edenVisitMgr = GameObject.Instantiate((GameObject)Resources.Load("Eden Arcana Visit"));
            edenVisitMgr.GetComponent<EdenVisitManager>().LaunchVisit();
        }
        playerData = PlayerSystem.AwardPlayerXP(playerData, xpAmount);

        if (retBool)
        {
            if (ggm != null)
                ggm.StackNotifications(PlayerSystem.GetLevelUpNotifications(playerData.level));
            else
                Debug.LogWarning("--- PlayerControlManager [AwardXP] : no reference to game manager for notifications. will ignore.");
        }

        // PLAYER STATS:
        playerData.stats.totalXPEarned += xpAmount;
        if (retBool)
            playerData.stats.totalLevelsEarned++;

        XPDisplayTimer = XPDISPLAYTIME;
        if (retBool)
        {
            if (sfxAudio != null)
                sfxAudio.StartSound("Player Level Up");
            levelUpDisplayTimer = LEVELUPDISPLAYTIME;
        }

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

    public int GetPlayerCurrentItemSelectionIndex()
    {
        return currentInventorySelection;
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

    bool HandlePlayerTakeItem()
    {
        bool retBool = false;

        // check in with almanac to see if entry can be unlocked
        InGameAlmanac iga = GameObject.FindFirstObjectByType<InGameAlmanac>();
        if (iga == null)
            Debug.LogWarning("--- PlayerControlManager [HandlePlayerTakeItem] : no in game almanac found in scene. will ignore.");

        // check almanac if plant entry revealed
        if (activeItem.looseItem.inv.items[0].type == ItemType.Seed ||
            activeItem.looseItem.inv.items[0].type == ItemType.Plant ||
            activeItem.looseItem.inv.items[0].type == ItemType.Stalk ||
            activeItem.looseItem.inv.items[0].type == ItemType.Fruit)
        {
            if (iga != null && iga.IsEntryHidden(PlantSystem.InitializePlant(activeItem.looseItem.inv.items[0].plant).plantName))
                iga.AlmanacReveal(PlantSystem.InitializePlant(activeItem.looseItem.inv.items[0].plant).plantName);
        }

        // before normal loose item pickup, check if active item type is special ...
        // ... and meant to perform an action upon 'take', detect andd handle by type
        // (gold coin, gold sack, package, letter, coupon), destroyed upon handling
        bool skipPickup = false;
        bool unpackLooseItem = false;
        switch (activeItem.looseItem.inv.items[0].type)
        {
            case ItemType.GoldCoin:
                playerData.gold++;
                playerData.stats.totalGoldEarned++;
                skipPickup = true;
                break;
            case ItemType.GoldSack:
                if (activeItem.looseItem.inv.items.Length > 25)
                {
                    // with so many gold coins, just collect gold directly
                    playerData.gold += activeItem.looseItem.inv.items.Length - 1;
                    playerData.stats.totalGoldEarned += activeItem.looseItem.inv.items.Length - 1;
                    // sfx collect gold
                    if (sfxAudio != null)
                        sfxAudio.StartSound("Player Pickup GoldSack");
                }
                else
                    unpackLooseItem = true;
                skipPickup = true;
                break;
            case ItemType.Rock:
                if (iga != null)
                {
                    if (iga.IsEntryHidden("Rock"))
                        iga.AlmanacReveal("Rock");
                }
                break;
            case ItemType.Package:
                unpackLooseItem = true;
                skipPickup = true;
                if (iga != null)
                {
                    if (iga.IsEntryHidden("Package"))
                        iga.AlmanacReveal("Package");
                }
                break;
            case ItemType.Letter:
                characterFrozen = true;
                freezeCharacterActions = true;
                letterPopup = true;
                // use quality property as message id for post office
                letterMessage = pom.GetLetterMessage(activeItem.looseItem.inv.items[0].quality);
                letterPopupTimer = LETTERPOPUPTIME;
                skipPickup = true;
                if (iga != null)
                {
                    if (iga.IsEntryHidden("Letter"))
                        iga.AlmanacReveal("Letter");
                }
                break;
            case ItemType.Coupon:
                if (iga != null)
                {
                    if (iga.IsEntryHidden("Coupon"))
                        iga.AlmanacReveal("Coupon");
                }
                break;
            case ItemType.Scroll:
                // detect scroll effect
                ItemEffect scrollEffect = activeItem.looseItem.inv.items[0].effects[0];
                SpellType scrollAddsCharge = SpellType.Default;
                int scrollCharge = 0;
                switch (scrollEffect)
                {
                    case ItemEffect.Default:
                        // we should never be here
                        break;
                    case ItemEffect.ScrollRandomSpellCharge:
                        // weighted random spell type
                        scrollCharge = GameSystem.RoundedResult(RandomSystem.WeightedRandom01(), 34);
                        scrollAddsCharge = (SpellType)scrollCharge;
                        break;
                    case ItemEffect.ScrollLevelOneSpellCharge:
                        scrollCharge = GameSystem.RoundedResult(RandomSystem.FlatRandom01(), 4) - 1;
                        scrollAddsCharge = (SpellType)(scrollCharge + 1); // level one spells start at 1
                        break;
                    case ItemEffect.ScrollLevelTwoSpellCharge:
                        scrollCharge = GameSystem.RoundedResult(RandomSystem.FlatRandom01(), 7) - 1;
                        scrollAddsCharge = (SpellType)(scrollCharge + 5); // level two spells start at 5
                        break;
                    case ItemEffect.ScrollLevelThreeSpellCharge:
                        scrollCharge = GameSystem.RoundedResult(RandomSystem.FlatRandom01(), 5) - 1;
                        scrollAddsCharge = (SpellType)(scrollCharge + 12); // level three spells start at 12
                        break;
                    case ItemEffect.ScrollLevelFourSpellCharge:
                        scrollCharge = GameSystem.RoundedResult(RandomSystem.FlatRandom01(), 6) - 1;
                        scrollAddsCharge = (SpellType)(scrollCharge + 17); // level four spells start at 17
                        break;
                    case ItemEffect.ScrollLevelFiveSpellCharge:
                        scrollCharge = GameSystem.RoundedResult(RandomSystem.FlatRandom01(), 7) - 1;
                        scrollAddsCharge = (SpellType)(scrollCharge + 23); // level five spells start at 23
                        break;
                    case ItemEffect.ScrollLevelSixSpellCharge:
                        scrollCharge = GameSystem.RoundedResult(RandomSystem.FlatRandom01(), 3) - 1;
                        scrollAddsCharge = (SpellType)(scrollCharge + 30); // level five spells start at 30
                        break;
                    case ItemEffect.ScrollLevelSevenSpellCharge:
                        scrollCharge = GameSystem.RoundedResult(RandomSystem.FlatRandom01(), 2) - 1;
                        scrollAddsCharge = (SpellType)(scrollCharge + 33); // level five spells start at 33
                        break;
                    case ItemEffect.ScrollMirrorMirror:
                        scrollAddsCharge = SpellType.MirrorMirror;
                        break;
                    case ItemEffect.ScrollColorTrailI:
                        scrollAddsCharge = SpellType.ColorTrailI;
                        break;
                    case ItemEffect.ScrollColorTrailII:
                        scrollAddsCharge = SpellType.ColorTrailII;
                        break;
                    case ItemEffect.ScrollColorTrailIII:
                        scrollAddsCharge = SpellType.ColorTrailIII;
                        break;
                    case ItemEffect.ScrollSplaturn:
                        scrollAddsCharge = SpellType.Splaturn;
                        break;
                    case ItemEffect.ScrollStarbloomBurst:
                        scrollAddsCharge = SpellType.StarbloomBurst;
                        break;
                    case ItemEffect.ScrollFogOfWar:
                        scrollAddsCharge = SpellType.FogOfWar;
                        break;
                }
                // apply effect
                playerData.magic.library = MagicSystem.AddChargeToSpellBook(scrollAddsCharge, playerData.magic.library);
                string[] scrollNotify = new string[2];
                scrollNotify[0] = "You have read a magic scroll\nand it gives you a spell!";
                scrollNotify[1] = "Charge added to spell book!\n" + scrollAddsCharge.ToString() + " ADDED!";
                ggm.StackNotifications(scrollNotify);
                skipPickup = true;
                if (iga != null)
                {
                    if (iga.IsEntryHidden("Scroll"))
                        iga.AlmanacReveal("Scroll");
                }
                break;
            case ItemType.Potion:
                // detect potion effect
                if (activeItem.looseItem.inv.items[0].effects != null &&
                    activeItem.looseItem.inv.items[0].effects.Length > 0)
                {
                    ItemEffect potionEffect = activeItem.looseItem.inv.items[0].effects[0];
                    //PlayerEffect potionAddsEffect = PlayerEffect.Default;
                    string[] potionNotify = new string[2];
                    potionNotify[0] = "You consumed a magic potion\nand it has an effect!";
                    switch (potionEffect)
                    {
                        case ItemEffect.Default:
                            // we should never be here
                            break;
                        case ItemEffect.PotionPlayerEffect:
                            /*
                             * disabled atm
                             * TODO: figure out how to time this player effect, like a cast does but not a spell type
                            // player effect detailed in quality of potion item
                            potionAddsEffect = (PlayerEffect)((int)activeItem.looseItem.inv.items[0].quality);
                            // apply effect
                            playerData = PlayerSystem.AddPlayerEffect(playerData, PlayerEffect.Default);
                            string effectDescription = potionAddsEffect.ToString(); // temp
                            potionNotify[1] = "You now feel very much\n" + effectDescription + "!";
                            // TODO: create a special cast to handle the effect wearing off over time
                            CastManager cm = GameObject.FindFirstObjectByType<CastManager>();
                            if (cm != null)
                            {
                                CastData cData = new CastData();
                                cData.type = SpellType.Default;
                                cData.lifetime = 300f; // 5 game-time hours
                                //cData.lifeTimestamp = 
                                cm.AcquireNewCast(cData);
                            }
                            // apply effect
                            */
                            break;
                        case ItemEffect.PotionClearOneCooldown:
                            if (playerData.magic.library.spellBook.Length > 0)
                            {
                                int rndspell = GameSystem.RoundedResult(RandomSystem.FlatRandom01(), 
                                    playerData.magic.library.spellBook.Length) - 1;
                                int safety = 50;
                                // ensure potion clears spell cooldowns not skills
                                while (safety > 0 && (playerData.magic.library.spellBook[rndspell].cooldownDuration <= 0.1f))
                                {
                                    safety--;
                                    rndspell = GameSystem.RoundedResult(RandomSystem.FlatRandom01(),
                                        playerData.magic.library.spellBook.Length) - 1;
                                }
                                playerData.magic.library.spellBook[rndspell].cooldown = 0.1f;
                                potionNotify[1] = playerData.magic.library.spellBook[rndspell].name + "\ncooldown is CLEARED!";
                            }
                            else
                                potionNotify[1] = "You feel warm and happy\nbut no other effects.";
                            break;
                        case ItemEffect.PotionClearAllCooldowns:
                            for (int i = 0; i < playerData.magic.library.spellBook.Length; i++)
                            {
                                playerData.magic.library.spellBook[i].cooldown = 0.1f;
                            }
                            potionNotify[1] = "All spell cooldowns\nare CLEARED!";
                            break;
                    }
                    ggm.StackNotifications(potionNotify);
                    skipPickup = true;
                }
                else
                    Debug.LogWarning("--- PlayerControlManager [HandlePlayerTakeItem] : potion pickup failed due to lack of potion effects. will ignore.");
                if (iga != null)
                {
                    if (iga.IsEntryHidden("Potion"))
                        iga.AlmanacReveal("Potion");
                }
                break;
        }
        if (unpackLooseItem)
        {
            // because all loose items are technically mobile inventory, ...
            // .. just 'unpack' other inventory items and spawn them
            if (activeItem.looseItem.inv.items.Length > 1)
            {
                ItemSpawnManager ism = GameObject.FindFirstObjectByType<ItemSpawnManager>();
                Vector3 pos = activeItem.gameObject.transform.position;
                pos += Vector3.down * 0.25f;
                for (int i = 1; i < activeItem.looseItem.inv.items.Length; i++)
                {
                    Vector3 targ = pos;
                    // spread out target pos
                    targ.x += (1f * (1f - RandomSystem.GaussianRandom01())) - 0.5f; 
                    targ.z += (.5f * (1f - RandomSystem.GaussianRandom01())) - 0.25f;
                    LooseItemData liData = InventorySystem.CreateItem(activeItem.looseItem.inv.items[i].type);
                    liData.inv.items[0] = activeItem.looseItem.inv.items[i];
                    ism.SpawnItem(liData, pos, targ, true);
                }
            }
            else
                Debug.LogWarning("--- PlayerControlManager [HandlePlayerTakeItem] : active item type '" + activeItem.looseItem.inv.items[0].type + "' does not contain items to unpack. will ignore.");
        }
        if (skipPickup)
        {
            Destroy(activeItem.gameObject);
            return retBool;
        }

        // pick up loose item, transfer to inventory
        playerInventory = InventorySystem.TakeItem(activeItem.looseItem, out activeItem.looseItem, playerInventory);
        // auto-select pickup item in inventory
        //currentInventorySelection = playerInventory.items.Length - 1;
        retBool = true;

        return retBool;
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
        if (dist > radius)
        {
            Vector3 pushBack = (center - gameObject.transform.position);
            float extraPush = Mathf.Clamp((7f - pushBack.magnitude), 1f, 7f);
            pushBack *= ISLANDTETHERSTRENGTH * extraPush * Time.deltaTime;
            
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

    void DoColorTrail( Vector3 pos )
    {
        // REVIEW: cleanup
        pos += (Vector3.up * 0.01f);

        // SPELL COLOR TRAIL I, II, II
        if (RandomSystem.FlatRandom01() < 0.1f)
        {
            GameObject lightingFolderObject = GameObject.Find("Lighting");
            if (PlayerSystem.PlayerHasEffect(playerData, PlayerEffect.SpellColorTrailI))
            {
                Color c = PlayerSystem.GetPlayerColor(playerData.options.mainColor);
                GameObject vfx = GameObject.Instantiate((GameObject)Resources.Load("Spells/VFX Spell Color Trail"));
                vfx.transform.position = pos;
                Vector3 offset = Vector3.zero;
                offset.x = RandomSystem.GaussianRandom01() - 0.5f;
                offset.z = RandomSystem.GaussianRandom01() - 0.5f;
                offset *= 0.381f;
                vfx.transform.position += offset;
                Vector3 scl = Vector3.one;
                scl *= RandomSystem.GaussianRandom01();
                vfx.transform.localScale = scl;
                vfx.name = "VFX Color Trail I";
                vfx.transform.parent = lightingFolderObject.transform;
                vfx.GetComponent<Renderer>().material.color = c;
                Destroy(vfx, 3.81f);
            }
            if (PlayerSystem.PlayerHasEffect(playerData, PlayerEffect.SpellColorTrailII))
            {
                Color c = PlayerSystem.GetPlayerColor(playerData.options.secondaryColor);
                GameObject vfx = GameObject.Instantiate((GameObject)Resources.Load("Spells/VFX Spell Color Trail"));
                vfx.transform.position = pos;
                Vector3 offset = Vector3.zero;
                offset.x = RandomSystem.GaussianRandom01() - 0.5f;
                offset.z = RandomSystem.GaussianRandom01() - 0.5f;
                offset *= 0.381f;
                vfx.transform.position += offset;
                Vector3 scl = Vector3.one;
                scl *= RandomSystem.GaussianRandom01();
                vfx.transform.localScale = scl;
                vfx.name = "VFX Color Trail II";
                vfx.transform.parent = lightingFolderObject.transform;
                vfx.GetComponent<Renderer>().material.color = c;
                Destroy(vfx, 3.81f);
            }
            if (PlayerSystem.PlayerHasEffect(playerData, PlayerEffect.SpellColorTrailIII))
            {
                Color c = PlayerSystem.GetPlayerColor(playerData.options.accentColor);
                GameObject vfx = GameObject.Instantiate((GameObject)Resources.Load("Spells/VFX Spell Color Trail"));
                vfx.transform.position = pos;
                Vector3 offset = Vector3.zero;
                offset.x = RandomSystem.GaussianRandom01() - 0.5f;
                offset.z = RandomSystem.GaussianRandom01() - 0.5f;
                offset *= 0.381f;
                vfx.transform.position += offset;
                Vector3 scl = Vector3.one;
                scl *= RandomSystem.GaussianRandom01();
                vfx.transform.localScale = scl;
                vfx.name = "VFX Color Trail III";
                vfx.transform.parent = lightingFolderObject.transform;
                vfx.GetComponent<Renderer>().material.color = c;
                Destroy(vfx, 3.81f);
            }
        }
    }

    void DoCharacterMove()
    {
        Vector3 pos = gameObject.transform.position;
        // SPELL COLOR TRAIL
        DoColorTrail(pos);
        pos += characterMove;
        // SPELL SWIFTNESS
        if (PlayerSystem.PlayerHasEffect(playerData, PlayerEffect.SpellSwiftness))
            pos += (characterMove * 0.5f); // 150% movement rate
        pam.characterMoveVector = characterMove;
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
        characterActions.graftPlantDown = Input.GetKeyDown(graftKey);

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
            // REVIEW: cast magic control on gamepad is pressing D pad left
            characterActions.castMagic = padMgr.gPadDown[0].DpadLeft;
            // REVIEW: grafting control on gamepad is pressing D pad right
            characterActions.graftPlant = padMgr.gamepads[0].DpadRight;
            characterActions.graftPlantDown = padMgr.gPadDown[0].DpadRight;
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
        // override with keyboard number keys (quick select)
        if (!Input.anyKeyDown)
            return;
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                currentInventorySelection = 0;
            if (Input.GetKeyDown(KeyCode.Alpha2))
                currentInventorySelection = 1;
            if (Input.GetKeyDown(KeyCode.Alpha3))
                currentInventorySelection = 2;
            if (Input.GetKeyDown(KeyCode.Alpha4))
                currentInventorySelection = 3;
            if (Input.GetKeyDown(KeyCode.Alpha5))
                currentInventorySelection = 4;
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
                if (pam.GetImageFlipped())
                    pos += Vector3.left * PROXIMITYRANGE;
                else
                    pos += Vector3.right * PROXIMITYRANGE;
                pos.x += (RandomSystem.GaussianRandom01() * PROXIMITYRANGE) - (PROXIMITYRANGE / 2f);
                ism.SpawnItem(lid, gameObject.transform.position, pos, false);
            }
            AwardXP(PlayerData.XP_DROPITEM);
            // auto-select last item in inventory
            //currentInventorySelection = Mathf.Max(playerInventory.items.Length - 1, 0);
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
        // SPELL RABBIT HOLE
        if (inARabbitHole)
        {
            Rect rabbitHole = new Rect();
            rabbitHole.x = 0f;
            rabbitHole.y = 0f;
            rabbitHole.width = 1f;
            rabbitHole.height = 1f;
            GUI.color = Color.black;
            GUI.DrawTexture(rabbitHole, Texture2D.blackTexture);
            return;
        }
        
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
                    if (i != currentInventorySelection)
                    {
                        // adjust smaller
                        r.x += 0.005f * w;
                        r.y += (0.005f * w);
                        r.width -= (0.01f * w);
                        r.height -= (0.01f * w);
                        // draw inventory item
                        t = alm.itemImages[alm.GetArtData(playerInventory.items[i].type, playerInventory.items[i].plant).artIndexBase];
                        GUI.DrawTexture(r, t);
                        // re-adjust larger again
                        r.x -= 0.005f * w;
                        r.y -= (0.005f * w);
                        r.width += (0.01f * w);
                        r.height += (0.01f * w);
                    }
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
            // pulse larger for selected item
            if (i == currentInventorySelection && 
                playerInventory.items != null && playerInventory.items.Length > i &&
                playerInventory.items[i].type != ItemType.Default)
            {
                float pulseSize = (Mathf.Sin(Time.time * 6.18f) * 0.00125f) + 0.00125f;
                pulseSize = Mathf.RoundToInt(pulseSize * w);
                r.x -= pulseSize;
                r.y -= pulseSize;
                r.width += (pulseSize * 2f);
                r.height += (pulseSize * 2f);
                // draw selected inventory larger over slot frame
                t = alm.itemImages[alm.GetArtData(playerInventory.items[i].type, playerInventory.items[i].plant).artIndexBase];
                GUI.DrawTexture(r, t);
                r.x += pulseSize;
                r.y += pulseSize;
                r.width -= (pulseSize * 2f);
                r.height -= (pulseSize * 2f);
            }
        }

        // player stats display
        // arcana, gold, level, xp
        r.x = 0.6375f * w;
        r.y = 0.015f * h;
        r.width = 0.125f * w;
        r.height = 0.05f * h;
        GUIStyle g = new GUIStyle(GUI.skin.label);
        g.alignment = TextAnchor.MiddleLeft;
        g.fontSize = Mathf.RoundToInt(16f * (w / 1024f));
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
        r.y = 0.025f * h;
        r.width = 0.125f * w;
        r.height = 0.03f * h;
        t = Texture2D.whiteTexture;
        c = Color.black;
        c.a = 0.381f;
        GUI.color = c;
        GUI.DrawTexture(r, t);
        float levelInterval = PlayerSystem.GetXPLevelInterval(playerData.level);
        float amtToNext = PlayerSystem.GetXPAmountToNextLevel(playerData.xp, playerData.level);
        float amtToCurrent = PlayerSystem.GetXPAmountToLevel(playerData.level);
        r.width = (0.125f * w) * ( (playerData.xp - amtToCurrent) / levelInterval);
        c = Color.yellow;
        c.a = 0.381f;
        GUI.color = c;
        GUI.DrawTexture(r, t);

        r.x += 0.005f * w;
        r.y = 0.015f * h;
        r.width = 0.125f * w;
        r.height = 0.05f * h;
        s = "XP: ";
        s += playerData.xp.ToString();

        r.x += 0.0006f * w;
        r.y += 0.001f * h;
        g.fontSize = Mathf.RoundToInt(14f * (w / 1024f));
        c = Color.black;
        if (XPDisplayTimer > 0f)
            c.a = Mathf.Clamp01(.381f + (1f - (XPDisplayTimer * 2f) % 1f));
        GUI.color = c;
        GUI.Label(r, s, g);
        r.x -= 0.0012f * w;
        r.y -= 0.002f * h;
        c = Color.yellow;
        if (XPDisplayTimer > 0f)
            c.a = Mathf.Clamp01(.381f + (1f - (XPDisplayTimer * 2f) % 1f));
        GUI.color = c;
        GUI.Label(r, s, g);
        GUI.color = Color.white;

        r.x -= 0.0025f * w;
        r.y += 0.04f * h;
        g.fontSize = Mathf.RoundToInt(16f * (w / 1024f));
        g.padding = new RectOffset(0, 0, 0, 0);
        s = "LEVEL: ";
        s += playerData.level.ToString();

        r.x += 0.0006f * w;
        r.y += 0.001f * h;
        c = Color.black;
        if (levelUpDisplayTimer > 0f)
            c.a = Mathf.Clamp01(.381f + (1f - (levelUpDisplayTimer * 3f) % 1f));
        GUI.color = c;
        GUI.Label(r, s, g);
        r.x -= 0.0012f * w;
        r.y -= 0.002f * h;
        c = Color.yellow;
        if (levelUpDisplayTimer > 0f)
            c.a = Mathf.Clamp01(.381f + (1f - (levelUpDisplayTimer * 3f) % 1f));
        GUI.color = c;
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
        if (!letterPopup && !hidePlayerNameTag && playerName != "")
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
            g.fontSize = Mathf.RoundToInt(14f * (w / 1024f));
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

        if (letterPopup)
        {
            float progress = letterPopupCurve.Evaluate(letterPopupTimer/LETTERPOPUPTIME);
            if (letterPopsDown)
                progress = 1f - progress;

            // centered and square, based on height
            r.x = (0.5f * w) - ((1f * h) / 2f);
            r.y = 0.025f * h;
            r.y += (0.9f * progress) * h;
            r.width = 1f * h; // square
            r.height = 1f * h;

            // letter image
            t = (Texture2D)Resources.Load("Popup_Open Letter");
            GUI.color = Color.white;
            GUI.DrawTexture(r, t);

            // letter text
            r.x += 0.0725f * w;
            r.y += 0.15f * h;
            r.width = 0.32f * w;
            r.height -= 0.2f * h;
            g = new GUIStyle(GUI.skin.label);
            g.font = (Font)Resources.Load("ChewedPenBB"); // "BRUSHSCI");
            g.fontSize = Mathf.RoundToInt(26f * (w / 1024f));
            g.alignment = TextAnchor.UpperLeft;
            g.wordWrap = true;
            g.normal.textColor = Color.black;
            g.hover.textColor = Color.black;
            g.active.textColor = Color.black;
            s = letterMessage;
            GUI.Label(r, s, g);

            // confirm button
            r.x = 0.45f * w;
            r.y = h - (0.1f * h);
            r.width = 0.1f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.button);
            g.fontSize = Mathf.RoundToInt(16f * (w / 1024f));
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.yellow;
            g.active.textColor = Color.white;
            if (letterPopOKSelected)
            {
                g.normal.textColor = Color.yellow;
                g.hover.textColor = Color.yellow;
                g.active.textColor = Color.yellow;
            }
            s = "OK";
            if (letterPopupTimer == 0f && 
                (GUI.Button(r,s,g) || 
                (padMgr != null && padMgr.gamepads[0].isActive && 
                letterPopOKSelected && padMgr.gPadDown[0].aButton)))
            {
                letterPopupTimer = LETTERPOPUPTIME;
                letterPopsDown = true;

                // consuming input, but why?
                if (padMgr != null && padMgr.gamepads[0].isActive)
                    padMgr.gPadDown[0].aButton = false;
            }
        }

        if (levelUpDisplayTimer > 0f)
        {
            int guiDepth = GUI.depth;
            // level up banner
            r.x = 0.2f * w;
            r.y = 0.225f * h;
            r.y -= 0.1f * h * (1f - (levelUpDisplayTimer/LEVELUPDISPLAYTIME));
            r.width = 0.6f * w;
            r.height = 0.2f * h;
            g = new GUIStyle(GUI.skin.label);
            g.alignment = TextAnchor.MiddleCenter;
            g.fontSize = Mathf.RoundToInt(100f * (w / 1024f));
            g.fontStyle = FontStyle.BoldAndItalic;
            s = "LEVEL UP!";

            GUI.depth = -999; // TEST: banner on top always?

            r.x += 0.0024f * w;
            r.y += 0.004f * h;
            c = Color.black;
            c.a = Mathf.Clamp01(levelUpDisplayTimer);
            GUI.color = c;
            GUI.Label(r, s, g);
            r.x -= 0.0048f * w;
            r.y -= 0.008f * h;
            c = Color.yellow;
            c.a = Mathf.Clamp01(levelUpDisplayTimer);
            GUI.color = c;
            GUI.Label(r, s, g);

            GUI.depth = guiDepth;
        }
        else if (groceryList != "")
        {
            // ARCANA SKILL : Grocery List
            // grocery list display
            r.x = 0.2f * w;
            r.y = 0.9f * h;
            r.width = 0.6f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.label);
            g.alignment = TextAnchor.MiddleCenter;
            g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
            g.fontStyle = FontStyle.Italic;
            g.wordWrap = true;
            s = groceryList;
            r.x += 0.0004f * w;
            r.y += 0.0005f * w;
            GUI.color = Color.black;
            GUI.Label(r, s, g);
            r.x -= 0.0008f * w;
            r.y -= 0.001f * w;
            GUI.color = Color.white;
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

        g = new GUIStyle(GUI.skin.label);
        g.alignment = TextAnchor.MiddleCenter;
        g.fontSize = Mathf.RoundToInt(20f * (w/ 1024f));
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
