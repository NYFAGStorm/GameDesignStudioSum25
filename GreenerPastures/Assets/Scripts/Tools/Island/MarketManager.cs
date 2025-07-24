using UnityEngine;

public class MarketManager : MonoBehaviour
{
    // Author: Glenn Storm
    // This handles the market transactions with players

    // REVIEW: we have the ability to set individual items 'available to buy' or not
    // we should consider _not_ offering everything to buy, so rare items are rare
    // For Example:
    // all special fruit and seed not available to buy
    // all rare seed not available to buy
    // while we would need to have another way for players to get these (like grafting)
    // the point would be, the market isn't the 'all too easy' way to get very rare items

    public enum CustomerMode
    {
        Default,
        Buy,
        Sell
    }
    
    [System.Serializable]
    public struct MenuItem
    {
        public string itemName;
        public Texture2D itemIcon;
        public ItemType itemType;
        public PlantType plantType;
        public bool availableToBuy;
        public int buyItemValue;
        public int sellItemValue;
    }
    public MenuItem[] menuItems;

    private MultiGamepad padMgr;

    private string marketInstructions;
    private float playerCheckTimer;
    private PlayerControlManager currentCustomer;
    private int menuItemSelection = -1;
    private int topOfMenuList = 0;
    private CustomerMode customerMode;
    private float rejectFlashTimer;
    private Vector3 purchaseOffset = new Vector3(-1f,0f,0f);

    private float discountBuy;
    private int playerItemFinalBuyValue; // value determined after applying coupon discount
    private int playerItemFinalSellValue; // value determined when selling item, per quality

    private int[] maxMenuListPerLevel = new int[11];
    
    int generalItems = 1; // fertilizer
    int plantItemTypes = 3; // seed, fruit, plant

    int commonPlants = 10;
    int uncommonPlants = 11;
    int rarePlants = 10;
    int specialPlants = 10;
    int uniquePlants = 9;

    private ArtLibraryManager alm;

    const float PLAYERCHECKTIME = 1f;
    const float MARKETPROXIMITYRANGE = .5f;
    const float REJECTFLASHTIME = 1f;
    const int MENUITEMSINLIST = 4;
    const float MENUVERTICALOFFSETPERCENT = 0.06f;


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
        // initialize
        if ( enabled )
        {
            InitializeMenu();
            InitializeMaxMenuList();
            playerCheckTimer = PLAYERCHECKTIME;
            marketInstructions = "MARKET [Welcome]\nE=BUY F=SELL";
            if (padMgr != null && padMgr.gamepads[0].isActive)
                marketInstructions = "MARKET [Welcome]\nA=BUY B=SELL";
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
                case >9: // TODO:
                    maxMenuListPerLevel[i] = generalItems +
                        (commonPlants * plantItemTypes) + 
                        (uncommonPlants * plantItemTypes) +
                        (rarePlants * plantItemTypes) +
                        (specialPlants * plantItemTypes);
                    break;
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

        // run player check timer
        if ( playerCheckTimer > 0f )
        {
            playerCheckTimer -= Time.deltaTime;
            if ( playerCheckTimer < 0f )
            {
                playerCheckTimer = PLAYERCHECKTIME;

                // allow player to 'enter' and 'exit' (proximity)
                PlayerControlManager[] pcms = GameObject.FindObjectsByType<PlayerControlManager>(FindObjectsSortMode.None);
                int found = -1;
                for ( int i=0; i<pcms.Length; i++ )
                {
                    float dist = Vector3.Distance(gameObject.transform.position, pcms[i].gameObject.transform.position);
                    if ( dist < MARKETPROXIMITYRANGE )
                    {
                        found = i;
                        break;
                    }
                }
                if (found > -1)
                    currentCustomer = pcms[found];
                else
                {
                    if (currentCustomer != null )
                    {
                        currentCustomer.characterFrozen = false;
                        currentCustomer.hidePlayerNameTag = false;
                    }
                    currentCustomer = null;
                }
            }
        }

        if (currentCustomer == null)
            return;

        PlayerControlManager.PlayerActions pa = currentCustomer.GetPlayerActions();

        // allow player to enter buy mode
        CheckBuyMode( pa );

        // allow player to sell inventory item
        CheckSellMode( pa );
    }

