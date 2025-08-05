using UnityEngine;

public class MarketManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the market transactions with players

    public enum CustomerMode
    {
        Default,
        Buy,
        Sell
    }
    
    [System.Serializable]
    public class MenuItem
    {
        public string itemName;
        public Texture2D itemIcon;
        public ItemType itemType;
        public PlantType plantType;
        public ItemEffect effect; // scrolls and potions defined by item effect
        public bool availableToBuy;
        public int buyItemValue;
        public int sellItemValue;
    }
    public MenuItem[] menuItems;

    private MultiGamepad padMgr;

    private string marketInstructions;
    private float playerCheckTimer;
    private PlayerControlManager currentCustomer;
    private PlayerControlManager leavingCustomer;
    private int menuItemSelection = -1;
    private int topOfMenuList = 0;
    private CustomerMode customerMode;
    private float rejectFlashTimer;
    private Vector3 purchaseOffset = new Vector3(-1f,0f,0f);

    private float discountBuy;
    private int playerItemFinalSellValue; // value determined when selling item, per quality

    private int[] maxMenuListPerLevel = new int[11];
    
    int generalItems = 18; // fertilizer + 15 scrolls + 2 potions
    int plantItemTypes = 3; // seed, fruit, plant

    int commonPlants = 10;
    int uncommonPlants = 11;
    int rarePlants = 10;
    int specialPlants = 10;
    int uniquePlants = 9;

    private ArtLibraryManager alm;
    private QuitOnEscape qoe; // disable to suspend use of start button while in market
    private PostOfficeManager pom;

    public Texture2D marketUIOL;
    public Texture2D marketBookOL;
    public Texture2D[] marketNPCFrames;
    public Texture2D marketBG;

    public enum CustomerState
    {
        Default,        // ready to be approached by customer
        Activating,     // beginning to change interface
        Active,         // in market, now handled by market state
        Deactivating    // end market interface, detect player left to reset
    }
    public CustomerState customerState;
    public enum MarketState
    {
        Default,        // ready to enter market
        StandardMarket, // REVIEW: can implement tavern, chicken race, alternate market stalls here
        Exiting         // leaving market
    }
    public MarketState marketState;

    private float customerStateTimer;
    private float marketStateTimer;
    private bool marketDisplay;

    private bool fadingOverlay;
    private bool fadingFromBlack;
    private Texture2D currentBackground;

    private int currentNPCFrame;
    private float npcFrameTimer;

    private Texture2D[] buttonTex;

    private ItemData[] packingItems;

    const float PLAYERCHECKTIME = 1f;
    const float MARKETPROXIMITYRANGE = .5f;
    const float REJECTFLASHTIME = 1f;
    const int MENUITEMSINLIST = 2;
    const float CUSTOMERSTATETIMERMAX = 1f;
    const float MARKETSTATETIMERMAX = 1f;
    const float MARKETNPCFRAMETIME = 3.81f;


    void Start()
    {
        // validate
        padMgr = GameObject.FindFirstObjectByType<MultiGamepad>();
        if (padMgr == null)
            Debug.LogWarning("--- MarketManager [Start] : no multi gamepad found. will ignore.");
        alm = GameObject.FindFirstObjectByType<ArtLibraryManager>();
        if (alm == null)
        {
            Debug.LogError("--- MarketManager [Start] : no art library manager found in scene. aborting.");
            enabled = false;
        }
        qoe = GameObject.FindFirstObjectByType<QuitOnEscape>();
        if (qoe == null)
        {
            Debug.LogError("--- MarketManager [Start] : no quit on escape found in scene. aborting.");
            enabled = false;
        }
        pom = GameObject.FindFirstObjectByType<PostOfficeManager>();
        if (pom == null)
        {
            Debug.LogError("--- MarketManager [Start] : no post office manager found in scene. aborting.");
            enabled = false;
        }
        // initialize
        if ( enabled )
        {
            InitializeMenu();
            InitializeMaxMenuList();
            playerCheckTimer = PLAYERCHECKTIME;
            marketInstructions = "Welcome, Biomancer!\nE=BUY F=SELL";
            if (padMgr != null && padMgr.gamepads[0].isActive)
                marketInstructions = "Welcome, Biomancer!\nA=BUY B=SELL";
            packingItems = new ItemData[0];

            // GUI Button Textures for build
            if (!Application.isEditor)
            {
                buttonTex = new Texture2D[3];
                buttonTex[0] = (Texture2D)Resources.Load("Button_Normal");
                buttonTex[1] = (Texture2D)Resources.Load("Button_Hover");
                buttonTex[2] = (Texture2D)Resources.Load("Button_Active");
            }
        }
    }

    void InitializeMaxMenuList()
    {
        for (int i = 0; i < maxMenuListPerLevel.Length; i++)
        {
            switch (i)
            {
                case 0:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes);
                    break;
                case 1:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes);
                    break;
                case 2:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes);
                    break;
                case 3:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes);
                    break;
                case 4:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes);
                    break;
                case 5:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes) +
                        (rarePlants * plantItemTypes);
                    break;
                case 6:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes) +
                        (rarePlants * plantItemTypes);
                    break;
                case 7:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes) +
                        (rarePlants * plantItemTypes);
                    break;
                case 8:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes) +
                        (rarePlants * plantItemTypes) +
                        (specialPlants * plantItemTypes);
                    break;
                case 9:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes) +
                        (rarePlants * plantItemTypes) +
                        (specialPlants * plantItemTypes);
                    break;
            }
            if (i>9)
            {
                maxMenuListPerLevel[i] = generalItems +
                (commonPlants * plantItemTypes) +
                (uncommonPlants * plantItemTypes) +
                (rarePlants * plantItemTypes) +
                (specialPlants * plantItemTypes);
            }
            maxMenuListPerLevel[i]--;
        }
    }

    void Update()
    {
        // run reject flash timer
        if ( rejectFlashTimer > 0f )
        {
            rejectFlashTimer -= Time.deltaTime;
            if (rejectFlashTimer < 0f)
                rejectFlashTimer = 0f;
        }

        if (!DetectPlayerCustomer())
            return;

        RunMarketNPCFrameTimer();

        HandleCustomerStates();

        RunMarketStateTimer();

        HandleMarketStates();
    }

    bool DetectPlayerCustomer()
    {
        if (customerState != CustomerState.Default && customerState != CustomerState.Deactivating)
            return true; // we must already have player engaged, skip

        // if no player, run player check timer
        if (currentCustomer == null && playerCheckTimer > 0f)
        {
            playerCheckTimer -= Time.deltaTime;
            if (playerCheckTimer < 0f)
            {
                playerCheckTimer = 0f;
                // detect player in proximity
                PlayerControlManager[] pcms = GameObject.FindObjectsByType<PlayerControlManager>(FindObjectsSortMode.None);
                // 
                for (int i = 0; i < pcms.Length; i++)
                {
                    float dist = Vector3.Distance(gameObject.transform.position, pcms[i].gameObject.transform.position);
                    if (dist < MARKETPROXIMITYRANGE)
                    {
                        currentCustomer = pcms[i];
                        break;
                    }
                }
                // if no player, reset check timer
                if (currentCustomer == null)
                {
                    playerCheckTimer = PLAYERCHECKTIME;
                    if (leavingCustomer != null)
                    {
                        // customer has left, reset
                        leavingCustomer = null;
                        customerState = CustomerState.Default;
                        customerStateTimer = 0f;
                    }
                }
                else if (leavingCustomer != null && currentCustomer == leavingCustomer)
                {
                    // REVIEW: remain active until player has left?
                    currentCustomer = null;
                    playerCheckTimer = PLAYERCHECKTIME;
                }
                else
                {
                    // customer engagement, activate
                    currentCustomer.characterFrozen = true;
                    currentCustomer.hidePlayerNameTag = true;
                    currentCustomer.hidePlayerHUD = true;
                    // disable almanac
                    InGameAlmanac iga = GameObject.FindFirstObjectByType<InGameAlmanac>();
                    if (iga != null)
                        iga.enabled = false;
                    // disable controls display
                    InGameControls igc = GameObject.FindFirstObjectByType<InGameControls>();
                    if (igc != null)
                        igc.enabled = false;
                    customerState = CustomerState.Activating;
                    customerStateTimer = CUSTOMERSTATETIMERMAX;
                }
            }
        }

        return (currentCustomer != null);
    }

    void HandleCustomerStates()
    {
        if (customerStateTimer == 0f)
            return;

        if (customerStateTimer > 0f)
        {
            customerStateTimer -= Time.deltaTime;
            if (customerStateTimer < 0f)
            {
                customerStateTimer = 0f;
                // handle state
                switch (customerState)
                {
                    case CustomerState.Default:
                        // we should never be here
                        break;
                    case CustomerState.Activating:
                        customerState = CustomerState.Active;
                        marketDisplay = true;
                        marketState = MarketState.StandardMarket;
                        marketStateTimer = MARKETSTATETIMERMAX;
                        fadingOverlay = true;
                        qoe.enabled = false;
                        break;
                    case CustomerState.Active:
                        currentCustomer.characterFrozen = false;
                        currentCustomer.freezeCharacterActions = false;
                        currentCustomer.hidePlayerHUD = false;
                        // re-able almanac
                        InGameAlmanac iga = GameObject.FindFirstObjectByType<InGameAlmanac>();
                        if (iga != null)
                            iga.enabled = true;
                        // show controls display hud item
                        InGameControls igc = GameObject.FindFirstObjectByType<InGameControls>();
                        if (igc != null)
                            igc.enabled = true;
                        customerState = CustomerState.Deactivating;
                        customerStateTimer = CUSTOMERSTATETIMERMAX;
                        break;
                    case CustomerState.Deactivating:
                        // send any packed items via post office
                        if (packingItems.Length > 0)
                        {
                            pom.SendPackage("Mr. Sells Alat",
                                currentCustomer.playerData.playerName,
                                packingItems);
                            packingItems = new ItemData[0]; // reset
                            GreenerGameManager ggm = GameObject.FindFirstObjectByType<GreenerGameManager>();
                            if (ggm != null)
                                ggm.AddNotification("Your package is has been\nsent to your mailbox.");
                        }
                        // handle customer as leaving
                        if (currentCustomer != null)
                        {
                            leavingCustomer = currentCustomer;
                            currentCustomer = null;
                        }
                        // remain in this state until leaving customer not detected
                        playerCheckTimer = PLAYERCHECKTIME;
                        customerStateTimer = CUSTOMERSTATETIMERMAX;
                        qoe.enabled = true;
                        break;
                    default:
                        Debug.LogWarning("--- MarketManager [HandleCustomerStates] : customer state undefined. will ignore.");
                        break;
                }
            }
        }
    }

    void RunMarketStateTimer()
    {
        if (marketStateTimer > 0f)
        {
            marketStateTimer -= Time.deltaTime;
            if (marketStateTimer < (MARKETSTATETIMERMAX / 2f))
            {
                // configure market background images between overlay fades
                switch (marketState)
                {
                    case MarketState.Default:
                        break;
                    case MarketState.StandardMarket:
                        if (!fadingFromBlack)
                        {
                            if (currentBackground == null && marketBG != null)
                                currentBackground = marketBG;
                            if (currentBackground == null)
                                currentBackground = Texture2D.whiteTexture; // TEMP
                            currentNPCFrame = 0;
                            npcFrameTimer = MARKETNPCFRAMETIME;
                        }
                        break;
                    case MarketState.Exiting:
                        if (!fadingFromBlack)
                            currentBackground = null;
                        break;
                }
                fadingFromBlack = true;
            }
            if (marketStateTimer < 0f)
            {
                marketStateTimer = 0f;
                fadingFromBlack = false;
                // handle market state changes
                switch (marketState)
                {
                    case MarketState.Default:
                        // we should never be here
                        break;
                    case MarketState.StandardMarket:
                        fadingOverlay = false;
                        break;
                    case MarketState.Exiting:
                        customerStateTimer = (CUSTOMERSTATETIMERMAX/2f); // exit faster
                        marketState = MarketState.Default;
                        marketDisplay = false;
                        fadingOverlay = false;
                        break;
                    default:
                        Debug.LogWarning("--- MarketManager [RunMarketStateTimer] : market state undefined. will ignore.");
                        break;
                }
            }
        }
    }

    void HandleMarketStates()
    {
        switch (marketState)
        {
            case MarketState.Default:
                // we should never be here
                break;
            case MarketState.StandardMarket:
                PlayerControlManager.PlayerActions pa = currentCustomer.GetPlayerActions();
                // allow player to enter buy mode
                CheckBuyMode(pa);
                // allow player to sell inventory item
                CheckSellMode(pa);
                break;
            case MarketState.Exiting:
                break;
        }
    }

    void RunMarketNPCFrameTimer()
    {
        if (npcFrameTimer > 0f)
        {
            npcFrameTimer -= Time.deltaTime;
            if (npcFrameTimer < 0f)
            {
                npcFrameTimer = RandomSystem.GaussianRandom01() * MARKETNPCFRAMETIME;
                currentNPCFrame = Mathf.RoundToInt(RandomSystem.FlatRandom01() * 3);
                currentNPCFrame = Mathf.Clamp(currentNPCFrame, 0, 2);
            }
        }
    }

    void CheckBuyMode( PlayerControlManager.PlayerActions pa )
    {
        if (customerMode == CustomerMode.Default && pa.actionADown)
        {
            customerMode = CustomerMode.Buy;
            currentCustomer.characterFrozen = true;
            //menuItemSelection = 0;
            marketInstructions = "- BUY MODE -\nE=BUY V=EXIT";
            if (padMgr != null && padMgr.gamepads[0].isActive)
                marketInstructions = "- BUY MODE -\nA=BUY Y=EXIT";
            return; // consume input, do not allow purchase with current actionA signal
        }
        if (customerMode == CustomerMode.Buy)
        {
            // allow player to select item on menu
            if (Input.GetKeyDown(currentCustomer.upKey) || 
                (padMgr != null && padMgr.gamepads[0].isActive &&
                    padMgr.gPadDown[0].YaxisL > 0f))
            {
                menuItemSelection--;
                rejectFlashTimer = 0f;
            }
            if (Input.GetKeyDown(currentCustomer.downKey) || 
                (padMgr != null && padMgr.gamepads[0].isActive && 
                    padMgr.gPadDown[0].YaxisL < 0f))
            {
                menuItemSelection++;
                rejectFlashTimer = 0f;
            }
            int maxMenuList = menuItems.Length - 1;
            maxMenuList = maxMenuListPerLevel[Mathf.Clamp(currentCustomer.playerData.level,0,9)];
            menuItemSelection = Mathf.Clamp(menuItemSelection, 0, maxMenuList);
            // set top of menu list
            if (menuItemSelection < topOfMenuList)
                topOfMenuList = menuItemSelection;
            if (menuItemSelection > topOfMenuList + MENUITEMSINLIST)
                topOfMenuList = menuItemSelection - MENUITEMSINLIST;
            // detect player coupon is selected in inventory
            ItemData coupon = currentCustomer.GetPlayerCurrentItemSelection();
            discountBuy = 1f; // 100% purchase price (no discount by default)
            if (coupon != null && coupon.type == ItemType.Coupon)
            {
                discountBuy -= coupon.quality; // coupon quality represents % off price
                discountBuy = Mathf.Clamp01(discountBuy);
            }
            // continue to lower prices (increase discount) if player has gilded words I or II
            if (PlayerSystem.PlayerHasEffect(currentCustomer.playerData, PlayerEffect.SpellGildedWordsI))
                discountBuy = Mathf.Clamp01(discountBuy - .25f); // 25% off
            if (PlayerSystem.PlayerHasEffect(currentCustomer.playerData, PlayerEffect.SpellGildedWordsII))
                discountBuy = Mathf.Clamp01(discountBuy - .5f); // 50% off
            // allow player to buy item
            if (Input.GetKeyDown(currentCustomer.actionAKey) || 
                (padMgr != null && padMgr.gamepads[0].isActive && 
                    padMgr.gPadDown[0].aButton))
            {
                if (menuItems[menuItemSelection].availableToBuy && 
                    menuItems[menuItemSelection].buyItemValue <= currentCustomer.playerData.gold)
                {
                    if (discountBuy < 1f)
                    {
                        currentCustomer.playerData.gold -= Mathf.RoundToInt(menuItems[menuItemSelection].buyItemValue * discountBuy);
                        // take coupon
                        if (coupon != null && coupon.type == ItemType.Coupon)
                            currentCustomer.playerData.inventory = InventorySystem.RemoveItemFromInventory(currentCustomer.playerData.inventory, currentCustomer.GetPlayerCurrentItemSelection() );
                    }
                    else
                        currentCustomer.playerData.gold -= menuItems[menuItemSelection].buyItemValue;
                    currentCustomer.AwardXP(PlayerData.XP_BUYFROMSHOP);

                    // try place in inventory, (do not spawn to the side)
                    if (InventorySystem.InvHasSlot(currentCustomer.playerData.inventory))
                    {
                        ItemData iData = InventorySystem.InitializeItem(menuItems[menuItemSelection].itemType);
                        if (iData == null)
                            Debug.LogWarning("--- MarketManager [CheckBuyMode] : unable to initialize item. will ignore.");
                        else
                        {
                            iData.plant = menuItems[menuItemSelection].plantType;
                            if (menuItems[menuItemSelection].itemType == ItemType.Seed ||
                                menuItems[menuItemSelection].itemType == ItemType.Fruit ||
                                menuItems[menuItemSelection].itemType == ItemType.Plant)
                            {
                                PlantType p = iData.plant;
                                iData.name += " (" + p.ToString() + ")";
                            }
                            else if (menuItems[menuItemSelection].itemType == ItemType.Scroll ||
                                menuItems[menuItemSelection].itemType == ItemType.Potion)
                            {
                                // magic item config
                                iData.name = menuItems[menuItemSelection].itemName.Replace("\n", " ");
                                iData.effects = new ItemEffect[1];
                                iData.effects[0] = menuItems[menuItemSelection].effect;
                            }
                        }
                        currentCustomer.playerData.inventory = InventorySystem.AddToInventory(currentCustomer.playerData.inventory, iData);
                    }
                    else
                    {
                        // gather items bought with no inventory room
                        // upon exiting market, send package to player mailbox

                        // add to packing items
                        ItemData[] tmp = new ItemData[packingItems.Length + 1];
                        for (int i = 0; i < packingItems.Length; i++)
                        {
                            tmp[i] = packingItems[i];
                        }
                        tmp[packingItems.Length] = InventorySystem.InitializeItem(menuItems[menuItemSelection].itemType);
                        if (menuItems[menuItemSelection].plantType != PlantType.Default)
                        {
                            // plant config
                            PlantData pData = PlantSystem.InitializePlant(menuItems[menuItemSelection].plantType);
                            tmp[packingItems.Length] = InventorySystem.SetItemAsPlant(tmp[packingItems.Length], pData);
                        }
                        if (menuItems[menuItemSelection].itemType == ItemType.Scroll ||
                            menuItems[menuItemSelection].itemType == ItemType.Potion)
                        {
                            // magic item config
                            tmp[packingItems.Length].name = menuItems[menuItemSelection].itemName;
                            tmp[packingItems.Length].effects = new ItemEffect[1];
                            tmp[packingItems.Length].effects[0] = menuItems[menuItemSelection].effect;
                        }
                        packingItems = tmp;
                        // notify about packed items
                        GreenerGameManager ggm = GameObject.FindFirstObjectByType<GreenerGameManager>();
                        if (ggm != null)
                            ggm.AddNotification(packingItems[packingItems.Length-1].name + "\nis packed for shipping.");
                    }
                }
                else
                    rejectFlashTimer = REJECTFLASHTIME;
            }
            // allow customer to exit buy mode
            if (Input.GetKeyDown(currentCustomer.actionDKey) || 
                (padMgr != null && padMgr.gamepads[0].isActive && 
                    padMgr.gPadDown[0].yButton))
            {
                customerMode = CustomerMode.Default;
                //currentCustomer.characterFrozen = false;
                menuItemSelection = -1;
                marketInstructions = "Welcome, Biomancer!\nE=BUY F=SELL";
                if (padMgr != null && padMgr.gamepads[0].isActive)
                    marketInstructions = "Welcome, Biomancer!\nA=BUY B=SELL";
            }
        }
    }

    void CheckSellMode( PlayerControlManager.PlayerActions pa )
    {
        if (customerMode == CustomerMode.Default && pa.actionBDown)
        {
            customerMode = CustomerMode.Sell;
            //currentCustomer.characterFrozen = true;
            menuItemSelection = -1;
            marketInstructions = "- SELL MODE -\nE=SELL V=EXIT";
            if (padMgr != null && padMgr.gamepads[0].isActive)
                marketInstructions = "- SELL MODE -\nA=SELL Y=EXIT";
            return; // consume input of actionB signal
        }
        if (customerMode == CustomerMode.Sell)
        {
            // find item on menu matching player selection and display sell value
            ItemData iData = currentCustomer.GetPlayerCurrentItemSelection();
            bool found = false;
            int value = 0;
            topOfMenuList = 0;
            menuItemSelection = -1;
            // NOTE: this is here until leveling tweaked
            int maxMenuList = maxMenuListPerLevel[Mathf.Clamp(currentCustomer.playerData.level,0,9)];
            if (iData != null)
            {
                // cannot sell fertilizer (or 'default' type item)
                if ( (int)iData.type > 1 )
                {
                    for (int i = 0; i < menuItems.Length; i++)
                    {
                        if (menuItems[i].itemType == iData.type &&
                            menuItems[i].plantType == iData.plant)
                        {
                            // use item quality in determining final sell value
                            playerItemFinalSellValue = GetFinalMarketSellValue(iData);
                            // display final sell value and use as amount to give player
                            value = playerItemFinalSellValue;
                            found = true;
                            menuItemSelection = i;
                            topOfMenuList = i;
                            if (topOfMenuList > menuItems.Length - MENUITEMSINLIST)
                                topOfMenuList = menuItems.Length - MENUITEMSINLIST;
                            
                            if (topOfMenuList > maxMenuList - MENUITEMSINLIST)
                                topOfMenuList = maxMenuList - MENUITEMSINLIST;
                            break;
                        }
                    }
                }
            }
            // allow customer to sell selected item
            if (Input.GetKeyDown(currentCustomer.actionAKey) || 
                (padMgr != null && padMgr.gamepads[0].isActive && 
                    padMgr.gPadDown[0].aButton))
            {
                // cannot sell fertilizer (or 'default' type item)
                if (iData != null && (int)iData.type > 1)
                {
                    if (found)
                    {
                        currentCustomer.playerData.gold += value;
                        // ARCANA SKILL : Friends of the Merchant
                        if (PlayerSystem.PlayerHasEffect(currentCustomer.playerData, PlayerEffect.SkillFriendsMerchant) &&
                            !InventorySystem.InvHasItemOfType(currentCustomer.playerData.inventory, ItemType.Coupon) &&
                            RandomSystem.FlatRandom01() < .05f)
                        {
                            // REVIEW: should just add to inventory after deleting sold item?
                            // replace sold item with coupon
                            ItemData coupon = InventorySystem.InitializeItem(ItemType.Coupon);
                            coupon.quality = 1f; // 100% off one item
                            currentCustomer.playerData.inventory.items[currentCustomer.GetPlayerCurrentItemSelectionIndex()] = coupon;
                            // notify coupon reward
                            GreenerGameManager ggm = GameObject.FindFirstObjectByType<GreenerGameManager>();
                            if (ggm != null)
                                ggm.AddNotification("You received a coupon!\n100% off one item!");
                        }
                        else
                            currentCustomer.DeleteCurrentItemSelection();
                        // PLAYER STATS:
                        currentCustomer.playerData.stats.totalGoldEarned += value;
                        currentCustomer.AwardXP(PlayerData.XP_SELLTOSHOP);
                        // reset final sell value
                        playerItemFinalSellValue = 0;
                    }
                }
            }
            // allow customer to exit sell mode
            if (Input.GetKeyDown(currentCustomer.actionDKey) || 
                (padMgr != null && padMgr.gamepads[0].isActive && 
                    padMgr.gPadDown[0].yButton))
            {
                customerMode = CustomerMode.Default;
                //currentCustomer.characterFrozen = false;
                menuItemSelection = -1;
                marketInstructions = "Welcome, Biomancer!\nE=BUY F=SELL";
                if (padMgr != null && padMgr.gamepads[0].isActive)
                    marketInstructions = "Welcome, Biomancer!\nA=BUY B=SELL";
                playerItemFinalSellValue = 0;
            }
        }
    }

    /// <summary>
    /// Returns the market buy price of given item type and give plant type
    /// </summary>
    /// <param name="iType">item type</param>
    /// <param name="pType">plant type</param>
    /// <returns>market price</returns>
    public int GetMarketBuyPrice( ItemType iType, PlantType pType )
    {
        int retInt = 0;

        if (iType == ItemType.Default)
            return retInt;

        bool found = false;

        for (int i = 0; i < menuItems.Length; i++)
        {
            if (menuItems[i].itemType == iType && menuItems[i].plantType == pType )
            {
                retInt = menuItems[i].buyItemValue;
                found = true;
                break;
            }
        }

        if (!found)
            Debug.LogWarning("--- MarketManager [GetMarketBuyPrice] : sell price not found for item '" + iType + "' and plant type '" + pType + "'. will return value of zero.");

        return retInt;
    }

    /// <summary>
    /// Returns market sell value of given item type and given plant type (does not factor in quality, see GetFinalMarketSellPrice).
    /// </summary>
    /// <param name="iType">item type</param>
    /// <param name="pType">plant type</param>
    /// <returns>market sell value</returns>
    public int GetMarketSellValue( ItemType iType, PlantType pType )
    {
        int retInt = 0;

        if (iType == ItemType.Default)
            return retInt;

        bool found = false;

        for (int i = 0; i < menuItems.Length; i++)
        {
            if (menuItems[i].itemType == iType && menuItems[i].plantType == pType)
            {
                retInt = menuItems[i].sellItemValue;
                found = true;
                break;
            }
        }

        if (!found)
            Debug.LogWarning("--- MarketManager [GetMarketSellValue] : sell price not found for item '" + iType + "' and plant type '" + pType + "'. will return value of zero.");

        return retInt;
    }

    /// <summary>
    /// Returns final sell value of given specific item data taking factors like quality into account
    /// </summary>
    /// <param name="item">specific item data</param>
    /// <returns>final sell value, or zero if not found on menu</returns>
    public int GetFinalMarketSellValue( ItemData item )
    {
        if (item == null || item.type == ItemType.Default)
            return 0;

        return GetAdjustedSellValue(item, GetMarketSellValue(item.type, item.plant));
    }

    int GetAdjustedSellValue( ItemData item, int baseValue )
    {
        int retInt = baseValue;

        if (item == null || item.type == ItemType.Default)
            return retInt;

        // amount of quality ding
        float qualityReduction = 1f - item.quality;
        // quality factor
        int qualityDing = Mathf.RoundToInt(baseValue * qualityReduction);
        retInt -= qualityDing;
        // REVIEW: other factors

        return retInt;
    }

    MenuItem SetMenuItems( ItemType iType, PlantType pType, int buy, int sell)
    {
        MenuItem retMenuItem = new MenuItem();

        if (iType == ItemType.Seed || iType == ItemType.Fruit || iType == ItemType.Plant)
        {
            PlantData pData = PlantSystem.InitializePlant(pType);
            retMenuItem.itemName = iType.ToString() + " (" + pData.plantName + ")";
        }
        else
            retMenuItem.itemName = iType.ToString();

        retMenuItem.itemType = iType;
        retMenuItem.plantType = pType;
        retMenuItem.availableToBuy = true;
        retMenuItem.buyItemValue = buy;
        retMenuItem.sellItemValue = sell;

        return retMenuItem;
    }

    void SetAllWholePlantPrices()
    {
        // perform after seed and fruit have been priced
        for (int i=0; i < menuItems.Length; i++)
        {
            if (menuItems[i].itemType == ItemType.Plant)
            {
                PlantData pData = PlantSystem.InitializePlant(menuItems[i].plantType);
                int seedBuy = GetMarketBuyPrice(ItemType.Seed, pData.type);
                int fruitBuy = GetMarketBuyPrice(ItemType.Fruit, pData.type);
                // 
                menuItems[i].buyItemValue = seedBuy + (pData.harvestAmount * fruitBuy) + 1;
                if (pData.canReFruit)
                    menuItems[i].buyItemValue *= 2;
                menuItems[i].sellItemValue = menuItems[i].buyItemValue + seedBuy;
            }
        }
    }

    void SetMenuItemAvailable( ItemType iType, PlantType pType, bool available )
    {
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (menuItems[i].itemType == iType && menuItems[i].plantType == pType)
            {
                menuItems[i].availableToBuy = available;
                break;
            }
        }
    }

    void SetLimitedItemAvailability()
    {
        // popcorn seed and plant
        SetMenuItemAvailable(ItemType.Seed, PlantType.Popcorn, false);
        SetMenuItemAvailable(ItemType.Plant, PlantType.Popcorn, false);
    }

    void SetMenuItemEffect( MenuItem mItem, ItemEffect iEffect, string itemNameSuffix )
    {
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (menuItems[i] == mItem)
            {
                menuItems[i].effect = iEffect;
                menuItems[i].itemName += itemNameSuffix;
                break;
            }
        }
    }

    void InitializeMenu()
    {
        menuItems = new MenuItem[generalItems + (plantItemTypes *
            (commonPlants + uncommonPlants + rarePlants + 
            specialPlants + uniquePlants + 1))];

        int idx = 0;

        // -- GENERAL ITEMS --

        menuItems[idx] = SetMenuItems(ItemType.Scroll, PlantType.Default, 100, 50);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.ScrollRandomSpellCharge, "\n(Unknown)");
        menuItems[idx] = SetMenuItems(ItemType.Scroll, PlantType.Default, 50, 25);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.ScrollLevelOneSpellCharge, "\n(Level One)");
        menuItems[idx] = SetMenuItems(ItemType.Scroll, PlantType.Default, 65, 32);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.ScrollLevelTwoSpellCharge, "\n(Level Two)");
        menuItems[idx] = SetMenuItems(ItemType.Scroll, PlantType.Default, 80, 40);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.ScrollLevelThreeSpellCharge, "\n(Level Three)");
        menuItems[idx] = SetMenuItems(ItemType.Scroll, PlantType.Default, 100, 50);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.ScrollRandomSpellCharge, "\n(Level Four)");
        menuItems[idx] = SetMenuItems(ItemType.Scroll, PlantType.Default, 120, 60);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.ScrollRandomSpellCharge, "\n(Level Five)");
        menuItems[idx] = SetMenuItems(ItemType.Scroll, PlantType.Default, 150, 75);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.ScrollRandomSpellCharge, "\n(Level Six)");
        menuItems[idx] = SetMenuItems(ItemType.Scroll, PlantType.Default, 200, 100);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.ScrollRandomSpellCharge, "\n(Level Seven)");

        menuItems[idx] = SetMenuItems(ItemType.Scroll, PlantType.Default, 60, 35);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.ScrollMirrorMirror, "\n(Mirror Mirror)");
        menuItems[idx] = SetMenuItems(ItemType.Scroll, PlantType.Default, 75, 42);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.ScrollColorTrailI, "\n(Color Trail I)");
        menuItems[idx] = SetMenuItems(ItemType.Scroll, PlantType.Default, 75, 42);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.ScrollColorTrailII, "\n(Color Trail II)");
        menuItems[idx] = SetMenuItems(ItemType.Scroll, PlantType.Default, 75, 42);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.ScrollColorTrailIII, "\n(Color Trail III)");
        menuItems[idx] = SetMenuItems(ItemType.Scroll, PlantType.Default, 90, 50);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.ScrollSplaturn, "\n(Splaturn)");
        menuItems[idx] = SetMenuItems(ItemType.Scroll, PlantType.Default, 110, 60);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.ScrollStarbloomBurst, "\n(Starbloom Burst)");
        menuItems[idx] = SetMenuItems(ItemType.Scroll, PlantType.Default, 125, 75);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.ScrollFogOfWar, "\n(Fog Of War)");

        menuItems[idx] = SetMenuItems(ItemType.Potion, PlantType.Default, 30, 20);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.PotionClearOneCooldown, " (Grey)");
        menuItems[idx] = SetMenuItems(ItemType.Potion, PlantType.Default, 75, 50);
        SetMenuItemEffect(menuItems[idx++], ItemEffect.PotionClearAllCooldowns, " (White)");

        menuItems[idx++] = SetMenuItems(ItemType.Fertilizer, PlantType.Default, 1, 0);

        // -- COMMON PLANTS --

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Corn, 3, 2);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Corn, 7, 6);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Corn, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Tomato, 4, 3);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Tomato, 9, 8);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Tomato, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Carrot, 5, 4);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Carrot, 11, 10);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Carrot, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Poppy, 5, 4);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Poppy, 11, 10);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Poppy, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Rose, 5, 4);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Rose, 11, 10);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Rose, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Sunflower, 5, 4);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Sunflower, 11, 10);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Sunflower, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Moonflower, 5, 4);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Moonflower, 11, 10);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Moonflower, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Apple, 5, 4);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Apple, 11, 10);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Apple, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Orange, 5, 4);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Orange, 11, 10);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Orange, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Lemon, 5, 4);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Lemon, 11, 10);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Lemon, 0, 0);

        // -- UNCOMMON PLANTS --

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Lotus, 7, 6);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Lotus, 16, 15);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Lotus, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Marigold, 6, 5);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Marigold, 15, 14);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Marigold, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Magnolia, 5, 4);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Magnolia, 11, 10);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Magnolia, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Myosotis, 8, 6);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Myosotis, 17, 15);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Myosotis, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Chrystalia, 7, 5);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Chrystalia, 17, 15);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Chrystalia, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Pumpkin, 7, 6);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Pumpkin, 16, 15);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Pumpkin, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Underbloom, 7, 6);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Underbloom, 15, 14);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Underbloom, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.WaterLily, 8, 5);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.WaterLily, 15, 12);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.WaterLily, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Snowgrace, 7, 6);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Snowgrace, 15, 14);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Snowgrace, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Popcorn, 7, 6);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Popcorn, 15, 14);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Popcorn, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.EclipseFlower, 6, 4);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.EclipseFlower, 17, 16);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.EclipseFlower, 0, 0);

        // -- RARE PLANTS --

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.GoldenApple, 9, 8);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.GoldenApple, 21, 19);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.GoldenApple, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Hollowbloom, 6, 5);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Hollowbloom, 15, 14);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Hollowbloom, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Mandrake, 10, 8);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Mandrake, 21, 18);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Mandrake, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.FrostLily, 10, 9);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.FrostLily, 21, 20);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.FrostLily, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Banana, 9, 8);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Banana, 23, 19);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Banana, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Coconut, 9, 8);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Coconut, 22, 21);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Coconut, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Mysteria, 9, 8);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Mysteria, 23, 21);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Mysteria, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Nightshade, 10, 9);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Nightshade, 23, 21);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Nightshade, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.CrystalRose, 10, 9);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.CrystalRose, 22, 21);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.CrystalRose, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Yarrow, 9, 8);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Yarrow, 23, 19);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Yarrow, 0, 0);

        // -- SPECIAL PLANTS --

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Dragonroot, 15, 12);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Dragonroot, 32, 30);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Dragonroot, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.WinterRose, 15, 12);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.WinterRose, 32, 30);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.WinterRose, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.FleurDeLis, 14, 11);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.FleurDeLis, 32, 31);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.FleurDeLis, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Tropicus, 14, 11);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Tropicus, 32, 31);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Tropicus, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.MourningNyx, 14, 11);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.MourningNyx, 34, 31);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.MourningNyx, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.BlastApple, 16, 11);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.BlastApple, 33, 30);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.BlastApple, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.PixiePlumeria, 16, 13);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.PixiePlumeria, 33, 31);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.PixiePlumeria, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.FaeFoxglove, 15, 11);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.FaeFoxglove, 32, 31);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.FaeFoxglove, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.DruidsLotus, 15, 13);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.DruidsLotus, 33, 31);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.DruidsLotus, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.SplatBerry, 18, 16);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.SplatBerry, 40, 35);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.SplatBerry, 0, 0);

        // -- UNIQUE PLANTS --

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Jazzmyne, 25, 20);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Jazzmyne, 38, 35);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Jazzmyne, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.Mashroom, 27, 23);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.Mashroom, 40, 33);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.Mashroom, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.HerbalPert, 30, 27);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.HerbalPert, 42, 37);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.HerbalPert, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.FireflyTrap, 32, 30);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.FireflyTrap, 48, 42);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.FireflyTrap, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.BettingHedge, 36, 32);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.BettingHedge, 50, 46);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.BettingHedge, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.BawnSigh, 38, 35);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.BawnSigh, 53, 48);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.BawnSigh, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.HerbalPert, 40, 38);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.HerbalPert, 55, 50);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.HerbalPert, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.WillowWisp, 42, 40);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.WillowWisp, 57, 52);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.WillowWisp, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.WalkingStick, 45, 42);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.WalkingStick, 60, 55);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.WalkingStick, 0, 0);

        menuItems[idx++] = SetMenuItems(ItemType.Seed, PlantType.GenesisSapling, 75, 70);
        menuItems[idx++] = SetMenuItems(ItemType.Fruit, PlantType.GenesisSapling, 100, 90);
        menuItems[idx++] = SetMenuItems(ItemType.Plant, PlantType.GenesisSapling, 0, 0);

        // add icon art to all menu items
        for (int i = 0; i < (idx-1); i++)
        {
            menuItems[i].itemIcon = alm.GetImageList(alm.GetArtData(menuItems[i].itemType, menuItems[i].plantType))[0];
        }

        // default availability
        for (int i = 0; i < (idx - 1); i++)
        {
            menuItems[i].availableToBuy = true;
        }
        // limited item availability
        SetLimitedItemAvailability();

        // whole plants given prices based on formula
        SetAllWholePlantPrices();
    }

    void OnGUI()
    {
        if (!marketDisplay)
            return;

        // TODO: de-conflict HUD layout between market UI overlay and player inventory HUD + gold display
        // (they need to sell and more)

        // handle player tag display
        currentCustomer.hidePlayerNameTag = true;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        GUIStyle g = new GUIStyle(GUI.skin.label);
        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.white;
        string s = "";

        r.x = 0f;
        r.y = 0f;
        r.width = w;
        r.height = h;

        // crafting background image appears halfway through overlay fading
        if (currentBackground != null)
        {
            c = Color.white;
            GUI.color = c;
            // REVIEW: we could alter background for different market stalls
            if (marketState == MarketState.StandardMarket || marketState == MarketState.Exiting)
            {
                t = marketBG;
                GUI.DrawTexture(r, t); // market bg
                t = marketNPCFrames[currentNPCFrame];
                GUI.DrawTexture(r, t); // market npc
                t = marketBookOL;
                GUI.DrawTexture(r, t); // market book OL
                t = marketUIOL;
                GUI.DrawTexture(r, t); // market UI OL
            }
        }

        // handle fading to and from black for market state transitions
        if (fadingOverlay)
        {
            t = Texture2D.whiteTexture;
            c = Color.black;
            if (fadingFromBlack)
                c.a = ((marketStateTimer * 2f) / MARKETSTATETIMERMAX);
            else
                c.a = 1f - (((marketStateTimer * 2f) / MARKETSTATETIMERMAX) - 1f);
            GUI.color = c;
            GUI.DrawTexture(r, t);
            // if fading overlay, no other display
            return;
        }

        if (marketStateTimer > 0f)
            return;

        c = Color.white;
        GUI.color = c;

        // legacy market UI
        // TODO: revise layout to match market bg art

        r.x = 0.375f * w;
        r.y = 0.08f * h;
        r.width = 0.2522f * w;
        r.height = 0.1f * h;

        // draw bg
        t = Texture2D.whiteTexture;
        c = Color.black;
        c *= 0.1f;
        c.g = 0.381f;
        c.a = 0.381f;
        GUI.color = c;
        GUI.DrawTexture(r, t);

        g.fontStyle = FontStyle.Bold;
        g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));

        r.x = 0.375f * w;
        r.y = 0.08f * h;
        r.width = 0.2522f * w;
        r.height = 0.1f * h;
        //r.x = 0.1f * w;
        //r.y = 0.165f * h;
        //r.width = 0.305f * w;
        //r.height = 0.1f * h;
        g.alignment = TextAnchor.MiddleCenter;
        s = marketInstructions;
        r.x += 0.0007f * w;
        r.y += 0.0009f * w;
        GUI.color = Color.black;
        GUI.Label(r, s, g);
        r.x -= 0.0014f * w;
        r.y -= 0.0018f * w;
        GUI.color = Color.white;
        GUI.Label(r, s, g);

        r.y = 0.2875f * h;
        r.height = 0.125f * h;

        for (int i = 0; i < menuItems.Length; i++)
        {
            r.x = 0.1f * w;
            r.width = 0.305f * w;
            g.fontSize = Mathf.RoundToInt(18 * (w / 1024f));

            if (i < topOfMenuList || i > topOfMenuList + MENUITEMSINLIST)
                continue;
            if (i > maxMenuListPerLevel[Mathf.Clamp(currentCustomer.playerData.level, 0, 9)])
                continue;

            c = new Color(0.381f, 0.381f, 0.381f, 0.618f);
            if (i == menuItemSelection)
            {
                c = new Color(0.25f, 0.6f, 0.2f, 0.618f);
                if (rejectFlashTimer > 0f)
                    c.g = ((rejectFlashTimer * 5f) % 1f ) * .6f;
            }

            // Item Background
            GUI.color = c;
            GUI.DrawTexture(r, t);

            // Display Name
            r.x = 0.175f * w;
            r.width = 0.19f * w;
            s = menuItems[i].itemName;
            if (customerMode == CustomerMode.Sell && menuItemSelection == i &&
                currentCustomer.GetPlayerCurrentItemSelection() != null)
                s = menuItems[i].itemName + "\nQuality "+(Mathf.RoundToInt(currentCustomer.GetPlayerCurrentItemSelection().quality * 1000f)/10f)+"%";
            g.alignment = TextAnchor.MiddleLeft;
            g.wordWrap = true;
            if (menuItems[i].availableToBuy)
                GUI.color = Color.white;
            else
                GUI.color = Color.gray;
            GUI.Label(r, s, g);

            // Icon (placed above)
            r.x = .1025f * w;
            r.width = r.height;
            GUI.DrawTexture(r, menuItems[i].itemIcon);

            // Market Value
            g.fontSize = Mathf.RoundToInt(g.fontSize * 1.5f);
            g.alignment = TextAnchor.MiddleRight;
            // not dollars, currency is gold
            // and items have an individual sell value
            r.x = .2875f * w;
            r.width = 0.1f * w;
            if (menuItems[i].availableToBuy)
            {
                // buy value
                if (discountBuy < 1f)
                    s = Mathf.RoundToInt(menuItems[i].buyItemValue * discountBuy ).ToString();
                else
                    s = menuItems[i].buyItemValue.ToString();
            }
            else
                s = "--";
            if (customerMode == CustomerMode.Sell)
            {
                if (i == menuItemSelection)
                    s = playerItemFinalSellValue.ToString(); // starting with item sell value taking quality into account
                else
                    s = menuItems[i].sellItemValue.ToString(); // normal sell value displayed for other items
            }
            GUI.Label(r, s, g);

            if( !menuItems[i].availableToBuy )
            {
                // 'Not Available' overlay
                r.x = 0.175f * w;
                r.width = 0.19f * w;
                g.fontStyle = FontStyle.Italic;
                GUI.color = Color.white;
                GUI.Label(r, "Not Available", g);
                g.fontStyle = FontStyle.Bold;
            }

            r.y += 0.175f * h;
        }

        // -- special player HUD just for market --
        ItemData iData = null;
        if (currentCustomer != null)
            iData = currentCustomer.GetPlayerCurrentItemSelection();
        InventoryData pInv = currentCustomer.playerData.inventory;
        // inventory display
        r.x = 0.225f * w;
        r.y = 0.8325f * h;
        r.width = 0.05f * w;
        r.height = r.width;

        r.x -= (0.05f * w) * ((pInv.maxSlots / 2f) + 0.5f);
        for (int i = 0; i < 5; i++)
        {
            r.x += 0.05f * w;
            if (pInv.items != null && pInv.items.Length > i)
            {
                if (pInv.items[i].type != ItemType.Default)
                {
                    // adjust smaller
                    r.x += 0.005f * w;
                    r.y += (0.005f * w);
                    r.width -= (0.01f * w);
                    r.height -= (0.01f * w);
                    // draw inventory item
                    t = alm.itemImages[alm.GetArtData(pInv.items[i].type, pInv.items[i].plant).artIndexBase];
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
            if (i == currentCustomer.GetPlayerCurrentItemSelectionIndex())
                c = Color.yellow;
            GUI.color = c;
            GUI.DrawTexture(r, t);
            GUI.color = Color.white;
        }

        // selected item label
        r.x = 0.125f * w;
        r.y = 0.9225f * h;
        r.width = 0.25f * w;
        r.height = 0.05f * h;
        // label bg
        t = Texture2D.whiteTexture;
        c = Color.white;
        c.r = .1f;
        c.g = .1f;
        c.b = .1f;
        c.a = 0.25f;
        GUI.color = c;
        GUI.DrawTexture(r, t);
        // label
        GUI.color = Color.white;
        g = new GUIStyle(GUI.skin.label);
        g.alignment = TextAnchor.MiddleCenter;
        g.fontSize = Mathf.RoundToInt(20f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        s = "";
        if (iData != null)
            s = iData.name;
        r.x += 0.0005f * w;
        r.y += 0.0008f * w;
        GUI.color = Color.black;
        GUI.Label(r, s, g);
        r.x -= 0.001f * w;
        r.y -= 0.0016f * w;
        GUI.color = Color.white;
        GUI.Label(r, s, g);

        // gold display
        r.x = 0.025f * w;
        r.y = 0.85f * h;
        r.width = 0.125f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.label);
        g.alignment = TextAnchor.MiddleLeft;
        g.fontSize = Mathf.RoundToInt(16f * (w / 1024f));
        g.fontStyle = FontStyle.Bold;
        s = "GOLD: ";
        s += currentCustomer.playerData.gold.ToString();
        r.x += 0.0006f * w;
        r.y += 0.001f * h;
        GUI.color = Color.black;
        GUI.Label(r, s, g);
        r.x -= 0.0012f * w;
        r.y -= 0.002f * h;
        GUI.color = Color.yellow;
        GUI.Label(r, s, g);
        GUI.color = Color.white;


        // exit market button
        r.x = 0.4f * w;
        r.y = 0.9f * h;
        r.width = 0.2f * w;
        r.height = 0.05f * h;
        g = new GUIStyle(GUI.skin.button);
        if (padMgr != null && padMgr.gamepads[0].isActive)
            g.fontSize = Mathf.RoundToInt(12 * (w / 1024f));
        else
            g.fontSize = Mathf.RoundToInt(16 * (w / 1024f));
        g.normal.textColor = Color.white;
        g.hover.textColor = Color.yellow;
        g.active.textColor = Color.white;
        if (!Application.isEditor)
        {
            g.normal.background = buttonTex[0];
            g.hover.background = buttonTex[1];
            g.active.background = buttonTex[2];
        }
        s = "EXIT MARKET";
        if (padMgr != null && padMgr.gamepads[0].isActive)
            s += "\n[BACK BUTTON]";

        GUI.enabled = (customerMode == CustomerMode.Default);
        if (GUI.Button(r,s,g) || (padMgr != null &&
            padMgr.gamepads[0].isActive && padMgr.gPadDown[0].backButton))
        {
            marketState = MarketState.Exiting;
            marketStateTimer = MARKETSTATETIMERMAX;
            fadingOverlay = true;
        }
    }
}
