using UnityEngine;

public class IslandUpgradeMenu : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the island upgrade menu

    public enum StageOfTransaction
    {
        None,
        Purchases,
        Composition,
        Completion
    }

    public enum UpgradeCategory
    {
        Islands,
        Farms,
        Towers,
        OutdoorProps,
        IndoorProps
    }

    public enum  UpgradeType
    {
        IslandSmall,
        IslandMedium,
        IslandLarge,
        IslandVeryLarge,
        FarmTiny,
        FarmModest,
        FarmSizable,
        FarmHuge,
        FarmVast,
        HermitTower,
        WizardTower,
        SorcererTower,
        TeleportNode,
        Mailbox,
        CompostBin,
        BushA,
        BushB,
        BushC,
        RockA,
        RockB,
        RockC,
        LampPostA,
        LampPostB,
        BannerA,
        BannerB,
        IntCandleStickA,
        IntCandleStickB,
        IntFireplace,
        IntBookshelf,
        IntWritingDesk,
        IntTapestryA,
        IntTapestryB
    }

    public enum ConfirmType
    {
        None,
        ReplaceExisting,
        Purchase,
        ClearPurchases,
        ComposeIsland,
        CompleteIsland
    }

    [System.Serializable]
    public class MenuItem
    {
        public string name;
        public UpgradeCategory category;
        public UpgradeType type;
        public string description;
        public int price;
        public Texture2D icon;
        public GameObject prefab;
        public float itemPadding;
        public bool playerNowHas;
    }
    public MenuItem[] items;

    private PlayerControlManager pcm;
    private GreenerGameManager ggm;
    private IslandManager im;

    private bool hasSalesmanDiscount;

    private bool cursorMode;
    private GameObject cursor;
    private float cursorSpeed = 1f;
    private Vector3 cursorMove;
    private bool gridLockCursor;

    private GameObject islandObj;
    private Vector3 islandSavedPosition;
    private Vector3 islandCenter;
    private float islandRange = 7f;

    private GameObject currentConfigObject;
    private Color currentObjectColor;
    private float currentObjectPadding; // amount effecting bounds

    private bool configPulse;
    private float configObjectPulse;

    private bool validConfig;
    private string configFeedback;
    private float feedbackTimer;

    private MultiGamepad padMgr;
    private bool usingPad;
    private int padButtonSelection = -1;
    private int padMaxButton = 0;
    private int padClickButton = -1;
    private int padMove = -1;

    private StageOfTransaction stage;

    private UpgradeCategory currentCategory;
    private MenuItem[] displayItems;
    private int menuItemSelection = -1;
    private int topOfMenuList = 0;
    private int maxDisplayItems = 5;

    private MenuItem currentPurchaseItem;
    private int currentPurchsePrice;
    private MenuItem[] purchaseItemsHeld = new MenuItem[0];
    private int purchaseGoldHeld;

    private float invalidPulseTimer;

    private AudioManager sfxAudio;

    private SalesVisitManager salesVisit;
    private bool menuOpen;

    private bool confirmPopup;
    private ConfirmType confirmType;
    private string popupMessage;
    private float popupTimer;
    private bool popUpMoveDown;

    private AnimationCurve greenerAnimCurve;
    private float greenerPasturesTimer;
    private bool greenerMoveUp;

    const float FEEDBACKTIME = 3f;
    const float PULSETIME = 2f;
    const float INVALIDPULSETIME = 1f;
    const float POPTIME = 1f;
    const float GREENERPASTURESTIME = 4f;
    const float GREENERDEPTH = 40f;


    void Start()
    {
        // validate
        pcm = GameObject.FindFirstObjectByType<PlayerControlManager>();
        if (pcm == null)
        {
            Debug.LogError("--- IslandUpgradeMenu [Start] : no player control manager found in scene. aborting.");
            enabled = false;
        }
        ggm = GameObject.FindFirstObjectByType<GreenerGameManager>();
        if (ggm == null)
        {
            Debug.LogError("--- IslandUpgradeMenu [Start] : no game manager manager found in scene. aborting.");
            enabled = false;
        }
        im = GameObject.FindFirstObjectByType<IslandManager>();
        if (im == null)
        {
            Debug.LogError("--- IslandUpgradeMenu [Start] : no island manager manager found in scene. aborting.");
            enabled = false;
        }
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
        {
            Debug.LogError("--- IslandUpgradeMenu [Start] : no multigamepad manager manager found in scene. aborting.");
            enabled = false;
        }
        GameObject sfxObj = GameObject.Find("AudioMgr SFX");
        if (sfxObj != null)
            sfxAudio = sfxObj.GetComponent<AudioManager>();
        if (items == null || items.Length == 0)
        {
            Debug.LogError("--- IslandUpgradeMenu [Start] : no items configured on this tool (need icons and prefabs). aborting.");
            enabled = false;
        }
        // initialize
        if (enabled)
        {
            validConfig = true; // begin with valid island configuration
            CreateCursor();

            // set up menu
            ConfigureMenuItems();

            // scan player's current configuration (matched to menu items)
            ScanPlayerIslandConfig();

            // lock down player
            pcm.characterFrozen = true;
            pcm.freezeCharacterActions = true;
            // detect salesman discount
            hasSalesmanDiscount = PlayerSystem.PlayerHasEffect(pcm.playerData, PlayerEffect.SkillFriendsSalesman);
            
            // start menu on islands
            currentCategory = UpgradeCategory.Islands;
            displayItems = GetDisplayItems(currentCategory);

            // set stage to purchasing
            stage = StageOfTransaction.Purchases;

            // greenerAnimCurve for 'greener pastures' moment
            greenerAnimCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
        else if (salesVisit != null)
            salesVisit.IslandMenuClosed(); // abort menu operation, free the salesman
    }

    void ConfigureMenuItems()
    {
        if (items == null || items.Length == 0)
        {
            Debug.LogWarning("--- IslandUpgradeMenu [ConfigureMenuItemDescriptions] : no items. will ignore.");
            return;
        }

        for (int i =0; i < items.Length; i++)
        {
            if (items[i].icon == null)
                Debug.LogWarning("--- IslandUpgradeMenu [ConfigureMenuItemDescriptions] : item #" + i + " is missing an icon. will ignore.");
            if (items[i].prefab == null)
                Debug.LogWarning("--- IslandUpgradeMenu [ConfigureMenuItemDescriptions] : item #" + i + " is missing a prefab. will ignore.");

            switch (items[i].type)
            {
                // islands
                case UpgradeType.IslandSmall:
                    items[i].name = "Small Island";
                    items[i].description = "Our smallest island measuring 14 meters in diameter. It comes with a flat grassy surface and enough fertile ground to grow upon.";
                    items[i].price = 7500;
                    items[i].itemPadding = 7f;
                    break;
                case UpgradeType.IslandMedium:
                    items[i].name = "Medium Island";
                    items[i].description = "If you like a little more room, this 16 meter diameter island is priced to fit your budget. Make the move to this great island.";
                    items[i].price = 10000;
                    items[i].itemPadding = 8f;
                    break;
                case UpgradeType.IslandLarge:
                    items[i].name = "Large Island";
                    items[i].description = "This island boasts a 18 meter diameter scale and can accomodate most of our larger upgrades. Keep this at the top of your wish list.";
                    items[i].price = 15000;
                    items[i].itemPadding = 9f;
                    break;
                case UpgradeType.IslandVeryLarge:
                    items[i].name = "Very Large Island";
                    items[i].description = "Our largest island available is 20 meters in diameter and demands a fair amount of accessories to fill it. A very impressive island.";
                    items[i].price = 20000;
                    items[i].itemPadding = 10f;
                    break;
                // farms
                case UpgradeType.FarmTiny:
                    items[i].name = "Tiny Farm";
                    items[i].description = "10 plots of land make up the smallest of farms. Manageable for any Biomancer. They say a garden is all we really need to grow.";
                    items[i].price = 1000;
                    items[i].itemPadding = 2f;
                    break;
                case UpgradeType.FarmModest:
                    items[i].name = "Modest Farm";
                    items[i].description = "When you're ready to handle more crops and more harvests, this upgrade is for you. A 4x4 farm with 16 plots in total.";
                    items[i].price = 2000;
                    items[i].itemPadding = 2.5f;
                    break;
                case UpgradeType.FarmSizable:
                    items[i].name = "Sizable Farm";
                    items[i].description = "A 5x5 farm with 25 plots, brings in a large harvest while maintainable for the casual Biomancer. A good mid-sized farm.";
                    items[i].price = 3000;
                    items[i].itemPadding = 3f;
                    break;
                case UpgradeType.FarmHuge:
                    items[i].name = "Huge Farm";
                    items[i].description = "To impress your guests and the market at the same time, look no further than this glorioud 6x6 farm with a total of 36 plots!";
                    items[i].price = 4000;
                    items[i].itemPadding = 3.5f;
                    break;
                case UpgradeType.FarmVast:
                    items[i].name = "Vast Farm";
                    items[i].description = "Only for the advanced Biomancer, we offer this incredible 7x7 farm with an amazing 49 plots! Only for the larger islands.";
                    items[i].price = 5000;
                    items[i].itemPadding = 4f;
                    break;
                // towers
                case UpgradeType.HermitTower:
                    items[i].name = "Hermit Tower";
                    items[i].description = "A humble tower for a humble Biomancer. This tower need only be an indoor space to craft magic or retreat from dramatic weather.";
                    items[i].price = 10000;
                    items[i].itemPadding = 2f;
                    break;
                case UpgradeType.WizardTower:
                    items[i].name = "Wizard Tower";
                    items[i].description = "Impressive tower and a marvelous upgrade. The classic roofline and taller stature appears as sturdy as it is welcoming to guests.";
                    items[i].price = 10000;
                    items[i].itemPadding = 2.5f;
                    break;
                case UpgradeType.SorcererTower:
                    items[i].name = "Sorcerer Tower";
                    items[i].description = "There is no finer residence for Biomancers. The sorcerer tower is on a grand scale; standing proud and tall to be seen from afar.";
                    items[i].price = 20000;
                    items[i].itemPadding = 3f;
                    break;
                // outdoor props
                case UpgradeType.BushA:
                    items[i].name = "Bush (Style A)";
                    items[i].description = "Bushy! When you need a bush on your farm, look no further! It has everything you want in a bush, and it is maintenance free!";
                    items[i].price = 50;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.BushB:
                    items[i].name = "Bush (Style B)";
                    items[i].description = "An alternate bush. Sometimes the standard bush just isn't right. That's when you turn to this; a standout bush.";
                    items[i].price = 50;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.BushC:
                    items[i].name = "Bush (Style C)";
                    items[i].description = "A different kind of bush. This bush sets itself apart from all others. It definitely is distinctive in it's own way.";
                    items[i].price = 50;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.RockA:
                    items[i].name = "Rock (Style A)";
                    items[i].description = "Rock of Ages; truly a classic rock. Very solid. Guaranteed to hold up under any weather. This rock will never let you down.";
                    items[i].price = 100;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.RockB:
                    items[i].name = "Rock (Style B)";
                    items[i].description = "Rocks come and go, but this one is here to stay. Owning this rock is owning a piece of history; literally very old!";
                    items[i].price = 100;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.RockC:
                    items[i].name = "Rock (Style C)";
                    items[i].description = "A rock without the roll. A no-nonsense rock when you just need a rock you can count on to be there - every time.";
                    items[i].price = 100;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.LampPostA:
                    items[i].name = "Lamp Post (Style A)";
                    items[i].description = "A fancy lamp post, for the discerning Biomancer who fancies themselves cultured and modern.";
                    items[i].price = 500;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.LampPostB:
                    items[i].name = "Lamp Post (Style B)";
                    items[i].description = "A modest lamp post with purpose. A lamp post that says, 'I can be a light for all and not be fancy. Watch me.'";
                    items[i].price = 500;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.BannerA:
                    items[i].name = "Banner (Style A)";
                    items[i].description = "Declare your island clearly and proudly with this broad banner. It automagically conforms to your personal colors!";
                    items[i].price = 1250;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.BannerB:
                    items[i].name = "Banner (Style B)";
                    items[i].description = "A sturdy banner that displays your personal colors automagically! Tell all who see this banner whose farm this is.";
                    items[i].price = 1250;
                    items[i].itemPadding = 0.5f;
                    break;
                // indoor props
                case UpgradeType.IntCandleStickA:
                    items[i].name = "Candle Stick (Style A)";
                    items[i].description = "Everyone needs some light. These candle sticks stand tall on study wooden posts to cast the warm light across your interior.";
                    items[i].price = 750;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.IntCandleStickB:
                    items[i].name = "Candle Stick (Style B)";
                    items[i].description = "Everyone could use more light. This one stands closer to the doorway to help guests remove shoes and coats. Always lit!";
                    items[i].price = 750;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.IntFireplace:
                    items[i].name = "Fireplace";
                    items[i].description = "A warm hearth for cold nights. This is a center piece to any tower interior, and the warm glow and crackling fire is a treat!";
                    items[i].price = 2500;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.IntBookshelf:
                    items[i].name = "Bookshelf";
                    items[i].description = "A bookshelf tells your guests you value knowledge. A must for every well-read Biomancer. Comes with assorted books!";
                    items[i].price = 1750;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.IntWritingDesk:
                    items[i].name = "Writing Desk";
                    items[i].description = "This valuable addition to your tower interior is as useful as it is stylish. Compose letters or write that great novel!";
                    items[i].price = 1500;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.IntTapestryA:
                    items[i].name = "Tapestry (Style A)";
                    items[i].description = "Colorful banner in your style. Hangs on most any wall. Always a fine decor choice for the Biomancer with taste.";
                    items[i].price = 1000;
                    items[i].itemPadding = 0.5f;
                    break;
                case UpgradeType.IntTapestryB:
                    items[i].name = "Tapestry (Style B)";
                    items[i].description = "Truly a magnificent tapestry, made from the finest material. Automagically configures to your personal colors.";
                    items[i].price = 1000;
                    items[i].itemPadding = 0.5f;
                    break;
            }
        }
    }

    void ScanPlayerIslandConfig()
    {
        // island
        // 7m radius = smallest, 10m radius = largest
        //if (pcm.playerData.island.w == 7)
        // farm
        // # of plots
        // 10 = smallest, 49 = largest
        // tower
        // tower structure type

        // outdoor props
        // island props
        // im.islands[pcm.playerData.playerIsland]
        // indoor props
        // island props (int)
    }

    MenuItem[] GetDisplayItems( UpgradeCategory category )
    {
        // find number of items
        int num = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].category == category)
                num++;
        }
        MenuItem[] retItems = new MenuItem[num];

        int count = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].category == category)
            {
                retItems[count] = items[i];
                count++;
            }
        }

        topOfMenuList = 0;

        return retItems;
    }

    public void ConfigSalesVisit( SalesVisitManager sales )
    {
        salesVisit = sales;
        menuOpen = true;
    }

    void SignalMenuClose()
    {
        // remove cursor
        Destroy(cursor);
        // release player
        pcm.characterFrozen = false;
        pcm.freezeCharacterActions = false;
        // close menu
        menuOpen = false;
        salesVisit.IslandMenuClosed();
        // remove this tool
        Destroy(gameObject, 1f);
    }

    void CreateCursor()
    {
        cursor = GameObject.Instantiate((GameObject)Resources.Load("Cast Cursor"));
        cursor.name = "Island Upgrade Cursor";
        SetCursorVisible(false);
    }

    void SetCursorVisible( bool visible )
    {
        cursor.GetComponent<Renderer>().enabled = visible;
        cursor.transform.GetChild(0).GetComponent<Renderer>().enabled = visible;
    }

    void MoveCursor()
    {
        Vector3 pos = cursor.transform.position;
        if (gridLockCursor)
        {
            pos += cursorMove.normalized;
            cursorMove = Vector3.zero;
        }
        else
            pos += cursorMove * cursorSpeed * Time.deltaTime;
        cursor.transform.position = pos;

        // cursor bounds (island center and range)
        float dist = Vector3.Distance(cursor.transform.position, islandCenter);
        // account for 'padding' of current object
        if (dist + currentObjectPadding > islandRange)
        {
            pos += (islandCenter - cursor.transform.position) *
                ((dist + currentObjectPadding) - islandRange);
            cursor.transform.position = pos;
        }
    }

    bool CheckValidCursorLocation()
    {
        bool retBool = true;

        // check if overlapping critical elements (structures or other props)

        return retBool;
    }

    void ConfigFeedback( string feedbackString )
    {
        configFeedback = feedbackString;
        feedbackTimer = FEEDBACKTIME;
    }

    void ConfirmPopup( string message, ConfirmType type )
    {
        confirmType = type;
        popupMessage = message;
        popUpMoveDown = false;
        popupTimer = POPTIME;
        confirmPopup = true;
        if (usingPad)
        {
            padMaxButton = 1;
            padButtonSelection = -1;
        }
    }

    void SetCurrentConfigObject( bool set, GameObject obj, float objectPadding )
    {
        // validate
        if (currentConfigObject != null)
        {
            Debug.LogWarning("--- IslandUpgradeMenu [SetCurrentConfigObject] : already current config object set. will ignore.");
            return;
        }
        if (obj == null)
        {
            Debug.LogWarning("--- IslandUpgradeMenu [SetCurrentConfigObject] : no object given to set. will ignore.");
            return;
        }
        if (objectPadding <= 0f)
        {
            Debug.LogWarning("--- IslandUpgradeMenu [SetCurrentConfigObject] : object padding is "+objectPadding+". will ignore.");
            return;
        }

        currentConfigObject = obj;
        Renderer r = currentConfigObject.GetComponentInChildren<Renderer>();
        if (r != null)
            currentObjectColor = r.material.color;
        currentObjectPadding = objectPadding;
    }

    void ReleaseCurrentConfigObject()
    {
        if (currentConfigObject == null)
        {
            Debug.LogWarning("--- IslandUpgradeMenu [ReleaseCurrentConfigObject] : no object found to release. will ignore.");
            return;
        }

        Renderer r = currentConfigObject.GetComponentInChildren<Renderer>();
        if (r != null)
            r.material.color = currentObjectColor;
        currentConfigObject = null;
        currentObjectColor = Color.white;
        currentObjectPadding = 0f;
    }

    void Update()
    {
        // run timers
        if (feedbackTimer > 0f)
        {
            feedbackTimer -= Time.deltaTime;
            if (feedbackTimer < 0f)
            {
                feedbackTimer = 0f;
                configFeedback = "";
            }
        }
        if (popupTimer > 0f)
        {
            popupTimer -= Time.deltaTime;
            if (popupTimer < 0f)
            {
                popupTimer = 0f;
                if (!popUpMoveDown)
                    popUpMoveDown = true;
                else
                    confirmPopup = false;
            }
        }
        if (invalidPulseTimer > 0f)
        {
            invalidPulseTimer -= Time.deltaTime;
            if (invalidPulseTimer < 0f)
                invalidPulseTimer = 0f;
        }
        if (greenerPasturesTimer > 0f)
        {
            greenerPasturesTimer -= Time.deltaTime;
            if (greenerPasturesTimer < 0f)
            {
                greenerPasturesTimer = 0f;
                if (!greenerMoveUp)
                {
                    // TODO: perform replacement swaps (new island, new tower, repositioned mailbox and tporter)
                    greenerMoveUp = true;
                    greenerPasturesTimer = GREENERPASTURESTIME;
                }
            }
            // island move progress
            float progress = greenerAnimCurve.Evaluate(greenerPasturesTimer / GREENERPASTURESTIME);
            if (!greenerMoveUp)
                progress = 1f - progress;
            if (islandObj != null)
            {
                Vector3 pos = islandSavedPosition;
                pos.y -= progress * GREENERDEPTH;
                islandObj.transform.position = pos;
            }
        }

        // run config pulse
        if (configPulse)
        {
            configObjectPulse = Mathf.Sin(Time.time * PULSETIME);
            configObjectPulse += 1f;
            configObjectPulse *= 0.5f;
            // handle various config object renderers
            if (currentConfigObject != null)
            {
                Renderer r = currentConfigObject.GetComponentInChildren<Renderer>();
                if (r != null)
                {
                    Color c = Color.white;
                    c *= configObjectPulse;
                    r.material.color = c;
                }
            }
        }
        else
            configObjectPulse = 1f;

        // cursor move
        if (cursorMode)
        {
            if (!cursor.GetComponent<Renderer>().enabled)
                SetCursorVisible(true);
            MoveCursor();
            if (currentConfigObject != null)
                currentConfigObject.transform.position = cursor.transform.position;
        }
        else if (cursor != null &&
            cursor.GetComponent<Renderer>().enabled)
            SetCursorVisible(false);

        // gamepad control
        usingPad = false;
        if (padMgr != null && padMgr.gamepads[0].isActive)
        {
            usingPad = true;   
            padClickButton = -1;
            padMove = -1;
            // selection movement
            if (padMgr.gPadDown[0].YaxisL > 0f)
                padMove = 0; // up
            if (padMgr.gPadDown[0].YaxisL < 0f)
                padMove = 1; // down
            if (padMgr.gPadDown[0].XaxisL < 0f)
                padMove = 2; // left
            if (padMgr.gPadDown[0].XaxisL > 0f)
                padMove = 3; // right
            // cursor mode
            if (cursorMode)
            {
                cursorMove.x = padMgr.gPadDown[0].XaxisL;
                cursorMove.z = padMgr.gPadDown[0].YaxisL;
            }
            // consume input (for some reason)
            padMgr.gPadDown[0].YaxisL = 0f;
            padMgr.gPadDown[0].XaxisL = 0f;
            // general button navigation
            if (!cursorMode)
            {
                if (padMove == 0)
                    padButtonSelection--;
                if (padMove == 1)
                    padButtonSelection++;
                padButtonSelection = Mathf.Clamp(padButtonSelection, 0, padMaxButton);
            }
            // button press
            if (padMgr.gPadDown[0].aButton)
            {
                padClickButton = padButtonSelection;
                // consume input (for some reason)
                padMgr.gPadDown[0].aButton = false;
            }
        }

        // keyboard controls
        int keyMove = -1;
        if (gridLockCursor || !cursorMode)
        {
            if (Input.GetKeyDown(pcm.upKey))
                keyMove = 0; // up
            if (Input.GetKeyDown(pcm.downKey))
                keyMove = 1; // down
            if (Input.GetKeyDown(pcm.leftKey))
                keyMove = 2; // left
            if (Input.GetKeyDown(pcm.rightKey))
                keyMove = 3; // right
        }
        else
        {
            if (Input.GetKey(pcm.upKey))
                keyMove = 0; // up
            if (Input.GetKey(pcm.downKey))
                keyMove = 1; // down
            if (Input.GetKey(pcm.leftKey))
                keyMove = 2; // left
            if (Input.GetKey(pcm.rightKey))
                keyMove = 3; // right
        }            
        // cursor mode
        if (cursorMode)
        {
            if (keyMove == 0)
                cursorMove.z = 1f;
            if (keyMove == 1)
                cursorMove.z = -1f;
            if (keyMove == 2)
                cursorMove.x = -1f;
            if (keyMove == 3)
                cursorMove.x = 1f;
        }
        else
        {
            if (keyMove == 0)
                menuItemSelection--;
            if (keyMove == 1)
                menuItemSelection++;
            if (menuItemSelection < 0)
                menuItemSelection = 0;
            if (menuItemSelection > displayItems.Length-1)
                menuItemSelection = displayItems.Length-1;
        }

        // configure top of menu
        if (menuItemSelection > topOfMenuList + maxDisplayItems - 1)
            topOfMenuList = menuItemSelection - maxDisplayItems + 1;
        if (menuItemSelection < topOfMenuList)
            topOfMenuList = menuItemSelection;

        // purchase selection
        if (stage == StageOfTransaction.Purchases && 
            (Input.GetKeyDown(pcm.actionAKey) || usingPad && padMgr.gPadDown[0].aButton))
        {
            if (CanPurchase(displayItems[menuItemSelection]))
            {
                ConfirmPopup("Are you sure you want to buy\n"
                    + displayItems[menuItemSelection].name + "\nfor " +
                    GetPurchasePrice(displayItems[menuItemSelection]) + " gold?",
                    ConfirmType.Purchase);
                currentPurchaseItem = displayItems[menuItemSelection];
                currentPurchsePrice = GetPurchasePrice(displayItems[menuItemSelection]);
            }
            else
            {
                float rnd = RandomSystem.FlatRandom01();
                if (rnd < .25f)
                    salesVisit.MenuDialogBeat("It looks like that is a little more than you have, friend.");
                else if (rnd >= .25f && rnd < .5f)
                    salesVisit.MenuDialogBeat("Some things are worth waiting for. Just save up your gold.");
                else if (rnd >= .5f && rnd < .75f)
                    salesVisit.MenuDialogBeat("You've got good taste, friend. That item will have to wait.");
                else
                    salesVisit.MenuDialogBeat("It seems you're a little short on gold to buy that item.");
                invalidPulseTimer = INVALIDPULSETIME;
            }
        }
    }

    int GetPurchasePrice( MenuItem item )
    {
        int thisPrice = item.price;
        
        if (hasSalesmanDiscount)
            thisPrice = Mathf.RoundToInt(thisPrice * 0.75f);

        return thisPrice;
    }

    bool CanPurchase( MenuItem item )
    {
        bool retBool = false;

        int thisPrice = item.price;
        if (hasSalesmanDiscount)
            thisPrice = Mathf.RoundToInt(thisPrice * 0.75f);
        retBool = ((pcm.playerData.gold - purchaseGoldHeld) >= thisPrice);

        return retBool;
    }

    void MakePurchase( MenuItem item )
    {
        int thisPrice = item.price;
        if (hasSalesmanDiscount)
            thisPrice = Mathf.RoundToInt(thisPrice * 0.75f);
        pcm.playerData.gold -= thisPrice;
    }

    void AddToPurchaseItemsHeld( MenuItem item )
    {
        MenuItem[] tmp = new MenuItem[purchaseItemsHeld.Length + 1];
        for (int i = 0; i < purchaseItemsHeld.Length; i++)
        {
            tmp[i] = purchaseItemsHeld[i];
        }
        tmp[purchaseItemsHeld.Length] = item;
        purchaseItemsHeld = tmp;
    }

    void PrepareForGreenerPastures()
    {
        // if we are swapping islands, perform the lowering of island, rising of new island

    }

    void LaunchGreenerPastures()
    {
        // TODO: get current island, prepare to revise
        // prepare to swap tower
        // prepare to swap farm
        // prepare to move mail box and teleporter
        greenerMoveUp = false;
        greenerPasturesTimer = GREENERPASTURESTIME;
    }

    void PerformReplacementSwaps()
    {
        // all purchase items held that represent replacement upgrades, swap them
        for (int i = 0; i < purchaseItemsHeld.Length; i++)
        {
            if (purchaseItemsHeld[i].type == UpgradeType.IslandMedium ||
                purchaseItemsHeld[i].type == UpgradeType.IslandLarge ||
                purchaseItemsHeld[i].type == UpgradeType.IslandVeryLarge )
            {
                // REVIEW: we would like to do the 'greener pastures' move here
                // step off the current island (float on color trail spots?)
                // watch old island sink, watch new island rise
                // step back on island?
                
                // REVIEW: should towers be set already?
                // REVIEW: should farm be set already?
                // REVIEW: should outdoor props be set as they were before to start?


            }
        }
    }

    string FormCategoryLabel( UpgradeCategory category )
    {
        string retString = "";

        switch (category)
        {
            case UpgradeCategory.Islands:
                retString = "Islands";
                break;
            case UpgradeCategory.Farms:
                retString = "Farms";
                break;
            case UpgradeCategory.Towers:
                retString = "Towers";
                break;
            case UpgradeCategory.OutdoorProps:
                retString = "Outdoor Props";
                break;
            case UpgradeCategory.IndoorProps:
                retString = "Indoor Props";
                break;
        }

        return retString;
    }

    void OnGUI()
    {
        if (!menuOpen)
            return;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        r.x = 0.05f * w;
        r.y = 0.1f * h;
        r.width = 0.9f * w;
        r.height = 0.875f * h;

        GUIStyle g = new GUIStyle(GUI.skin.box);
        g.padding = new RectOffset(0, 0, 30, 0);
        g.fontSize = Mathf.RoundToInt(24 * (w/1024f));
        g.fontStyle = FontStyle.Bold;
        //g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;

        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.white;
        string s = "";

        // menu box
        s = "ISLAND UPGRADES";
        GUI.Box(r, s, g);

        // category label
        r.x = 0.4f * w;
        r.y = 0.175f * h;
        r.width = 0.2f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.label);
        g.alignment = TextAnchor.MiddleCenter;
        g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        g.fontStyle = FontStyle.BoldAndItalic;
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;
        s = FormCategoryLabel(currentCategory);
        GUI.Label(r, s, g);
        // category nav buttons
        r.x = 0.2f * w;
        r.y = 0.175f * h;
        r.width = 0.1f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.button);
        if (usingPad)
            g.fontSize = Mathf.RoundToInt(12 * (w / 1024f));
        g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.yellow;
        s = "<<";
        if (usingPad)
            s += "\nL Bump";
        if (GUI.Button(r,s,g) || 
            (usingPad && padMgr.gPadDown[0].LBump))
        {
            currentCategory--;
            if (currentCategory < UpgradeCategory.Islands)
                currentCategory = UpgradeCategory.IndoorProps;
            displayItems = GetDisplayItems(currentCategory);
            topOfMenuList = 0;

            // consuming input, but why?
            if (usingPad)
                padMgr.gPadDown[0].LBump = false;
        }
        r.x = 0.7f * w;
        s = ">>";
        if (usingPad)
            s += "\nR Bump";
        if (GUI.Button(r, s, g) ||
            (usingPad && padMgr.gPadDown[0].RBump))
        {
            currentCategory++;
            if (currentCategory > UpgradeCategory.IndoorProps)
                currentCategory = UpgradeCategory.Islands;
            displayItems = GetDisplayItems(currentCategory);
            topOfMenuList = 0;

            // consuming input, but why?
            if (usingPad)
                padMgr.gPadDown[0].RBump = false;
        }

        GUI.enabled = !confirmPopup;

        // menu item list
        r.y = 0.25f * h;
        for (int i = topOfMenuList; i < Mathf.Min((topOfMenuList+maxDisplayItems),displayItems.Length); i++)
        {
            if (displayItems == null || displayItems.Length == 0 ||
                i < 0 || i > displayItems.Length)
                continue;

            // icon
            r.x = 0.1f * w;
            r.width = 0.05f * w;
            r.height = r.width; // square
            t = displayItems[i].icon;
            c = Color.white;
            GUI.color = c;
            GUI.DrawTexture(r, t);
            // name
            r.x = 0.175f * w;
            r.width = 0.2f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.label);
            g.alignment = TextAnchor.MiddleLeft;
            g.fontSize = Mathf.RoundToInt(18 * (w/1024f));
            g.fontStyle = FontStyle.Bold;
            s = displayItems[i].name;
            r.x += 0.0008f * w;
            r.y += 0.001f * w;
            g.normal.textColor = Color.black;
            g.hover.textColor = Color.black;
            g.active.textColor = Color.black;
            GUI.Label(r, s, g);
            r.x -= 0.0016f * w;
            r.y -= 0.002f * w;
            g.normal.textColor = Color.white;
            if (menuItemSelection == (i + 0) || 
                (usingPad && padButtonSelection == (i + 0)))
                g.normal.textColor = Color.yellow;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            GUI.Label(r, s, g);
            r.x += 0.0008f * w;
            r.y += 0.001f * w;
            // price
            r.x = 0.8f * w;
            r.width = 0.1f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.label);
            g.padding = new RectOffset(0, 20, 0, 0);
            g.alignment = TextAnchor.MiddleRight;
            g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            int thisPrice = displayItems[i].price;
            if (hasSalesmanDiscount)
                thisPrice = Mathf.RoundToInt(thisPrice * 0.75f);
            s = thisPrice.ToString();
            r.x += 0.0008f * w;
            r.y += 0.001f * w;
            g.normal.textColor = Color.black;
            g.hover.textColor = Color.black;
            g.active.textColor = Color.black;
            GUI.Label(r, s, g);
            r.x -= 0.0016f * w;
            r.y -= 0.002f * w;
            c = Color.white;
            if (menuItemSelection == (i + 0) ||
                (usingPad && padButtonSelection == (i + 0)))
                c = Color.yellow;
            if (invalidPulseTimer > 0f && menuItemSelection == (i + 0))
            {
                c = Color.red;
                c *= (invalidPulseTimer * 5f) % 1f;
            }
            g.normal.textColor = c;
            g.hover.textColor = c;
            g.active.textColor = c;
            GUI.Label(r, s, g);
            r.x += 0.0008f * w;
            r.y += 0.001f * w;
            // description
            r.x = 0.2f * w;
            r.y += 0.025f * h;
            r.width = 0.6f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.label);
            g.alignment = TextAnchor.MiddleLeft;
            g.wordWrap = true;
            g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
            g.fontStyle = FontStyle.Italic;
            if (menuItemSelection == (i + 0) ||
                (usingPad && padButtonSelection == (i + 0)))
                g.fontStyle = FontStyle.BoldAndItalic;
            s = displayItems[i].description;
            r.x += 0.0008f * w;
            r.y += 0.001f * w;
            g.normal.textColor = Color.black;
            g.hover.textColor = Color.black;
            g.active.textColor = Color.black;
            GUI.Label(r, s, g);
            r.x -= 0.0016f * w;
            r.y -= 0.002f * w;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            GUI.Label(r, s, g);
            r.x += 0.0008f * w;
            r.y += 0.001f * w;

            r.y += 0.1f * h;
        }

        GUI.enabled = true;
        // confirm popup
        if (confirmPopup)
        {
            // box message
            r.x = 0.3f * w;
            r.y = 0.35f * h;
            r.width = 0.4f * w;
            r.height = 0.3f * h;
            g = new GUIStyle(GUI.skin.box);
            g.padding = new RectOffset(0, 0, 30, 0);
            g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            s = popupMessage;
            GUI.Box(r, s, g);
            // accept button
            r.x = 0.35f * w;
            r.y = 0.5f * h;
            r.width = 0.1f * w;
            r.height = 0.1f * h;
            g = new GUIStyle(GUI.skin.box);
            g.padding = new RectOffset(0, 0, 30, 0);
            g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));
            g.fontStyle = FontStyle.Bold;
            g.normal.textColor = Color.white;
            if (usingPad && padClickButton == 0)
                g.normal.textColor = Color.yellow;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            s = "ACCEPT";
            if (GUI.Button(r,s,g) ||
                (usingPad && padClickButton == 0))
            {
                if (confirmType == ConfirmType.Purchase)
                {
                    if (currentPurchaseItem != null)
                    {
                        AddToPurchaseItemsHeld(currentPurchaseItem);
                        purchaseGoldHeld += currentPurchsePrice;
                    }
                }
                if (confirmType == ConfirmType.ClearPurchases)
                {
                    purchaseItemsHeld = new MenuItem[0];
                    purchaseGoldHeld = 0;
                }
                if (confirmType == ConfirmType.ComposeIsland)
                {
                    // REVIEW: at this moment, we should have the salesman review all replacement items?
                    // TODO: step off island, etc. etc. etc.
                    //stage = StageOfTransaction.Composition;
                }
                confirmType = ConfirmType.None;
                popupTimer = POPTIME;
            }
            // cancel button
            r.x = 0.55f * w;
            g.normal.textColor = Color.white;
            if (usingPad && padClickButton == 1)
                g.normal.textColor = Color.yellow;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            s = "CANCEL";
            if (GUI.Button(r, s, g) ||
                (usingPad && padClickButton == 1))
            {

                if (confirmType == ConfirmType.Purchase)
                {                
                    // clear current purchase item consideration
                    currentPurchaseItem = null;
                    currentPurchsePrice = 0;
                }
                confirmType = ConfirmType.None;
                popupTimer = POPTIME;
            }
        }

        GUI.enabled = true;
        // total purchases cost display
        r.x = 0.075f * w;
        r.y = 0.85f * h;
        r.width = 0.2f * w;
        r.height = 0.1f * h;
        g = new GUIStyle(GUI.skin.label);
        g.fontSize = Mathf.RoundToInt(14 * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleLeft;
        g.normal.textColor = Color.yellow;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.yellow;
        s = "TOTAL GOLD TO COMPLETE OUR BUSINESS: " + purchaseGoldHeld;
        if (purchaseItemsHeld.Length > 0)
            GUI.Label(r, s, g);

        // purchases complete button
        r.x = 0.225f * w;
        r.y = 0.9f * h;
        r.width = 0.15f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        //g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = Color.white;
        if (usingPad && padButtonSelection == padMaxButton)
            g.normal.textColor = Color.yellow;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;
        s = "Compose Puchases";
        if (purchaseItemsHeld.Length > 0 && (GUI.Button(r, s, g) ||
            (usingPad && padClickButton == padMaxButton)))
        {
            ConfirmPopup("Are you sure you're done purchasing\neverything you want today?", ConfirmType.ComposeIsland);
        }

        // config feedback label
        if (feedbackTimer > 0f)
        {
            r.x = 0.05f * w;
            r.y = 0.85f * h;
            r.width = 0.9f * w;
            r.height = 0.05f * h;
            g = new GUIStyle(GUI.skin.label);
            g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
            g.fontStyle = FontStyle.BoldAndItalic;
            g.alignment = TextAnchor.MiddleCenter;
            g.wordWrap = true;
            g.normal.textColor = Color.white;
            g.hover.textColor = Color.white;
            g.active.textColor = Color.white;
            s = configFeedback;
            GUI.Label(r, s, g);
        }

        // clear purchase items button
        r.x = 0.775f * w;
        r.y = 0.9f * h;
        r.width = 0.15f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        //g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = Color.white;
        if (usingPad && padButtonSelection == padMaxButton)
            g.normal.textColor = Color.yellow;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;
        s = "Clear All Purchases";
        if (purchaseItemsHeld.Length > 0 && (GUI.Button(r, s, g) ||
            (usingPad && padClickButton == padMaxButton)))
        {
            ConfirmPopup("Are you sure you want to clear\nall purchases you've considered so far?", ConfirmType.ClearPurchases);
        }

        GUI.enabled = !confirmPopup;
        // close menu button
        r.x = 0.4f * w;
        r.y = 0.9f * h;
        r.width = 0.2f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.button);
        g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        //g.fontStyle = FontStyle.Bold;
        g.alignment = TextAnchor.MiddleCenter;
        g.normal.textColor = Color.white;
        if (usingPad && padButtonSelection == padMaxButton - 1)
            g.normal.textColor = Color.yellow;
        g.hover.textColor = Color.white;
        g.active.textColor = Color.white;
        s = "End Island Upgrades";
        if (validConfig && (GUI.Button(r, s, g) || 
            (usingPad && padClickButton == padMaxButton - 1)))
        {
            SignalMenuClose();
        }
    }
}