    void CheckBuyMode( PlayerControlManager.PlayerActions pa )
    {
        if (customerMode == CustomerMode.Default && pa.actionADown)
        {
            customerMode = CustomerMode.Buy;
            currentCustomer.characterFrozen = true;
            menuItemSelection = 0;
            marketInstructions = "MARKET [BUY MODE]\nE=BUY V=EXIT";
            if (padMgr != null && padMgr.gamepads[0].isActive)
                marketInstructions = "MARKET [BUY MODE]\nA=BUY Y=EXIT";
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
                        currentCustomer.playerData.inventory = InventorySystem.RemoveItemFromInventory(currentCustomer.playerData.inventory, currentCustomer.GetPlayerCurrentItemSelection() );
                    }
                    else
                        currentCustomer.playerData.gold -= menuItems[menuItemSelection].buyItemValue;
                    currentCustomer.AwardXP(PlayerData.XP_BUYFROMSHOP);

                    // try place in inventory, spawn to the side if fail
                    if (InventorySystem.InvHasSlot(currentCustomer.playerData.inventory))
                    {
                        ItemData iData = InventorySystem.InitializeItem(menuItems[menuItemSelection].itemType);
                        if (iData == null)
                            Debug.LogWarning("--- MarketManager [CheckBuyMode] : unable to initialize item. will ignore.");
                        else
                        {
                            iData.plant = (PlantType)menuItems[menuItemSelection].plantType;
                            if (menuItems[menuItemSelection].itemType == ItemType.Seed ||
                                menuItems[menuItemSelection].itemType == ItemType.Fruit)
                            {
                                PlantType p = iData.plant;
                                iData.name += " (" + p.ToString() + ")";
                            }
                        }
                        currentCustomer.playerData.inventory = InventorySystem.AddToInventory(currentCustomer.playerData.inventory, iData);
                    }
                    else
                    {
                        Vector3 pos = gameObject.transform.position;
                        pos += purchaseOffset;
                        Vector3 targ = (currentCustomer.transform.position - gameObject.transform.position) * 4f;
                        targ += pos;
                        ItemSpawnManager ism = GameObject.FindFirstObjectByType<ItemSpawnManager>();
                        LooseItemData loose = InventorySystem.CreateItem(menuItems[menuItemSelection].itemType);
                        loose.inv.items[0].plant = (PlantType)menuItems[menuItemSelection].plantType;
                        if (menuItems[menuItemSelection].itemType == ItemType.Seed ||
                            menuItems[menuItemSelection].itemType == ItemType.Fruit)
                        {
                            PlantType p = loose.inv.items[0].plant;
                            loose.inv.items[0].name += " (" + p.ToString() + ")";
                        }
                        ism.SpawnItem(loose, pos, targ, true);
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
                currentCustomer.characterFrozen = false;
                menuItemSelection = -1;
                marketInstructions = "MARKET [Welcome]\nE=BUY F=SELL";
                if (padMgr != null && padMgr.gamepads[0].isActive)
                    marketInstructions = "MARKET [Welcome]\nA=BUY B=SELL";
            }
        }
    }

    void CheckSellMode( PlayerControlManager.PlayerActions pa )
    {
        if (customerMode == CustomerMode.Default && pa.actionBDown)
        {
            customerMode = CustomerMode.Sell;
            currentCustomer.characterFrozen = true;
            menuItemSelection = -1;
            marketInstructions = "MARKET [SELL MODE]\nE=SELL V=EXIT";
            if (padMgr != null && padMgr.gamepads[0].isActive)
                marketInstructions = "MARKET [SELL MODE]\nA=SELL Y=EXIT";
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
                currentCustomer.characterFrozen = false;
                menuItemSelection = -1;
                marketInstructions = "MARKET [Welcome]\nE=BUY F=SELL";
                if (padMgr != null && padMgr.gamepads[0].isActive)
                    marketInstructions = "MARKET [Welcome]\nA=BUY B=SELL";
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

    void InitializeMenu()
    {
        menuItems = new MenuItem[generalItems + (plantItemTypes *
            (commonPlants + uncommonPlants + rarePlants + 
            specialPlants + uniquePlants + 1))];

        int idx = 0;

        // -- GENERAL ITEMS --

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
        if (currentCustomer == null)
            return;

        // handle player tag display
        currentCustomer.hidePlayerNameTag = true;

        Rect r = new Rect();
        float w = Screen.width;
        float h = Screen.height;

        r.x = (w - 0.8f * h) / 2;
        r.y = 0.1f * h + (MENUVERTICALOFFSETPERCENT * h);
        r.width = 0.8f * h;
        r.height = 0.8f * h;

        // draw bg
        Texture2D t = Texture2D.whiteTexture;
        Color c = Color.black;
        c.g = 0.618f;
        c.a = 0.381f;
        GUI.color = c;
        GUI.DrawTexture(r, t);

        GUIStyle g = new GUIStyle(GUI.skin.label);
        g.fontStyle = FontStyle.Bold;
        g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));

        string s = "";

        r.x = (w - 0.8f * h) / 2;
        r.y = 0.1f * h + (MENUVERTICALOFFSETPERCENT * h);
        r.width = 0.8f * h;
        r.height = 0.1f * h;
        g.alignment = TextAnchor.MiddleCenter;
        s = marketInstructions;
        GUI.color = Color.white;
        GUI.Label(r, s, g);

        r.y = 0.2f * h + (MENUVERTICALOFFSETPERCENT * h);
        for (int i = 0; i < menuItems.Length; i++)
        {
            r.x = (w - 0.7f * h) / 2;
            r.width = 0.7f * h;
            r.height = 0.1f * h;
            g.fontSize = Mathf.RoundToInt(20 * (w / 1024f));

            if (i < topOfMenuList || i > topOfMenuList + MENUITEMSINLIST)
                continue;
            if (currentCustomer.playerData.level < 2 && i > 20)
                continue;

            c = new Color(0.3f, 0.3f, 0.3f);
            if (i == menuItemSelection)
            {
                c = new Color(0.25f, 0.6f, 0.2f);
                if (rejectFlashTimer > 0f)
                    c.g = (rejectFlashTimer * 5f) % 1f;
            }

            // Item Background
            GUI.color = c;
            GUI.DrawTexture(r, t);

            r.x = (w - 0.575f * h) * 0.55f;
            r.width = 0.5f * h;

            // Display Name
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

            // Market Value
            g.fontSize = Mathf.RoundToInt(g.fontSize * 1.5f);
            g.alignment = TextAnchor.MiddleRight;

            // Icon (placed above)
            r.x = (w - 0.7f * h) / 2;
            r.width = r.height;
            GUI.DrawTexture(r, menuItems[i].itemIcon);

            // not dollars, currency is gold
            // and items have an individual sell value, per GDD tables
            // (which is part of menu item list data already)
            r.x = (w - 0.5f * h) * 0.55f;
            r.width = 0.5f * h;
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
                r.x = (w - 0.5f * h) * 0.45f;
                r.width = 0.5f * h;
                g.fontStyle = FontStyle.Italic;
                GUI.color = Color.white;
                GUI.Label(r, "Not Available", g);
                g.fontStyle = FontStyle.Bold;
            }

            r.y += 0.12f * h;
        }

        //r.x = (w - 0.7f * h) / 2;
        //r.y = 0.2f * h;
        //r.width = 0.7f * h;
        //r.height = 0.05f * h;
        //for (int i = 0; i < menuItems.Length; i++)
        //{
        //    if (i < topOfMenuList || i > topOfMenuList + MENUITEMSINLIST)
        //        continue;
        //    if (currentCustomer.playerData.level < 2 && i > 20)
        //        continue;

        //    //c = Color.white;
        //    //if (i == menuItemSelection)
        //    //{
        //    //    c = Color.yellow;
        //    //    if (rejectFlashTimer > 0f)
        //    //        c.g = (rejectFlashTimer * 5f) % 1f;
        //    //}
        //    //GUI.color = c;


        //    r.y += 0.06f * h;
        //}
    }
}
